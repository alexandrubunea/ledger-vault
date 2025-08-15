using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ledger_vault.Crypto;
using ledger_vault.Data;
using ledger_vault.Models;

namespace ledger_vault.Services;

public class TransactionLoader(TransactionRepository transactionRepository, HmacService hmacService)
{
    #region PUBLIC API

    public async Task<List<Transaction>> LoadAndVerifyTransactionsAsync(CancellationToken ct = default)
    {
        var channel = Channel.CreateUnbounded<Transaction>(
            new UnboundedChannelOptions { SingleReader = false, SingleWriter = true });

        var verifiedTransactions = new ConcurrentBag<Transaction>();

        // Workers doing the hash/signature checks
        var workers = Enumerable.Range(0, Environment.ProcessorCount).Select(_ => Task.Run(async () =>
        {
            await foreach (Transaction tx in channel.Reader.ReadAllAsync(ct))
            {
                tx.HashVerifiedStatus = HashStatus.Invalid;
                tx.SignatureVerifiedStatus = SignatureStatus.Invalid;

                if (await TransactionHashing.VerifyHashAsync(tx, ct) &&
                    await TransactionHashing.VerifyFileHashAsync(tx, ct))
                    tx.HashVerifiedStatus = HashStatus.Valid;

                if (hmacService.VerifySignature(tx.GetSigningData(), Convert.FromBase64String(tx.Signature)))
                    tx.SignatureVerifiedStatus = SignatureStatus.Valid;

                verifiedTransactions.Add(tx);
            }
        }, ct)).ToArray();

        // Stream the data from the database direct into workers
        await foreach (Transaction tx in transactionRepository.StreamTransactionsAsync(ct))
        {
            await channel.Writer.WriteAsync(tx, ct);
        }

        channel.Writer.Complete();

        await Task.WhenAll(workers);

        List<Transaction> ordered = verifiedTransactions.OrderBy(t => t.Id).ToList();
        VerifyChain(ordered);

        return ordered;
    }

    #endregion

    #region PRIVATE METHODS

    private void VerifyChain(List<Transaction> transactions)
    {
        // A chain with fewer than two transactions cannot be broken.
        if (transactions.Count < 2)
        {
            return;
        }

        for (int i = 1; i < transactions.Count; i++)
        {
            // Check if the chain is broken, either by an explicit invalid status
            // or a mismatch in the previous hash.
            bool isBroken = transactions[i - 1].HashVerifiedStatus == HashStatus.BrokenChain ||
                            transactions[i - 1].HashVerifiedStatus == HashStatus.Invalid ||
                            transactions[i].PreviousHash != transactions[i - 1].Hash;

            if (isBroken)
            {
                transactions[i].HashVerifiedStatus = HashStatus.BrokenChain;
            }
        }
    }

    #endregion
}