using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Vinicola_app.DAO;
using Vinicola_app.Models;
using System;

namespace Vinicola_app.Controllers
{
    public class WineryController : Controller
    {
        // LISTAGEM (Index)
        public IActionResult Index()
        {
            // Verifica se está logado
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            WineryDAO dao = new WineryDAO();
            // Passa o ID do usuário para listar apenas as vinícolas dele
            List<WineryViewModel> lista = dao.Listagem(usuarioId.Value);

            return View(lista);
        }

        // TELA DE CRIAÇÃO (Create)
        public IActionResult Create()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            ViewBag.Operacao = "I"; // I = Inserir

            WineryViewModel model = new WineryViewModel();
            model.UserId = usuarioId.Value;

            return View("Form", model);
        }

        // TELA DE EDIÇÃO (Edit)
        public IActionResult Edit(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            ViewBag.Operacao = "A"; // A = Alterar

            WineryDAO dao = new WineryDAO();
            WineryViewModel model = dao.Consulta(id);

            if (model == null)
                return RedirectToAction("Index");

            // Segurança: impede editar vinícola de outro usuário
            if (model.UserId != usuarioId.Value)
                return RedirectToAction("Index");

            return View("Form", model);
        }

        // AÇÃO DE SALVAR (Recebe o Form) - ESTE ERA O TRECHO FALTANTE
        [HttpPost]
        public IActionResult Salvar(WineryViewModel model, string operacao)
        {
            // 1. Validação de Sessão
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Index", "Login");

            model.UserId = usuarioId.Value; // Garante que a vinícola pertence ao usuário logado

            try
            {
                // 2. Validação Manual de Dados
                ValidaDados(model);

                if (ModelState.IsValid)
                {
                    WineryDAO dao = new WineryDAO();

                    if (operacao == "I")
                    {
                        dao.Inserir(model);
                    }
                    else
                    {
                        dao.Alterar(model);
                    }

                    // Sucesso: volta para a listagem
                    return RedirectToAction("Index");
                }
                else
                {
                    // Erro de Validação: devolve o formulário com os erros
                    ViewBag.Operacao = operacao;
                    return View("Form", model);
                }
            }
            catch (Exception erro)
            {
                // Erro de Banco/Sistema: exibe mensagem na tela
                ViewBag.Operacao = operacao;
                ViewBag.Erro = "Ocorreu um erro ao salvar: " + erro.Message;
                return View("Form", model);
            }
        }

        // AÇÃO DE EXCLUIR
        public IActionResult Delete(int id)
        {
            try
            {
                // Validação de segurança básica antes de excluir
                int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (usuarioId == null) return RedirectToAction("Index", "Login");

                WineryDAO dao = new WineryDAO();
                WineryViewModel model = dao.Consulta(id);

                if (model != null && model.UserId == usuarioId.Value)
                {
                    dao.Excluir(id);
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // Opcional: Adicionar mensagem de erro via TempData se falhar (ex: chave estrangeira)
                TempData["Erro"] = "Não foi possível excluir. Verifique se há sensores vinculados.";
                return RedirectToAction("Index");
            }
        }

        // MÉTODOS AUXILIARES
        private void ValidaDados(WineryViewModel model)
        {
            ModelState.Clear(); // Limpa validações automáticas para usarmos as nossas

            if (string.IsNullOrEmpty(model.Name))
                ModelState.AddModelError("Name", "O nome da vinícola é obrigatório.");

            if (string.IsNullOrEmpty(model.Cnpj))
                ModelState.AddModelError("Cnpj", "O CNPJ é obrigatório.");

            if (string.IsNullOrEmpty(model.Address))
                ModelState.AddModelError("Address", "O endereço é obrigatório.");

            if (string.IsNullOrEmpty(model.Email))
                ModelState.AddModelError("Email", "O e-mail é obrigatório.");

            if (string.IsNullOrEmpty(model.Telephone))
                ModelState.AddModelError("Telephone", "O telefone é obrigatório.");
        }
    }
}