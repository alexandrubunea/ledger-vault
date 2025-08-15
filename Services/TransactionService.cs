using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ledger_vault.Crypto;
using ledger_vault.Data;
using ledger_vault.Models;

namespace ledger_vault.Services;

public class TransactionService(
    TransactionCacheService transactionCacheService,
    TransactionRepository transactionRepository,
    TransactionLoader transactionLoader,
    HmacService hmacService)
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

    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        try
        {
            await RefreshCache();
            return transactionCacheService.GetCachedTransactions();
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting transactions", ex);
        }
    }

    public async Task AddTransaction(Transaction transaction)
    {
        // Wait for any ongoing refresh to complete first
        await _refreshSemaphore.WaitAsync();
        try
        {
            if (!transactionCacheService.IsCacheValid())
            {
                // Cache is invalid, the transaction will be loaded by the next refresh
                return;
            }

            // Check if transaction already exists in cache to prevent duplicates
            var existingTransactions = transactionCacheService.GetCachedTransactions();
            if (existingTransactions.Any(t => t.Id == transaction.Id))
            {
                // Transaction already exists in cache, no need to add
                return;
            }

            Transaction? lastTransaction = existingTransactions.LastOrDefault();

            if (lastTransaction == null)
                throw new NullReferenceException("Last transaction cannot be null");

            transaction.HashVerifiedStatus = HashStatus.Invalid;
            transaction.SignatureVerifiedStatus = SignatureStatus.Invalid;

            if (transaction.PreviousHash == lastTransaction.Hash &&
                lastTransaction.HashVerifiedStatus == HashStatus.Valid &&
                await TransactionHashing.VerifyHashAsync(transaction, CancellationToken.None) &&
                await TransactionHashing.VerifyFileHashAsync(transaction, CancellationToken.None))
                transaction.HashVerifiedStatus = HashStatus.Valid;

            if (hmacService.VerifySignature(transaction.GetSigningData(),
                    Convert.FromBase64String(transaction.Signature)))
                transaction.SignatureVerifiedStatus = SignatureStatus.Valid;

            transactionCacheService.AddTransaction(transaction);
        }
        catch (Exception ex)
        {
            throw new Exception("Error adding transaction", ex);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

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

    private async Task RefreshCache()
    {
        await _refreshSemaphore.WaitAsync();
        try
        {
            if (transactionCacheService.IsCacheValid())
                return;

            transactionCacheService.InvalidateCache();

            List<Transaction> transactions = await transactionLoader.LoadAndVerifyTransactionsAsync();
            transactionCacheService.SetCache(transactions);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    #endregion
}