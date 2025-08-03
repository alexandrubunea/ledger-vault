using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class HomeViewModel : PageViewModel
{
    [ObservableProperty] private string _userFullName;

    public HomeViewModel(UserStateService userStateService)
    {
        PageName = ApplicationPages.Home;

        UserFullName = userStateService.FullUserName;
    }
}