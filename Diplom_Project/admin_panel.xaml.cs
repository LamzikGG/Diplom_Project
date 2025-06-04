using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Diplom_Project.Models;
using Diplom_Project.Services;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Data.SQLite;

namespace Diplom_Project.Views
{
    public partial class AdminPanel : Window
    {
        public AdminPanel()
        {
            InitializeComponent();
            LoadAllData();
            LoadSeasons();
            LoadSlopes();
            LoadSlopePrices();
        }

        private void LoadAllData()
        {
            LoadEquipment();
            LoadAccommodations();
            LoadReviews();
        }

        // === Работа с оборудованием ===
        private void LoadEquipment()
        {
            try
            {
                var equipmentList = new ObservableCollection<EquipmentModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT equipment_id, type, brand, price, image_path FROM equipment";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            equipmentList.Add(new EquipmentModel
                            {
                                EquipmentId = reader.GetInt32(0),
                                Type = reader.GetString(1),
                                Brand = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                ImagePath = reader.IsDBNull(4) ? null : reader.GetString(4)
                            });
                        }
                    }
                }
                EquipmentDataGrid.ItemsSource = equipmentList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оборудования: {ex.Message}");
            }
        }
        private void SelectAccommodationImage_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var accommodation = button?.Tag as AccommodationModel;
            if (accommodation == null) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };
            if (dialog.ShowDialog() == true)
            {
                string imagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!System.IO.Directory.Exists(imagesDir))
                    System.IO.Directory.CreateDirectory(imagesDir);

                string fileName = System.IO.Path.GetFileName(dialog.FileName);
                string destPath = System.IO.Path.Combine(imagesDir, fileName);

                if (!System.IO.File.Exists(destPath))
                    System.IO.File.Copy(dialog.FileName, destPath);

                accommodation.ImagePath = $"Images/{fileName}";
            }
        }
        private void SelectEquipmentImage_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var equipment = button?.Tag as EquipmentModel;
            if (equipment == null)
            {
                MessageBox.Show("Не удалось определить объект оборудования.");
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };
            if (dialog.ShowDialog() == true)
            {
                string imagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!System.IO.Directory.Exists(imagesDir))
                    System.IO.Directory.CreateDirectory(imagesDir);

                string fileName = System.IO.Path.GetFileName(dialog.FileName);
                string destPath = System.IO.Path.Combine(imagesDir, fileName);

                if (!System.IO.File.Exists(destPath))
                    System.IO.File.Copy(dialog.FileName, destPath);

                equipment.ImagePath = $"Images/{fileName}";
                EquipmentDataGrid.Items.Refresh();
            }
        }
        private void SaveEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentDataGrid.ItemsSource is ObservableCollection<EquipmentModel> equipmentList)
            {
                try
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        foreach (var item in equipmentList)
                        {
                            if (item.EquipmentId > 0)
                            {
                                // Обновление существующей записи
                                const string sql = "UPDATE equipment SET type = @type, brand = @brand, price = @price, image_path = @image WHERE equipment_id = @id";
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@type", item.Type);
                                    cmd.Parameters.AddWithValue("@brand", item.Brand);
                                    cmd.Parameters.AddWithValue("@price", item.Price);
                                    cmd.Parameters.AddWithValue("@image", (object)item.ImagePath ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@id", item.EquipmentId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Добавление новой записи
                                const string sql = "INSERT INTO equipment (type, brand, price, image_path) VALUES (@type, @brand, @price, @image)";
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@type", item.Type);
                                    cmd.Parameters.AddWithValue("@brand", item.Brand);
                                    cmd.Parameters.AddWithValue("@price", item.Price);
                                    cmd.Parameters.AddWithValue("@image", (object)item.ImagePath ?? DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                    item.EquipmentId = (int)conn.LastInsertRowId;
                                }
                            }
                        }
                    }
                    MessageBox.Show("Оборудование сохранено успешно");
                    LoadEquipment(); // Обновить таблицу после сохранения
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения оборудования: {ex.Message}");
                }
            }
        }

        private void AddEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentDataGrid.ItemsSource is ObservableCollection<EquipmentModel> equipmentList)
            {
                var newItem = new EquipmentModel
                {
                    Type = "Лыжи",
                    Brand = "Неизвестно",
                    Price = 500
                };
                equipmentList.Add(newItem);
                EquipmentDataGrid.ScrollIntoView(newItem);
                EquipmentDataGrid.SelectedItem = newItem;
            }
        }

        private void DeleteEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentDataGrid.SelectedItem is EquipmentModel selected)
            {
                var result = MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    if (selected.EquipmentId > 0)
                    {
                        try
                        {
                            using (var conn = Database.GetConnection())
                            {
                                conn.Open();
                                const string sql = "DELETE FROM equipment WHERE equipment_id = @id";
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@id", selected.EquipmentId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления: {ex.Message}");
                        }
                    }
                    (EquipmentDataGrid.ItemsSource as ObservableCollection<EquipmentModel>)?.Remove(selected);
                }
            }
        }

        // === Работа с жильём ===
        private void LoadAccommodations()
        {
            try
            {
                var list = new ObservableCollection<AccommodationModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT accommodation_id, name, address, price_per_night, image_path FROM accommodations";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new AccommodationModel
                            {
                                AccommodationId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Address = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                PricePerNight = reader.GetDecimal(3),
                                ImagePath = reader.IsDBNull(4) ? null : reader.GetString(4)
                            });
                        }
                    }
                }
                AccommodationsDataGrid.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жилья: {ex.Message}");
            }
        }
        private void SelectSlopeImage_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var slope = button?.Tag as SlopeModel;
            if (slope == null)
            {
                MessageBox.Show("Не удалось определить объект трассы.");
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };
            if (dialog.ShowDialog() == true)
            {
                string imagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!System.IO.Directory.Exists(imagesDir))
                    System.IO.Directory.CreateDirectory(imagesDir);

                string fileName = System.IO.Path.GetFileName(dialog.FileName);
                string destPath = System.IO.Path.Combine(imagesDir, fileName);

                if (!System.IO.File.Exists(destPath))
                    System.IO.File.Copy(dialog.FileName, destPath);

                slope.ImagePath = $"Images/{fileName}";
                SlopesDataGrid.Items.Refresh();
            }
        }
        private void SaveAccommodationPrices_Click(object sender, RoutedEventArgs e)
        {
            if (AccommodationsDataGrid.ItemsSource is ObservableCollection<AccommodationModel> list)
            {
                try
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        foreach (var item in list)
                        {
                            if (item.AccommodationId > 0)
                            {
                                const string sql = "UPDATE accommodations SET name = @name, address = @addr, price_per_night = @price, image_path = @image WHERE accommodation_id = @id";
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@name", item.Name);
                                    cmd.Parameters.AddWithValue("@addr", item.Address);
                                    cmd.Parameters.AddWithValue("@price", item.PricePerNight);
                                    cmd.Parameters.AddWithValue("@id", item.AccommodationId);
                                    cmd.Parameters.AddWithValue("@image", (object)item.ImagePath ?? DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                const string sql = "INSERT INTO accommodations (name, address, price_per_night, image_path) VALUES (@name, @addr, @price, @image)";
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@name", item.Name);
                                    cmd.Parameters.AddWithValue("@addr", item.Address);
                                    cmd.Parameters.AddWithValue("@price", item.PricePerNight);
                                    cmd.Parameters.AddWithValue("@image", (object)item.ImagePath ?? DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                    item.AccommodationId = (int)conn.LastInsertRowId;
                                }
                            }
                        }
                    }
                    MessageBox.Show("Жильё сохранено успешно");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения жилья: {ex.Message}");
                }
            }
        }

        private void AddAccommodation_Click(object sender, RoutedEventArgs e)
        {
            if (AccommodationsDataGrid.ItemsSource is ObservableCollection<AccommodationModel> list)
            {
                var newItem = new AccommodationModel
                {
                    Name = "Новое жильё",
                    Address = "ул. Пушкина, д. Колотушкина",
                    PricePerNight = 2000
                };
                list.Add(newItem);
                AccommodationsDataGrid.ScrollIntoView(newItem);
                AccommodationsDataGrid.SelectedItem = newItem;
            }
        }

        private void DeleteAccommodation_Click(object sender, RoutedEventArgs e)
        {
            if (AccommodationsDataGrid.SelectedItem is AccommodationModel selected)
            {
                var result = MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    if (selected.AccommodationId > 0)
                    {
                        try
                        {
                            using (var conn = Database.GetConnection())
                            {
                                conn.Open();
                                const string sql = "DELETE FROM accommodations WHERE accommodation_id = @id";
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@id", selected.AccommodationId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления: {ex.Message}");
                        }
                    }
                    (AccommodationsDataGrid.ItemsSource as ObservableCollection<AccommodationModel>)?.Remove(selected);
                }
            }
        }

        // === Работа с отзывами ===
        private void LoadReviews()
        {
            try
            {
                var reviews = new ObservableCollection<ReviewModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT r.review_id, u.username, r.rating, r.content, r.created_at FROM reviews r JOIN users u ON r.user_id = u.id";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reviews.Add(new ReviewModel
                            {
                                ReviewId = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Rating = reader.GetInt32(2),
                                Content = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                CreatedAt = reader.GetDateTime(4)
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
            if (ReviewsDataGrid.SelectedItem is ReviewModel selected)
            {
                var result = MessageBox.Show("Удалить отзыв?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = Database.GetConnection())
                        {
                            conn.Open();
                            const string sql = "DELETE FROM reviews WHERE review_id = @id";
                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", selected.ReviewId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        if (ReviewsDataGrid.ItemsSource is ObservableCollection<ReviewModel> reviews)
                        {
                            reviews.Remove(selected);
                        }
                        MessageBox.Show("Отзыв удален");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите отзыв для удаления");
            }
        }

        // === Работа с трассами ===
        private void LoadSlopes()
        {
            try
            {
                var slopes = new ObservableCollection<SlopeModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT slope_id, name, difficulty, status, description, image_path FROM slopes";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            slopes.Add(new SlopeModel
                            {
                                SlopeId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Difficulty = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Status = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                                ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5)
                            });
                        }
                    }
                }
                SlopesDataGrid.ItemsSource = slopes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки трасс: {ex.Message}");
            }
        }

        private void AddSlope_Click(object sender, RoutedEventArgs e)
        {
            if (SlopesDataGrid.ItemsSource is ObservableCollection<SlopeModel> slopes)
            {
                var newItem = new SlopeModel
                {
                    Name = "Новая трасса",
                    Difficulty = "Средняя",
                    Status = "Открыта",
                    Description = "",
                    ImagePath = null
                };
                slopes.Add(newItem);
                SlopesDataGrid.ScrollIntoView(newItem);
                SlopesDataGrid.SelectedItem = newItem;
            }
        }

        private void DeleteSlope_Click(object sender, RoutedEventArgs e)
        {
            if (SlopesDataGrid.SelectedItem is SlopeModel selected)
            {
                var result = MessageBox.Show("Удалить трассу?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    if (selected.SlopeId > 0)
                    {
                        try
                        {
                            using (var conn = Database.GetConnection())
                            {
                                conn.Open();
                                const string sql = "DELETE FROM slopes WHERE slope_id = @id";
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@id", selected.SlopeId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления: {ex.Message}");
                        }
                    }
                    (SlopesDataGrid.ItemsSource as ObservableCollection<SlopeModel>)?.Remove(selected);
                }
            }
        }

        private void SaveSlopes_Click(object sender, RoutedEventArgs e)
        {
            if (SlopesDataGrid.ItemsSource is ObservableCollection<SlopeModel> slopes)
            {
                // Проверка сложности
                var allowed = new[] { "easy", "medium", "hard" };
                foreach (var item in slopes)
                {
                    if (string.IsNullOrWhiteSpace(item.Difficulty) || !allowed.Contains(item.Difficulty.Trim().ToLower()))
                    {
                        MessageBox.Show("Сложность может быть только: easy, medium, hard", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                try
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        foreach (var item in slopes)
                        {
                            if (item.SlopeId > 0)
                            {
                                const string updateSql = "UPDATE slopes SET name = @name, difficulty = @difficulty, status = @status, description = @description, image_path = @image WHERE slope_id = @id";
                                using (var cmd = new SQLiteCommand(updateSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@name", item.Name);
                                    cmd.Parameters.AddWithValue("@difficulty", (object)item.Difficulty ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@status", (object)item.Status ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@description", (object)item.Description ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@image", (object)item.ImagePath ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@id", item.SlopeId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                const string insertSql = "INSERT INTO slopes (name, difficulty, status, description, image_path) VALUES (@name, @difficulty, @status, @description, @image)";
                                using (var cmd = new SQLiteCommand(insertSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@name", item.Name);
                                    cmd.Parameters.AddWithValue("@difficulty", (object)item.Difficulty ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@status", (object)item.Status ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@description", (object)item.Description ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@image", (object)item.ImagePath ?? DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                    item.SlopeId = (int)conn.LastInsertRowId;
                                }
                            }
                        }
                    }
                    MessageBox.Show("Трассы успешно сохранены");
                    LoadSlopes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения трасс: {ex.Message}");
                }
            }
        }

        // === Работа с сезонами ===
        private void LoadSeasons()
        {
            try
            {
                var seasons = new ObservableCollection<SeasonModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT season_id, name, start_date, end_date FROM seasons";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seasons.Add(new SeasonModel
                            {
                                SeasonId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                StartDate = reader.GetDateTime(2),
                                EndDate = reader.GetDateTime(3)
                            });
                        }
                    }
                }
                SeasonsDataGrid.ItemsSource = seasons;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сезонов: {ex.Message}");
            }
        }

        private void AddSeason_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void DeleteSeason_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void SaveSeasons_Click(object sender, RoutedEventArgs e) { /* ... */ }

        // === Работа с ценами на трассы ===
        private void LoadSlopePrices()
        {
            try
            {
                var slopePrices = new ObservableCollection<SlopePriceModel>();
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = @"
                        SELECT sp.price_id, sp.slope_id, sp.season_id, sp.price,
                               s.name AS slope_name, se.name AS season_name
                        FROM slope_prices sp
                        JOIN slopes s ON sp.slope_id = s.slope_id
                        JOIN seasons se ON sp.season_id = se.season_id";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            slopePrices.Add(new SlopePriceModel
                            {
                                PriceId = reader.GetInt32(0),
                                SlopeId = reader.GetInt32(1),
                                SeasonId = reader.GetInt32(2),
                                Price = reader.GetDecimal(3),
                                SlopeName = reader.GetString(4),
                                SeasonName = reader.GetString(5)
                            });
                        }
                    }
                }

                this.Resources["Slopes"] = GetSlopes();
                this.Resources["Seasons"] = GetSeasons();

                SlopePricesDataGrid.ItemsSource = slopePrices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки цен на трассы: {ex.Message}");
            }
        }

        private List<SlopeModel> GetSlopes()
        {
            var list = new List<SlopeModel>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                const string sql = "SELECT slope_id, name FROM slopes";
                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SlopeModel
                        {
                            SlopeId = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        private List<SeasonModel> GetSeasons()
        {
            var list = new List<SeasonModel>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                const string sql = "SELECT season_id, name FROM seasons";
                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SeasonModel
                        {
                            SeasonId = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        private void SaveSlopePrices_Click(object sender, RoutedEventArgs e)
        {
            if (SlopePricesDataGrid.ItemsSource is ObservableCollection<SlopePriceModel> prices)
            {
                try
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        foreach (var item in prices)
                        {
                            if (item.PriceId > 0)
                            {
                                const string updateSql = "UPDATE slope_prices SET slope_id = @slope, season_id = @season, price = @price WHERE price_id = @id";
                                using (var cmd = new SQLiteCommand(updateSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@slope", item.SlopeId);
                                    cmd.Parameters.AddWithValue("@season", item.SeasonId);
                                    cmd.Parameters.AddWithValue("@price", item.Price);
                                    cmd.Parameters.AddWithValue("@id", item.PriceId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                const string insertSql = "INSERT INTO slope_prices (slope_id, season_id, price) VALUES (@slope, @season, @price)";
                                using (var cmd = new SQLiteCommand(insertSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@slope", item.SlopeId);
                                    cmd.Parameters.AddWithValue("@season", item.SeasonId);
                                    cmd.Parameters.AddWithValue("@price", item.Price);
                                    cmd.ExecuteNonQuery();
                                    item.PriceId = (int)conn.LastInsertRowId;
                                }
                            }
                        }
                    }
                    MessageBox.Show("Цены на трассы сохранены успешно");
                    LoadSlopePrices(); // Перезагрузка данных
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения цен на трассы: {ex.Message}");
                }
            }
        }

        private void AddSlopePrice_Click(object sender, RoutedEventArgs e)
        {
            if (SlopePricesDataGrid.ItemsSource is ObservableCollection<SlopePriceModel> prices)
            {
                var newItem = new SlopePriceModel
                {
                    SlopeId = 1,
                    SeasonId = 1,
                    Price = 0
                };
                prices.Add(newItem);
                SlopePricesDataGrid.ScrollIntoView(newItem);
                SlopePricesDataGrid.SelectedItem = newItem;
                SlopePricesDataGrid.Focus();
            }
        }

        private void DeleteSlopePrice_Click(object sender, RoutedEventArgs e)
        {
            if (SlopePricesDataGrid.SelectedItem is SlopePriceModel selectedItem)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту цену?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (selectedItem.PriceId > 0)
                        {
                            using (var conn = Database.GetConnection())
                            {
                                conn.Open();
                                const string sql = "DELETE FROM slope_prices WHERE price_id = @id";
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@id", selectedItem.PriceId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        if (SlopePricesDataGrid.ItemsSource is ObservableCollection<SlopePriceModel> priceList)
                        {
                            priceList.Remove(selectedItem);
                        }
                        MessageBox.Show("Цена удалена успешно");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления цены: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.");
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new authorisation();
            authWindow.Show();
            this.Close();
        }
    }
}