using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vinicola_app.Models;

namespace Vinicola_app.DAO
{
    public class WineryDAO
    {
        public void Inserir(WineryViewModel winery)
        {
            HelperDAO.ExecutaProc("sp_winery_insert", CriaParametros(winery, false));
        }

        public void Alterar(WineryViewModel winery)
        {
            HelperDAO.ExecutaProc("sp_winery_update", CriaParametros(winery, true));
        }

        private SqlParameter[] CriaParametros(WineryViewModel model, bool incluirId)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
    {
        new SqlParameter("user_id", model.UserId),
        new SqlParameter("name", model.Name ?? (object)DBNull.Value),
        new SqlParameter("description", model.Description ?? (object)DBNull.Value),
        new SqlParameter("address", model.Address ?? (object)DBNull.Value),
        new SqlParameter("cnpj", model.Cnpj ?? (object)DBNull.Value),
        new SqlParameter("email", model.Email ?? (object)DBNull.Value),
        new SqlParameter("telephone", model.Telephone ?? (object)DBNull.Value),
        
        new SqlParameter("logo_pic", SqlDbType.VarBinary, -1)
        {
            Value = (object)model.LogoPic ?? DBNull.Value
        }
    };

            if (incluirId)
            {
                parametros.Add(new SqlParameter("id", model.Id));
            }

            return parametros.ToArray();
        }

        public void Excluir(int id)
        {
            SqlParameter[] p = { new SqlParameter("id", id) };
            HelperDAO.ExecutaProc("sp_winery_delete", p);
        }

        private WineryViewModel MontaWinery(DataRow registro)
        {
            WineryViewModel w = new WineryViewModel();
            w.Id = Convert.ToInt32(registro["id"]);

            // ... (outros campos mantêm iguais) ...
            if (registro.Table.Columns.Contains("user_id"))
                w.UserId = Convert.ToInt32(registro["user_id"]);
            else if (registro.Table.Columns.Contains("userId"))
                w.UserId = Convert.ToInt32(registro["userId"]);

            w.Name = registro["name"].ToString();
            w.Description = registro["description"] != DBNull.Value ? registro["description"].ToString() : "";
            w.Address = registro["address"].ToString();
            w.Cnpj = registro["cnpj"].ToString();
            w.Email = registro["email"].ToString();
            w.Telephone = registro["telephone"].ToString();

            if (registro["logo_pic"] != DBNull.Value)
            {
                if (registro["logo_pic"] is byte[])
                {
                    w.LogoPic = (byte[])registro["logo_pic"];
                }
            }

            return w;
        }

        public WineryViewModel Consulta(int id)
        {
            var p = new SqlParameter[] { new SqlParameter("id", id) };
            DataTable tabela = HelperDAO.ExecutaProcSelect("sp_winery_select", p);

            if (tabela.Rows.Count == 0) return null;
            return MontaWinery(tabela.Rows[0]);
        }

        public List<WineryViewModel> Listagem(int userId)
        {
            List<WineryViewModel> lista = new List<WineryViewModel>();
            SqlParameter[] p = { new SqlParameter("user_id", userId) };

            DataTable tabela = HelperDAO.ExecutaProcSelect("sp_winery_select_by_user", p);

            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaWinery(registro));

            return lista;
        }

        public int ProximoId()
        {
            string sql = "select isnull(max(id) + 1, 1) as 'MAIOR' from winery";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            return Convert.ToInt32(tabela.Rows[0]["MAIOR"]);
        }
    }
}