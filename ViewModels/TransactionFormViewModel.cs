using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Messaging;
using ledger_vault.Models;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class TransactionFormViewModel : PageComponentViewModel
{
    #region PUBLIC API

    public uint? ReverseTransactionId { get; set; }
    public bool AnyTagExist => Tags.Count > 0;

    public string SelectFileButtonContent => AttachmentName.Length > 0 ? "Change file" : "Select file";

    public TransactionFormViewModel(UserStateService userStateService, TransactionService transactionService,
        MediatorService<ReturnFromTransactionFormMessage> cancelMediator)
    {
        PageComponentName = PageComponents.TransactionForm;

        _transactionService = transactionService;
        _userStateService = userStateService;
        _cancelMediator = cancelMediator;

        Tags.CollectionChanged += (_, _) => OnPropertyChanged(nameof(AnyTagExist));
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public TransactionFormViewModel()
    {
    }
#pragma warning restore

    #endregion

    #region PRIVATE PROPERTIES

    private readonly TransactionService _transactionService;
    private readonly UserStateService _userStateService;
    private readonly MediatorService<ReturnFromTransactionFormMessage> _cancelMediator;

    [ObservableProperty] private string _counterparty = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private string _amount = "1";
    [ObservableProperty] private string _tagToAdd = "";
    [ObservableProperty] private ObservableCollection<string> _tags = [];
    [ObservableProperty] private string _attachmentPath = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(SelectFileButtonContent))]
    private string _attachmentName = "";

    [ObservableProperty] private bool _invalidTag;
    [ObservableProperty] private bool _invalidCounterparty;
    [ObservableProperty] private bool _invalidDescription;

    [GeneratedRegex(@"^[a-zA-Z0-9]+$")]
    private static partial Regex AlphaNumericalRegex();

    #endregion

    #region PRIVATE METHODS

    [RelayCommand]
    private void AddTag()
    {
        InvalidTag = !AlphaNumericalRegex().IsMatch(TagToAdd);
        if (InvalidTag)
            return;

        string lowercaseTag = TagToAdd.ToLower();
        if (lowercaseTag.Length == 0 || Tags.Contains(lowercaseTag))
            return;

        Tags.Add(lowercaseTag);
        TagToAdd = "";
    }

    [RelayCommand]
    private void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }

    [RelayCommand]
    private void AddTransaction()
    {
        InvalidCounterparty = Counterparty.Length == 0;
        InvalidDescription = Description.Length == 0;

        if (InvalidCounterparty || InvalidDescription)
            return;

        // If the transaction is a payment, the amount is negative
        decimal amountDecimal = decimal.TryParse(Amount, out var convertedAmount)
            ? CurrentTransactionType == TransactionType.Income ? convertedAmount : -convertedAmount
            : 0;
        
        if (amountDecimal == 0)
            return;

        Transaction tx =
            _transactionService.CreateTransaction(Counterparty, Description, amountDecimal, [..Tags], AttachmentPath,
                ReverseTransactionId);

        _userStateService.Balance += amountDecimal;
        _userStateService.SaveUserBalance();

        // Return to the list
        _cancelMediator.Publish(CreateReturnFromTransactionMessage(true, amountDecimal, tx));
    }

    [RelayCommand]
    private void CancelTransaction()
    {
        _cancelMediator.Publish(CreateReturnFromTransactionMessage(false, 0, null));
    }

    [RelayCommand]
    private async Task SelectFile()
    {
        var file = await PickTransactionFileAsync();

        if (file.Count == 0)
            return;

        AttachmentPath = file[0].TryGetLocalPath() ?? "";

        string name = file[0].Name;
        AttachmentName = name.Length > 50
            ? name[..50] + "..."
            : name;
    }

    private static async Task<IReadOnlyList<IStorageFile>> PickTransactionFileAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(
                Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null);

            if (topLevel?.StorageProvider == null)
                return [];

            var file = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Attach receipt or invoice",
                    FileTypeFilter = [ImageAndPdfFiles],
                    AllowMultiple = false
                });

            return file;
        }
        catch (Exception ex)
        {
            throw new Exception($"File picker error: {ex.Message}");
        }
    }

    private static FilePickerFileType ImageAndPdfFiles =>
        new("Image and PDF Files")
        {
            Patterns = ["*.png", "*.jpg", "*.jpeg", "*.pdf"],
            AppleUniformTypeIdentifiers = ["public.image", "public.pdf"],
            MimeTypes = ["image/png", "image/jpeg", "application/pdf"]
        };

    private static ReturnFromTransactionFormMessage CreateReturnFromTransactionMessage(bool confirmed, decimal amount,
        Transaction? tx) =>
        new()
        {
            TransactionConfirmed = confirmed,
            TransactionAmount = amount,
            Transaction = tx
        };

    #endregion
}