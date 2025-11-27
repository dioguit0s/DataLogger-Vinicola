using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Vinicola_app.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Vinicola_app.DAO; 
using Vinicola_app.Models;

namespace Vinicola_app.Controllers
{
    public class HomeController : Controller
    {
        private readonly FiwareService _fiwareService;

        public HomeController(FiwareService fiwareService)
        {
            _fiwareService = fiwareService;
        }

        public IActionResult Index()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            WineryDAO wineryDao = new WineryDAO();
            DataLoggerDAO loggerDao = new DataLoggerDAO();

            DashboardViewModel model = new DashboardViewModel();
            model.Vinicolas = wineryDao.Listagem(usuarioId.Value);
            model.Loggers = loggerDao.Listagem(usuarioId.Value);

            return View(model);
        }

        public IActionResult Sobre()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ObterDadosDashboard(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId) || deviceId == "all")
            {
                return Json(new { error = "Selecione um sensor." });
            }

            // 1. Busca dados atuais do FIWARE
            var jsonAtual = await _fiwareService.ObterDadosAtuais(deviceId);

            double tempAtual = 0;
            double humAtual = 0;
            double lumAtual = 0;

            if (!string.IsNullOrEmpty(jsonAtual))
            {
                try
                {
                    var dados = Newtonsoft.Json.Linq.JObject.Parse(jsonAtual);

                    // --- FUNÇÃO DE LEITURA SEGURA ---
                    double LerValorSeguro(Newtonsoft.Json.Linq.JToken token)
                    {
                        if (token == null || token["value"] == null) return 0;

                        string valorStr = token["value"].ToString();

                        if (valorStr.ToLower().Contains("nan")) return 0;

                        valorStr = valorStr.Replace(",", ".");

                        if (double.TryParse(valorStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double valor))
                        {
                            return valor;
                        }
                        return 0;
                    }

                    tempAtual = LerValorSeguro(dados["temperature"]);
                    humAtual = LerValorSeguro(dados["humidity"]);
                    lumAtual = LerValorSeguro(dados["luminosity"]);

                    // =========================================================================
                    // 🔒 LÓGICA DE VERIFICAÇÃO DE ALARMES (ATUALIZADA)
                    // =========================================================================
                    try
                    {
                        DataLoggerDAO dao = new DataLoggerDAO();
                        var config = dao.BuscarPorDeviceId(deviceId);

                        if (config != null)
                        {
                            // Lógica em cadeia: Verifica um por um.
                            // O primeiro que estiver fora dos limites dispara o alarme e impede o comando OFF.

                            bool acionouAlarme = false;

                            // 1. Verifica TEMPERATURA
                            if (tempAtual < config.TempMin || tempAtual > config.TempMax)
                            {
                                await _fiwareService.EnviarComando(deviceId, "TEMP_ALARM");
                                acionouAlarme = true;
                            }
                            // 2. Se Temperatura OK, Verifica UMIDADE
                            else if (humAtual < config.HumidMin || humAtual > config.HumidMax)
                            {
                                await _fiwareService.EnviarComando(deviceId, "HUM_ALARM");
                                acionouAlarme = true;
                            }
                            // 3. Se Umidade OK, Verifica LUMINOSIDADE
                            else if (lumAtual < config.LumMin || lumAtual > config.LumMax)
                            {
                                await _fiwareService.EnviarComando(deviceId, "LUM_ALARM");
                                acionouAlarme = true;
                            }

                            // 4. Se NENHUM alarme foi acionado (tudo dentro dos limites), desliga o LED
                            if (!acionouAlarme)
                            {
                                await _fiwareService.EnviarComando(deviceId, "OFF");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro na lógica de alarme: {ex.Message}");
                    }
                    // =========================================================================
                }
                catch { /* Ignora erro de JSON */ }
            }

            return Json(new
            {
                kpi = new
                {
                    temp = tempAtual,
                    hum = humAtual,
                    lum = lumAtual
                },
                charts = new { temp = new System.Collections.Generic.List<double>(), hum = new System.Collections.Generic.List<double>(), lum = new System.Collections.Generic.List<double>() }
            });
        }

        private List<double> ProcessarHistoricoFIWARE(string jsonSth)
        {
            var listaValores = new List<double>();
            try
            {
                var dados = JObject.Parse(jsonSth);
                var arrayValues = dados["contextResponses"]?[0]?["contextElement"]?["attributes"]?[0]?["values"] as JArray;

                if (arrayValues != null)
                {
                    foreach (var item in arrayValues)
                    {
                        listaValores.Add((double)item["attrValue"]);
                    }
                }
            }
            catch { }
            return listaValores;
        }
    }
}