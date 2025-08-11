using ledger_vault.Models;

namespace ledger_vault.Messaging;

public class ReturnFromTransactionFormMessage
{
    #region PUBLIC PROPERTIES

    public bool TransactionConfirmed { get; init; }
    public decimal TransactionAmount { get; init; }
    public Transaction? Transaction { get; init; }

    #endregion
}