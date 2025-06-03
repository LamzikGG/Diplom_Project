using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Diplom_Project.Models;
using Diplom_Project.Services;
using Npgsql;

namespace Diplom_Project
{
    public partial class routes : Window
    {
        private UserModel _user;

        // Конструктор с пользователем
        public routes(UserModel user)
        {
            InitializeComponent();
            _user = user;
            LoadSlopes();
        }

        // Конструктор по умолчанию
        public routes()
        {
            InitializeComponent();
            LoadSlopes();
        }

        private void LoadSlopes()
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT name, description, lift_price FROM slopes WHERE status = 'open'";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GenerateSlopeBlock(reader.GetString(0), reader.GetString(1), reader.GetDecimal(2));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки трасс: {ex.Message}");
            }
        }

        private void GenerateSlopeBlock(string name, string description, decimal liftPrice)
        {
            var border = new Border
            {
                Background = System.Windows.Media.Brushes.DarkGray,
                CornerRadius = new System.Windows.CornerRadius(10),
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(15)
            };

            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var image = new Image
            {
                Source = new BitmapImage(new Uri("/Image/slope1.jpg", UriKind.Relative)),
                Width = 120,
                Height = 80,
                Stretch = System.Windows.Media.Stretch.UniformToFill
            };
            Grid.SetColumn(image, 0);

            var stackPanel1 = new StackPanel { Margin = new Thickness(15, 0, 0, 0) };
            var textBlock1 = new TextBlock
            {
                Text = name,
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            stackPanel1.Children.Add(textBlock1);
            Grid.SetColumn(stackPanel1, 1);

            var stackPanel2 = new StackPanel();
            var button = new Button
            {
                Content = "Подробная информация",
                Width = 120,
                Height = 30
            };
            button.Click += (sender, e) => ShowDetails(name, description, liftPrice);
            stackPanel2.Children.Add(button);
            Grid.SetColumn(stackPanel2, 2);

            grid.Children.Add(image);
            grid.Children.Add(stackPanel1);
            grid.Children.Add(stackPanel2);

            border.Child = grid;

            SlopesStackPanel.Children.Add(border); // Теперь корректно добавляем
        }

        private void ShowDetails(string name, string description, decimal price)
        {
            MessageBox.Show($"Название: {name}\n\nОписание: {description}\n\nЦена подъемника: {price} руб.", "Информация о трассе");
        }

        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}