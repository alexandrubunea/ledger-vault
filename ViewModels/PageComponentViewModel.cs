using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public partial class PageComponentViewModel : ViewModelBase
{
    [ObservableProperty] private PageComponents _pageComponentName;
}