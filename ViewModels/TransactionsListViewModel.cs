using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public class TransactionsListViewModel : PageComponentViewModel
{
    public TransactionsListViewModel()
    {
        PageComponentName = PageComponents.TransactionList;
    }
}