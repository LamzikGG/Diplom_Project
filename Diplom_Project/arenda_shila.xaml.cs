using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diplom_Project.Services;
using Npgsql;

namespace Diplom_Project
{
    public partial class arenda_zhilya : Window
    {
        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        public class CartItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int AccommodationId { get; set; }
        }

        private List<CartItem> cartItems = new List<CartItem>();
        private decimal totalPrice = 0;
        private int nextItemId = 1;

        public arenda_zhilya()
        {
            InitializeComponent();
            UpdateCartDisplay();
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
            totalPrice += item.Price;
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
                            VALUES (@userId, @accommodationId, CURRENT_DATE, CURRENT_DATE + INTERVAL '1 day', @price)";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("userId", GetCurrentUserId());
                            cmd.Parameters.AddWithValue("accommodationId", item.AccommodationId);
                            cmd.Parameters.AddWithValue("price", item.Price);
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

        private int GetCurrentUserId()
        {
            return 1; // Заменить на реальный ID
        }
    }
}