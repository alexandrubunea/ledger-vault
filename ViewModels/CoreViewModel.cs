using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public partial class CoreViewModel : ViewModelBase
{
    [ObservableProperty] private CoreViews _viewModelName;
}