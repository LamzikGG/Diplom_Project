using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Diplom_Project.Models;
using Diplom_Project.Services;
using Npgsql;

namespace Diplom_Project
{
    public partial class arenda_ekipa : Window, INotifyPropertyChanged
    {
        private ObservableCollection<RentalItem> _cartItems = new ObservableCollection<RentalItem>();
        private decimal _totalPrice;
        private string _statusMessage;
        private readonly UserModel _user;

        public ObservableCollection<RentalItem> CartItems
        {
            get => _cartItems;
            set
            {
                _cartItems = value;
                OnPropertyChanged(nameof(CartItems));
                UpdateTotalPrice();
            }
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public arenda_ekipa(UserModel user)
        {
            InitializeComponent();
            DataContext = this;
            _user = user;
            LoadEquipmentFromDatabase();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                            var border = new Border { Style = Resources["EquipmentCardStyle"] as Style };
                            var stackPanel = new StackPanel();

                            var image = new Image
                            {
                                Source = new BitmapImage(new Uri("/Image/ski.png", UriKind.Relative)),
                                Style = Resources["EquipmentImageStyle"] as Style
                            };
                            stackPanel.Children.Add(image);

                            var brandText = new TextBlock
                            {
                                Text = brand,
                                Style = Resources["EquipmentTitleStyle"] as Style
                            };
                            stackPanel.Children.Add(brandText);

                            var typeText = new TextBlock
                            {
                                Text = type,
                                Style = Resources["EquipmentTextStyle"] as Style
                            };
                            stackPanel.Children.Add(typeText);

                            var priceText = new TextBlock
                            {
                                Text = $"{price} руб./день",
                                Style = Resources["EquipmentPriceStyle"] as Style
                            };
                            stackPanel.Children.Add(priceText);

                            var rentButton = new Button
                            {
                                Content = "Арендовать",
                                Style = Resources["RentButtonStyle"] as Style,
                                Tag = price,
                                CommandParameter = id
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
                StatusMessage = $"Ошибка загрузки экипировки: {ex.Message}";
            }
        }

        private void RentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var parent = button.Parent as StackPanel;
            string name = ((TextBlock)parent.Children[1]).Text + " " + ((TextBlock)parent.Children[2]).Text;
            decimal price = (decimal)button.Tag;
            int equipmentId = (int)button.CommandParameter;

            var item = new RentalItem
            {
                Id = CartItems.Any() ? CartItems.Max(x => x.Id) + 1 : 1,
                Name = name,
                Price = price,
                EquipmentId = equipmentId
            };

            CartItems.Add(item);
            StatusMessage = $"Добавлено: {item.Name}";
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int id = (int)button.Tag;
            var itemToRemove = CartItems.FirstOrDefault(x => x.Id == id);
            if (itemToRemove != null)
            {
                CartItems.Remove(itemToRemove);
                StatusMessage = $"Удалено: {itemToRemove.Name}";
            }
        }

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var mainWindow = new MainWindow(_user);
            mainWindow.Show();
            this.Close();
        }

        private void UpdateTotalPrice()
        {
            TotalPrice = CartItems.Sum(x => x.Price);
        }

        private void ConfirmRentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CartItems.Any())
            {
                StatusMessage = "Корзина пуста!";
                return;
            }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    foreach (var item in CartItems)
                    {
                        string sql = @"
                            INSERT INTO rentals (user_id, equipment_id, start_time, end_time, total_price)
                            VALUES (@userId, @equipmentId, CURRENT_TIMESTAMP, NULL, @price)";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("userId", _user.Id);
                            cmd.Parameters.AddWithValue("equipmentId", item.EquipmentId);
                            cmd.Parameters.AddWithValue("price", item.Price);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                CartItems.Clear();
                StatusMessage = "Аренда успешно оформлена!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка оформления аренды: {ex.Message}";
            }
        }
    }

    public class RentalItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int EquipmentId { get; set; }
    }
}