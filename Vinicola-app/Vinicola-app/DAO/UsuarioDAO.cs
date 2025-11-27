using Vinicola_app.DAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Vinicola_app.Models;

namespace Vinicola_app.DAO
{
    public class UsuarioDAO
    {
        private SqlParameter[] CriaParametros(UsuarioViewModel usuario)
        {
            SqlParameter[] p = new SqlParameter[5];
            p[0] = new SqlParameter("id", usuario.Id);
            p[1] = new SqlParameter("nome", usuario.Nome);
            p[2] = new SqlParameter("email", usuario.Email);
            p[3] = new SqlParameter("password_hash", usuario.SenhaHash);
            p[4] = new SqlParameter("profile_pic", SqlDbType.VarBinary, -1)
            {
                Value = (object)usuario.FotoProfile ?? DBNull.Value
            };
            return p;
        }

        public void Inserir(UsuarioViewModel usuario)
        {
            string sql = "insert into users(nome, email, password_hash, profile_pic) values (@nome, @email, @password_hash, @profile_pic)";
            HelperDAO.ExecutaSQL(sql, CriaParametros(usuario));
        }

        public void Alterar(UsuarioViewModel usuario)
        {
            string sql = "update users set nome = @nome, email = @email, password_hash = @password_hash, profile_pic = @profile_pic where id = @id";
            HelperDAO.ExecutaSQL(sql, CriaParametros(usuario));
        }

        public void Excluir(int id)
        {
            string sql = "delete from users where id = @id";
            SqlParameter[] p = { new SqlParameter("id", id) };
            HelperDAO.ExecutaSQL(sql, p);
        }

        private UsuarioViewModel MontaUsuario(DataRow registro)
        {
            UsuarioViewModel u = new UsuarioViewModel();
            u.Id = Convert.ToInt32(registro["id"]);
            u.Nome = registro["nome"].ToString();
            u.Email = registro["email"].ToString();
            u.SenhaHash = registro["password_hash"].ToString();

            // Verificação de nulo e conversão correta
            if (registro["profile_pic"] != DBNull.Value)
            {
                var colType = registro.Table.Columns["profile_pic"].DataType;
                if (colType == typeof(byte[]))
                    u.FotoProfile = (byte[])registro["profile_pic"];
            }

            return u;
        }

        public UsuarioViewModel Consulta(int id)
        {
            string sql = "select * from users where id = @id";
            SqlParameter[] p = { new SqlParameter("id", id) };
            DataTable tabela = HelperDAO.ExecutaSelect(sql, p);
            return tabela.Rows.Count == 0 ? null : MontaUsuario(tabela.Rows[0]);
        }

        public List<UsuarioViewModel> Listagem()
        {
            List<UsuarioViewModel> lista = new List<UsuarioViewModel>();
            string sql = "select * from users order by nome";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaUsuario(registro));
            return lista;
        }

        public int ProximoId()
        {
            string sql = "select isnull(max(id) + 1, 1) as 'MAIOR' from users";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            return Convert.ToInt32(tabela.Rows[0]["MAIOR"]);
        }

        public UsuarioViewModel VerificarLogin(string email, string senha)
        {
            UsuarioViewModel usuario = null; // Inicializa como nulo. Se não achar, retorna nulo.

            // 1. Criar a conexão (Use a sua string de conexão ou método Helper)
            using (SqlConnection conexao = ConexaoBD.GetConexao())
            {
                // 2. Criar a instrução SQL
                // NOTA: Verifique se o nome da coluna no banco é 'Senha' ou 'SenhaHash'
                string sql = "SELECT Id, Nome, Email, password_hash FROM users WHERE Email = @email AND password_hash = @senha";

                using (SqlCommand comando = new SqlCommand(sql, conexao))
                {
                    // 3. Adicionar parâmetros (Segurança contra SQL Injection)
                    comando.Parameters.Add(new SqlParameter("@email", email));
                    comando.Parameters.Add(new SqlParameter("@senha", senha));

                    try
                    {
                        if (conexao.State != System.Data.ConnectionState.Open)
                        {
                            conexao.Open();
                        }

                        using (SqlDataReader leitor = comando.ExecuteReader())
                        {
                            if (leitor.Read())
                            {
                                usuario = new UsuarioViewModel();
                                usuario.Id = Convert.ToInt32(leitor["Id"]);
                                usuario.Nome = leitor["Nome"].ToString();
                                usuario.Email = leitor["Email"].ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // É uma boa prática logar o erro ou lançar para o controller tratar
                        throw new Exception("Erro ao verificar login: " + ex.Message);
                    }
                }
            }

            return usuario;
        }
    }
}