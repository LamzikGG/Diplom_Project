using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Diplom_Project
{
    public partial class arenda_zhilya : Window
    {
        public class CartItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
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

            // Получаем информацию о товаре из карточки
            string name = ((TextBlock)parent.Children[1]).Text;
            decimal price = decimal.Parse(button.Tag.ToString());

            var item = new CartItem
            {
                Id = nextItemId++,
                Name = name,
                Price = price
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
            string orderDetails = "Ваша заявка:\n\n";
            foreach (var item in cartItems)
            {
                orderDetails += $"- {item.Name}: {item.Price} руб.\n";
            }
            orderDetails += $"\nИтого: {totalPrice} руб.";

            MessageBox.Show(orderDetails, "Подтверждение заявки",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            // Отправка данных продавцу
            SendOrderToSeller();

            // Очистка корзины после оформления
            cartItems.Clear();
            totalPrice = 0;
            nextItemId = 1;
            UpdateCartDisplay();

            StatusText.Text = "Ваша заявка отправлена продавцу!";
        }

        private void SendOrderToSeller()
        {
            try
            {
                // Здесь должна быть реальная логика отправки заявки
                // Например, сохранение в базу данных или отправка email

                // Пример логирования (можно заменить на реальную отправку)
                string logMessage = $"Новая заявка от {DateTime.Now}\n";
                logMessage += $"Количество позиций: {cartItems.Count}\n";
                logMessage += $"Общая сумма: {totalPrice} руб.\n";

                // В реальном приложении здесь будет код для сохранения или отправки данных
                System.Diagnostics.Debug.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке заявки: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
}