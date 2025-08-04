using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class LoginViewModel : CoreViewModel
{
    private readonly CoreViewNavigatorService _navigator;
    private readonly AuthService _authService;

    [ObservableProperty] private bool _wrongPassword;
    [ObservableProperty] private bool _databaseError;
    [ObservableProperty] private string _password;

    public LoginViewModel(CoreViewNavigatorService navigator, AuthService authService)
    {
        ViewModelName = CoreViews.Login;

        Password = "";

        _navigator = navigator;
        _authService = authService;
    }

    [RelayCommand]
    public void ProcessLoginCommand()
    {
        WrongPassword = false;
        DatabaseError = false;
        
        LoginResult result = _authService.Login(Password);

        switch (result)
        {
            case LoginResult.WrongPassword:
                WrongPassword = true;
                break;
            case LoginResult.DatabaseError:
                DatabaseError = true;
                break;
            // If there is NoUser, and the user somehow end up in the login view send him back to the setup view
            case LoginResult.NoUser:
                _navigator.NavigateTo(CoreViews.Setup);
                break;
            case LoginResult.Success:
                _navigator.NavigateTo(CoreViews.Main);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}