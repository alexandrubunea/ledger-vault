using ledger_vault.Data;

namespace ledger_vault.Messaging;

// This fixes a bug in the transaction page
// We need to message in which page we are after pressing the "reverse transaction" button
// Because if it was a payment we need to create an income transaction, otherwise we will create
// a payment transaction. So the pages in the sidebar need to be updated.
public class UpdateSidebarMessage
{
    public TransactionType TransactionType { get; set; }
}