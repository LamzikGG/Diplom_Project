using System;
using Diplom_Project.Models;

namespace Diplom_Project.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }

        // Ссылка на пользователя
        public UserModel User { get; set; }
    }
}