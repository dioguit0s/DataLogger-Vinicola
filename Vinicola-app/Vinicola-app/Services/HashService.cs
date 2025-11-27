using System.Security.Cryptography;
using System.Text;

namespace Vinicola_app.Services
{
    public static class HashService
    {
        public static string GerarHash(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Converte a string para bytes e calcula o hash
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));

                // Converte os bytes de volta para string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
