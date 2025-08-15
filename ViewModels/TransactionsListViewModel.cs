using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Messaging;
using ledger_vault.Models;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class TransactionsListViewModel : PageComponentViewModel, IDisposable
{
    #region PUBLIC PROPERTIES

    public ObservableCollection<TransactionViewModel> TransactionsOnPage { get; private set; } = [];

    #endregion

    #region PUBLIC API

    public TransactionsListViewModel(TransactionService transactionService,
        MediatorService<AddToTransactionListMessage> addToListMediator,
        MediatorService<ReverseTransactionMessage> reverseTransactionMediator,
        UserStateService userStateService)
    {
        PageComponentName = PageComponents.TransactionList;
        _transactionService = transactionService;
        _addToListMediator = addToListMediator;
        _reverseTransactionMediator = reverseTransactionMediator;
        _userStateService = userStateService;

        LoadTransactions();

        _addToListMediator.Subscribe(OnSuccessfulTransaction);
    }

    public void Dispose()
    {
        _addToListMediator.Unsubscribe(OnSuccessfulTransaction);
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public TransactionsListViewModel()
    {
    }
#pragma warning restore

    public ObservableCollection<TransactionViewModel> GetCurrentPageContent =>
        new(_transactions.Skip(TransactionsPerPage * (CurrentPage - 1)).Take(TransactionsPerPage).ToList());

    public int NumberOfPages => (int)Math.Ceiling((double)_transactions.Count / TransactionsPerPage);

    #endregion

    #region PRIVATE PROPERTIES

    private readonly TransactionService _transactionService;
    private readonly MediatorService<AddToTransactionListMessage> _addToListMediator;
    private readonly UserStateService _userStateService;
    private readonly MediatorService<ReverseTransactionMessage> _reverseTransactionMediator;

    private readonly List<TransactionViewModel> _transactions = [];
    private const int TransactionsPerPage = 5;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(GetCurrentPageContent))]
    private int _currentPage = 1;

    #endregion

    #region PRIVATE METHODS

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage + 1 > NumberOfPages)
            return;

        CurrentPage += 1;
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage - 1 <= 0)
            return;

        CurrentPage -= 1;
    }

    private void OnSuccessfulTransaction(AddToTransactionListMessage message)
    {
        if (message.Transaction == null)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await _transactionService.AddTransaction(message.Transaction);
                LoadTransactions();
            }
            catch (Exception ex)
            {
                throw new NullReferenceException("OnSuccessfulTransaction", ex);
            }
        });
    }

    private void LoadTransactions()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                List<Transaction> transactions = await _transactionService.GetTransactionsAsync();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _transactions.Clear();
                    foreach (Transaction tx in Enumerable.Reverse(transactions))
                    {
                        if ((CurrentTransactionType == TransactionType.Income && tx.Amount <= 0) ||
                            (CurrentTransactionType == TransactionType.Payment && tx.Amount > 0))
                            continue;

                        var vm = new TransactionViewModel(tx, _userStateService.CurrencyId, ReverseTransaction);
                        _transactions.Add(vm);
                    }

                    OnPropertyChanged(nameof(GetCurrentPageContent));
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in transaction list", ex);
            }
        });
    }

    private void ReverseTransaction(Transaction transaction)
    {
        _reverseTransactionMediator.Publish(new ReverseTransactionMessage { Transaction = transaction });
    }

    #endregion
}