using System;

namespace Diplom_Project.Models
{
    public class SeasonModel
    {
        public int SeasonId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}