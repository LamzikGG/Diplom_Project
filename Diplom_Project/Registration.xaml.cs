using System;
using System.Windows;
using Npgsql;
using Diplom_Project.Services;

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
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password.Trim();
            string confirmPassword = txtConfirmPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Все обязательные поля должны быть заполнены!");
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

                    string sql = @"
                INSERT INTO users 
                (username, password_hash, first_name, last_name, phone) 
                VALUES 
                (@login, crypt(@password, gen_salt('bf')), @firstName, @lastName, @phone)";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@firstName", firstName);
                        cmd.Parameters.AddWithValue("@lastName", lastName);
                        cmd.Parameters.AddWithValue("@phone", phone ?? (object)DBNull.Value); // если телефон пустой — NULL

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