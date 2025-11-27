namespace Vinicola_app.Models
{
    public class WineryViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Cnpj { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public byte[]? LogoPic { get; set; }
        public IFormFile? LogoUpload { get; set; }
    }
}
