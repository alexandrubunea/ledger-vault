namespace ledger_vault.Data;

public enum ApplicationPages : byte
{
    Home,
    Transaction,
    Settings,
    Export,
    Backups,
    
    // Not used in factory, just for convenience
    Income,
    Payments
}