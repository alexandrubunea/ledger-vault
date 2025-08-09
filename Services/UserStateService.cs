namespace ledger_vault.Services;

public class UserStateService
{
    private readonly UserService _userService;

    public string FullUserName { get; set; } = "";
    public short CurrencyId { get; set; }
    public decimal Balance { get; set; }
    public short ThemeId { get; set; }

    public UserStateService(UserService userService)
    {
        _userService = userService;
    }

    public void SaveUserState()
    {
        _userService.UpdateUserName(FullUserName);
        _userService.UpdateUserCurrencyId(CurrencyId);
        _userService.UpdateUserThemeId(ThemeId);
    }
    
    public void SaveUserBalance()
    {
        _userService.UpdateUserBalance(Balance);
    }
}