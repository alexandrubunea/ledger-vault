using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class SettingsViewModel : PageViewModel
{
    [ObservableProperty] private string _currentPassword = "";
    [ObservableProperty] private string _oldPassword = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordTooWeak))]
    [NotifyPropertyChangedFor(nameof(DifferentPasswords))]
    private string _newPassword = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DifferentPasswords))]
    private string _retypePassword = "";

    [ObservableProperty] private short _currencyIndex;
    [ObservableProperty] private short _themeIndex;
    [ObservableProperty] private string _userCompleteName;

    [GeneratedRegex(@"^(?=.+)(?!(?=.*[A-Z])(?=.*[\d\W]).{8,}).*$")]
    private static partial Regex StrongPasswordRegex();

    private readonly UserStateService _userStateService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private bool _wrongOldPassword;
    [ObservableProperty]
    private bool _wrongCurrentPassword;

    public bool IsPasswordTooWeak => StrongPasswordRegex().IsMatch(NewPassword);
    public bool DifferentPasswords => NewPassword.Length > 0 &&
                                      RetypePassword.Length > 0 &&
                                      NewPassword != RetypePassword;

    public SettingsViewModel(UserStateService userStateService, AuthService authService)
    {
        PageName = ApplicationPages.Settings;
        _userStateService = userStateService;
        _authService = authService;

        UserCompleteName = _userStateService.FullUserName;
        CurrencyIndex = _userStateService.CurrencyId;
        ThemeIndex = _userStateService.ThemeId;
    }

    [RelayCommand]
    public void ChangePassword()
    {
        // Check old password
        WrongOldPassword = false;
        if (!_authService.CheckUserPassword(OldPassword))
        {
            WrongOldPassword = true;
            return;
        }

        // Check if the new password is valid
        if (IsPasswordTooWeak || NewPassword.Length == 0)
            return;

        // Check if the retyped password is the same with the new password
        if (DifferentPasswords)
            return;

        // Update the password
        _authService.UpdateUserPassword(OldPassword, NewPassword);

        // Reset data
        OldPassword = "";
        NewPassword = "";
        RetypePassword = "";
    }

    [RelayCommand]
    public void UpdatePreferences()
    {
        _userStateService.FullUserName = UserCompleteName;
        _userStateService.CurrencyId = CurrencyIndex;
        _userStateService.ThemeId = ThemeIndex;

        _userStateService.SaveUserState();
    }

    [RelayCommand]
    public void DeleteData()
    {
        WrongCurrentPassword = false;
        if (!_authService.CheckUserPassword(CurrentPassword))
        {
            WrongCurrentPassword = true;
            return;
        }

        _authService.DeleteAccount();
    }
}