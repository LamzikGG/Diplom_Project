using System;
using System.Windows;
using Npgsql;

namespace Diplom_Project
{
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Все поля должны быть заполнены!");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();

                    string sql = "INSERT INTO users (username, password_hash) VALUES (@login, crypt(@password, gen_salt('bf')))";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Регистрация успешна!");
                            var authWindow = new authorisation();
                            authWindow.Show();
                            this.Close();
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                // Проверяем текст ошибки на наличие дубликата логина
                if (ex.Message.Contains("duplicate key value violates unique constraint") ||
                    ex.Message.Contains("уже существует"))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.");
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неизвестная ошибка: {ex.Message}");
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new authorisation();
            authWindow.Show();
            this.Close();
        }
    }
}