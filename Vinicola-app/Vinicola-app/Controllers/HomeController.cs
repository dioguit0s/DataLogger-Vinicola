using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Vinicola_app.Services;
using System.Collections.Generic;
using System; // Necessário para Random e DateTime
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

        // --- NOVA ACTION: RELATÓRIO ---
        public IActionResult Relatorio()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            // MOCK: Gerando lista de dados fictícios para o relatório
            var listaRelatorio = new List<LeituraSensorViewModel>();
            var random = new Random();

            for (int i = 0; i < 20; i++)
            {
                listaRelatorio.Add(new LeituraSensorViewModel
                {
                    DataHora = DateTime.Now.AddMinutes(-i * 15), // A cada 15 min atrás
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
            // Manteve a lógica de validação
            if (string.IsNullOrEmpty(deviceId) || deviceId == "all")
            {
                return Json(new { error = "Selecione um sensor." });
            }

            // Busca o dado atual (mantemos a lógica real do Fiware para o KPI atual)
            var jsonAtual = await _fiwareService.ObterDadosAtuais(deviceId);

            // Variáveis iniciais
            double tempAtual = 0;
            double humAtual = 0;
            double lumAtual = 0;

            // ... (Mantenha sua lógica de "LerValorSeguro" e Parsing do JSON atual aqui) ...
            // Para simplificar o exemplo, vou assumir que você manteve o bloco try/catch existente
            // Se o Fiware falhar ou não tiver dados, geramos um random para o "Atual" também para não quebrar o mock
            Random rnd = new Random();
            if (tempAtual == 0) tempAtual = Math.Round(22 + rnd.NextDouble() * 5, 1);
            if (humAtual == 0) humAtual = Math.Round(50 + rnd.NextDouble() * 10, 1);
            if (lumAtual == 0) lumAtual = rnd.Next(300, 500);

            // =========================================================================
            // MOCK PARA OS GRÁFICOS (Histórico)
            // =========================================================================
            // Gera 12 pontos de dados (última hora, por exemplo) para preencher os gráficos
            var chartTemp = new List<double>();
            var chartHum = new List<double>();
            var chartLum = new List<double>();

            for (int i = 0; i < 12; i++)
            {
                chartTemp.Add(Math.Round(tempAtual - 2 + (rnd.NextDouble() * 4), 1)); // Variação próxima da atual
                chartHum.Add(Math.Round(humAtual - 5 + (rnd.NextDouble() * 10), 1));
                chartLum.Add(Math.Round(lumAtual - 50 + (rnd.NextDouble() * 100), 0));
            }

            return Json(new
            {
                kpi = new
                {
                    temp = tempAtual,
                    hum = humAtual,
                    lum = lumAtual
                },
                charts = new
                {
                    temp = chartTemp,
                    hum = chartHum,
                    lum = chartLum
                }
            });
        }
    }

    // Classe simples para usar apenas na View de Relatório
    public class LeituraSensorViewModel
    {
        public DateTime DataHora { get; set; }
        public double Temperatura { get; set; }
        public double Umidade { get; set; }
        public double Luminosidade { get; set; }
        public string Status { get; set; }
    }
}