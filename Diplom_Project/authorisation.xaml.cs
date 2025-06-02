using System;
using System.Windows;
using Diplom_Project.Services;
using Diplom_Project.Views;
using Npgsql;
using Diplom_Project.Models;

namespace Diplom_Project
{
    public partial class authorisation : Window
    {
        private UserModel _currentUser = new UserModel(); // Экземпляр UserModel для хранения данных

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

        private void LoginButton(object sender, RoutedEventArgs e)
        {
            var login = txtLogin.Text.Trim();
            var password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Сначала проверяем специальные аккаунты
            if (login == "admin" && password == "admin")
            {
                _currentUser = new UserModel { Id = -1, Username = "admin", Role = "admin" };
                OpenAdminPanel();
                return;
            }

            if (login == "kasir" && password == "kasir")
            {
                _currentUser = new UserModel { Id = -2, Username = "kasir", Role = "cashier" };
                OpenCashierWindow(_currentUser);
                return;
            }

            // Затем проверяем обычных пользователей
            CheckRegularUser(login, password);
        }

        private void OpenAdminPanel()
        {
            var adminPanel = new AdminPanel(); // Передаем UserModel в конструктор
            adminPanel.Show();
            this.Close();
        }

        private void OpenCashierWindow(UserModel user)
        {
            var cashierWindow = new CashierWindow(user); // Передаем UserModel в конструктор
            cashierWindow.Show();
            this.Close();
        }

        private void CheckRegularUser(string login, string password)
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                SELECT u.id, u.role, u.first_name, u.last_name, u.phone 
                FROM users u
                WHERE u.username = @login AND u.password_hash = crypt(@password, u.password_hash)";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = new UserModel
                                {
                                    Id = reader.GetInt32(0),
                                    Username = login,
                                    Role = reader.GetString(1),
                                    FirstName = reader.GetString(2),
                                    LastName = reader.GetString(3),
                                    Phone = reader.GetString(4)
                                };

                                var mainWindow = new MainWindow(user);
                                mainWindow.Show();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}