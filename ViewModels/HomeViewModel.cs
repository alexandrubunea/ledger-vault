using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class HomeViewModel : PageViewModel
{
    #region PUBLIC API

    public HomeViewModel(UserStateService userStateService)
    {
        PageName = ApplicationPages.Home;

        _userFullName = userStateService.FullUserName;
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public HomeViewModel()
    {
    }
#pragma warning restore

    #endregion

    #region PRIVATE PROPERTIES

    [ObservableProperty] private string _userFullName;

    #endregion
}