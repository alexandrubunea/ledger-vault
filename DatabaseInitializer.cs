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
                                        CREATE TABLE IF NOT EXISTS transactions (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            Counterparty TEXT NOT NULL,
                                            Description TEXT NOT NULL,
                                            Amount REAL NOT NULL,
                                            Tags TEXT NOT NULL,
                                            ReceiptImage TEXT,
                                            ReceiptImageHash TEXT,
                                            DateTime TIMESTAMP NOT NULL,
                                            Hash TEXT NOT NULL,
                                            PreviousHash TEXT,
                                            Signature TEXT NOT NULL,
                                            ReversalOfTransactionId INTEGER,
                                            
                                            FOREIGN KEY (ReversalOfTransactionId) REFERENCES transactions(id)
                                        );
                              """;
        command.ExecuteNonQuery();

        connection.Close();
    }
}