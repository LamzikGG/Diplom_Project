using System.Collections.Generic;
using System.Data;
using System.Windows;
using Diplom_Project.Services;
using Npgsql;

namespace Diplom_Project.Views
{
    public partial class CashierWindow : Window
    {
        public CashierWindow()
        {
            InitializeComponent();
            LoadBookings();
            LoadRentals();
        }

        private void LoadBookings()
        {
            List<BookingModel> bookings = new List<BookingModel>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT user_id, accommodation_id, check_in, check_out, total_price FROM bookings";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookings.Add(new BookingModel
                        {
                            UserId = reader.GetInt32(0),
                            AccommodationId = reader.GetInt32(1),
                            CheckIn = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                            CheckOut = reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                            TotalPrice = reader.GetDecimal(4)
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
                string sql = "SELECT user_id, equipment_id, start_time, end_time, total_price FROM rentals";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rentals.Add(new RentalModel
                        {
                            UserId = reader.GetInt32(0),
                            EquipmentId = reader.GetInt32(1),
                            StartTime = reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm"),
                            EndTime = reader.IsDBNull(3) ? "В процессе" : reader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm"),
                            TotalPrice = reader.GetDecimal(4)
                        });
                    }
                }
            }
            RentalsDataGrid.ItemsSource = rentals;
        }

        private void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            var booking = BookingsDataGrid.SelectedItem as BookingModel;
            if (booking != null)
            {
                if (MessageBox.Show("Вы уверены?", "Удалить запись", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        string sql = "DELETE FROM bookings WHERE user_id = @userId AND accommodation_id = @accId";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("userId", booking.UserId);
                            cmd.Parameters.AddWithValue("accId", booking.AccommodationId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadBookings();
                }
            }
        }

        private void DeleteRental_Click(object sender, RoutedEventArgs e)
        {
            var rental = RentalsDataGrid.SelectedItem as RentalModel;
            if (rental != null)
            {
                if (MessageBox.Show("Вы уверены?", "Удалить запись", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        string sql = "DELETE FROM rentals WHERE user_id = @userId AND equipment_id = @eqId";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("userId", rental.UserId);
                            cmd.Parameters.AddWithValue("eqId", rental.EquipmentId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadRentals();
                }
            }
        }
    }

    public class BookingModel
    {
        public int UserId { get; set; }
        public int AccommodationId { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class RentalModel
    {
        public int UserId { get; set; }
        public int EquipmentId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal TotalPrice { get; set; }
    }
}