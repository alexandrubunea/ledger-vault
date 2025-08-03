namespace ledger_vault.Services;

public interface IUserStateService
{
    public string FullUserName { get; set; }
    public ushort CurrencyId { get; set; }
    public ulong Balance { get; set; }
    public ushort ThemeId { get; set; }
}