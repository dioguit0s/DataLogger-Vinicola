using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Vinicola_app.DAO;
using Vinicola_app.Models;

namespace Vinicola_app.Controllers
{
    public class WineryController : Controller
    {
        public ActionResult Index()
        {
            WineryDAO dao = new WineryDAO();
            List<WineryViewModel> lista = dao.Listagem();
            return View(lista);
        }

        public ActionResult Create()
        {
            ViewBag.Operacao = "I";

            WineryDAO dao = new WineryDAO();
            WineryViewModel winery = new WineryViewModel();

            winery.Id = dao.ProximoId();

            return View("Form", winery);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Operacao = "A";

            WineryDAO dao = new WineryDAO();
            WineryViewModel winery = dao.Consulta(id);

            if (winery == null)
                return RedirectToAction("Index");
            else
                return View("Form", winery);
        }

        public ActionResult Salvar(WineryViewModel winery, string operacao)
        {
            try
            {
                ValidaDados(winery);

                if (ModelState.IsValid)
                {
                    WineryDAO dao = new WineryDAO();

                    if (operacao == "I")
                        dao.Inserir(winery);
                    else
                        dao.Alterar(winery);

                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Operacao = operacao;
                    return View("Form", winery);
                }
            }
            catch (Exception erro)
            {
                ViewBag.Operacao = operacao;
                ModelState.AddModelError("Erro", "Ocorreu um erro: " + erro.Message);
                return View("Form", winery);
            }
        }

        private void ValidaDados(WineryViewModel winery)
        {
            ModelState.Clear();

            if (winery.Id <= 0)
                ModelState.AddModelError("Id", "Campo ID é obrigatório.");

            if (string.IsNullOrWhiteSpace(winery.Name))
                ModelState.AddModelError("Name", "O Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(winery.Cnpj))
                ModelState.AddModelError("Cnpj", "O CNPJ é obrigatório.");

        }

        public ActionResult Delete(int id)
        {
            try
            {
                WineryDAO dao = new WineryDAO();
                dao.Excluir(id);
                return RedirectToAction("Index");
            }
            catch (Exception erro)
            {
                return Content("Erro ao excluir: " + erro.Message);
            }
        }
    }
}