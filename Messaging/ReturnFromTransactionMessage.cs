namespace ledger_vault.Messaging;

public class ReturnFromTransactionMessage
{
    #region PUBLIC PROPERTIES

    public bool TransactionConfirmed { get; init; }
    public decimal TransactionAmount { get; init; }

    #endregion

    // TODO: Maybe return the whole transaction and send it further to the list of transactions?
}