using System;
using ledger_vault.Data;
using System.Threading.Tasks;

namespace ledger_vault.Services;

public class AuthService(UserStateService userState, UserRepository userRepository)
{
    #region PUBLIC PROPERTIES

    public bool LoggedIn { get; private set; }

    #endregion

    #region PUBLIC API

    public async Task<LoginResult> Login(string password)
    {
        LoginResult loginResult = await userRepository.CheckUserPassword(password);
        if (loginResult != LoginResult.Success)
            return loginResult;

        LoggedIn = true;
        UserData? loadedState = await userRepository.LoadUserState();

        if (loadedState == null)
            throw new Exception("Could not load user state");

        userState.FullUserName = loadedState.FullUserName;
        userState.Balance = loadedState.Balance;
        userState.CurrencyId = loadedState.CurrencyId;
        userState.ThemeId = loadedState.ThemeId;

        return LoginResult.Success;
    }

    public async Task UpdateUserPassword(string password, string newPassword) => 
        await userRepository.UpdateUserPassword(password, newPassword);

    public async Task<bool> CheckUserPassword(string password) => 
        await userRepository.CheckUserPassword(password) == LoginResult.Success;

    public async Task DeleteAccount()
    {
        await userRepository.DeleteAllUserData();
    }

    #endregion
}
