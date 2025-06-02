using System.Windows;
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
    }
}