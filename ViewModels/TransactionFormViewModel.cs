using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Messaging;
using ledger_vault.Models;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class TransactionFormViewModel : PageComponentViewModel
{
    private readonly TransactionService _transactionService;
    private readonly UserStateService _userStateService;
    private readonly MediatorService<ReturnFromTransactionMessage> _cancelMediator;

    [ObservableProperty] private string _counterparty = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private decimal _amount = 1;
    [ObservableProperty] private string _tagToAdd = "";
    [ObservableProperty] private ObservableCollection<string> _tags = [];

    public TransactionType TransactionType { get; set; }
    public bool AnyTagExist => Tags.Count > 0;

    public TransactionFormViewModel(UserStateService userStateService, TransactionService transactionService,
        MediatorService<ReturnFromTransactionMessage> cancelMediator)
    {
        PageComponentName = PageComponents.TransactionForm;

        _transactionService = transactionService;
        _userStateService = userStateService;
        _cancelMediator = cancelMediator;

        Tags.CollectionChanged += (_, _) => OnPropertyChanged(nameof(AnyTagExist));
    }

    [RelayCommand]
    private void AddTag()
    {
        // TODO: Check if tags contains only valid alphanumerical chars, if not, show an error
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
        // TODO: Add warning messages when empty
        if (Counterparty.Length == 0)
            return;
        if (Description.Length == 0)
            return;

        // If the transaction is a payment, the amount is negative
        decimal amount = (TransactionType == TransactionType.Income) ? Amount : -Amount;

        Transaction tx = _transactionService.CreateTransaction(Counterparty, Description, amount, [..Tags], "");

        _userStateService.Balance += amount;
        _userStateService.SaveUserBalance();

        // Return to the list
        _cancelMediator.Publish(CreateReturnFromTransactionMessage(true, amount));
    }

    [RelayCommand]
    private void CancelTransaction()
    {
        _cancelMediator.Publish(CreateReturnFromTransactionMessage(false, 0));
    }

    private static ReturnFromTransactionMessage CreateReturnFromTransactionMessage(bool confirmed, decimal amount) =>
        new()
        {
            TransactionConfirmed = confirmed,
            TransactionAmount = amount
        };
}