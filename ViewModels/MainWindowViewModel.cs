using CommunityToolkit.Mvvm.ComponentModel;

namespace ledger_vault.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] 
    private ViewModelBase _currentViewModel;
    
    private readonly MainViewModel _mainViewModel;

    public MainWindowViewModel(MainViewModel mainViewModel, SetupViewModel setupViewModel)
    {
        _mainViewModel = mainViewModel;
        
        // TODO: Verify if the application is set-up
        CurrentViewModel = setupViewModel;
    }

    private void OnLoginSuccess()
    {
        CurrentViewModel = _mainViewModel;
    }
}