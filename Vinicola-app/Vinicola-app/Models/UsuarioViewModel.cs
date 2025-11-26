namespace Vinicola_app.Models
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        public string Email { get; set; }

        public string SenhaHash { get; set; }

        public string FotoProfile { get; set; }
    }
}