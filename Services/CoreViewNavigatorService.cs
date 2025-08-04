using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Factories;
using ledger_vault.ViewModels;

namespace ledger_vault.Services;

public class CoreViewNavigatorService : ObservableObject
{
    private CoreViewModel _currentViewModel;
    public CoreViewModel CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    private readonly CoreViewFactory _coreViewFactory;

    public CoreViewNavigatorService(CoreViewFactory coreViewFactory)
    {
        _coreViewFactory = coreViewFactory;
    }

    public void NavigateTo(CoreViews coreView)
    {
        CurrentViewModel = _coreViewFactory.GetCoreViewModel(coreView);
    }
}