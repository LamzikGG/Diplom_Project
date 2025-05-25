using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diplom_Project.Services;
using Npgsql;

namespace Diplom_Project
{
    public partial class arenda_ekipa : Window
    {
        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private List<RentalItem> cartItems = new List<RentalItem>();
        private decimal totalPrice = 0;
        private int nextItemId = 1;

        public class RentalItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int EquipmentId { get; set; }
        }

        public arenda_ekipa()
        {
            InitializeComponent();
        }

        private void RentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var parent = button.Parent as StackPanel;

            string name = ((TextBlock)parent.Children[1]).Text;
            decimal price = decimal.Parse(button.Tag.ToString());
            int equipmentId = int.Parse(button.CommandParameter?.ToString() ?? "0");

            var item = new RentalItem
            {
                Id = nextItemId++,
                Name = name,
                Price = price,
                EquipmentId = equipmentId
            };

            cartItems.Add(item);
            totalPrice += item.Price;
            ShowConfirmationDialog(item);
        }

        private void ShowConfirmationDialog(RentalItem item)
        {
            if (MessageBox.Show($"Подтвердите аренду:\n- {item.Name}: {item.Price} руб.", "Подтверждение",
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
            return 1; // Заменить на реальный ID
        }
    }
}