using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public partial class ChartViewModel : ViewModelBase
{
    #region PRIVATE PROPERTIES

    [ObservableProperty] private ChartType _chartType;

    #endregion
}