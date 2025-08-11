using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ledger_vault.Data;
using ledger_vault.Messaging;
using ledger_vault.Models;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public class TransactionsListViewModel : PageComponentViewModel, IDisposable
{
    #region PUBLIC PROPERTIES

    public TransactionType TransactionType { get; set; }

    public ObservableCollection<Transaction> Transactions { get; private set; } = [];

    #endregion

    #region PUBLIC API

    public TransactionsListViewModel(TransactionService transactionService,
        MediatorService<AddToTransactionListMessage> addToListMediator)
    {
        PageComponentName = PageComponents.TransactionList;
        _transactionService = transactionService;
        _addToListMediator = addToListMediator;

        Task.Run(async () =>
        {
            Transactions = new ObservableCollection<Transaction>(await transactionService.GetTransactionsAsync());
        });

        _addToListMediator.Subscribe(OnSuccessfulTransaction);
    }

    public void Dispose()
    {
        _addToListMediator.Unsubscribe(OnSuccessfulTransaction);
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly TransactionService _transactionService;
    private readonly MediatorService<AddToTransactionListMessage> _addToListMediator;

    #endregion

    #region PRIVATE METHODS

    private void OnSuccessfulTransaction(AddToTransactionListMessage message)
    {
        if (message.Transaction == null)
            return;

        Task.Run(async () => { await _transactionService.AddTransaction(message.Transaction); });
        Transactions.Add(message.Transaction);
    }

    #endregion
}