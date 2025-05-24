using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using Diplom_Project.Models;
using Diplom_Project.Services;

namespace Diplom_Project.Views
{
    public partial class AdminPanel : Window
    {
        public AdminPanel()
        {
            InitializeComponent();
            LoadUsers();
            LoadEquipment();
            LoadAccommodations();
            LoadSellers();
        }

        // === Работа с пользователями ===
        private void LoadUsers()
        {
            var users = new List<UserModel>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT id, username, role FROM users";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new UserModel
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Role = reader.GetString(2)
                        });
                    }
                }
            }
            UsersDataGrid.ItemsSource = users;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            foreach (UserModel user in UsersDataGrid.Items)
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE users SET role = @role WHERE id = @id";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("role", user.Role);
                        cmd.Parameters.AddWithValue("id", user.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            MessageBox.Show("Роли успешно обновлены");
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        // === Работа с ценами на оборудование ===
        private void LoadEquipment()
        {
            var equipmentList = new List<EquipmentModel>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT equipment_id, type, brand, price FROM equipment";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        equipmentList.Add(new EquipmentModel
                        {
                            EquipmentId = reader.GetInt32(0),
                            Type = reader.GetString(1),
                            Brand = reader.GetString(2),
                            Price = reader.GetDecimal(3)
                        });
                    }
                }
            }
            EquipmentDataGrid.ItemsSource = equipmentList;
        }

        private void SaveEquipmentPrices_Click(object sender, RoutedEventArgs e)
        {
            foreach (EquipmentModel item in EquipmentDataGrid.Items)
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE equipment SET price = @price WHERE equipment_id = @id";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("price", item.Price);
                        cmd.Parameters.AddWithValue("id", item.EquipmentId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            MessageBox.Show("Цены на оборудование обновлены");
        }

        // === Работа с ценами на жильё ===
        private void LoadAccommodations()
        {
            var accommodations = new List<AccommodationModel>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT accommodation_id, name, address, price_per_night FROM accommodations";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        accommodations.Add(new AccommodationModel
                        {
                            AccommodationId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Address = reader.GetString(2),
                            PricePerNight = reader.GetDecimal(3)
                        });
                    }
                }
            }
            AccommodationsDataGrid.ItemsSource = accommodations;
        }

        private void SaveAccommodationPrices_Click(object sender, RoutedEventArgs e)
        {
            foreach (AccommodationModel item in AccommodationsDataGrid.Items)
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE accommodations SET price_per_night = @price WHERE accommodation_id = @id";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("price", item.PricePerNight);
                        cmd.Parameters.AddWithValue("id", item.AccommodationId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            MessageBox.Show("Цены на жильё обновлены");
        }

        // === Работа с продавцами ===
        private void LoadSellers()
        {
            var sellers = new List<SellerModel>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT id, username, created_at FROM users WHERE role = 'seller'";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sellers.Add(new SellerModel
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            CreatedAt = reader.GetDateTime(2)
                        });
                    }
                }
            }
            SellersDataGrid.ItemsSource = sellers;
        }

        private void RefreshSellers_Click(object sender, RoutedEventArgs e)
        {
            LoadSellers();
        }
    }

    // === Модели для отображения ===
    public class EquipmentModel
    {
        public int EquipmentId { get; set; }
        public string Type { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
    }

    public class AccommodationModel
    {
        public int AccommodationId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal PricePerNight { get; set; }
    }

    public class SellerModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}