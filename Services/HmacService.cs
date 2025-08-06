using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace ledger_vault.Services;

public class HmacService
{
    private readonly IDataProtector _protector;
    private byte[] _hmacKey = [];

    private const string Purpose = "LedgerVault.TransactionSigning";

    private readonly string _keyFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LedgerVault",
        "hmac_key.protected");

    public HmacService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(Purpose);
        LoadOrGenerateKey();
    }

    private void LoadOrGenerateKey()
    {
        byte[] protectedData;

        if (File.Exists(_keyFilePath))
        {
            protectedData = File.ReadAllBytes(_keyFilePath);
            _hmacKey = _protector.Unprotect(protectedData);
            return;
        }

        _hmacKey = RandomNumberGenerator.GetBytes(32);
        protectedData = _protector.Protect(_hmacKey);

        Directory.CreateDirectory(Path.GetDirectoryName(_keyFilePath)!);
        File.WriteAllBytes(_keyFilePath, protectedData);
    }

    public byte[] ComputeSignature(byte[] data)
    {
        using var hmac = new HMACSHA256(_hmacKey);
        return hmac.ComputeHash(data);
    }

    public bool VerifySignature(byte[] data, byte[] signature)
    {
        var computed = ComputeSignature(data);
        return CryptographicOperations.FixedTimeEquals(computed, signature);
    }
}