namespace ledger_vault.Data;

public enum LoginResult : byte
{
    Success,
    WrongPassword,
    DatabaseError,
    NoUser,
}