using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public partial class PageComponentViewModel : ViewModelBase
{
    #region PRIVATE PROPERTIES

    [ObservableProperty] private PageComponents _pageComponentName;
    [ObservableProperty] private TransactionType _currentTransactionType;

    #endregion
}