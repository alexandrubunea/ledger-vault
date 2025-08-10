using ledger_vault.Data;
using Microsoft.Data.Sqlite;

namespace ledger_vault.Services;

public class AuthService(DatabaseManagerService dbManager, UserStateService userState, UserService userService)
{
    #region PUBLIC PROPERTIES

    public bool LoggedIn { get; private set; }

    #endregion

    #region PUBLIC API

    public LoginResult Login(string password)
    {
        try
        {
            using var conn = dbManager.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT password FROM user_information LIMIT 1";

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return LoginResult.NoUser;

            string storedHash = reader.GetString(0);
            if (!BCrypt.Net.BCrypt.EnhancedVerify(password, storedHash))
                return LoginResult.WrongPassword;

            LoggedIn = true;
            LoadUserState(conn);
            return LoginResult.Success;
        }
        catch
        {
            return LoginResult.DatabaseError;
        }
    }

    public void UpdateUserPassword(string password, string newPassword) =>
        userService.UpdateUserPassword(password, newPassword);

    public bool CheckUserPassword(string password) => userService.CheckUserPassword(password);

    public void DeleteAccount()
    {
        userService.DeleteAllUserData();
    }

    #endregion

    #region PRIVATE METHODS

    private void LoadUserState(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT full_name, balance, currencyId, themeId FROM user_information LIMIT 1";

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return;

        userState.FullUserName = reader.GetString(0);
        userState.Balance = reader.GetDecimal(1);
        userState.CurrencyId = reader.GetInt16(2);
        userState.ThemeId = reader.GetInt16(3);
    }

    #endregion
}