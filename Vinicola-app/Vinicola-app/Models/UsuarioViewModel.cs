namespace Vinicola_app.Models
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        public string Email { get; set; }

        public string SenhaHash { get; set; }

        // Campo para armazenar os bits da imagem (Vai para o Banco)
        public byte[]? FotoProfile { get; set; }

        // Campo auxiliar para receber o upload no formulário (Não vai para o Banco)
        public IFormFile? FotoUpload { get; set; }

        // Novo campo para receber a senha digitada na tela de edição
        public string? NovaSenha { get; set; }


    }
}