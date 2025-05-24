using System.Collections.Generic;
using System.Windows;
using Diplom_Project.Models;
using Diplom_Project.Services;
using Npgsql;

namespace Diplom_Project.Views
{
    public partial class AdminPanel : Window
    {
        public AdminPanel()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = new List<UserModel>();

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT id, username, role FROM users";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new UserModel
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Role = reader.GetString(2)
                        });
                    }
                }
            }

            UsersDataGrid.ItemsSource = users;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            foreach (UserModel user in UsersDataGrid.Items)
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE users SET role = @role WHERE id = @id";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@role", user.Role);
                        cmd.Parameters.AddWithValue("@id", user.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            MessageBox.Show("Роли успешно обновлены");
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }
}