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
            SqlParameter[] p = new SqlParameter[2];
            p[0] = new SqlParameter("id", usuario.Id);
            p[1] = new SqlParameter("nome", usuario.Nome);
            p[2] = new SqlParameter("email", usuario.Email);
            p[3] = new SqlParameter("senhaHash", usuario.SenhaHash);
            p[4] = new SqlParameter("fotoProfile", usuario.FotoProfile); 
            return p;
        }

        public void Inserir(UsuarioViewModel usuario)
        {
            string sql = "insert into usuarios(id, nome, email, senhaHash, fotoProfile) values (@id, @nome, @email, @password_hash, @profile_pic)";
            HelperDAO.ExecutaSQL(sql, CriaParametros(usuario));
        }

        public void Alterar(UsuarioViewModel usuario)
        {
            string sql = "update usuarios set nome = @nome, email = @email, senhaHash = @password_hash, fotoProfile = @profile_pic where id = @id";
            HelperDAO.ExecutaSQL(sql, CriaParametros(usuario));
        }

        public void Excluir(int id)
        {
            string sql = "delete from usuarios where id = @id";
            SqlParameter[] p = { new SqlParameter("id", id) };
            HelperDAO.ExecutaSQL(sql, p);
        }

        private UsuarioViewModel MontaUsuario(DataRow registro)
        {
            UsuarioViewModel u = new UsuarioViewModel();
            u.Id = Convert.ToInt32(registro["id"]);
            u.Nome = registro["nome"].ToString();
            u.Email = registro["email"].ToString();
            u.SenhaHash = registro["senhaHash"].ToString();
            u.FotoProfile = registro["fotoProfile"].ToString();
            return u;
        }

        public UsuarioViewModel Consulta(int id)
        {
            string sql = "select * from usuarios where id = @id";
            SqlParameter[] p = { new SqlParameter("id", id) };
            DataTable tabela = HelperDAO.ExecutaSelect(sql, p);
            return tabela.Rows.Count == 0 ? null : MontaUsuario(tabela.Rows[0]);
        }

        public List<UsuarioViewModel> Listagem()
        {
            List<UsuarioViewModel> lista = new List<UsuarioViewModel>();
            string sql = "select * from usuarios order by nome";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaUsuario(registro));
            return lista;
        }

        public int ProximoId()
        {
            string sql = "select isnull(max(id) + 1, 1) as 'MAIOR' from usuarios";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            return Convert.ToInt32(tabela.Rows[0]["MAIOR"]);
        }
    }
}