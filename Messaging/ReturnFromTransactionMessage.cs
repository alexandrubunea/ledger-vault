namespace ledger_vault.Messaging;

public class ReturnFromTransactionMessage
{
    public bool TransactionConfirmed { get; set; }
    public decimal TransactionAmount { get; set; }
    
    // TODO: Maybe return the whole transaction and send it further to the list of transactions?
}