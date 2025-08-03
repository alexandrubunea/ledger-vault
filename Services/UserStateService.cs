namespace ledger_vault.Services;

public class UserStateService
{
    private string _fullUserName = "";
    private ushort _currencyId = 0;
    private ulong _balance = 0;
    private ushort _themeId = 0;
    
    public string FullUserName
    {
        get => _fullUserName;
        
        // TODO: Create the logic to update the database.
        set => _fullUserName = value;
    }

    public ushort CurrencyId
    {
        get => _currencyId;
        
        // TODO: Create the logic to update the database.
        set => _currencyId = value;
    }

    public ulong Balance
    {
        get => _balance;
        
        // TODO: Create the logic to update the database.
        set => _balance = value;
    }

    public ushort ThemeId
    {
        get => _themeId;
        
        // TODO: Create the logic to update the database.
        set => _themeId = value;
    }

    public UserStateService()
    {
        // TODO: Load the data from the database.
        _fullUserName = "John Doe";
        _currencyId = 0;
        _balance = 100000;
    }
}