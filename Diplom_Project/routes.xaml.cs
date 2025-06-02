using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Diplom_Project.Models;

namespace Diplom_Project
{
    public partial class routes : Window
    {
        private readonly UserModel _user;

        public routes(UserModel user)
        {
            InitializeComponent();
            _user = user;
        }

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var mainWindow = new MainWindow(_user);
            mainWindow.Show();
            this.Close();
        }

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string trackName)
            {
                var detailsWindow = new Window
                {
                    Title = $"Подробности: {trackName}",
                    Width = 400,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize,
                    Background = new SolidColorBrush(Color.FromRgb(37, 37, 37)),
                    Content = new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = GetTrackDetails(trackName),
                            TextWrapping = TextWrapping.Wrap,
                            Padding = new Thickness(15),
                            Foreground = Brushes.White,
                            FontSize = 14
                        }
                    }
                };

                detailsWindow.ShowDialog();
            }
        }

        private string GetTrackDetails(string trackName)
        {
            switch (trackName)
            {
                case "Чёрная трасса":
                    return "Чёрная трасса - самая сложная трасса на курорте.\n\n" +
                           "Характеристики:\n" +
                           "- Длина: 3500 метров\n" +
                           "- Перепад высот: 850 метров\n" +
                           "- Средний уклон: 28°\n" +
                           "- Максимальный уклон: 45°\n\n" +
                           "Описание:\n" +
                           "Трасса для опытных лыжников и сноубордистов. Имеет крутые спуски, узкие участки и естественные препятствия. Рекомендуется только для профессионалов.";

                case "Красная трасса":
                    return "Красная трасса - трасса для продвинутых лыжников.\n\n" +
                           "Характеристики:\n" +
                           "- Длина: 2800 метров\n" +
                           "- Перепад высот: 650 метров\n" +
                           "- Средний уклон: 22°\n" +
                           "- Максимальный уклон: 35°\n\n" +
                           "Описание:\n" +
                           "Трасса с разнообразным рельефом, подходит для тех, кто уверенно стоит на лыжах. Имеет несколько крутых участков и широкие повороты.";

                case "Синяя трасса":
                    return "Синяя трасса - трасса средней сложности.\n\n" +
                           "Характеристики:\n" +
                           "- Длина: 2000 метров\n" +
                           "- Перепад высот: 450 метров\n" +
                           "- Средний уклон: 15°\n" +
                           "- Максимальный уклон: 25°\n\n" +
                           "Описание:\n" +
                           "Идеальный выбор для тех, кто уже освоил базовые навыки катания. Трасса широкая, с плавными поворотами и умеренным уклоном.";

                case "Зелёная трасса":
                    return "Зелёная трасса - трасса для начинающих.\n\n" +
                           "Характеристики:\n" +
                           "- Длина: 1500 метров\n" +
                           "- Перепад высот: 250 метров\n" +
                           "- Средний уклон: 8°\n" +
                           "- Максимальный уклон: 12°\n\n" +
                           "Описание:\n" +
                           "Просторная и пологая трасса, идеально подходящая для обучения и первых спусков. Полностью безопасна для новичков.";

                default:
                    return "Информация о данной трассе отсутствует.";
            }
        }
    }
}