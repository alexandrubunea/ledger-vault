using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class LoginViewModel : CoreViewModel
{
    #region PUBLIC API

    public LoginViewModel(CoreViewNavigatorService navigator, AuthService authService)
    {
        ViewModelName = CoreViews.Login;

        _password = "";

        _navigator = navigator;
        _authService = authService;
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public LoginViewModel()
    {
    }
#pragma warning restore

    #endregion

    #region PRIVATE PROPERTIES

    private readonly CoreViewNavigatorService _navigator;
    private readonly AuthService _authService;

    [ObservableProperty] private bool _wrongPassword;
    [ObservableProperty] private bool _databaseError;
    [ObservableProperty] private string _password;

    #endregion

    #region PRIVATE METHODS

    [RelayCommand]
    private void ProcessLogin()
    {
        _ = Task.Run(async () =>
        {
            WrongPassword = false;
            DatabaseError = false;

            LoginResult result = await Task.Run(() => _authService.Login(Password));

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                switch (result)
                {
                    case LoginResult.WrongPassword:
                        WrongPassword = true;
                        break;
                    case LoginResult.DatabaseError:
                        DatabaseError = true;
                        break;
                    case LoginResult.NoUser:
                        _navigator.NavigateTo(CoreViews.Setup);
                        break;
                    case LoginResult.Success:
                        _navigator.NavigateTo(CoreViews.Main);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        });

    }

    #endregion
}
