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
            // Para inserir, não passamos o ID (pois é Identity/Auto-incremento)
            // Mas DEVEMOS passar o user_id
            HelperDAO.ExecutaProc("sp_winery_insert", CriaParametros(winery, false));
        }

        public void Alterar(WineryViewModel winery)
        {
            // Para alterar, precisamos do ID para o WHERE
            HelperDAO.ExecutaProc("sp_winery_update", CriaParametros(winery, true));
        }

        // Adicionei um booleano 'incluirId' para diferenciar Insert de Update
        private SqlParameter[] CriaParametros(WineryViewModel model, bool incluirId)
        {
            // Se for Update (incluirId=true), temos 9 parametros. Se for Insert, 8.
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                // ATENÇÃO: O nome do parâmetro aqui deve ser igual ao da Procedure (@user_id, @name, etc)
                new SqlParameter("user_id", model.UserId),
                new SqlParameter("name", model.Name ?? (object)DBNull.Value),
                new SqlParameter("description", model.Description ?? (object)DBNull.Value),
                new SqlParameter("address", model.Address ?? (object)DBNull.Value),
                new SqlParameter("cnpj", model.Cnpj ?? (object)DBNull.Value),
                new SqlParameter("email", model.Email ?? (object)DBNull.Value),
                new SqlParameter("telephone", model.Telephone ?? (object)DBNull.Value),
                new SqlParameter("logo_pic", model.LogoPic ?? (object)DBNull.Value)
            };

            // Só adiciona o ID se for operação de Update
            if (incluirId)
            {
                parametros.Add(new SqlParameter("id", model.Id));
            }

            return parametros.ToArray();
        }

        public void Excluir(int id)
        {
            string sql = "delete from winery where id = @id";
            SqlParameter[] p = { new SqlParameter("id", id) };
            HelperDAO.ExecutaSQL(sql, p);
        }

        private WineryViewModel MontaWinery(DataRow registro)
        {
            WineryViewModel w = new WineryViewModel();
            w.Id = Convert.ToInt32(registro["id"]);

            // CORREÇÃO: No banco a coluna chama "user_id", verifique se sua procedure retorna com esse nome ou como "userId"
            // Se der erro aqui, mude para "userId" caso sua procedure faça um alias
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
            w.LogoPic = registro["logo_pic"] != DBNull.Value ? registro["logo_pic"].ToString() : "";

            return w;
        }

        public WineryViewModel Consulta(int id)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("id", id)
            };

            DataTable tabela = HelperDAO.ExecutaProcSelect("sp_winery_select", p);

            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaWinery(tabela.Rows[0]);
        }

        public List<WineryViewModel> Listagem(int userId)
        {
            List<WineryViewModel> lista = new List<WineryViewModel>();
            // Usando SQL direto para garantir compatibilidade se a procedure não existir
            string sql = "select * from winery where user_id = @id order by name";
            SqlParameter[] p = { new SqlParameter("id", userId) };

            DataTable tabela = HelperDAO.ExecutaSelect(sql, p);

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