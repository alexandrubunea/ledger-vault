using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ledger_vault.Crypto;
using ledger_vault.Data;
using ledger_vault.Handlers;
using ledger_vault.Models;

namespace ledger_vault.Services;

public class TransactionService(
    TransactionCacheService transactionCacheService,
    TransactionRepository transactionRepository,
    HmacService hmacService)
{
    #region PUBLIC API

    public Transaction CreateTransaction(string counterparty, string description, decimal amount, List<string> tags,
        string receiptImagePath, uint? reversalOfTransactionId = null)
    {
        string previousHash = transactionRepository.GetLastHash();
        string receiptPath = FileHandler.HandleReceipt(receiptImagePath);

        Transaction tx = new Transaction()
            .SetCounterparty(counterparty)
            .SetDescription(description)
            .SetAmount(amount)
            .SetTags(tags)
            .SetPreviousHash(previousHash)
            .SetReceiptImage(Path.GetFileName(receiptPath))
            .SetReversalOfTransactionId(reversalOfTransactionId)
            .SetTimestamp(DateTime.Now);

        string receiptImageHash =
            receiptImagePath.Length > 0 ? TransactionHashing.GenerateFileHash(receiptImagePath) : String.Empty;
        tx.SetReceiptImageHash(receiptImageHash);

        byte[] signature = hmacService.ComputeSignature(tx.GetSigningData());
        tx.SetSignature(signature);

        tx.SetHash(TransactionHashing.GenerateHash(tx));

        transactionRepository.SaveTransaction(tx);

        tx = transactionRepository.GetLastTransaction();
        if (tx.ReversalOfTransactionId == null) return tx;

        transactionRepository.AddReverseRelation(tx);
        return tx;
    }

    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        try
        {
            await transactionCacheService.RefreshCache();
            return transactionCacheService.GetCachedTransactions();
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting transactions", ex);
        }
    }

    public async Task AddTransaction(Transaction transaction)
    {
        await transactionCacheService.RunCacheSensitiveAction(() => AddTransactionAsync(transaction));
    }

    #endregion

    #region PRIVATE METHODS

    private async Task AddTransactionAsync(Transaction transaction)
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
            await TransactionHashing.VerifyHashAsync(transaction, CancellationToken.None) &&
            await TransactionHashing.VerifyFileHashAsync(transaction, CancellationToken.None))
            transaction.HashVerifiedStatus = HashStatus.Valid;

        if (lastTransaction.HashVerifiedStatus != HashStatus.Valid)
            transaction.HashVerifiedStatus = HashStatus.BrokenChain;

        if (hmacService.VerifySignature(transaction.GetSigningData(),
                Convert.FromBase64String(transaction.Signature)))
            transaction.SignatureVerifiedStatus = SignatureStatus.Valid;

        if (!transaction.ReversalOfTransactionId.HasValue)
        {
            transactionCacheService.AddTransaction(transaction);
            return;
        }

        Transaction txToSearch = new Transaction().SetId((uint)transaction.ReversalOfTransactionId);

        // Maybe in the future use a Map for O(1), but consider the fact that is more memory consuming
        // and the Transaction object is pretty big already, balance the performance
        int index = transactionCacheService.GetCachedTransactions().BinarySearch(txToSearch);

        if (index >= 0)
            transactionCacheService.GetCachedTransactions()[index].SetIsReverted(true);

        transactionCacheService.AddTransaction(transaction);
    }

    #endregion
}