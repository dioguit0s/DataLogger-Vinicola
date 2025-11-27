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
        private SqlParameter[] CriaParametros(UsuarioViewModel usuario, bool incluirId)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("nome", usuario.Nome),
                new SqlParameter("email", usuario.Email),
                new SqlParameter("password_hash", usuario.SenhaHash),
                new SqlParameter("profile_pic", SqlDbType.VarBinary, -1)
                {
                    Value = (object)usuario.FotoProfile ?? DBNull.Value
                }
            };

            if (incluirId)
            {
                parametros.Add(new SqlParameter("id", usuario.Id));
            }

            return parametros.ToArray();
        }

        public void Inserir(UsuarioViewModel usuario)
        {
            HelperDAO.ExecutaProc("sp_users_insert", CriaParametros(usuario, false));
        }

        public void Alterar(UsuarioViewModel usuario)
        {
            HelperDAO.ExecutaProc("sp_users_update", CriaParametros(usuario, true));
        }

        public void Excluir(int id)
        {
            SqlParameter[] p = { new SqlParameter("id", id) };
            HelperDAO.ExecutaProc("sp_users_delete", p);
        }

        private UsuarioViewModel MontaUsuario(DataRow registro)
        {
            UsuarioViewModel u = new UsuarioViewModel();
            u.Id = Convert.ToInt32(registro["id"]);
            u.Nome = registro["nome"].ToString();
            u.Email = registro["email"].ToString();

            if (registro.Table.Columns.Contains("password_hash"))
                u.SenhaHash = registro["password_hash"].ToString();

            if (registro["profile_pic"] != DBNull.Value)
            {
                if (registro["profile_pic"] is byte[])
                    u.FotoProfile = (byte[])registro["profile_pic"];
            }

            return u;
        }

        public UsuarioViewModel Consulta(int id)
        {
            SqlParameter[] p = { new SqlParameter("id", id) };
            DataTable tabela = HelperDAO.ExecutaProcSelect("sp_users_select", p);
            return tabela.Rows.Count == 0 ? null : MontaUsuario(tabela.Rows[0]);
        }

        public List<UsuarioViewModel> Listagem()
        {
            List<UsuarioViewModel> lista = new List<UsuarioViewModel>();
            DataTable tabela = HelperDAO.ExecutaProcSelect("sp_users_select", null);

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
            SqlParameter[] p = {
                new SqlParameter("email", email),
                new SqlParameter("password_hash", senha)
            };

            DataTable tabela = HelperDAO.ExecutaProcSelect("sp_users_login", p);

            if (tabela.Rows.Count == 0)
                return null;

            return MontaUsuario(tabela.Rows[0]);
        }
    }
}