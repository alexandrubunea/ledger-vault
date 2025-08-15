using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ledger_vault.Crypto;
using ledger_vault.Data;

namespace ledger_vault.Models;

public class Transaction : IComparable<Transaction>
{
    #region PUBLIC PROPERTIES

    public uint Id { get; private set; }
    public string Counterparty { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public List<string> Tags { get; private set; }
    public string ReceiptImage { get; private set; }
    public string ReceiptImageHash { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Hash { get; private set; }
    public string PreviousHash { get; private set; }
    public string Signature { get; private set; }
    public uint? ReversalOfTransactionId { get; private set; }
    public bool IsReverted { get; private set; }

    // Not database related properties
    public HashStatus HashVerifiedStatus { get; set; } = HashStatus.InProgress;
    public SignatureStatus SignatureVerifiedStatus { get; set; } = SignatureStatus.InProgress;

    #endregion

    #region PUBLIC API
    
    /// <summary>
    /// Creates a dummy transaction.
    /// Example when needed: trying to binary search an id in a ordered list of transactions.
    /// USE WITH CAUTION.
    /// </summary>
    /// <param name="id">id of the transaction, should be an already existing id</param>
    public Transaction(uint id)
    {
        Id = id;

        Counterparty = string.Empty;
        Description = string.Empty;
        Tags = [];
        ReceiptImage = string.Empty;
        ReceiptImageHash = string.Empty;
        Hash = string.Empty;
        PreviousHash = string.Empty;
        Signature = string.Empty;
    }
    
    public static Transaction Create(string counterparty, string description, decimal amount, List<string> tags,
        string receiptImagePath, string previousHash, uint? reversalOfTransactionId = null)
    {
        string receiptImageHash =
            receiptImagePath.Length > 0 ? TransactionHashing.GenerateFileHash(receiptImagePath) : "";

        // File folder its known to be in the special folder -> attachments
        // There is no need to store the whole path in the db
        string receiptImage = Path.GetFileName(receiptImagePath);

        Transaction tx =
            new Transaction(counterparty, description, amount, tags, receiptImage, previousHash,
                reversalOfTransactionId);

        tx.ReceiptImageHash = receiptImageHash;
        tx.Hash = TransactionHashing.GenerateHash(tx);

        return tx;
    }

    public static Transaction Load(uint id, string counterparty, string description, decimal amount, List<string> tags,
        string receiptImage, string receiptImageHash, DateTime timestamp, string previousHash, string hash,
        string signature, bool isReverted, uint? reversalOfTransactionId = null)
    {
        Transaction tx = new Transaction(id, counterparty, description, amount, tags, receiptImage, receiptImageHash,
            previousHash, timestamp, hash, signature, isReverted, reversalOfTransactionId);

        return tx;
    }

    public byte[] GetSigningData()
    {
        string tags = string.Join(",", Tags);
        string amountString = Amount.ToString("0.########", CultureInfo.InvariantCulture);
        string timestampString = Timestamp.ToString("M/d/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

        string data =
            $"{Counterparty}{Description}{amountString}{timestampString}{tags}{ReceiptImage}{ReceiptImageHash}";

        return System.Text.Encoding.UTF8.GetBytes(data);
    }

    public void SetSignature(byte[] signature)
    {
        if (Signature != "")
            return;

        Signature = Convert.ToBase64String(signature);
    }

    public void SetIsReverted(bool isReverted)
    {
        if (_isRevertedSetOnce) return;

        _isRevertedSetOnce = true;
        IsReverted = isReverted;
    }

    public int CompareTo(Transaction? other)
    {
        return other == null ? 1 : Id.CompareTo(other.Id);
    }

    #endregion

    #region PRIVATE PROPERTIES

    private bool _isRevertedSetOnce = false;

    #endregion

    #region PRIVATE CONSTRUCTORS

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

        ReceiptImageHash = "";
        Hash = "";
        Signature = "";
    }

    // Constructor for existing transaction in the database
    private Transaction(uint id, string counterparty, string description, decimal amount, List<string> tags,
        string receiptImage, string receiptImageHash, string previousHash, DateTime timestamp, string hash,
        string signature, bool isReverted, uint? reversalOfTransactionId = null)
    {
        Id = id;
        Counterparty = counterparty;
        Description = description;
        Amount = amount;
        Tags = tags;
        ReceiptImage = receiptImage;
        ReceiptImageHash = receiptImageHash;
        PreviousHash = previousHash;
        ReversalOfTransactionId = reversalOfTransactionId;
        IsReverted = isReverted;
        Timestamp = timestamp;
        Signature = signature;
        Hash = hash;
    }

    #endregion
}