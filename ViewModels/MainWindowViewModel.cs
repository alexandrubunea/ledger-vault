using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] CoreViewNavigatorService _navigator;
    
    public MainWindowViewModel(CoreViewNavigatorService navigatorService)
    {
        _navigator = navigatorService;
        
        navigatorService.NavigateTo(CoreViews.Setup);
    }
}