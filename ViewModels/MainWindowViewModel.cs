using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region PUBLIC API

    public MainWindowViewModel(CoreViewNavigatorService navigatorService, DatabaseManagerService dbManager)
    {
        _navigator = navigatorService;

        navigatorService.NavigateTo(dbManager.IsSetup() ? CoreViews.Login : CoreViews.Setup);
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public MainWindowViewModel()
    {
    }
#pragma warning restore

    #endregion

    #region PRIVATE PROPERTIES

    [ObservableProperty] private CoreViewNavigatorService _navigator;

    #endregion
}