namespace Diplom_Project.Models
{
    public class RentalModel
    {
        public int UserId { get; set; }
        public int EquipmentId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal TotalPrice { get; set; }
    }
}