using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ledger_vault.Models;

namespace ledger_vault.Crypto;

public static class TransactionHashing
{
    #region PUBLIC API

    public static string GenerateHash(Transaction transaction)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(GenerateInput(transaction));
        byte[] hashBytes = sha256.ComputeHash(bytes);

        return Convert.ToHexString(hashBytes);
    }

    public static string GenerateHash(string input)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = sha256.ComputeHash(bytes);

        return Convert.ToHexString(hashBytes);
    }

    public static string GenerateFileHash(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return "";

        using SHA256 sha256 = SHA256.Create();
        using FileStream stream = File.OpenRead(filePath);
        byte[] hashBytes = sha256.ComputeHash(stream);

        return Convert.ToHexString(hashBytes);
    }

    public static async Task<bool> VerifyHashAsync(Transaction tx, CancellationToken ct) =>
        tx.Hash.Length > 0 && tx.Hash == await GenerateHashAsync(tx, ct);

    public static async Task<bool> VerifyFileHashAsync(Transaction tx, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tx.ReceiptImage))
            return true;

        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "attachments", tx.ReceiptImage);

        var expectedHash = await GenerateFileHashAsync(filePath, ct);
        return tx.ReceiptImageHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region PRIVATE METHODS

    private static Task<string> GenerateHashAsync(Transaction tx, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();

            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(GenerateInput(tx));
            byte[] hashBytes = sha256.ComputeHash(bytes);
            
            return Convert.ToHexString(hashBytes);
        }, ct);
    }

    private static async Task<string> GenerateFileHashAsync(string filePath, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return "";

        using SHA256 sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        byte[] hashBytes = await sha256.ComputeHashAsync(stream, ct);

        return Convert.ToHexString(hashBytes);
    }

    private static string GenerateInput(Transaction transaction)
    {
        string tags = string.Join(",", transaction.Tags);
        string amountString = transaction.Amount.ToString("0.########", CultureInfo.InvariantCulture);
        string timestampString = transaction.Timestamp.ToString("M/d/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

        string input =
            $"{transaction.Counterparty}{transaction.Description}{amountString}{tags}{transaction.ReceiptImage}" +
            $"{transaction.ReceiptImageHash}{transaction.PreviousHash}{transaction.ReversalOfTransactionId}{timestampString}";

        return input;
    }

    #endregion
}