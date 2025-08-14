using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using ledger_vault.Data;
using ledger_vault.Messaging;
using ledger_vault.Models;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public class TransactionsListViewModel : PageComponentViewModel, IDisposable
{
    #region PUBLIC PROPERTIES

    public ObservableCollection<TransactionViewModel> Transactions { get; private set; } = [];

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

        // Loading transactions
        _ = Task.Run(async () =>
        {
            try
            {
                List<Transaction> transactions = await _transactionService.GetTransactionsAsync();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Transactions.Clear();
                    foreach (Transaction tx in transactions)
                    {
                        if ((CurrentTransactionType == TransactionType.Income && tx.Amount <= 0) ||
                            (CurrentTransactionType == TransactionType.Payment && tx.Amount > 0))
                            continue;

                        var vm = new TransactionViewModel(tx, _userStateService.CurrencyId, ReverseTransaction);
                        Transactions.Add(vm);
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in transaction list", ex);
            }
        });

        _addToListMediator.Subscribe(OnSuccessfulTransaction);
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public TransactionsListViewModel()
    {
    }
#pragma warning restore

    public void Dispose()
    {
        _addToListMediator.Unsubscribe(OnSuccessfulTransaction);
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly TransactionService _transactionService;
    private readonly MediatorService<AddToTransactionListMessage> _addToListMediator;
    private readonly UserStateService _userStateService;
    private readonly MediatorService<ReverseTransactionMessage> _reverseTransactionMediator;

    #endregion

    #region PRIVATE METHODS

    private void OnSuccessfulTransaction(AddToTransactionListMessage message)
    {
        if (message.Transaction == null)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await _transactionService.AddTransaction(message.Transaction);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var newTransactionViewModel = new TransactionViewModel(message.Transaction,
                        _userStateService.CurrencyId, ReverseTransaction);
                    Transactions.Insert(0, newTransactionViewModel);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding transaction", ex);
            }
        });
    }

    private void ReverseTransaction(Transaction transaction)
    {
        _reverseTransactionMediator.Publish(new ReverseTransactionMessage { Transaction = transaction });
    }

    #endregion
}