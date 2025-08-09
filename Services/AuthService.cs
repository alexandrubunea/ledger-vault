using ledger_vault.Data;
using Microsoft.Data.Sqlite;

namespace ledger_vault.Services;

public class AuthService
{
    private readonly DatabaseManagerService _dbManager;
    private readonly UserStateService _userState;
    private readonly UserService _userService;

    public bool LoggedIn { get; private set; }

    public AuthService(DatabaseManagerService dbManager, UserStateService userState,  UserService userService)
    {
        _dbManager = dbManager;
        _userState = userState;
        _userService = userService;
    }

    public LoginResult Login(string password)
    {
        try
        {
            using var conn = _dbManager.GetConnection();
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
        _userService.UpdateUserPassword(password, newPassword);

    public bool CheckUserPassword(string password) => _userService.CheckUserPassword(password);

    public void DeleteAccount()
    {
        _userService.DeleteAllUserData();
    }

    private void LoadUserState(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT full_name, balance, currencyId, themeId FROM user_information LIMIT 1";

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return;

        _userState.FullUserName = reader.GetString(0);
        _userState.Balance = reader.GetDecimal(1);
        _userState.CurrencyId = reader.GetInt16(2);
        _userState.ThemeId = reader.GetInt16(3);
    }
}