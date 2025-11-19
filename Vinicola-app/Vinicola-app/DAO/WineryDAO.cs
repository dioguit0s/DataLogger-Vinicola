/* using System.Data.SqlClient;
using System.Data;
using Vinicola_app.Models;

namespace Vinicola_app.DAO
{
    public class WineryDAO
    {

        private SqlParameter[] CriaParametros(WineryViewModel usuario)
        {
            SqlParameter[] p = new SqlParameter[2];
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

        public void Inserir(WineryViewModel usuario)
        {
            string sql = "insert into usuarios(id, name, email, description, adress, cnpj, email, telephone, logoPic) values (@id, @name, @description, @password_hash, @profile_pic)";
            HelperDAO.ExecutaSQL(sql, CriaParametros(usuario));
        }

        public void Alterar(WineryViewModel usuario)
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

        private WineryViewModel MontaUsuario(DataRow registro)
        {
            WineryViewModel u = new WineryViewModel();
            u.Id = Convert.ToInt32(registro["id"]);
            u.Nome = registro["nome"].ToString();
            u.Email = registro["email"].ToString();
            u.SenhaHash = registro["senhaHash"].ToString();
            u.FotoProfile = registro["fotoProfile"].ToString();
            return u;
        }

        public WineryViewModel Consulta(int id)
        {
            string sql = "select * from usuarios where id = @id";
            SqlParameter[] p = { new SqlParameter("id", id) };
            DataTable tabela = HelperDAO.ExecutaSelect(sql, p);
            return tabela.Rows.Count == 0 ? null : MontaUsuario(tabela.Rows[0]);
        }

        public List<WineryViewModel> Listagem()
        {
            List<WineryViewModel> lista = new List<WineryViewModel>();
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
*/