using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Npgsql;
using Diplom_Project.Models;
using Diplom_Project.Services;
using System.Windows.Media.Imaging;

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

        // Загрузка открытых трасс из базы данных
        private void LoadSlopes()
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT name, difficulty, description FROM slopes WHERE status = 'open'";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GenerateSlopeBlock(
                                reader.GetString(0),              // name
                                reader.GetString(1),              // difficulty
                                reader.IsDBNull(2) ? "" : reader.GetString(2)  // description
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки трасс: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Создание UI-блока для каждой трассы
        private void GenerateSlopeBlock(string name, string difficulty, string description)
        {
            var border = new Border
            {
                Background = Brushes.DarkGray,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(15)
            };

            var grid = new Grid();

            // Колонки сетки
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Изображение
            var image = new Image
            {
                Source = new BitmapImage(new Uri("/Image/slope1.jpg", UriKind.Relative)),
                Width = 120,
                Height = 80,
                Stretch = Stretch.UniformToFill
            };
            Grid.SetColumn(image, 0);

            // Текстовая информация
            var stackPanel1 = new StackPanel { Margin = new Thickness(15, 0, 0, 0) };
            var textBlockName = new TextBlock
            {
                Text = name,
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            var textBlockDifficulty = new TextBlock
            {
                Text = $"Сложность: {difficulty}",
                Foreground = Brushes.LightBlue,
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 0)
            };
            stackPanel1.Children.Add(textBlockName);
            stackPanel1.Children.Add(textBlockDifficulty);
            Grid.SetColumn(stackPanel1, 1);

            // Кнопка подробностей
            var stackPanel2 = new StackPanel();
            var button = new Button
            {
                Content = "Подробная информация",
                Width = 120,
                Height = 30
            };
            button.Click += (sender, e) => ShowDetails(name, difficulty, description);
            stackPanel2.Children.Add(button);
            Grid.SetColumn(stackPanel2, 2);

            // Добавляем элементы в Grid
            grid.Children.Add(image);
            grid.Children.Add(stackPanel1);
            grid.Children.Add(stackPanel2);
            border.Child = grid;

            // Добавляем блок в основной контейнер
            SlopesStackPanel.Children.Add(border);
        }

        // Отображение информации о трассе
        private void ShowDetails(string name, string difficulty, string description)
        {
            MessageBox.Show(
                $"Название: {name}\n\n" +
                $"Сложность: {difficulty}\n\n" +
                $"Описание: {(string.IsNullOrEmpty(description) ? "Нет описания" : description)}",
                "Информация о трассе",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        // Закрытие окна по клику на лого
        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}