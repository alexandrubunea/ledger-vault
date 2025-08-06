using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ledger_vault.Models;

namespace ledger_vault.Crypto;

public static class TransactionHashing
{
    public static string GenerateHash(Transaction transaction)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(GenerateInput(transaction));
        byte[] hashBytes = sha256.ComputeHash(bytes);

        return BitConverter.ToString(hashBytes).Replace("-", "");
    }

    public static bool VerifyTransactions(List<Transaction> transactions)
    {
        for (int i = 1; i < transactions.Count; i++)
        {
            Transaction currentTx = transactions[i];
            Transaction previousTx = transactions[i - 1];

            if (!VerifyChainBetweenTransactions(currentTx, previousTx))
                return false;
        }

        return true;
    }

    public static bool VerifyRecentTransactions(List<Transaction> transactions, int windowSize = 100)
    {
        int start = Math.Max(1, transactions.Count - windowSize);
        for (int i = start; i < transactions.Count; i++)
        {
            Transaction currentTx = transactions[i];
            Transaction previousTx = transactions[i - 1];

            if (!VerifyChainBetweenTransactions(currentTx, previousTx))
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
            $"{transaction.Description}{transaction.Amount}{tags}{transaction.ReceiptImage}" +
            $"{transaction.PreviousHash}{transaction.ReversalOfTransactionId}{transaction.Timestamp}";

        return input;
    }
}