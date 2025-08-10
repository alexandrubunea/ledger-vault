using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ledger_vault.Services;

public class DatabaseManagerService
{
    #region PUBLIC API

    public SqliteConnection GetConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public bool IsSetup()
    {
        using var conn = GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "SELECT COUNT(*) FROM user_information";

        var result = (long?)command.ExecuteScalar();
        return result > 0;
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly string _connectionString = CreateConnectionString();

    #endregion

    #region PRIVATE METHODS

    private static string CreateConnectionString()
    {
        string appDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LedgerVault"
        );
        Directory.CreateDirectory(appDir);

        string dbPath = Path.Combine(appDir, "ledger.db");
        return $"Data Source={dbPath}";
    }

    #endregion
}