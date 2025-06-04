using Diplom_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Data.SQLite;

namespace Diplom_Project.Services
{
    public static class DatabaseHelper
    {
        // Получение всех отзывов для всех пользователей
        public static List<Review> GetAllReviews()
        {
            var reviews = new List<Review>();

            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"
                            SELECT r.review_id, r.user_id, r.content, r.rating, r.created_at, 
                                   u.first_name, u.last_name, u.role
                            FROM reviews r
                            JOIN users u ON r.user_id = u.id
                            ORDER BY r.created_at DESC";

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

        // Получение отзывов конкретного пользователя
        public static List<Review> GetUserReviews(int userId)
        {
            var reviews = new List<Review>();

            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand())
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

        // Получение пользователя по ID
        public static UserModel GetUserById(int userId)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand())
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
                                    Phone = reader.IsDBNull(5) ? null : reader.GetString(5)
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

        // Добавление нового отзыва
        public static bool AddReview(Review review)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"
                            INSERT INTO reviews (user_id, content, rating)
                            VALUES (@userId, @content, @rating)";
                        cmd.Parameters.AddWithValue("@userId", review.UserId);
                        cmd.Parameters.AddWithValue("@content", review.Content);
                        cmd.Parameters.AddWithValue("@rating", review.Rating);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
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

        // Удаление отзыва
        public static bool DeleteReview(int reviewId)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "DELETE FROM reviews WHERE review_id = @reviewId";
                        cmd.Parameters.AddWithValue("@reviewId", reviewId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении отзыва: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        // Получение средней оценки
        public static double GetAverageRating()
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT AVG(rating) FROM reviews";

                        var result = cmd.ExecuteScalar();
                        return result is DBNull ? 0 : Convert.ToDouble(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при расчете средней оценки: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }
            }
        }

        // Получение количества отзывов
        public static int GetReviewsCount()
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT COUNT(*) FROM reviews";

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении количества отзывов: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }
            }
        }
    }
}