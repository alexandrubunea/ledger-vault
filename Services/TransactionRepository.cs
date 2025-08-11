using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ledger_vault.Models;
using Microsoft.Data.Sqlite;

namespace ledger_vault.Services;

public class TransactionRepository(DatabaseManagerService databaseManagerService)
{
    #region PUBLIC API

    public string GetLastHash()
    {
        using var conn = databaseManagerService.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT hash FROM transactions ORDER BY id DESC LIMIT 1";
        using var reader = cmd.ExecuteReader();

        return reader.Read() ? reader.GetString(0) : "";
    }

    public async IAsyncEnumerable<Transaction> StreamTransactionsAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var conn = databaseManagerService.GetConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT * FROM transactions ORDER BY id DESC;";
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            Transaction? tx = ExtractTransactionFromReader(reader);

            if (tx == null)
                throw new Exception("Could not read transaction");

            yield return tx;
        }
    }

    public Transaction GetLastTransaction()
    {
        using var conn = databaseManagerService.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"SELECT * FROM transactions ORDER BY id DESC LIMIT 1;";
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            throw new Exception("No transaction found");

        Transaction? tx = ExtractTransactionFromReader(reader);

        if (tx == null)
            throw new Exception("Could not read transaction");

        return tx;
    }

    public void SaveTransaction(Transaction transaction)
    {
        using var conn = databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();
        string tags = string.Join(",", transaction.Tags);

        try
        {
            command.CommandText = """
                                    INSERT INTO transactions(counterparty, description, amount, tags, receiptimage, receiptimagehash,
                                                             datetime, hash, previoushash, signature, reversaloftransactionid)
                                    VALUES(@counterparty, @description, @amount, @tags, @receiptImage, @receiptImageHash,
                                           @datetime, @hash, @previousHash, @signature, @reversalOfTransactionId);
                                  """;

            command.Parameters.AddWithValue("@counterparty", transaction.Counterparty);
            command.Parameters.AddWithValue("@description", transaction.Description);
            command.Parameters.AddWithValue("@amount", transaction.Amount);
            command.Parameters.AddWithValue("@tags", tags);
            command.Parameters.AddWithValue("@receiptImage", transaction.ReceiptImage);
            command.Parameters.AddWithValue("@receiptImageHash", transaction.ReceiptImageHash);
            command.Parameters.AddWithValue("@datetime", transaction.Timestamp);
            command.Parameters.AddWithValue("@hash", transaction.Hash);
            command.Parameters.AddWithValue("@previousHash", transaction.PreviousHash);
            command.Parameters.AddWithValue("@signature", transaction.Signature);
            command.Parameters.AddWithValue("@reversalOfTransactionId",
                transaction.ReversalOfTransactionId.HasValue
                    ? transaction.ReversalOfTransactionId.Value
                    : DBNull.Value);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            string message = $"""
                              Error saving transaction:
                              Counterparty: {transaction.Counterparty}
                              Description: {transaction.Description}
                              Amount: {transaction.Amount}
                              Tags: {tags}
                              Receipt Image: {transaction.ReceiptImage}
                              Receipt ImageHash: {transaction.ReceiptImageHash}
                              DateTime: {transaction.Timestamp}
                              Hash: {transaction.Hash}
                              Previous Hash: {transaction.PreviousHash}
                              Signature: {transaction.Signature}
                              ReversalOfTransactionId: {transaction.ReversalOfTransactionId}
                              Exception: {ex.Message}
                              """;
            throw new Exception(message);
        }
    }

    #endregion

    #region PRIVATE METHODS

    // I don't think this should be used with any other database
    // If this is the case, maybe implement the same function for different readers
    private Transaction? ExtractTransactionFromReader(SqliteDataReader reader)
    {
        try
        {
            uint id = (uint)reader.GetInt32(0);
            string counterparty = reader.GetString(1);
            string description = reader.GetString(2);
            decimal amount = reader.GetDecimal(3);
            string tagsString = reader.GetString(4);
            string receiptImage = reader.GetString(5);
            string receiptImageHash = reader.GetString(6);
            DateTime timestamp = reader.GetDateTime(7);
            string hash = reader.GetString(8);
            string previousHash = reader.GetString(9);
            string signature = reader.GetString(10);
            uint? reversalOfTransactionId = reader.IsDBNull(11) ? null : (uint)reader.GetInt32(11);

            List<string> tags = tagsString.Split(",").ToList();

            return Transaction.Load(id, counterparty, description, amount, tags, receiptImage, receiptImageHash,
                timestamp, previousHash, hash, signature, reversalOfTransactionId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading transaction: {ex.Message}");
        }

        return null;
    }

    #endregion
}