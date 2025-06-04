using System.Collections.Generic;
using System.Data;
using System.Windows;
using Diplom_Project.Models;
using Diplom_Project.Services;
using System.Data.SQLite;

namespace Diplom_Project.Views
{
    public partial class CashierWindow : Window
    {
        private readonly UserModel _user;

        public CashierWindow(UserModel user)
        {
            InitializeComponent();
            _user = user;
            LoadBookings();
            LoadRentals();
        }

        private void LoadBookings()
        {
            List<BookingModel> bookings = new List<BookingModel>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"
            SELECT b.booking_id, u.first_name, u.last_name, u.phone, b.accommodation_id, b.check_in, b.check_out, b.total_price 
            FROM bookings b
            JOIN users u ON b.user_id = u.id";
                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookings.Add(new BookingModel
                        {
                            BookingId = reader.GetInt32(0),
                            UserName = $"{reader.GetString(1)} {reader.GetString(2)}",
                            UserPhone = reader.GetString(3),
                            AccommodationId = reader.GetInt32(4),
                            CheckIn = reader.GetDateTime(5).ToString("yyyy-MM-dd"),
                            CheckOut = reader.GetDateTime(6).ToString("yyyy-MM-dd"),
                            TotalPrice = reader.GetDecimal(7)
                        });
                    }
                }
            }
            BookingsDataGrid.ItemsSource = bookings;
        }

        private void LoadRentals()
        {
            List<RentalModel> rentals = new List<RentalModel>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"
            SELECT r.rental_id, u.first_name, u.last_name, u.phone, e.type, r.start_time, r.end_time, r.total_price 
            FROM rentals r
            JOIN users u ON r.user_id = u.id
            JOIN equipment e ON r.equipment_id = e.equipment_id";
                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rentals.Add(new RentalModel
                        {
                            RentalId = reader.GetInt32(0),
                            UserName = $"{reader.GetString(1)} {reader.GetString(2)}",
                            UserPhone = reader.GetString(3),
                            EquipmentName = reader.GetString(4),
                            StartTime = reader.GetDateTime(5).ToString("yyyy-MM-dd HH:mm"),
                            EndTime = reader.IsDBNull(6) ? "В процессе" : reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm"),
                            TotalPrice = reader.GetDecimal(7)
                        });
                    }
                }
            }
            RentalsDataGrid.ItemsSource = rentals;
        }

        // Удаление бронирования
        private void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is CashierWindow.BookingModel booking)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = Database.GetConnection())
                        {
                            conn.Open();
                            string sql = "DELETE FROM bookings WHERE booking_id = @id";
                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", booking.BookingId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadBookings(); // Обновляем таблицу
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении: " + ex.Message);
                    }
                }
            }
        }

        // Удаление аренды
        private void DeleteRental_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsDataGrid.SelectedItem is CashierWindow.RentalModel rental)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = Database.GetConnection())
                        {
                            conn.Open();
                            string sql = "DELETE FROM rentals WHERE rental_id = @id";
                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", rental.RentalId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadRentals(); // Обновляем таблицу
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении: " + ex.Message);
                    }
                }
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new authorisation();
            authWindow.Show();
            this.Close();
        }

        // Модели
        public class BookingModel
        {
            public int BookingId { get; set; }
            public string UserName { get; set; }
            public string UserPhone { get; set; }
            public int AccommodationId { get; set; }
            public string CheckIn { get; set; }
            public string CheckOut { get; set; }
            public decimal TotalPrice { get; set; }
        }

        public class RentalModel
        {
            public int RentalId { get; set; }
            public string UserName { get; set; }
            public string UserPhone { get; set; }
            public string EquipmentName { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}