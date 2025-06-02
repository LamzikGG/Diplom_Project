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
            LoadUsers();
            LoadEquipment();
            LoadAccommodations();
            LoadSellers();
        }

        // === Работа с пользователями ===
        private void LoadUsers()
        {
            try
            {
                var users = new ObservableCollection<UserModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT id, username, role FROM users";
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private void SaveUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsersDataGrid.ItemsSource is ObservableCollection<UserModel> users)
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        foreach (var user in users)
                        {
                            const string sql = "UPDATE users SET role = @role WHERE id = @id";
                            using (var cmd = new NpgsqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("role", user.Role);
                                cmd.Parameters.AddWithValue("id", user.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    MessageBox.Show("Изменения сохранены успешно");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void RefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
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

                // Прокрутка и фокус на новом элементе
                EquipmentDataGrid.ScrollIntoView(newItem);
                EquipmentDataGrid.SelectedItem = newItem;
                EquipmentDataGrid.Focus();
            }
            else
            {
                // Если коллекция не создана, создаем новую
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

                        // Начинаем транзакцию
                        using (var transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                // 1. Сначала удаляем все связанные записи аренды
                                const string deleteRentalsSql = "DELETE FROM rentals WHERE equipment_id = @equipmentId";
                                using (var cmd = new NpgsqlCommand(deleteRentalsSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("equipmentId", selectedItem.EquipmentId);
                                    int rentalsDeleted = cmd.ExecuteNonQuery();

                                    // Можно показать сколько записей аренды было удалено
                                    Debug.WriteLine($"Удалено записей аренды: {rentalsDeleted}");
                                }

                                // 2. Затем удаляем само оборудование
                                const string deleteEquipmentSql = "DELETE FROM equipment WHERE equipment_id = @equipmentId";
                                using (var cmd = new NpgsqlCommand(deleteEquipmentSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("equipmentId", selectedItem.EquipmentId);
                                    cmd.ExecuteNonQuery();
                                }

                                // Подтверждаем транзакцию
                                transaction.Commit();

                                // 3. Удаляем из коллекции
                                if (EquipmentDataGrid.ItemsSource is ObservableCollection<EquipmentModel> equipment)
                                {
                                    equipment.Remove(selectedItem);
                                }

                                MessageBox.Show("Оборудование и связанные записи аренды успешно удалены");
                            }
                            catch
                            {
                                // Откатываем транзакцию при ошибке
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}\n\n" +
                                  "Возможно, оборудование используется в активных арендах " +
                                  "или произошла другая ошибка базы данных.");
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
                                // Обновление существующей записи
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
                                // Добавление новой записи
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
                    // Удаляем только если элемент уже сохранен в БД
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

                    // Удаляем из коллекции
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
                                // Обновление существующей записи
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
                                // Добавление новой записи
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

        // === Работа с продавцами ===
        private void LoadSellers()
        {
            try
            {
                var sellers = new ObservableCollection<SellerModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT id, username, created_at FROM users WHERE role = 'seller'";
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продавцов: {ex.Message}");
            }
        }

        private void RefreshSellers_Click(object sender, RoutedEventArgs e)
        {
            LoadSellers();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            authorisation mainwindow = new authorisation();
            mainwindow.Show();
            this.Close();
        }
    }
}