using System.Windows;
using Diplom_Project.Models;
using Diplom_Project.Views;

namespace Diplom_Project
{
    public partial class MainWindow : Window
    {
        private readonly UserModel _user;

        public MainWindow(UserModel user)
        {
            InitializeComponent();
            _user = user;
        }

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var authWindow = new authorisation();
            authWindow.Show();
            this.Close();
        }

        private void Image1_Click(object sender, RoutedEventArgs e)
        {
            var routesWindow = new routes(_user);
            routesWindow.Show();
        }

        private void Image2_Click(object sender, RoutedEventArgs e)
        {
            var arendaZhilYa = new arenda_zhilya(_user);
            arendaZhilYa.Show();
        }

        private void Image3_Click(object sender, RoutedEventArgs e)
        {
            var arendaEkipirovka = new arenda_ekipa(_user);
            arendaEkipirovka.Show();
        }

        private void ReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            var reviewsWindow = new ReviewsWindow(_user);
            reviewsWindow.Show();
        }
    }
}