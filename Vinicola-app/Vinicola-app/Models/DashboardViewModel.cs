using System.Collections.Generic;

namespace Vinicola_app.Models
{
    public class DashboardViewModel
    {
        public List<WineryViewModel> Vinicolas { get; set; }
        public List<DataLoggerViewModel> Loggers { get; set; }

        public DashboardViewModel()
        {
            Vinicolas = new List<WineryViewModel>();
            Loggers = new List<DataLoggerViewModel>();
        }
    }
}
