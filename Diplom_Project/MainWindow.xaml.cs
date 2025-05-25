using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diplom_Project
{
    public partial class MainWindow : Window
    {
        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            authorisation go_authorisation = new authorisation();
            go_authorisation.Show();
            this.Close();
        }
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