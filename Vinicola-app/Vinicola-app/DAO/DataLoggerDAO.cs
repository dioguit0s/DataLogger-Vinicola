using Vinicola_app.DAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Vinicola_app.Models;

namespace Vinicola_app.DAO
{
    public class DataLoggerDAO
    {
        private SqlParameter[] NewParameters(DataLoggerViewModel dataLogger)
        {
            SqlParameter[] p = new SqlParameter[8];
            p[0] = new SqlParameter("id", dataLogger.Id);
            p[1] = new SqlParameter("wineryId", dataLogger.WineryId);
            p[2] = new SqlParameter("userId", dataLogger.UserId);
            p[3] = new SqlParameter("tempMin", dataLogger.TempMin);
            p[4] = new SqlParameter("tempMax", dataLogger.TempMax);
            p[5] = new SqlParameter("lumMin", dataLogger.LumMin);
            p[6] = new SqlParameter("lumMax", dataLogger.LumMax);
            p[7] = new SqlParameter("humidMin", dataLogger.HumidMin);
            p[8] = new SqlParameter("humidMax", dataLogger.HumidMax);

        }

        public void Insert(DataLoggerViewModel dataLogger)
        {
            string sql = "insert into dataLogger(id, wineryId, userId, tempMin, tempMax, lumMin, lumMax, humidMin, humidMax) values (@id, @winery_id, @user_id, @temp_min, @temp_max, @lum_min, @lum_max, @humid_min, @humid_max)";
            HelperDAO.ExecutaSQL(sql, NewParameters(dataLogger));
        }


    }
}
