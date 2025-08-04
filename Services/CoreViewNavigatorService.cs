using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Factories;
using ledger_vault.ViewModels;

namespace ledger_vault.Services;

public partial class CoreViewNavigatorService : ObservableObject
{
    [ObservableProperty] private CoreViewModel _currentViewModel;
    
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