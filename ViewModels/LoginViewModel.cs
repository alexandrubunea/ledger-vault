using System;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class LoginViewModel : CoreViewModel
{
    private readonly CoreViewNavigatorService _navigator;
    public LoginViewModel(CoreViewNavigatorService navigator)
    {
        ViewModelName = CoreViews.Login;
        
        _navigator = navigator;
    }

    public void ProcessLoginCommand()
    {
        _navigator.NavigateTo(CoreViews.Main);
    }
}