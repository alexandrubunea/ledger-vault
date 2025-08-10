using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public class TransactionsListViewModel : PageComponentViewModel
{
    #region PUBLIC PROPERTIES

    public TransactionType TransactionType { get; set; }

    #endregion

    #region PUBLIC API

    public TransactionsListViewModel()
    {
        PageComponentName = PageComponents.TransactionList;
    }

    #endregion
}