using CommunityToolkit.Mvvm.ComponentModel;

namespace ledger_vault.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] 
    private ViewModelBase _currentViewModel;

    public MainWindowViewModel()
    {
        CurrentViewModel = new LoginViewModel(OnLoginSuccess);
    }

    private void OnLoginSuccess()
    {
        CurrentViewModel = new HomeViewModel();
    }
}