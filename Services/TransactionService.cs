using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using ledger_vault.Crypto;
using ledger_vault.Models;

namespace ledger_vault.Services;

public class TransactionService(TransactionRepository transactionRepository, HmacService hmacService)
{
    #region PUBLIC API

    public Transaction CreateTransaction(string counterparty, string description, decimal amount, List<string> tags,
        string receiptImagePath, uint? reversalOfTransactionId = null)
    {
        string previousHash = transactionRepository.GetLastHash();
        string receiptPath = HandleReceipt(receiptImagePath);

        Transaction tx = Transaction.Create(counterparty, description, amount, tags, receiptPath,
            previousHash, reversalOfTransactionId);

        byte[] signature = hmacService.ComputeSignature(tx.GetSigningData());
        tx.SetSignature(signature);

        transactionRepository.SaveTransaction(tx);

        tx = transactionRepository.GetLastTransaction();

        return tx;
    }

    #endregion

    #region PRIVATE METHODS

    private string HandleReceipt(string path)
    {
        if (path.Length == 0 || !File.Exists(path))
            return "";

        string name = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);
        string randomness = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        string newName = TransactionHashing.GenerateHash(name + randomness) + extension;

        string attachmentsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LedgerVault", "attachments");
        if (!Directory.Exists(attachmentsFolder))
            Directory.CreateDirectory(attachmentsFolder);

        string newPath = Path.Combine(attachmentsFolder, newName);

        File.Copy(path, newPath);

        return newPath;
    }

    #endregion
}