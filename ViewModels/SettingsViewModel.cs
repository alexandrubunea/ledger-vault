using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class SettingsViewModel : PageViewModel
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(WrongCurrentPassword))]
    private string _currentPassword = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(WrongOldPassword))]
    private string _oldPassword = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordTooWeak))]
    [NotifyPropertyChangedFor(nameof(DifferentPasswords))]
    private string _newPassword = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DifferentPasswords))]
    private string _retypePassword = "";

    [ObservableProperty] private ushort _currencyIndex;

    [ObservableProperty] private ushort _themeIndex;

    [ObservableProperty] private string _userCompleteName;

    [GeneratedRegex(@"^(?=.+)(?!(?=.*[A-Z])(?=.*[\d\W]).{8,}).*$")]
    private static partial Regex StrongPasswordRegex();

    private readonly UserStateService _userStateService;

    public bool WrongOldPassword => CheckPassword(OldPassword);
    public bool WrongCurrentPassword => CheckPassword(CurrentPassword);

    public bool IsPasswordTooWeak => StrongPasswordRegex().IsMatch(NewPassword);

    public bool DifferentPasswords => NewPassword.Length > 0 &&
                                      RetypePassword.Length > 0 &&
                                      NewPassword != RetypePassword;

    public SettingsViewModel(UserStateService userStateService)
    {
        PageName = ApplicationPages.Settings;
        _userStateService = userStateService;

        UserCompleteName = _userStateService.FullUserName;
        CurrencyIndex = _userStateService.CurrencyId;
        ThemeIndex = _userStateService.ThemeId;
    }

    private bool CheckPassword(string password)
    {
        // Check if the user have the right password

        return false;
    }

    [RelayCommand]
    public void ChangePassword()
    {
        // Check old password

        // Check if the new password is valid
        if (IsPasswordTooWeak || NewPassword.Length == 0)
            return;

        // Check if the retyped password is the same with the new password
        if (DifferentPasswords)
            return;

        // Update the password

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
    }

    [RelayCommand]
    public void DeleteData()
    {
        // Implement this later...
    }
}