/* using System.Data.SqlClient;
using System.Data;
using Vinicola_app.Models;

namespace Vinicola_app.DAO
{
    public class WineryDAO
    {
        public void Inserir(WineryViewModel winery)
        {
            HelperDAO.ExecutaProc("sp_winery_insert", CriaParametros(winery));
        }

        public void Alterar(WineryViewModel winery)
        {
            HelperDAO.ExecutaProc("sp_winery_update", CriaParametros(winery));
        }

        private SqlParameter[] CriaParametros(WineryViewModel usuario)
        {
            SqlParameter[] p = new SqlParameter[8];
            p[0] = new SqlParameter("id", usuario.Id);
            p[1] = new SqlParameter("name", usuario.Name);
            p[2] = new SqlParameter("description", usuario.Description);
            p[3] = new SqlParameter("adress", usuario.Adress);
            p[4] = new SqlParameter("cnpj", usuario.Cnpj);
            p[5] = new SqlParameter("email", usuario.Email);
            p[6] = new SqlParameter("telephone", usuario.Telephone);
            p[7] = new SqlParameter("logoPic", usuario.LogoPic);
            return p;
        }

        //Add procedure de Exclusão
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
            w.Name = registro["nome"].ToString();
            w.Description = registro["description"].ToString();
            w.Adress = registro["adress"].ToString();
            w.Cnpj = registro["cnpj"].ToString();
            w.Email = registro["email"].ToString();
            w.Telephone = registro["telephone"].ToString();
            w.LogoPic = registro["logoPìc"].ToString();
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

        //Add procedure de Listagem
        public List<WineryViewModel> Listagem()
        {
            List<WineryViewModel> lista = new List<WineryViewModel>();
            string sql = "select * from winery order by nome";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaWinery(registro));
            return lista;
        }

        //Add procedure de proximoId
        public int ProximoId()
        {
            string sql = "select isnull(max(id) + 1, 1) as 'MAIOR' from winery";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);
            return Convert.ToInt32(tabela.Rows[0]["MAIOR"]);
        }
    }
}
*/