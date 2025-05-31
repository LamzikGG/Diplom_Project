using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Diplom_Project.Services;
using Npgsql;

namespace Diplom_Project
{
    public partial class arenda_ekipa : Window
    {
        private ObservableCollection<RentalItem> CartItems { get; set; }

        public arenda_ekipa()
        {
            InitializeComponent();
            LoadEquipmentFromDatabase();
        }

        private void LoadEquipmentFromDatabase()
        {
            var wrapPanel = FindName("EquipmentWrapPanel") as WrapPanel;
            if (wrapPanel == null) return;

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT equipment_id, brand, type, price FROM equipment";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string brand = reader.GetString(1);
                            string type = reader.GetString(2);
                            decimal price = reader.GetDecimal(3);

                            // Создаем карточку
                            var border = new Border
                            {
                                Style = Resources["EquipmentCardStyle"] as Style
                            };
                            var stackPanel = new StackPanel();

                            // Картинка
                            var image = new Image
                            {
                                Source = new BitmapImage(new Uri("/Image/ski.png", UriKind.Relative)),
                                Height = 100,
                                Stretch = Stretch.Uniform
                            };
                            stackPanel.Children.Add(image);

                            // Бренд
                            var brandText = new TextBlock
                            {
                                Text = brand,
                                Style = Resources["EquipmentTextStyle"] as Style
                            };
                            stackPanel.Children.Add(brandText);

                            // Тип оборудования
                            var typeText = new TextBlock
                            {
                                Text = type,
                                Style = Resources["EquipmentTextStyle"] as Style
                            };
                            stackPanel.Children.Add(typeText);

                            // Цена
                            var priceText = new TextBlock
                            {
                                Text = $"{price} руб./день",
                                Style = Resources["EquipmentTextStyle"] as Style
                            };
                            stackPanel.Children.Add(priceText);

                            // Кнопка аренды
                            var rentButton = new Button
                            {
                                Content = "Арендовать",
                                Margin = new Thickness(0, 10, 0, 0),
                                Padding = new Thickness(5),
                                Background = Brushes.DarkGray,
                                Foreground = Brushes.White,
                                Tag = price.ToString(),
                                CommandParameter = id.ToString()
                            };
                            rentButton.Click += RentButton_Click;
                            stackPanel.Children.Add(rentButton);

                            border.Child = stackPanel;
                            wrapPanel.Children.Add(border);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки экипировки: {ex.Message}");
            }
        }

        private void RentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var parent = button.Parent as StackPanel;

            string name = ((TextBlock)parent.Children[2]).Text;
            decimal price = decimal.Parse(button.Tag.ToString());
            int equipmentId = int.Parse(button.CommandParameter?.ToString() ?? "0");

            var item = new RentalItem
            {
                Id = nextItemId++,
                Name = name,
                Price = price,
                EquipmentId = equipmentId
            };

            if (CartItems == null)
            {
                CartItems = new ObservableCollection<RentalItem>();
            }

            CartItems.Add(item);
            ShowConfirmationDialog(item);
        }

        private void ShowConfirmationDialog(RentalItem item)
        {
            if (MessageBox.Show($"Подтвердите аренду: {item.Name}, {item.Price} руб.", "Подтверждение",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SaveRentalToDatabase(item);
                MessageBox.Show("Аренда оформлена!", "Успех");
            }
        }

        private void SaveRentalToDatabase(RentalItem item)
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO rentals (user_id, equipment_id, start_time, end_time, total_price)
                        VALUES (@userId, @equipmentId, CURRENT_TIMESTAMP, NULL, @price)";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", GetCurrentUserId());
                        cmd.Parameters.AddWithValue("equipmentId", item.EquipmentId);
                        cmd.Parameters.AddWithValue("price", item.Price);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения аренды: {ex.Message}");
            }
        }

        private int GetCurrentUserId()
        {
            return 1; // Заменить на реальный ID пользователя
        }

        private int nextItemId = 1;
    }

    public class RentalItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int EquipmentId { get; set; }
    }
}