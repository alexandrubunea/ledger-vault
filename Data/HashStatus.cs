namespace ledger_vault.Data;

public enum HashStatus : byte
{
    InProgress,
    Valid,
    BrokenChain,
    Invalid
}