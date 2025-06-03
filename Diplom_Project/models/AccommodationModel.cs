public class AccommodationModel
{
    public int AccommodationId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public decimal PricePerNight { get; set; }
    public string ImagePath { get; set; } // Новое поле
}