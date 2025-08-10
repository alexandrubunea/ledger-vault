using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public class CashFlowViewModel : PageViewModel
{
    #region PUBLIC API

    public CashFlowViewModel()
    {
        PageName = ApplicationPages.CashFlow;
    }

    #endregion
}