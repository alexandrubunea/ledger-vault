namespace ledger_vault.Data;

public enum ApplicationPages : byte
{
    Home,
    Transaction,
    CashFlow,
    Settings,
    Export,
    Backups,
    
    // Not used in factory, just for convenience
    Income,
    Payments
}