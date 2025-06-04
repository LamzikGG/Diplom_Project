using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Diplom_Project.Models;
using Diplom_Project.Services;
using System.Data.SQLite;

namespace Diplom_Project
{
    public partial class arenda_zhilya : Window
    {
        private List<CartItem> cartItems = new List<CartItem>();
        private decimal totalPrice = 0;
        private int nextItemId = 1;
        private readonly UserModel _user;

        public arenda_zhilya(UserModel user)
        {
            InitializeComponent();
            _user = user;
            LoadAccommodationsFromDatabase();
        }

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var mainWindow = new MainWindow(_user);
            mainWindow.Show();
            this.Close();
        }

        private void LoadAccommodationsFromDatabase()
        {
            var wrapPanel = HousingItemsPanel;
            if (wrapPanel == null) return;

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT accommodation_id, name, address, price_per_night, image_path FROM accommodations";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string address = reader.GetString(2);
                            decimal price = reader.GetDecimal(3);
                            string imagePath = reader.IsDBNull(4) ? null : reader.GetString(4);

                            // Используем путь из базы, если он есть, иначе дефолтную картинку
                            var image = new Image
                            {
                                Source = new BitmapImage(new Uri(
                                    string.IsNullOrEmpty(imagePath) ? "Images/hotel.jpg" : imagePath,
                                    UriKind.RelativeOrAbsolute)),
                                Height = 150,
                                Stretch = Stretch.UniformToFill
                            };

                            var border = new Border { Style = Resources["HousingCardStyle"] as Style };
                            var stackPanel = new StackPanel();

                            stackPanel.Children.Add(image);

                            var nameText = new TextBlock
                            {
                                Text = name,
                                Style = Resources["CardTitleStyle"] as Style
                            };
                            stackPanel.Children.Add(nameText);

                            var addressText = new TextBlock
                            {
                                Text = address,
                                Style = Resources["CardDescriptionStyle"] as Style
                            };
                            stackPanel.Children.Add(addressText);

                            var priceText = new TextBlock
                            {
                                Text = $"{price} руб./ночь",
                                Style = Resources["CardPriceStyle"] as Style
                            };
                            stackPanel.Children.Add(priceText);

                            var rentButton = new Button
                            {
                                Content = "Добавить в корзину",
                                Style = Resources["RentButtonStyle"] as Style,
                                Tag = price.ToString(),
                                CommandParameter = id.ToString()
                            };
                            rentButton.Click += AddToCart_Click;
                            stackPanel.Children.Add(rentButton);

                            border.Child = stackPanel;
                            wrapPanel.Children.Add(border);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жилья: {ex.Message}");
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var parent = button.Parent as StackPanel;
            string name = ((TextBlock)parent.Children[1]).Text;
            decimal price = decimal.Parse(button.Tag.ToString());
            int accommodationId = int.Parse(button.CommandParameter?.ToString() ?? "0");

            var item = new CartItem
            {
                Id = nextItemId++,
                Name = name,
                Price = price,
                AccommodationId = accommodationId
            };

            cartItems.Add(item);
            totalPrice += price;
            UpdateCartDisplay();
            StatusText.Text = $"{item.Name} добавлен в корзину";
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int id = (int)button.Tag;
            var item = cartItems.Find(x => x.Id == id);
            if (item != null)
            {
                cartItems.Remove(item);
                totalPrice -= item.Price;
                UpdateCartDisplay();
                StatusText.Text = $"{item.Name} удален из корзины";
            }
        }

        private void UpdateCartDisplay()
        {
            CartListView.ItemsSource = null;
            CartListView.ItemsSource = cartItems;
            TotalPriceText.Text = $"{totalPrice} руб.";
            CheckoutButton.IsEnabled = cartItems.Count > 0;
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    foreach (var item in cartItems)
                    {
                        string sql = @"
                            INSERT INTO bookings (user_id, accommodation_id, check_in, check_out, total_price)
                            VALUES (@userId, @accommodationId, CURRENT_DATE, DATE('now', '+1 day'), @price)";
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", _user.Id);
                            cmd.Parameters.AddWithValue("@accommodationId", item.AccommodationId);
                            cmd.Parameters.AddWithValue("@price", item.Price);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Заявка успешно отправлена!", "Успех");
                cartItems.Clear();
                totalPrice = 0;
                nextItemId = 1;
                UpdateCartDisplay();
                StatusText.Text = "Ваша заявка отправлена продавцу!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении: {ex.Message}", "Ошибка");
            }
        }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int AccommodationId { get; set; }
    }
}