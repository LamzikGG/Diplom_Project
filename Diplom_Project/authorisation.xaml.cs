using System;
using System.Windows;
using Npgsql;

namespace Diplom_Project
{
    public partial class authorisation : Window
    {
        public authorisation()
        {
            InitializeComponent();
        }

        private void Button_in_Registration(object sender, RoutedEventArgs e)
        {
            var registration = new Registration();
            registration.Show();
            this.Close();
        }

        public void LoginButton(object sender, RoutedEventArgs e)
        {
            var login = txtLogin.Text;
            var password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();

                    string sql = "SELECT id FROM users WHERE username = @login AND password_hash = crypt(@password, password_hash)";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        var result = cmd.ExecuteScalar();

                        if (result != null)
                        { 
                            var mainWindow = new MainWindow();
                            mainWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль");
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}");
            }
        }
    }
}