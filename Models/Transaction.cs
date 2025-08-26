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

    public uint Id = 0;
    public string Counterparty = String.Empty;
    public string Description = String.Empty;
    public decimal Amount = 0m;
    public List<string> Tags = new List<string>();
    public string ReceiptImage = String.Empty;
    public string ReceiptImageHash = String.Empty;
    public DateTime Timestamp = DateTime.MinValue;
    public string Hash = String.Empty;
    public string PreviousHash = String.Empty;
    public string Signature = String.Empty;
    public uint? ReversalOfTransactionId = null;
    public bool IsReverted = false;

    public HashStatus HashVerifiedStatus = HashStatus.InProgress;
    public SignatureStatus SignatureVerifiedStatus = SignatureStatus.InProgress;

    #endregion

    #region PUBLIC API

    public Transaction SetId(uint id)
    {
        Id = id;
        return this;
    }

    public Transaction SetCounterparty(string counterparty)
    {
        Counterparty = counterparty;
        return this;
    }

    public Transaction SetDescription(string description)
    {
        Description = description;
        return this;
    }

    public Transaction SetAmount(decimal amount)
    {
        Amount = amount;
        return this;
    }

    public Transaction SetTags(List<string> tags)
    {
        Tags = tags;
        return this;
    }

    public Transaction SetReceiptImage(string receiptImage)
    {
        ReceiptImage = receiptImage;
        return this;
    }

    public Transaction SetReceiptImageHash(string receiptImageHash)
    {
        ReceiptImageHash = receiptImageHash;
        return this;
    }

    public Transaction SetTimestamp(DateTime timestamp)
    {
        Timestamp = timestamp;
        return this;
    }

    public Transaction SetPreviousHash(string previousHash)
    {
        PreviousHash = previousHash;
        return this;
    }

    public Transaction SetHash(string hash)
    {
        Hash = hash;
        return this;
    }

    public Transaction SetSignature(byte[] signature)
    {
        Signature = Convert.ToBase64String(signature);

        return this;
    }

    public Transaction SetSignature(string signature)
    {
        Signature = signature;
        return this;
    }

    public Transaction SetIsReverted(bool isReverted)
    {
        IsReverted = isReverted;
        return this;
    }

    public Transaction SetReversalOfTransactionId(uint? reversalOfTransactionId)
    {
        ReversalOfTransactionId = reversalOfTransactionId;
        return this;
    }

    public Transaction SetHashVerifiedStatus(HashStatus status)
    {
        HashVerifiedStatus = status;
        return this;
    }

    public Transaction SetSignatureStatus(SignatureStatus status)
    {
        SignatureVerifiedStatus = status;
        return this;
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

    public int CompareTo(Transaction? other)
    {
        return other == null ? 1 : Id.CompareTo(other.Id);
    }

    #endregion
}