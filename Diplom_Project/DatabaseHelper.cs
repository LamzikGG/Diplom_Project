using Diplom_Project.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Diplom_Project.Services
{
    public static class DatabaseHelper
    {
        public static List<Review> GetUserReviews(int userId)
        {
            var reviews = new List<Review>();

            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"
                            SELECT r.review_id, r.user_id, r.content, r.rating, r.created_at, 
                                   u.first_name, u.last_name, u.role
                            FROM reviews r
                            JOIN users u ON r.user_id = u.id
                            WHERE r.user_id = @userId
                            ORDER BY r.created_at DESC";
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reviews.Add(new Review
                                {
                                    ReviewId = reader.GetInt32(0),
                                    UserId = reader.GetInt32(1),
                                    Content = reader.GetString(2),
                                    Rating = reader.GetInt32(3),
                                    CreatedAt = reader.GetDateTime(4),
                                    User = new UserModel
                                    {
                                        Id = reader.GetInt32(1),
                                        FirstName = reader.GetString(5),
                                        LastName = reader.GetString(6),
                                        Role = reader.GetString(7)
                                    }
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке отзывов: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return reviews;
        }

        public static UserModel GetUserById(int userId)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"
                            SELECT id, username, first_name, last_name, role, phone
                            FROM users
                            WHERE id = @userId";
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new UserModel
                                {
                                    Id = reader.GetInt32(0),
                                    Username = reader.GetString(1),
                                    FirstName = reader.GetString(2),
                                    LastName = reader.GetString(3),
                                    Role = reader.GetString(4),
                                    Phone = reader.GetString(5)
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке пользователя: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }

        public static bool AddReview(Review review)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"
                            INSERT INTO reviews (user_id, content, rating)
                            VALUES (@userId, @content, @rating)
                            RETURNING review_id";
                        cmd.Parameters.AddWithValue("@userId", review.UserId);
                        cmd.Parameters.AddWithValue("@content", review.Content);
                        cmd.Parameters.AddWithValue("@rating", review.Rating);

                        var newId = (int)cmd.ExecuteScalar();
                        return newId > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении отзыва: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }
    }
}