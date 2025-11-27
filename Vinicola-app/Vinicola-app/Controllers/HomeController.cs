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

            // Busca dados atuais
            var jsonAtual = await _fiwareService.ObterDadosAtuais(deviceId);

            double tempAtual = 0;
            double humAtual = 0;
            double lumAtual = 0;

            if (!string.IsNullOrEmpty(jsonAtual))
            {
                try
                {
                    var dados = Newtonsoft.Json.Linq.JObject.Parse(jsonAtual);

                    // --- CORREÇÃO DE SEGURANÇA ---
                    // Função para converter "nan", nulos ou textos inválidos em 0.0 sem dar erro
                    double LerValorSeguro(Newtonsoft.Json.Linq.JToken token)
                    {
                        if (token == null || token["value"] == null) return 0;

                        string valorStr = token["value"].ToString();

                        // Se vier "nan" (erro comum do sensor), retorna 0
                        if (valorStr.ToLower().Contains("nan")) return 0;

                        // Tenta converter. Se falhar, retorna 0.
                        if (double.TryParse(valorStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double valor))
                        {
                            return valor;
                        }
                        return 0;
                    }

                    tempAtual = LerValorSeguro(dados["temperature"]);
                    humAtual = LerValorSeguro(dados["humidity"]);
                    lumAtual = LerValorSeguro(dados["luminosity"]);
                }
                catch { /* Ignora erro de JSON */ }
            }

            // Retorna JSON para o Dashboard
            return Json(new
            {
                kpi = new
                {
                    temp = tempAtual,
                    hum = humAtual,
                    lum = lumAtual
                },
                // Enviamos listas vazias para o gráfico não quebrar enquanto não focamos no histórico
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