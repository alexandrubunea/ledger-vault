namespace ledger_vault.Services;

public class UserStateService
{
    private readonly DatabaseManagerService _dbManager;

    public string FullUserName { get; set; } = "";
    public short CurrencyId { get; set; }
    public float Balance { get; set; }
    public short ThemeId { get; set; }

    public UserStateService(DatabaseManagerService dbManager)
    {
        _dbManager = dbManager;
    }

    public void SaveUserState()
    {
        _dbManager.UpdateUserName(FullUserName);
        _dbManager.UpdateUserCurrencyId(CurrencyId);
        _dbManager.UpdateUserThemeId(ThemeId);
    }
}