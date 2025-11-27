using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // Construtor para receber o serviço
        public DataLoggerController(FiwareService fiwareService)
        {
            _fiwareService = fiwareService;
        }

        public IActionResult Index()
        {
            DataLoggerDAO dao = new DataLoggerDAO();
            List<DataLoggerViewModel> lista = dao.Listagem();
            return View(lista);
        }

        public IActionResult Create()
        {
            ViewBag.Operacao = "I"; 

            DataLoggerDAO dao = new DataLoggerDAO();
            DataLoggerViewModel model = new DataLoggerViewModel();

            model.Id = dao.ProximoId();

            // Define valores padrão (exemplo)
            model.TempMin = 10;
            model.TempMax = 25;

            // Preenche o Dropdown de Vinícolas
            CarregaVinicolasViewBag();

            return View("Form", model);
        }

        //Tela de Edição (Formulário Preenchido)
        public IActionResult Edit(int id)
        {
            ViewBag.Operacao = "A";

            DataLoggerDAO dao = new DataLoggerDAO();
            DataLoggerViewModel model = dao.Consulta(id);

            if (model == null)
                return RedirectToAction("Index");

            // Preenche o Dropdown de Vinícolas (Selecionando a atual)
            CarregaVinicolasViewBag();

            return View("Form", model);
        }

        //Salvar (Recebe o Submit do Form)
        public async Task<IActionResult> Salvar(DataLoggerViewModel model, string operacao)
        {
            try
            {
                ValidaDados(model);

                if (ModelState.IsValid)
                {
                    DataLoggerDAO dao = new DataLoggerDAO();

                    // IMPORTANTE: Forçar o ID do usuário logado por segurança
                    // Se você ainda não tem login, deixe fixo 1 por enquanto
                    // model.UserId = ObterIdUsuarioLogado(); 
                    model.UserId = 1;

                    if (operacao == "I")
                    {
                        dao.Inserir(model);
                        // --- CHAMADA AO FIWARE ---
                        // Só chamamos ao inserir um novo, pois o ID não muda na edição geralmente
                        if (!string.IsNullOrEmpty(model.DeviceId))
                        {
                            bool sucessoFiware = await _fiwareService.CriarDispositivoFiware(model.DeviceId);
                            if (!sucessoFiware)
                            {
                                // Opcional: Adicionar um alerta que salvou no banco mas falhou no Fiware
                                TempData["Erro"] = "Dispositivo salvo localmente, mas houve erro na comunicação com o FIWARE.";
                            }
                        }

                    }
                    else
                        dao.Alterar(model);

                    return RedirectToAction("Index");
                }
                else
                {
                    // Se deu erro de validação, precisamos recarregar o ViewBag
                    // senão o dropdown some e dá erro na tela
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
                DataLoggerDAO dao = new DataLoggerDAO();
                dao.Excluir(id);
                return RedirectToAction("Index");
            }
            catch (Exception erro)
            {
                return RedirectToAction("Index");
            }
        }

        private void ValidaDados(DataLoggerViewModel model)
        {
            ModelState.Clear();

            if (model.Id <= 0)
                ModelState.AddModelError("Id", "ID inválido.");

            if (model.WineryId <= 0)
                ModelState.AddModelError("WineryId", "Selecione uma vinícola.");

            if (model.TempMin >= model.TempMax)
                ModelState.AddModelError("TempMin", "A temperatura mínima deve ser menor que a máxima.");

            if (string.IsNullOrEmpty(model.DeviceId))
                ModelState.AddModelError("DeviceId", "O ID do dispositivo (logger) é obrigatório.");
        }

        private void CarregaVinicolasViewBag()
        {
            WineryDAO wineryDao = new WineryDAO();
            var listaVinicolas = wineryDao.Listagem();

            ViewBag.Vinicolas = new SelectList(listaVinicolas, "Id", "Name");
        }
    }
}