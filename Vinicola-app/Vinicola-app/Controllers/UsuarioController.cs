using Vinicola_app.Models;
using Vinicola_app.DAO;
using Microsoft.AspNetCore.Mvc;
using System;
using Vinicola_app.Models;


namespace Vinicola_app.Controllers
{
    public class UsuarioController : Controller
    {
        public ActionResult Index()
        {
            UsuarioDAO dao = new UsuarioDAO();
            var listaDeUsuarios = dao.Listagem();
            return View(listaDeUsuarios);
        }

        public IActionResult Perfil()
        {
            // Obtém o ID da sessão (ajuste conforme sua lógica de sessão atual)
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            UsuarioDAO dao = new UsuarioDAO();
            var usuario = dao.Consulta(usuarioId.Value); // Assumindo que você tem um método Consulta(int id)

            return View(usuario);
        }

        [HttpPost]
        public IActionResult SalvarPerfil(UsuarioViewModel model)
        {
            try
            {
                UsuarioDAO dao = new UsuarioDAO();

                // Lógica de conversão de Imagem para Bits
                if (model.FotoUpload != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        model.FotoUpload.CopyTo(memoryStream);
                        model.FotoProfile = memoryStream.ToArray(); // Converte para array de bytes
                    }
                }
                else
                {
                    // Se não upou nova foto, tenta manter a antiga (precisa buscar do banco para não perder)
                    var usuarioAntigo = dao.Consulta(model.Id);
                    model.FotoProfile = usuarioAntigo.FotoProfile;
                }

                dao.Alterar(model); // Atualize seu DAO para salvar o campo FotoPerfil

                // Atualiza sessão com novo nome se mudou
                HttpContext.Session.SetString("UsuarioNome", model.Nome);

                // Atualiza a foto na sessão (opcional, para exibir no layout sem ir no banco)
                if (model.FotoProfile != null)
                    HttpContext.Session.Set("UsuarioFoto", model.FotoProfile);

                TempData["Sucesso"] = "Perfil atualizado com sucesso!";
                return RedirectToAction("Perfil");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro ao atualizar: " + ex.Message;
                return View("Perfil", model);
            }
        }

        public ActionResult Create()
        {
            ViewBag.Operacao = "I";

            UsuarioDAO dao = new UsuarioDAO();
            UsuarioViewModel usuario = new UsuarioViewModel();
            return View("~/Views/Login/Form.cshtml", usuario);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Operacao = "A";

            UsuarioDAO dao = new UsuarioDAO();
            UsuarioViewModel usuario = dao.Consulta(id);
            if (usuario == null)
                return RedirectToAction("index");
            else
                return View("Form", usuario);
        }

        public ActionResult Salvar(UsuarioViewModel usuario, string operacao)
        {
            try
            {
                ValidaDados(usuario);
                if (ModelState.IsValid)
                {
                    UsuarioDAO dao = new UsuarioDAO();
                    if (operacao == "I")
                        dao.Inserir(usuario);
                    else
                        dao.Alterar(usuario);
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Operacao = operacao;

                    return View("Form", usuario);
                }
            }
            catch (Exception erro)
            {
                ModelState.AddModelError("Erro", "Ocorreu um erro: " + erro.Message);
                return View("Form", usuario);
            }
        }

        private void ValidaDados(UsuarioViewModel usuario)
        {
            ModelState.Clear();

            if (usuario.Id <= 0)
                ModelState.AddModelError("Id", "Campo obrigatório.");

            if (string.IsNullOrWhiteSpace(usuario.Nome))
                ModelState.AddModelError("Nome", "O nome é obrigatório.");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                UsuarioDAO dao = new UsuarioDAO();
                dao.Excluir(id);
                return RedirectToAction("Index");
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }
    }
}