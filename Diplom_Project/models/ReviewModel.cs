using System;

namespace Diplom_Project.Models
{
    public class ReviewModel
    {
        public int ReviewId { get; set; }
        public string Username { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}