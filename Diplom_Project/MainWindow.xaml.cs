using System.Windows;
using System.Windows.Controls;

namespace Diplom_Project
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Image1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Выбраны трассы", "Уведомление",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Image2_Click(object sender, RoutedEventArgs e)
        {
            var arenda_zshilia = new arenda_zhilya();
            arenda_zshilia.Show();
            this.Close();
        }

        private void Image3_Click(object sender, RoutedEventArgs e)
        {
            var arenda_ekipirovki = new arenda_ekipa();
            arenda_ekipirovki.Show();
            this.Close();

        }
    }
}