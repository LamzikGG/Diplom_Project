using System.Data.SQLite;

namespace Diplom_Project.Services
{
    public static class Database
    {
        private static string connectionString = "Data Source=diplom_project.db";

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        // Метод для инициализации структуры базы данных
        public static void Initialize()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string[] tableCommands = new[]
                {
                    @"CREATE TABLE IF NOT EXISTS users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username VARCHAR(50) UNIQUE NOT NULL,
                        password_hash TEXT NOT NULL,
                        role VARCHAR(20) NOT NULL DEFAULT 'client',
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        first_name VARCHAR(100),
                        last_name VARCHAR(100),
                        phone VARCHAR(20)
                    );",
                    @"CREATE TABLE IF NOT EXISTS accommodations (
                        accommodation_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name VARCHAR(100) NOT NULL,
                        address TEXT,
                        price_per_night REAL NOT NULL,
                        image_path VARCHAR(255)
                    );",
                    @"CREATE TABLE IF NOT EXISTS equipment (
                        equipment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        type VARCHAR(50) NOT NULL,
                        brand VARCHAR(50),
                        price REAL NOT NULL,
                        image_path VARCHAR(255)
                    );",
                    @"CREATE TABLE IF NOT EXISTS resorts (
                        resort_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name VARCHAR(100) NOT NULL,
                        location VARCHAR(100),
                        description TEXT
                    );",
                    @"CREATE TABLE IF NOT EXISTS seasons (
                        season_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name VARCHAR(50) NOT NULL,
                        start_date DATE NOT NULL,
                        end_date DATE NOT NULL
                    );",
                    @"CREATE TABLE IF NOT EXISTS slopes (
                        slope_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name VARCHAR(100) NOT NULL,
                        difficulty VARCHAR(20),
                        status VARCHAR(20) DEFAULT 'open',
                        description TEXT,
                        image_path VARCHAR(255),
                        CHECK (difficulty IN ('easy', 'medium', 'hard'))
                    );",
                    @"CREATE TABLE IF NOT EXISTS slope_prices (
                        price_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        slope_id INTEGER REFERENCES slopes(slope_id),
                        season_id INTEGER REFERENCES seasons(season_id),
                        price REAL NOT NULL
                    );",
                    @"CREATE TABLE IF NOT EXISTS bookings (
                        booking_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER REFERENCES users(id),
                        accommodation_id INTEGER REFERENCES accommodations(accommodation_id),
                        check_in DATE NOT NULL,
                        check_out DATE NOT NULL,
                        total_price REAL
                    );",
                    @"CREATE TABLE IF NOT EXISTS rentals (
                        rental_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER REFERENCES users(id),
                        equipment_id INTEGER REFERENCES equipment(equipment_id),
                        start_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        end_time TIMESTAMP,
                        total_price REAL
                    );",
                    @"CREATE TABLE IF NOT EXISTS reviews (
                        review_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER REFERENCES users(id),
                        content TEXT NOT NULL,
                        rating INTEGER,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        CHECK (rating >= 1 AND rating <= 5)
                    );"
                };
                foreach (var cmdText in tableCommands)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = cmdText;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}