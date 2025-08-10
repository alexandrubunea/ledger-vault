using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public class TransactionsListViewModel : PageComponentViewModel
{
    #region PUBLIC API

    public TransactionsListViewModel()
    {
        PageComponentName = PageComponents.TransactionList;
    }

    #endregion
}