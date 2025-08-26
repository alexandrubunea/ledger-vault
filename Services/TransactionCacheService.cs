using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ledger_vault.Models;

namespace ledger_vault.Services;

// Maybe will be useful in the future, at the moment is just a list with a lifespan
public class TransactionCacheService(TransactionLoader transactionLoader)
{
    #region PUBLIC API

    public bool IsCacheValid()
    {
        lock (_lock)
        {
            return _cachedTransactions.Count > 0 && (DateTime.UtcNow - _lastLoadedTime) < _cacheDuration;
        }
    }

    public List<Transaction> GetCachedTransactions()
    {
        lock (_lock)
        {
            return !IsCacheValid() ? [] : _cachedTransactions;
        }
    }

    public void SetCache(List<Transaction> transactions)
    {
        lock (_lock)
        {
            _cachedTransactions = transactions;
            _lastLoadedTime = DateTime.UtcNow;
        }
    }

    public void InvalidateCache()
    {
        lock (_lock)
        {
            _cachedTransactions.Clear();
        }
    }

    public void AddTransaction(Transaction transaction)
    {
        lock (_lock)
        {
            if (!IsCacheValid())
                return;

            _cachedTransactions.Add(transaction);
        }
    }

    public async Task RefreshCache()
    {
        await _refreshSemaphore.WaitAsync();
        try
        {
            if (IsCacheValid())
                return;

            InvalidateCache();

            List<Transaction> transactions = await transactionLoader.LoadAndVerifyTransactionsAsync();
            SetCache(transactions);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    public async Task RunCacheSensitiveAction(Func<Task> action)
    {
        await _refreshSemaphore.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly Lock _lock = new();
    private List<Transaction> _cachedTransactions = [];
    private DateTime _lastLoadedTime;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);


    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(2);

    #endregion
}