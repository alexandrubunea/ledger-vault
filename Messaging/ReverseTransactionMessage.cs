using ledger_vault.Models;

namespace ledger_vault.Messaging;

public class ReverseTransactionMessage
{
    public Transaction? Transaction { get; set; }
}