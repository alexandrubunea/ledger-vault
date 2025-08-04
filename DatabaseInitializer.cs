using Microsoft.Data.Sqlite;

namespace ledger_vault;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
                                          CREATE TABLE IF NOT EXISTS user_information (
                                              id INTEGER PRIMARY KEY AUTOINCREMENT,
                                              full_name TEXT NOT NULL,
                                              currencyId INTEGER NOT NULL,
                                              themeId INTEGER NOT NULL,
                                              balance REAL NOT NULL DEFAULT 0,
                                              password TEXT NOT NULL
                                          );

                              """;
        command.ExecuteNonQuery();
        
        connection.Close();
    }
}