using System;
using CommunityToolkit.Mvvm.Input;

namespace ledger_vault.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly Action _onLoginSuccess;

    public LoginViewModel(Action onLoginSuccess)
    {
        _onLoginSuccess = onLoginSuccess;
    }
    
    [RelayCommand]
    private void ProcessLogin()
    {
        _onLoginSuccess.Invoke();
    }
}