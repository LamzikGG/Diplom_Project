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
                string sql = "SELECT booking_id, user_id, accommodation_id, check_in, check_out, total_price FROM bookings";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookings.Add(new BookingModel
                        {
                            BookingId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            AccommodationId = reader.GetInt32(2),
                            CheckIn = reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                            CheckOut = reader.GetDateTime(4).ToString("yyyy-MM-dd"),
                            TotalPrice = reader.GetDecimal(5)
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
                string sql = "SELECT rental_id, user_id, equipment_id, start_time, end_time, total_price FROM rentals";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rentals.Add(new RentalModel
                        {
                            RentalId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            EquipmentId = reader.GetInt32(2),
                            StartTime = reader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm"),
                            EndTime = reader.IsDBNull(4) ? "В процессе" : reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm"),
                            TotalPrice = reader.GetDecimal(5)
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
                if (MessageBox.Show("Вы уверены, что хотите удалить это бронирование?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        string sql = "DELETE FROM bookings WHERE booking_id = @bookingId";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("bookingId", booking.BookingId);
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
                if (MessageBox.Show("Вы уверены, что хотите удалить эту аренду?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        string sql = "DELETE FROM rentals WHERE rental_id = @rentalId";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("rentalId", rental.RentalId);
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
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int AccommodationId { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class RentalModel
    {
        public int RentalId { get; set; }
        public int UserId { get; set; }
        public int EquipmentId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal TotalPrice { get; set; }
    }
}