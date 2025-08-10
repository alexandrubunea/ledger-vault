using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using ledger_vault.Crypto;
using ledger_vault.Models;

namespace ledger_vault.Services;

public class TransactionService(DatabaseManagerService databaseManagerService, HmacService hmacService)
{
    #region PUBLIC API

    public Transaction CreateTransaction(string counterparty, string description, decimal amount, List<string> tags,
        string receiptImagePath, uint? reversalOfTransactionId = null)
    {
        string previousHash = GetLastHash();
        string receiptPath = HandleReceipt(receiptImagePath);

        Transaction tx = Transaction.Create(counterparty, description, amount, tags, receiptPath,
            previousHash, reversalOfTransactionId);

        byte[] signature = hmacService.ComputeSignature(tx.GetSigningData());
        tx.SetSignature(signature);

        SaveTransaction(tx);

        tx = GetLastTransaction();

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

    private string GetLastHash()
    {
        using var conn = databaseManagerService.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT hash FROM transactions ORDER BY id DESC LIMIT 1";
        using var reader = cmd.ExecuteReader();

        return reader.Read() ? reader.GetString(0) : "";
    }

    #region DATABASE QUERIES

    private Transaction GetLastTransaction()
    {
        using var conn = databaseManagerService.GetConnection();
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
        string receiptImageHash = reader.GetString(6);
        DateTime timestamp = reader.GetDateTime(7);
        string hash = reader.GetString(8);
        string previousHash = reader.GetString(9);
        string signature = reader.GetString(10);
        uint? reversalOfTransactionId = reader.IsDBNull(11) ? null : (uint)reader.GetInt32(11);

        List<string> tags = tagsString.Split(",").ToList();

        return Transaction.Load(id, counterparty, description, amount, tags, receiptImage, receiptImageHash, timestamp,
            hash,
            previousHash, signature, reversalOfTransactionId);
    }

    private void SaveTransaction(Transaction transaction)
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

    #endregion
}