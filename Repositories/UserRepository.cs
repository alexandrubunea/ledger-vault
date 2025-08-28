using ledger_vault.Data;
using System.Threading.Tasks;

namespace ledger_vault.Services;

public class UserRepository(DatabaseManagerService databaseManagerService)
{
    #region PUBLIC API

    public async Task CreateUser(string fullName, string password, ushort currencyId)
    {
        if (databaseManagerService.IsSetup())
            return;

        using var conn = databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText =
            """
            INSERT INTO user_information (full_name, currencyId, themeId, balance, password)
            VALUES(@fullname, @currency, 0, 0, @password_hashed)
            """;

        command.Parameters.AddWithValue("@fullname", fullName);
        command.Parameters.AddWithValue("@currency", currencyId);
        command.Parameters.AddWithValue("@password_hashed", BCrypt.Net.BCrypt.EnhancedHashPassword(password));

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateUserPassword(string password, string newPassword)
    {
        if (await CheckUserPassword(password) != LoginResult.Success)
            return;

        using var conn = databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "UPDATE user_information SET password = @password WHERE id > 0";
        command.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword));

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateUserName(string fullName)
    {
        using var conn = databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "UPDATE user_information SET full_name = @fullName WHERE id > 0";
        command.Parameters.AddWithValue("@fullName", fullName);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateUserCurrencyId(short currencyId)
    {
        using var conn = databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "UPDATE user_information SET currencyId = @currencyId WHERE id > 0";
        command.Parameters.AddWithValue("@currencyId", currencyId);


        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateUserThemeId(short themeId)
    {
        using var conn = databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "UPDATE user_information SET themeId = @themeId WHERE id > 0";
        command.Parameters.AddWithValue("@themeId", themeId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateUserBalance(decimal balance)
    {
        using var conn = databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = "UPDATE user_information SET balance = @balance WHERE id > 0";
        command.Parameters.AddWithValue("@balance", balance);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<LoginResult> CheckUserPassword(string password)
    {
        try
        {
            using var conn = databaseManagerService.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT password FROM user_information LIMIT 1";

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.Read())
                return LoginResult.NoUser;

            string storedHash = reader.GetString(0);

            return !BCrypt.Net.BCrypt.EnhancedVerify(password, storedHash)
                ? LoginResult.WrongPassword
                : LoginResult.Success;


        }
        catch
        {
            return LoginResult.DatabaseError;
        }
    }

    public async Task DeleteAllUserData()
    {
        using var conn = databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();

        command.CommandText = """
                              DELETE FROM user_information;
                              DELETE FROM reversed_transactions;
                              DELETE FROM transactions;
                              """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task<UserData?> LoadUserState()
    {
        using var conn = databaseManagerService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT full_name, balance, currencyId, themeId FROM user_information LIMIT 1";

        using var reader = await cmd.ExecuteReaderAsync();
        if (!reader.Read()) return null;

        return new UserData(
            reader.GetString(0),
            reader.GetDecimal(1),
            reader.GetInt16(2),
            reader.GetInt16(3)
        );

    }

    #endregion
}
