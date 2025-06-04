using System;
using System.Windows;
using Diplom_Project.Services;
using Diplom_Project.Views;
using System.Data.SQLite;
using Diplom_Project.Models;
using System.Security.Cryptography;
using System.Text;

namespace Diplom_Project
{
    public partial class authorisation : Window
    {
        private UserModel _currentUser = new UserModel();

        public authorisation()
        {
            InitializeComponent();
            // Инициализация базы данных при создании окна
            Database.Initialize();
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

            // Проверка специальных аккаунтов
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

            // Проверка обычных пользователей
            CheckRegularUser(login, password);
        }

        private void OpenAdminPanel()
        {
            var adminPanel = new AdminPanel();
            adminPanel.Show();
            this.Close();
        }

        private void OpenCashierWindow(UserModel user)
        {
            var cashierWindow = new CashierWindow(user);
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
                        SELECT id, role, first_name, last_name, phone, password_hash 
                        FROM users 
                        WHERE username = @login";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader.GetString(5);
                                if (VerifyPassword(password, storedHash))
                                {
                                    _currentUser = new UserModel
                                    {
                                        Id = reader.GetInt32(0),
                                        Username = login,
                                        Role = reader.GetString(1),
                                        FirstName = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        LastName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                        Phone = reader.IsDBNull(4) ? null : reader.GetString(4)
                                    };

                                    var mainWindow = new MainWindow(_currentUser);
                                    mainWindow.Show();
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("Неверный логин или пароль", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Пользователь не найден", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            string inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }

        private static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}