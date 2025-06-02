using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using Diplom_Project.Models;
using Diplom_Project.Services;
using System.Diagnostics;

namespace Diplom_Project.Views
{
    public partial class AdminPanel : Window
    {
        public AdminPanel()
        {
            InitializeComponent();
            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadEquipment();
            LoadAccommodations();
            LoadReviews(); // Загрузка отзывов
        }

        // === Работа с оборудованием ===
        private void LoadEquipment()
        {
            try
            {
                var equipment = new ObservableCollection<EquipmentModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT equipment_id, type, brand, price FROM equipment";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            equipment.Add(new EquipmentModel
                            {
                                EquipmentId = reader.GetInt32(0),
                                Type = reader.GetString(1),
                                Brand = reader.GetString(2),
                                Price = reader.GetDecimal(3)
                            });
                        }
                    }
                }
                EquipmentDataGrid.ItemsSource = equipment;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оборудования: {ex.Message}");
            }
        }

        private void AddEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentDataGrid.ItemsSource is ObservableCollection<EquipmentModel> equipment)
            {
                var newItem = new EquipmentModel
                {
                    Type = "Новый тип",
                    Brand = "Новый бренд",
                    Price = 0
                };
                equipment.Add(newItem);
                EquipmentDataGrid.ScrollIntoView(newItem);
                EquipmentDataGrid.SelectedItem = newItem;
                EquipmentDataGrid.Focus();
            }
            else
            {
                var newEquipment = new ObservableCollection<EquipmentModel>
                {
                    new EquipmentModel
                    {
                        Type = "Новый тип",
                        Brand = "Новый бренд",
                        Price = 0
                    }
                };
                EquipmentDataGrid.ItemsSource = newEquipment;
            }
        }

        private void DeleteEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentDataGrid.SelectedItem is EquipmentModel selectedItem)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это оборудование? " +
                                           "Все связанные записи аренды также будут удалены.",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;
                try
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                const string deleteRentalsSql = "DELETE FROM rentals WHERE equipment_id = @equipmentId";
                                using (var cmd = new NpgsqlCommand(deleteRentalsSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("equipmentId", selectedItem.EquipmentId);
                                    int rentalsDeleted = cmd.ExecuteNonQuery();
                                    Debug.WriteLine($"Удалено записей аренды: {rentalsDeleted}");
                                }

                                const string deleteEquipmentSql = "DELETE FROM equipment WHERE equipment_id = @id";
                                using (var cmd = new NpgsqlCommand(deleteEquipmentSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("id", selectedItem.EquipmentId);
                                    cmd.ExecuteNonQuery();
                                }

                                transaction.Commit();

                                if (EquipmentDataGrid.ItemsSource is ObservableCollection<EquipmentModel> equipment)
                                {
                                    equipment.Remove(selectedItem);
                                }
                                MessageBox.Show("Оборудование и связанные записи аренды успешно удалены");
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите оборудование для удаления");
            }
        }

        private void SaveEquipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EquipmentDataGrid.ItemsSource is ObservableCollection<EquipmentModel> equipment)
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        foreach (var item in equipment)
                        {
                            if (item.EquipmentId > 0)
                            {
                                const string updateSql = "UPDATE equipment SET type = @type, brand = @brand, price = @price WHERE equipment_id = @id";
                                using (var cmd = new NpgsqlCommand(updateSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("type", item.Type);
                                    cmd.Parameters.AddWithValue("brand", item.Brand);
                                    cmd.Parameters.AddWithValue("price", item.Price);
                                    cmd.Parameters.AddWithValue("id", item.EquipmentId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                const string insertSql = "INSERT INTO equipment (type, brand, price) VALUES (@type, @brand, @price) RETURNING equipment_id";
                                using (var cmd = new NpgsqlCommand(insertSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("type", item.Type);
                                    cmd.Parameters.AddWithValue("brand", item.Brand);
                                    cmd.Parameters.AddWithValue("price", item.Price);
                                    item.EquipmentId = (int)cmd.ExecuteScalar();
                                }
                            }
                        }
                    }
                    MessageBox.Show("Оборудование сохранено успешно");
                    LoadEquipment(); // Обновляем данные после сохранения
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения оборудования: {ex.Message}");
            }
        }

        // === Работа с жильём ===
        private void LoadAccommodations()
        {
            try
            {
                var accommodations = new ObservableCollection<AccommodationModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT accommodation_id, name, address, price_per_night FROM accommodations";
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жилья: {ex.Message}");
            }
        }

        private void AddAccommodation_Click(object sender, RoutedEventArgs e)
        {
            if (AccommodationsDataGrid.ItemsSource is ObservableCollection<AccommodationModel> accommodations)
            {
                var newItem = new AccommodationModel
                {
                    Name = "Новое жильё",
                    Address = "Новый адрес",
                    PricePerNight = 0
                };
                accommodations.Add(newItem);
                AccommodationsDataGrid.ScrollIntoView(newItem);
                AccommodationsDataGrid.SelectedItem = newItem;
                AccommodationsDataGrid.Focus();
            }
            else
            {
                var newAccommodations = new ObservableCollection<AccommodationModel>
                {
                    new AccommodationModel
                    {
                        Name = "Новое жильё",
                        Address = "Новый адрес",
                        PricePerNight = 0
                    }
                };
                AccommodationsDataGrid.ItemsSource = newAccommodations;
            }
        }

        private void DeleteAccommodation_Click(object sender, RoutedEventArgs e)
        {
            if (AccommodationsDataGrid.SelectedItem is AccommodationModel selectedItem)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это жильё?",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;
                try
                {
                    if (selectedItem.AccommodationId > 0)
                    {
                        using (var conn = Database.GetConnection())
                        {
                            conn.Open();
                            const string sql = "DELETE FROM accommodations WHERE accommodation_id = @id";
                            using (var cmd = new NpgsqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("id", selectedItem.AccommodationId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    if (AccommodationsDataGrid.ItemsSource is ObservableCollection<AccommodationModel> accommodations)
                    {
                        accommodations.Remove(selectedItem);
                    }
                    MessageBox.Show("Жильё удалено успешно");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления жилья: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите жильё для удаления");
            }
        }

        private void SaveAccommodationPrices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccommodationsDataGrid.ItemsSource is ObservableCollection<AccommodationModel> accommodations)
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        foreach (var item in accommodations)
                        {
                            if (item.AccommodationId > 0)
                            {
                                const string updateSql = "UPDATE accommodations SET name = @name, address = @address, price_per_night = @price WHERE accommodation_id = @id";
                                using (var cmd = new NpgsqlCommand(updateSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("name", item.Name);
                                    cmd.Parameters.AddWithValue("address", item.Address);
                                    cmd.Parameters.AddWithValue("price", item.PricePerNight);
                                    cmd.Parameters.AddWithValue("id", item.AccommodationId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                const string insertSql = "INSERT INTO accommodations (name, address, price_per_night) VALUES (@name, @address, @price) RETURNING accommodation_id";
                                using (var cmd = new NpgsqlCommand(insertSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("name", item.Name);
                                    cmd.Parameters.AddWithValue("address", item.Address);
                                    cmd.Parameters.AddWithValue("price", item.PricePerNight);
                                    item.AccommodationId = (int)cmd.ExecuteScalar();
                                }
                            }
                        }
                    }
                    MessageBox.Show("Жильё сохранено успешно");
                    LoadAccommodations(); // Обновляем данные после сохранения
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения жилья: {ex.Message}");
            }
        }

        // === Работа с отзывами ===
        private void LoadReviews()
        {
            try
            {
                var reviews = new ObservableCollection<Review>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = @"
                        SELECT 
                            r.review_id, 
                            r.user_id, 
                            r.content, 
                            r.rating, 
                            r.created_at, 
                            u.first_name || ' ' || u.last_name AS full_name
                        FROM reviews r
                        JOIN users u ON r.user_id = u.id";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new UserModel
                            {
                                Id = reader.GetInt32(1),
                                FirstName = reader.GetString(5) // Используем поле full_name из запроса
                            };

                            reviews.Add(new Review
                            {
                                ReviewId = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                Content = reader.GetString(2),
                                Rating = reader.GetInt32(3),
                                CreatedAt = reader.GetDateTime(4),
                                User = user
                            });
                        }
                    }
                }
                ReviewsDataGrid.ItemsSource = reviews;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отзывов: {ex.Message}");
            }
        }

        private void RefreshReviews_Click(object sender, RoutedEventArgs e)
        {
            LoadReviews();
        }

        private void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            if (ReviewsDataGrid.SelectedItem is Review selectedReview)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот отзыв?",
                                            "Подтверждение удаления",
                                            MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        const string sql = "DELETE FROM reviews WHERE review_id = @id";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("id", selectedReview.ReviewId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (ReviewsDataGrid.ItemsSource is ObservableCollection<Review> reviews)
                    {
                        reviews.Remove(selectedReview);
                    }

                    MessageBox.Show("Отзыв успешно удален");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления отзыва: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите отзыв для удаления");
            }
        }

        // === Выход ===
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            authorisation mainwindow = new authorisation();
            mainwindow.Show();
            this.Close();
        }
    }
}