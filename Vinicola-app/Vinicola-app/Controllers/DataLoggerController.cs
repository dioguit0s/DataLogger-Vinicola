using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http; // Necessário para Sessão
using System.Collections.Generic;
using Vinicola_app.DAO;
using Vinicola_app.Models;
using Vinicola_app.Services;
using System;
using System.Threading.Tasks;

namespace Vinicola_app.Controllers
{
    public class DataLoggerController : Controller
    {
        private readonly FiwareService _fiwareService;

        public DataLoggerController(FiwareService fiwareService)
        {
            _fiwareService = fiwareService;
        }

        public IActionResult Index()
        {
            // CORREÇÃO 1: "UsuarioId" com U maiúsculo (igual ao Login)
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            DataLoggerDAO dao = new DataLoggerDAO();
            // Busca apenas os sensores desse usuário
            List<DataLoggerViewModel> lista = dao.Listagem(usuarioId.Value);

            return View(lista);
        }

        public IActionResult Create()
        {
            // Verifica login
            if (HttpContext.Session.GetInt32("UsuarioId") == null)
                return RedirectToAction("Index", "Login");

            ViewBag.Operacao = "I";

            DataLoggerDAO dao = new DataLoggerDAO();
            DataLoggerViewModel model = new DataLoggerViewModel();

            // Gera próximo ID (apenas visual, o banco gera o real)
            model.Id = dao.ProximoId();

            // Valores padrão
            model.TempMin = 10;
            model.TempMax = 25;
            model.HumidMin = 30;
            model.HumidMax = 80;
            model.LumMin = 0;
            model.LumMax = 1000;

            CarregaVinicolasViewBag();

            return View("Form", model);
        }

        public IActionResult Edit(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            ViewBag.Operacao = "A";

            DataLoggerDAO dao = new DataLoggerDAO();
            DataLoggerViewModel model = dao.Consulta(id);

            if (model == null) return RedirectToAction("Index");

            // Segurança: impede editar sensor de outro usuário
            if (model.UserId != usuarioId.Value) return RedirectToAction("Index");

            CarregaVinicolasViewBag();

            return View("Form", model);
        }

        [HttpPost]
        public async Task<IActionResult> Salvar(DataLoggerViewModel model, string operacao)
        {
            // CORREÇÃO 2: Pega o ID da sessão corretamente
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            // CORREÇÃO 3: Removemos o "model.UserId = 1" fixo e usamos o da sessão
            model.UserId = usuarioId.Value;

            try
            {
                ValidaDados(model);

                if (ModelState.IsValid)
                {
                    DataLoggerDAO dao = new DataLoggerDAO();

                    if (operacao == "I")
                    {
                        dao.Inserir(model);

                        // Integração FIWARE (Só no cadastro)
                        if (!string.IsNullOrEmpty(model.DeviceId))
                        {
                            var responseFiware = await _fiwareService.CriarDispositivoFiware(model.DeviceId);

                            // Código 201 = Created, 200 = OK (alguns serviços retornam 200 se já existe)
                            if ((int)responseFiware.StatusCode == 201 || (int)responseFiware.StatusCode == 200)
                            {
                                TempData["Sucesso"] = "Salvo no banco e sincronizado com FIWARE!";
                            }
                            else
                            {
                                string erroConteudo = await responseFiware.Content.ReadAsStringAsync();
                                TempData["Erro"] = $"Salvo localmente, mas falha no FIWARE: {responseFiware.StatusCode} - {erroConteudo}";
                            }
                        }
                    }
                    else
                    {
                        dao.Alterar(model);
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Operacao = operacao;
                    CarregaVinicolasViewBag();
                    return View("Form", model);
                }
            }
            catch (Exception erro)
            {
                ViewBag.Operacao = operacao;
                CarregaVinicolasViewBag();
                ViewBag.Erro = "Ocorreu um erro: " + erro.Message;
                return View("Form", model);
            }
        }

        public IActionResult Delete(int id)
        {
            try
            {
                int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (usuarioId == null) return RedirectToAction("Index", "Login");

                DataLoggerDAO dao = new DataLoggerDAO();
                DataLoggerViewModel model = dao.Consulta(id);

                // Só exclui se pertencer ao usuário logado
                if (model != null && model.UserId == usuarioId.Value)
                {
                    dao.Excluir(id);
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }

        // Métodos Auxiliares
        private void ValidaDados(DataLoggerViewModel model)
        {
            ModelState.Clear();

            if (model.WineryId <= 0)
                ModelState.AddModelError("WineryId", "Selecione uma vinícola.");

            if (string.IsNullOrEmpty(model.DeviceId))
                ModelState.AddModelError("DeviceId", "O ID do dispositivo é obrigatório.");

            // Validações lógicas básicas
            if (model.TempMin >= model.TempMax)
                ModelState.AddModelError("TempMin", "Temp. Mínima deve ser menor que a Máxima.");

            // Você pode adicionar mais validações para Umidade e Luminosidade aqui se quiser
        }

        private void CarregaVinicolasViewBag()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId != null)
            {
                WineryDAO wineryDao = new WineryDAO();
                var listaVinicolas = wineryDao.Listagem(usuarioId.Value);
                ViewBag.Vinicolas = new SelectList(listaVinicolas, "Id", "Name");
            }
            else
            {
                ViewBag.Vinicolas = new SelectList(new List<WineryViewModel>(), "Id", "Name");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TesteFiware()
        {
            string resultadoJson = await _fiwareService.ListarDispositivos();
            return Content(resultadoJson, "application/json");
        }
    }
}