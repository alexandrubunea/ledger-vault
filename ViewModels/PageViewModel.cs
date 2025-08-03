using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty] private ApplicationPages _pageName;
}