using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class SetupViewModel : CoreViewModel
{
    [ObservableProperty] private bool _stepOne = true;
    [ObservableProperty] private bool _stepTwo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordTooWeak))]
    [NotifyPropertyChangedFor(nameof(DifferentPasswords))]
    private string _password = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DifferentPasswords))]
    private string _confirmPassword = "";

    [ObservableProperty] private string _userCompleteName = "";

    [ObservableProperty] private ushort _currencyIndex;

    [GeneratedRegex(@"^(?=.+)(?!(?=.*[A-Z])(?=.*[\d\W]).{8,}).*$")]
    private static partial Regex StrongPasswordRegex();

    public bool IsPasswordTooWeak => StrongPasswordRegex().IsMatch(Password);

    public bool DifferentPasswords => Password.Length > 0 &&
                                      ConfirmPassword.Length > 0 &&
                                      Password != ConfirmPassword;

    private bool _emptyUserName;
    public bool EmptyUserName
    {
        get => _emptyUserName;
        private set => SetProperty(ref _emptyUserName, value);
    }

    private string _finalPassword = "";

    private readonly CoreViewNavigatorService _navigator;
    private readonly DatabaseManagerService _dbManager;

    public SetupViewModel(CoreViewNavigatorService navigator, DatabaseManagerService dbManager)
    {
        ViewModelName = CoreViews.Setup;

        _navigator = navigator;
        _dbManager = dbManager;
    }

    public void NextStep()
    {
        if (Password.Length == 0 || IsPasswordTooWeak || DifferentPasswords)
            return;

        _finalPassword = Password;
        
        StepOne = false;
        StepTwo = true;
    }

    public void Save()
    {
        if (UserCompleteName.Length == 0)
        {
            EmptyUserName = true;
            return;
        }
        
        _dbManager.CreateUser(UserCompleteName, _finalPassword, CurrencyIndex);

        StepTwo = false;
        _navigator.NavigateTo(CoreViews.Login);
    }
}