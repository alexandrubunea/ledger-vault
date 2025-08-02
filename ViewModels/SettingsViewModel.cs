using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ledger_vault.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(WrongCurrentPassword))]
    private string _currentPassword = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(WrongOldPassword))]
    private string _oldPassword = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsPasswordTooWeak))]
    private string _newPassword = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DifferentPasswords))]
    private string _retypePassword = "";

    [ObservableProperty]
    private int _currencyIndex = 0; // TODO: Implement a way to extract this info from the settings table

    [ObservableProperty]
    private int _themeIndex = 0; // TODO: Implement a way to extract this info from the settings table

    [ObservableProperty]
    private string _userCompleteName = "John Doe"; // TODO: Implement a way to extract this info from the settings table

    [GeneratedRegex(@"^(?=.+)(?!(?=.*[A-Z])(?=.*[\d\W]).{8,}).*$")]
    private static partial Regex StrongPasswordRegex();

    public bool WrongOldPassword => CheckPassword(OldPassword);
    public bool WrongCurrentPassword => CheckPassword(CurrentPassword);

    public bool IsPasswordTooWeak => StrongPasswordRegex().IsMatch(NewPassword);

    public bool DifferentPasswords => NewPassword.Length > 0 &&
                                      RetypePassword.Length > 0 &&
                                      NewPassword != RetypePassword;

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
        // Implement this later...
    }

    [RelayCommand]
    public void DeleteData()
    {
        // Implement this later...
    }
}