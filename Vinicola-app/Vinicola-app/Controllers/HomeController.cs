using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Vinicola_app.Services;
using System.Collections.Generic;
using System; // Necess�rio para Random e DateTime
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
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            WineryDAO wineryDao = new WineryDAO();
            DataLoggerDAO loggerDao = new DataLoggerDAO();

            DashboardViewModel model = new DashboardViewModel();
            model.Vinicolas = wineryDao.Listagem(usuarioId.Value);
            model.Loggers = loggerDao.Listagem(usuarioId.Value);

            return View(model);
        }

        // --- NOVA ACTION: RELAT�RIO ---
        public IActionResult Relatorio()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            // MOCK: Gerando lista de dados fict�cios para o relat�rio
            var listaRelatorio = new List<LeituraSensorViewModel>();
            var random = new Random();

            for (int i = 0; i < 20; i++)
            {
                listaRelatorio.Add(new LeituraSensorViewModel
                {
                    DataHora = DateTime.Now.AddMinutes(-i * 15), // A cada 15 min atr�s
                    Temperatura = Math.Round(20 + (random.NextDouble() * 10), 2), // Entre 20 e 30
                    Umidade = Math.Round(40 + (random.NextDouble() * 40), 2),     // Entre 40 e 80
                    Luminosidade = random.Next(100, 900),
                    Status = (i % 5 == 0) ? "Alerta" : "Normal" // Simula um alerta as vezes
                });
            }

            return View(listaRelatorio);
        }

        public IActionResult Sobre()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ObterDadosDashboard(string deviceId)
        {
            // Manteve a l�gica de valida��o
            if (string.IsNullOrEmpty(deviceId) || deviceId == "all")
            {
                return Json(new { error = "Selecione um sensor." });
            }

            // Busca o dado atual (mantemos a l�gica real do Fiware para o KPI atual)
            var jsonAtual = await _fiwareService.ObterDadosAtuais(deviceId);

            // Vari�veis iniciais
            double tempAtual = 0;
            double humAtual = 0;
            double lumAtual = 0;

            // ... (Mantenha sua l�gica de "LerValorSeguro" e Parsing do JSON atual aqui) ...
            // Para simplificar o exemplo, vou assumir que voc� manteve o bloco try/catch existente
            // Se o Fiware falhar ou n�o tiver dados, geramos um random para o "Atual" tamb�m para n�o quebrar o mock
            Random rnd = new Random();
            if (tempAtual == 0) tempAtual = Math.Round(22 + rnd.NextDouble() * 5, 1);
            if (humAtual == 0) humAtual = Math.Round(50 + rnd.NextDouble() * 10, 1);
            if (lumAtual == 0) lumAtual = rnd.Next(300, 500);

            // =========================================================================
            // MOCK PARA OS GR�FICOS (Hist�rico)
            // =========================================================================
            // Gera 12 pontos de dados (�ltima hora, por exemplo) para preencher os gr�ficos
            var chartTemp = new List<double>();
            var chartHum = new List<double>();
            var chartLum = new List<double>();

            for (int i = 0; i < 12; i++)
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
    }

    // Classe simples para usar apenas na View de Relat�rio
    public class LeituraSensorViewModel
    {
        public DateTime DataHora { get; set; }
        public double Temperatura { get; set; }
        public double Umidade { get; set; }
        public double Luminosidade { get; set; }
        public string Status { get; set; }
    }
}