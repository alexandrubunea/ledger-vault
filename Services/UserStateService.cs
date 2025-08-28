using System.Threading.Tasks;

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

    public async Task SaveUserState()
    {
        await userRepository.UpdateUserName(FullUserName);
        await userRepository.UpdateUserCurrencyId(CurrencyId);
        await userRepository.UpdateUserThemeId(ThemeId);
    }

    public async Task SaveUserBalance()
    {
        await userRepository.UpdateUserBalance(Balance);
    }

    #endregion
}
