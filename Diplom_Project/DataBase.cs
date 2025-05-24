using Npgsql;

namespace Diplom_Project.Services
{
    public static class Database
    {
        private static string connectionString = "Host=localhost;Username=postgres;Password=2909;Database=diplom_project";

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}