using Microsoft.AspNetCore.Mvc;
using System;
using Vinicola_app.DAO;
using Vinicola_app.Models;
using Vinicola_app.Services;

namespace Vinicola_app.Controllers
{
    public class RegisterController : Controller
    {
        // Exibe a tela de cadastro
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Salvar(string nome, string email, string senhaHash, string confirmaSenha)
        {
            try
            {
                // 1. Validação básica de senhas
                if (senhaHash != confirmaSenha)
                {
                    TempData["ErroRegister"] = "As senhas não conferem.";
                    return RedirectToAction("Index");
                }

                UsuarioDAO dao = new UsuarioDAO();

                // 2. Monta o objeto Usuário
                UsuarioViewModel usuario = new UsuarioViewModel();
                usuario.Nome = nome;
                usuario.Email = email;
                usuario.SenhaHash = HashService.GerarHash(senhaHash);
                usuario.FotoProfile = null; // Define uma foto padrão para não quebrar o banco

                // 3. Salva no banco
                dao.Inserir(usuario);

                // 4. Redireciona para o Login após sucesso
                return RedirectToAction("Index", "Login");
            }
            catch (Exception erro)
            {
                TempData["ErroRegister"] = "Erro ao cadastrar: " + erro.Message;
                return RedirectToAction("Index");
            }
        }
    }
}