using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ledger_vault.Crypto;
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
                tx.HashVerified = await TransactionHashing.VerifyHashAsync(tx, ct) &&
                                  await TransactionHashing.VerifyFileHashAsync(tx, ct);

                tx.SignatureVerified =
                    hmacService.VerifySignature(tx.GetSigningData(), Convert.FromBase64String(tx.Signature));

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

        List<Transaction> ordered = verifiedTransactions.OrderByDescending(t => t.Id).ToList();
        VerifyChain(ordered);

        return ordered;
    }

    #endregion

    #region PRIVATE METHODS

    private void VerifyChain(List<Transaction> transactions)
    {
        bool brokenChain = false;

        for (int i = 1; i < transactions.Count; i++)
        {
            if (brokenChain)
            {
                transactions[i].HashVerified = false;
                continue;
            }

            if (!(transactions[i - 1].HashVerified && transactions[i].PreviousHash == transactions[i - 1].Hash))
                brokenChain = true;
        }
    }

    #endregion
}