using System.ComponentModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class SetupViewModel : CoreViewModel
{
    #region PUBLIC API

    public bool EmptyUserName
    {
        get => _emptyUserName;
        private set => SetProperty(ref _emptyUserName, value);
    }

    public bool IsPasswordTooWeak => StrongPasswordRegex().IsMatch(Password);

    public bool DifferentPasswords => Password.Length > 0 &&
                                      ConfirmPassword.Length > 0 &&
                                      Password != ConfirmPassword;

    public SetupViewModel(CoreViewNavigatorService navigator, UserRepository userRepository)
    {
        ViewModelName = CoreViews.Setup;

        _navigator = navigator;
        _userRepository = userRepository;
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public SetupViewModel()
    {
    }
#pragma warning restore

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

        _userRepository.CreateUser(UserCompleteName, _finalPassword, CurrencyIndex);

        StepTwo = false;
        _navigator.NavigateTo(CoreViews.Login);
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly CoreViewNavigatorService _navigator;
    private readonly UserRepository _userRepository;

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

    private bool _emptyUserName;
    private string _finalPassword = "";

    #endregion
}