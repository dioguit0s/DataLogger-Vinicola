using System.Data.SqlClient;
using System.Data;
using Vinicola_app.Models;

namespace Vinicola_app.DAO
{
    public class DataLoggerDAO
    {
        public void Inserir(DataLoggerViewModel dataLogger)
        {
            HelperDAO.ExecutaProc("sp_dataLogger_insert", CriaParametros(dataLogger));
        }

        public void Alterar(DataLoggerViewModel dataLogger)
        {
            HelperDAO.ExecutaProc("sp_dataLogger_update", CriaParametros(dataLogger));
        }

        private SqlParameter[] CriaParametros(DataLoggerViewModel dataLogger)
        {
            SqlParameter[] p = new SqlParameter[9];
            p[0] = new SqlParameter("id", dataLogger.Id);
            p[1] = new SqlParameter("wineryId", dataLogger.WineryId);
            p[2] = new SqlParameter("userId", dataLogger.UserId);
            p[3] = new SqlParameter("tempMin", dataLogger.TempMin);
            p[4] = new SqlParameter("tempMax", dataLogger.TempMax);
            p[5] = new SqlParameter("lumMin", dataLogger.LumMin);
            p[6] = new SqlParameter("lumMax", dataLogger.LumMax);
            p[7] = new SqlParameter("humidMin", dataLogger.HumidMin);
            p[8] = new SqlParameter("humidMax", dataLogger.HumidMax);

            return p;
        }

        //Add procedure de Exclusão
        public void Excluir(int id)
        {
            string sql = "delete from dataLogger where id = @id";
            SqlParameter[] p = { new SqlParameter("id", id) };
            HelperDAO.ExecutaSQL(sql, p);
        }

        private DataLoggerViewModel MontaDataLogger (DataRow registro)
        {
            DataLoggerViewModel d = new DataLoggerViewModel();
            d.Id = Convert.ToInt32(registro["id"]);
            d.WineryId = Convert.ToInt32(registro["wineryId"]);
            d.UserId = Convert.ToInt32(registro["userId"]);
            d.TempMin = Convert.ToInt32(registro["tempMin"]);
            d.TempMax = Convert.ToInt32(registro["tempMax"]);
            d.LumMin = Convert.ToInt32(registro["lumMin"]);
            d.LumMax = Convert.ToInt32(registro["lumMax"]);
            d.HumidMin = Convert.ToInt32(registro["humidMin"]);
            d.HumidMax = Convert.ToInt32(registro["humidMax"]);
            return d;
        }

        public DataLoggerViewModel Consulta(int id)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("id", id)
            };
            DataTable tabela = HelperDAO.ExecutaProcSelect("sp_dataLogger_select", p);
            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaDataLogger(tabela.Rows[0]);
        }

        //Add procedure de Listagem
        public List<DataLoggerViewModel> Listagem()
        {
            List<DataLoggerViewModel> lista = new List<DataLoggerViewModel>();
            string sql = "select * from dataLogger order by id";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaDataLogger(registro));
            return lista;
        }

        //Add procedure de proximoId
        public int ProximoId()
        {
            string sql = "select isnull(max(id) + 1, 1) as 'MAIOR' from dataLogger";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            return Convert.ToInt32(tabela.Rows[0]["MAIOR"]);
        }
    }
}
