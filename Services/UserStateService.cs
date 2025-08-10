namespace ledger_vault.Services;

public class UserStateService(UserService userService)
{
    #region PUBLIC PROPERTIES

    public string FullUserName { get; set; } = "";
    public short CurrencyId { get; set; }
    public decimal Balance { get; set; }
    public short ThemeId { get; set; }

    #endregion

    #region PUBLIC API

    public void SaveUserState()
    {
        userService.UpdateUserName(FullUserName);
        userService.UpdateUserCurrencyId(CurrencyId);
        userService.UpdateUserThemeId(ThemeId);
    }

    public void SaveUserBalance()
    {
        userService.UpdateUserBalance(Balance);
    }

    #endregion
}