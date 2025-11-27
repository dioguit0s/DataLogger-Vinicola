using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vinicola_app.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

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
            return View();
        }

        public IActionResult Sobre()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObterDadosDashboard(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId) || deviceId == "all")
                deviceId = "003";

            // 1. Busca dados atuais (Orion)
            var jsonAtual = await _fiwareService.ObterDadosAtuais(deviceId);

            // 2. Busca histórico (STH)
            var jsonHistTemp = await _fiwareService.ObterHistorico(deviceId, "temperature");
            var jsonHistHum = await _fiwareService.ObterHistorico(deviceId, "humidity");
            var jsonHistLum = await _fiwareService.ObterHistorico(deviceId, "luminosity");

            // 3. Processa o retorno (Simplificado)
            dynamic dadosAtuais = jsonAtual != null ? JObject.Parse(jsonAtual) : null;

            // Extrai valores atuais (seguro contra nulos)
            double tempAtual = dadosAtuais?.temperature?.value ?? 0;
            double humAtual = dadosAtuais?.humidity?.value ?? 0;
            double lumAtual = dadosAtuais?.luminosity?.value ?? 0;

            // Processa histórico para arrays simples [10, 12, 15...]
            var histTemp = ProcessarHistoricoFIWARE(jsonHistTemp);
            var histHum = ProcessarHistoricoFIWARE(jsonHistHum);
            var histLum = ProcessarHistoricoFIWARE(jsonHistLum);

            return Json(new
            {
                kpi = new { temp = tempAtual, hum = humAtual, lum = lumAtual },
                charts = new
                {
                    temp = histTemp,
                    hum = histHum,
                    lum = histLum
                }
            });
        }

        // Auxiliar para limpar o JSON complexo do STH e retornar só os valores
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