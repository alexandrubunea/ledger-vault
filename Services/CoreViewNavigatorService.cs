using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Factories;
using ledger_vault.ViewModels;

namespace ledger_vault.Services;

public class CoreViewNavigatorService : ObservableObject
{
    private CoreViewModel _currentViewModel = new();
    private readonly AuthService _authService;

    public CoreViewModel CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    private readonly CoreViewFactory _coreViewFactory;

    public CoreViewNavigatorService(CoreViewFactory coreViewFactory, AuthService authService)
    {
        _coreViewFactory = coreViewFactory;
        _authService = authService;
    }

    public void NavigateTo(CoreViews coreView)
    {
        if (coreView == CoreViews.Main && !_authService.LoggedIn)
        {
            CurrentViewModel = _coreViewFactory.GetCoreViewModel(CoreViews.Login);
            return;
        }

        CurrentViewModel = _coreViewFactory.GetCoreViewModel(coreView);
    }
}