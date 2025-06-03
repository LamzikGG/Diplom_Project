namespace Diplom_Project.Models
{
    public class SlopePriceModel
    {
        public int PriceId { get; set; }
        public int SlopeId { get; set; }
        public int SeasonId { get; set; }
        public decimal Price { get; set; }
        public string SlopeName { get; set; }
        public string SeasonName { get; set; }
    }
}