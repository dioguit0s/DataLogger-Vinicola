namespace Vinicola_app.Models
{
    public class DataLoggerViewModel
    {
        public int Id { get; set; }

        public int WineryId { get; }
        public int UserId { get; }

        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public double LumMin { get; set; }
        public double LumMax { get; set; }
        public double HumidMin { get; set; }
        public double HumidMax { get; set; }
    }
}
