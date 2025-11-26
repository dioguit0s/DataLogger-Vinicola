namespace Vinicola_app.Models
{
    public class DataLoggerViewModel
    {
        public int Id { get; set; }
        //Adicionei o set, dps eu explico
        public int WineryId { get; set; }
        //Adicionei o set 
        public int UserId { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public double LumMin { get; set; }
        public double LumMax { get; set; }
        public double HumidMin { get; set; }
        public double HumidMax { get; set; }
    }
}
