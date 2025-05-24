namespace Diplom_Project.Models
{
    public class BookingModel
    {
        public int UserId { get; set; }
        public int AccommodationId { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
    }
}