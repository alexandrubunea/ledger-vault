using System;
using System.IO;
using System.Security.Cryptography;
using ledger_vault.Crypto;

namespace ledger_vault.Handlers;

public static class FileHandler
{
    #region PUBLIC API

    public static string HandleReceipt(string path)
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

    #endregion
}