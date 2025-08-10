using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    #region PRIVATE PROPERTIES

    [ObservableProperty] private ApplicationPages _pageName;

    #endregion
}