using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ledger_vault.ViewModels;

public partial class SetupViewModel : ViewModelBase
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

    public void NextStep()
    {
        StepOne = false;
        StepTwo = true;
    }

    public void Save()
    {
        StepTwo = false;
    }
}