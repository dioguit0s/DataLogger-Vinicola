using System.Data.SqlClient;

namespace Vinicola_app.DAO
{
    public static class ConexaoBD
    {
        public static SqlConnection GetConexao()
        {
            string strCon = "Data Source=LOCALHOST\\SQLEXPRESS;Initial Catalog=vinicola_db;Integrated Security=True;";
            SqlConnection conexao = new SqlConnection(strCon);
            conexao.Open();
            return conexao;
        }
    }

}