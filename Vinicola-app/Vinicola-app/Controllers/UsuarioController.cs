using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using Vinicola_app.DAO;
using Vinicola_app.Models;
using Vinicola_app.Models;
using System.Text;             // Necessário para o Hash
using System.Security.Cryptography; // Necessário para o Hash


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

                // 1. Busca os dados atuais do banco para preservar informações se necessário
                var usuarioAntigo = dao.Consulta(model.Id);

                // 2. Lógica da SENHA
                if (!string.IsNullOrEmpty(model.NovaSenha))
                {
                    // Se digitou algo, gera o hash novo
                    model.SenhaHash = GerarHash(model.NovaSenha);
                }
                else
                {
                    // Se deixou em branco, mantém a senha antiga do banco
                    model.SenhaHash = usuarioAntigo.SenhaHash;
                }

                // 3. Lógica da IMAGEM
                if (model.FotoUpload != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        model.FotoUpload.CopyTo(memoryStream);
                        model.FotoProfile = memoryStream.ToArray();
                    }
                }
                else
                {
                    // Mantém a foto antiga
                    model.FotoProfile = usuarioAntigo.FotoProfile;
                }

                // 4. Salva no Banco
                dao.Alterar(model);

                // 5. Atualiza Sessão (Nome e Foto podem ter mudado)
                HttpContext.Session.SetString("UsuarioNome", model.Nome);
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

        // Método auxiliar para Hash (SHA256) - Copie se não tiver um helper global
        private string GerarHash(string senha)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Converte a senha para bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(senha));

                // Converte os bytes para string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
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