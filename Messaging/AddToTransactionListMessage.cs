using ledger_vault.Models;

namespace ledger_vault.Messaging;

public class AddToTransactionListMessage
{
    #region PUBLIC PROPERTIES

    public Transaction? Transaction { get; set; }

    #endregion
}