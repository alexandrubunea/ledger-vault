using System;
using System.Collections.Generic;
using System.Linq;
using ledger_vault.Models;

namespace ledger_vault.Services;

public class TransactionService
{
    private readonly DatabaseManagerService _databaseManagerService;
    private readonly HmacService _hmacService;

    public TransactionService(DatabaseManagerService databaseManagerService, HmacService hmacService)
    {
        _databaseManagerService = databaseManagerService;
        _hmacService = hmacService;
    }

    public Transaction CreateTransaction(string counterparty, string description, decimal amount, List<string> tags,
        string receiptImage, uint? reversalOfTransactionId = null)
    {
        string previousHash = GetLastHash();
        Transaction tx = Transaction.Create(counterparty, description, amount, tags, receiptImage, previousHash,
            reversalOfTransactionId);

        byte[] signature = _hmacService.ComputeSignature(tx.GetSigningData());
        tx.SetSignature(signature);

        SaveTransaction(tx);

        tx = GetLastTransaction();

        return tx;
    }

    private string GetLastHash()
    {
        using var conn = _databaseManagerService.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT hash FROM transactions ORDER BY id DESC LIMIT 1";
        using var reader = cmd.ExecuteReader();

        return reader.Read() ? reader.GetString(0) : "";
    }

    private Transaction GetLastTransaction()
    {
        using var conn = _databaseManagerService.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"SELECT * FROM transactions ORDER BY id DESC LIMIT 1;";
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            throw new Exception("No transaction found");

        uint id = (uint)reader.GetInt32(0);
        string counterparty = reader.GetString(1);
        string description = reader.GetString(2);
        decimal amount = reader.GetDecimal(3);
        string tagsString = reader.GetString(4);
        string receiptImage = reader.GetString(5);
        DateTime timestamp = reader.GetDateTime(6);
        string hash = reader.GetString(7);
        string previousHash = reader.GetString(8);
        string signature = reader.GetString(9);
        uint? reversalOfTransactionId = reader.IsDBNull(10) ? null : (uint)reader.GetInt32(10);

        List<string> tags = tagsString.Split(",").ToList();

        return Transaction.Load(id, counterparty, description, amount, tags, receiptImage, timestamp, hash,
            previousHash, signature, reversalOfTransactionId);
    }

    private void SaveTransaction(Transaction transaction)
    {
        using var conn = _databaseManagerService.GetConnection();
        using var command = conn.CreateCommand();
        string tags = string.Join(",", transaction.Tags);

        try
        {
            command.CommandText = """
                                    INSERT INTO transactions(counterparty, description, amount, tags, receiptimage, 
                                                             datetime, hash, previoushash, signature, reversaloftransactionid)
                                    VALUES(@counterparty, @description, @amount, @tags, @receiptImage, 
                                           @datetime, @hash, @previousHash, @signature, @reversalOfTransactionId);
                                  """;

            command.Parameters.AddWithValue("@counterparty", transaction.Counterparty);
            command.Parameters.AddWithValue("@description", transaction.Description);
            command.Parameters.AddWithValue("@amount", transaction.Amount);
            command.Parameters.AddWithValue("@tags", tags);
            command.Parameters.AddWithValue("@receiptImage", transaction.ReceiptImage);
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
}