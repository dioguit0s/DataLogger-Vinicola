using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Necessário para Sessão
using Vinicola_app.DAO;
using Vinicola_app.Models;
using System;

namespace Vinicola_app.Controllers
{
    public class LoginController : Controller
    {
        // Exibe a tela de login
        public IActionResult Index()
        {
            HttpContext.Session.Clear();

            return View();
        }

        // Processa o formulário de login
        [HttpPost]
        public IActionResult Autenticar(string email, string senhaHash) // senhaHash aqui seria a senha digitada
        {
            try
            {
                UsuarioDAO dao = new UsuarioDAO();

                // Você precisará criar este método 'VerificarLogin' no seu UsuarioDAO
                // Ele deve retornar o objeto Usuario se achar, ou null se falhar
                UsuarioViewModel usuario = dao.VerificarLogin(email, senhaHash);

                if (usuario != null)
                {
                    // Grava dados na sessão
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                    HttpContext.Session.SetString("UsuarioNome", usuario.Nome);

                    // Redireciona para a página principal do sistema
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErroLogin"] = "E-mail ou senha inválidos.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception erro)
            {
                TempData["ErroLogin"] = "Erro ao tentar logar: " + erro.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult Sair()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}