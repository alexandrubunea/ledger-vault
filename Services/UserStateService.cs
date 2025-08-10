using ledger_vault.Data;

namespace ledger_vault.Services;

public class UserStateService(UserRepository userRepository)
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
        userRepository.UpdateUserName(FullUserName);
        userRepository.UpdateUserCurrencyId(CurrencyId);
        userRepository.UpdateUserThemeId(ThemeId);
    }

    public void SaveUserBalance()
    {
        userRepository.UpdateUserBalance(Balance);
    }

    #endregion
}