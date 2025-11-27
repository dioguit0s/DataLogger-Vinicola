using System.Data.SqlClient;
using System.Data;
using Vinicola_app.Models;
using System;
using System.Collections.Generic;

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
            SqlParameter[] p = new SqlParameter[10];
            p[0] = new SqlParameter("id", dataLogger.Id);
            p[1] = new SqlParameter("winery_id", dataLogger.WineryId);
            p[2] = new SqlParameter("user_id", dataLogger.UserId);
            p[3] = new SqlParameter("device_id", dataLogger.DeviceId ?? (object)DBNull.Value);
            p[4] = new SqlParameter("temp_min", dataLogger.TempMin);
            p[5] = new SqlParameter("temp_max", dataLogger.TempMax);
            p[6] = new SqlParameter("lum_min", dataLogger.LumMin);
            p[7] = new SqlParameter("lum_max", dataLogger.LumMax);
            p[8] = new SqlParameter("humid_min", dataLogger.HumidMin);
            p[9] = new SqlParameter("humid_max", dataLogger.HumidMax);
            return p;
        }

        public void Excluir(int id)
        {
            SqlParameter[] p = { new SqlParameter("id", id) };
            HelperDAO.ExecutaProc("sp_dataLogger_delete", p);
        }

        internal DataLoggerViewModel MontaDataLogger(DataRow registro)
        {
            DataLoggerViewModel d = new DataLoggerViewModel();
            d.Id = Convert.ToInt32(registro["id"]);
            d.WineryId = Convert.ToInt32(registro["winery_id"]);
            d.UserId = Convert.ToInt32(registro["user_id"]);

            if (registro.Table.Columns.Contains("device_id") && registro["device_id"] != DBNull.Value)
            {
                d.DeviceId = registro["device_id"].ToString();
            }

            d.TempMin = Convert.ToDouble(registro["temp_min"]);
            d.TempMax = Convert.ToDouble(registro["temp_max"]);
            d.LumMin = Convert.ToDouble(registro["lum_min"]);
            d.LumMax = Convert.ToDouble(registro["lum_max"]);
            d.HumidMin = Convert.ToDouble(registro["humid_min"]);
            d.HumidMax = Convert.ToDouble(registro["humid_max"]);

            return d;
        }

        public DataLoggerViewModel Consulta(int id)
        {
            var p = new SqlParameter[] { new SqlParameter("id", id) };
            DataTable tabela = HelperDAO.ExecutaProcSelect("sp_dataLogger_select", p);
            if (tabela.Rows.Count == 0) return null;
            return MontaDataLogger(tabela.Rows[0]);
        }

        public List<DataLoggerViewModel> Listagem(int userId)
        {
            List<DataLoggerViewModel> lista = new List<DataLoggerViewModel>();
            SqlParameter[] p = { new SqlParameter("user_id", userId) };

            DataTable tabela = HelperDAO.ExecutaProcSelect("sp_dataLogger_select_by_user", p);

            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaDataLogger(registro));
            return lista;
        }

        public int ProximoId()
        {
            string sql = "select isnull(max(id) + 1, 1) as 'MAIOR' from dataLogger";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            return Convert.ToInt32(tabela.Rows[0]["MAIOR"]);
        }

        public DataLoggerViewModel BuscarPorDeviceId(string deviceIdFIWARE)
        {
            // Ajuste essa query conforme está salvo na sua coluna device_id do SQL
            string sql = "SELECT * FROM dataLogger WHERE device_id = @devId";

            // Tenta buscar exato primeiro
            SqlParameter[] p = { new SqlParameter("devId", deviceIdFIWARE) };

            DataTable tabela = HelperDAO.ExecutaSelect(sql, p);

            if (tabela.Rows.Count == 0) return null;

            // Reutiliza seu método existente para montar o objeto
            // (Precisa mudar o método MontaDataLogger para 'public' ou copiar a lógica aqui)
            return MontaDataLogger(tabela.Rows[0]);
        }
    }
}