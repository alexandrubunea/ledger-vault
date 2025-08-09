namespace ledger_vault.Services;

public class UserService
{
    private readonly DatabaseManagerService _databaseManagerService;

    public UserService(DatabaseManagerService databaseManagerService)
    {
        _databaseManagerService = databaseManagerService;
    }
    
    public void CreateUser(string fullName, string password, ushort currencyId)
    {
        if (_databaseManagerService.IsSetup())
            return;

        using var conn = _databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText =
            """
            INSERT INTO user_information (full_name, currencyId, themeId, balance, password)
            VALUES(@fullname, @currency, 0, 0, @password_hashed)
            """;

        command.Parameters.AddWithValue("@fullname", fullName);
        command.Parameters.AddWithValue("@currency", currencyId);
        command.Parameters.AddWithValue("@password_hashed", BCrypt.Net.BCrypt.EnhancedHashPassword(password));

        command.ExecuteNonQuery();
    }

    public void UpdateUserPassword(string password, string newPassword)
    {
        if (!CheckUserPassword(password))
            return;

        using var conn = _databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "UPDATE user_information SET password = @password WHERE id > 0";
        command.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword));

        command.ExecuteNonQuery();
    }

    public void UpdateUserName(string fullName)
    {
        using var conn = _databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "UPDATE user_information SET full_name = @fullName WHERE id > 0";
        command.Parameters.AddWithValue("@fullName", fullName);

        command.ExecuteNonQuery();
    }

    public void UpdateUserCurrencyId(short currencyId)
    {
        using var conn = _databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "UPDATE user_information SET currencyId = @currencyId WHERE id > 0";
        command.Parameters.AddWithValue("@currencyId", currencyId);

        command.ExecuteNonQuery();
    }

    public void UpdateUserThemeId(short themeId)
    {
        using var conn = _databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "UPDATE user_information SET themeId = @themeId WHERE id > 0";
        command.Parameters.AddWithValue("@themeId", themeId);

        command.ExecuteNonQuery();
    }

    public void UpdateUserBalance(decimal balance)
    {
        using var conn = _databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();
        
        command.CommandText = "UPDATE user_information SET balance = @balance WHERE id > 0";
        command.Parameters.AddWithValue("@balance", balance);
        
        command.ExecuteNonQuery();
    }

    public bool CheckUserPassword(string password)
    {
        using var conn = _databaseManagerService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT password FROM user_information LIMIT 1";

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return false;

        string storedHash = reader.GetString(0);
        return BCrypt.Net.BCrypt.EnhancedVerify(password, storedHash);
    }

    public void DeleteAllUserData()
    {
        using var conn = _databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "DELETE FROM user_information WHERE id > 0";
        command.ExecuteNonQuery();

        // TODO: Delete data from other tables too
    }
}