using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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

        return BitConverter.ToString(hashBytes).Replace("-", "");
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
        using SHA256 sha256 = SHA256.Create();
        using FileStream stream = File.OpenRead(filePath);
        byte[] hashBytes = sha256.ComputeHash(stream);

        return Convert.ToHexString(hashBytes);
    }

    public static bool VerifyTransactions(List<Transaction> transactions)
    {
        return IterateTransactions(transactions, 1, transactions.Count);
    }

    public static bool VerifyRecentTransactions(List<Transaction> transactions, int windowSize = 100)
    {
        int start = Math.Max(1, transactions.Count - windowSize);

        return IterateTransactions(transactions, start, transactions.Count);
    }

    #endregion

    #region PRIVATE METHODS

    private static bool VerifyImageHash(Transaction tx) =>
        tx.ReceiptImageHash.Length > 0 && tx.ReceiptImageHash == GenerateHash(tx.ReceiptImage);

    private static bool IterateTransactions(List<Transaction> transactions, int start, int stop)
    {
        if (start < 1 || stop > transactions.Count)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (!VerifyImageHash(transactions[start - 1]))
            return false;

        for (int i = start; i < stop; i++)
        {
            Transaction currentTx = transactions[i];
            Transaction previousTx = transactions[i - 1];

            if (!VerifyChainBetweenTransactions(currentTx, previousTx))
                return false;

            if (!VerifyImageHash(currentTx))
                return false;
        }

        return true;
    }

    private static bool VerifyChainBetweenTransactions(Transaction transactionA, Transaction transactionB)
    {
        if (transactionA.PreviousHash != transactionB.Hash)
            return false;

        return GenerateHash(transactionA) == transactionA.Hash;
    }

    private static string GenerateInput(Transaction transaction)
    {
        string tags = string.Join(",", transaction.Tags);
        string input =
            $"{transaction.Counterparty}{transaction.Description}{transaction.Amount}{tags}{transaction.ReceiptImage}" +
            $"{transaction.ReceiptImageHash}{transaction.PreviousHash}{transaction.ReversalOfTransactionId}{transaction.Timestamp}";

        return input;
    }

    #endregion
}