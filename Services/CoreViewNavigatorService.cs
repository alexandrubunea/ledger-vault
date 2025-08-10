using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Factories;
using ledger_vault.ViewModels;

namespace ledger_vault.Services;

public class CoreViewNavigatorService(CoreViewFactory coreViewFactory, AuthService authService) : ObservableObject
{
    #region PUBLIC API

    public CoreViewModel CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public void NavigateTo(CoreViews coreView)
    {
        if (coreView == CoreViews.Main && !authService.LoggedIn)
        {
            CurrentViewModel = coreViewFactory.GetCoreViewModel(CoreViews.Login);
            return;
        }

        CurrentViewModel = coreViewFactory.GetCoreViewModel(coreView);
    }

    #endregion

    #region PRIVATE PROPERTIES

    private CoreViewModel _currentViewModel = new();

    #endregion
}