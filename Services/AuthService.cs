using System;
using ledger_vault.Data;

namespace ledger_vault.Services;

public class AuthService(UserStateService userState, UserRepository userRepository)
{
    #region PUBLIC PROPERTIES

    public bool LoggedIn { get; private set; }

    #endregion

    #region PUBLIC API

    public LoginResult Login(string password)
    {
        LoginResult loginResult = userRepository.CheckUserPassword(password);
        if (loginResult != LoginResult.Success)
            return loginResult;

        LoggedIn = true;
        UserData? loadedState = userRepository.LoadUserState();
        
        if (loadedState == null)
            throw new Exception("Could not load user state");

        userState.FullUserName = loadedState.FullUserName;
        userState.Balance = loadedState.Balance;
        userState.CurrencyId = loadedState.CurrencyId;
        userState.ThemeId = loadedState.ThemeId;
        
        return LoginResult.Success;
    }

    public void UpdateUserPassword(string password, string newPassword) =>
        userRepository.UpdateUserPassword(password, newPassword);

    public bool CheckUserPassword(string password) => userRepository.CheckUserPassword(password) == LoginResult.Success;

    public void DeleteAccount()
    {
        userRepository.DeleteAllUserData();
    }

    #endregion
}