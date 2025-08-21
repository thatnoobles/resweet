using Npgsql;
using Resweet.Database.Entities;

namespace Resweet.Database.Utils;

public static class DatabaseUtils
{
    private static NpgsqlDataSource dataSource;

    public static void Connect(string hostname, string username, string password, string database)
    {
        string connectionString =
            $"Host={hostname};Username={username};Password={password};Database={database}";

        dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public static void SelectOne<T>()
        where T : Entity
    {
        using (NpgsqlCommand command = dataSource.CreateCommand(
            """
            SELECT * F
            """
        ))
    }
}
