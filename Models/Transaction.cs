using System;
using System.Collections.Generic;
using ledger_vault.Crypto;
using Tmds.DBus.Protocol;

namespace ledger_vault.Models;

public class Transaction
{
    public uint Id { get; private set; }
    public string Counterparty { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public List<string> Tags { get; private set; }
    public string ReceiptImage { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Hash { get; private set; }
    public string PreviousHash { get; private set; }
    public string Signature { get; private set; }
    public uint? ReversalOfTransactionId { get; private set; }


    // Constructor for non-existing transaction in the database
    private Transaction(string counterparty, string description, decimal amount, List<string> tags, string receiptImage,
        string previousHash, uint? reversalOfTransactionId = null)
    {
        Counterparty = counterparty;
        Description = description;
        Amount = amount;
        Tags = tags;
        ReceiptImage = receiptImage;
        PreviousHash = previousHash;
        ReversalOfTransactionId = reversalOfTransactionId;
        Timestamp = DateTime.Now;
        Hash = "";
        Signature = "";
    }

    // Constructor for existing transaction in the database
    private Transaction(uint id, string counterparty, string description, decimal amount, List<string> tags,
        string receiptImage, string previousHash, DateTime timestamp, string hash, string signature,
        uint? reversalOfTransactionId = null)
    {
        Id = id;
        Counterparty = counterparty;
        Description = description;
        Amount = amount;
        Tags = tags;
        ReceiptImage = receiptImage;
        PreviousHash = previousHash;
        ReversalOfTransactionId = reversalOfTransactionId;
        Timestamp = timestamp;
        Signature = signature;
        Hash = hash;
    }

    public static Transaction Create(string counterparty, string description, decimal amount, List<string> tags,
        string receiptImage, string previousHash, uint? reversalOfTransactionId = null)
    {
        Transaction tx =
            new Transaction(counterparty, description, amount, tags, receiptImage, previousHash,
                reversalOfTransactionId);

        tx.Hash = TransactionHashing.GenerateHash(tx);

        return tx;
    }

    public static Transaction Load(uint id, string counterparty, string description, decimal amount, List<string> tags,
        string receiptImage, DateTime timestamp, string previousHash, string hash, string signature,
        uint? reversalOfTransactionId = null)
    {
        Transaction tx = new Transaction(id, counterparty, description, amount, tags, receiptImage, previousHash,
            timestamp, hash, signature, reversalOfTransactionId);

        return tx;
    }

    public byte[] GetSigningData()
    {
        string tags = string.Join(", ", Tags);
        string data = $"{Id}{Counterparty}{Description}{Amount}{Timestamp}{tags}";

        return System.Text.Encoding.UTF8.GetBytes(data);
    }

    public void SetSignature(byte[] signature)
    {
        if (Signature != "")
            return;

        Signature = Convert.ToBase64String(signature);
    }
}