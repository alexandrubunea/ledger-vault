using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Factories;
using ledger_vault.Messaging;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class MainViewModel : CoreViewModel, IDisposable
{
    #region PUBLIC PROPERTIES

    public bool HomePageIsActive => CurrentPageViewModel is HomeViewModel;

    public bool IncomePageIsActive => CurrentPageViewModel is TransactionsViewModel
    {
        CurrentTransactionType: TransactionType.Income
    };

    public bool PaymentsPageIsActive => CurrentPageViewModel is TransactionsViewModel
    {
        CurrentTransactionType: TransactionType.Payment
    };

    public bool ExportPageIsActive => CurrentPageViewModel is ExportViewModel;
    public bool BackupsPageIsActive => CurrentPageViewModel is BackupsViewModel;
    public bool SettingsPageIsActive => CurrentPageViewModel is SettingsViewModel;

    #endregion

    #region PUBLIC API

    public MainViewModel(PageFactory pageFactory, TransactionService transactionService,
        MediatorService<UpdateSidebarMessage> updateSidebarMessageService)
    {
        ViewModelName = CoreViews.Main;

        // Start getting transactions before user starts using the app
        Task.Run(async () => { await transactionService.GetTransactionsAsync(); });

        _pageFactory = pageFactory;
        _updateSidebarMessage = updateSidebarMessageService;

        _updateSidebarMessage.Subscribe(OnSidebarUpdate);

        SwitchPageCommand(ApplicationPages.Home);
    }

    public void Dispose()
    {
        _updateSidebarMessage.Unsubscribe(OnSidebarUpdate);
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public MainViewModel()
    {
    }
#pragma warning restore

    public void SwitchPageCommand(ApplicationPages pageName)
    {
        if (CurrentPageViewModel is IDisposable disposable)
            disposable.Dispose();

        if (pageName != ApplicationPages.Payments && pageName != ApplicationPages.Income)
        {
            CurrentPageViewModel = _pageFactory.GetPageViewModel(pageName);
            return;
        }

        // Handle the chase were the user press "income" or "payments"
        // Do not change the current page view model before setting the transaction type
        TransactionsViewModel? vm =
            _pageFactory.GetPageViewModel(ApplicationPages.Transaction) as TransactionsViewModel;
        if (vm == null)
            throw new NullReferenceException($"{nameof(TransactionsViewModel)} must not be null");

        vm.CurrentTransactionType =
            pageName == ApplicationPages.Income ? TransactionType.Income : TransactionType.Payment;

        vm.SetActivePageComponent(PageComponents.TransactionList);

        CurrentPageViewModel = vm;
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly PageFactory _pageFactory;
    private readonly MediatorService<UpdateSidebarMessage> _updateSidebarMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HomePageIsActive))]
    [NotifyPropertyChangedFor(nameof(IncomePageIsActive))]
    [NotifyPropertyChangedFor(nameof(PaymentsPageIsActive))]
    [NotifyPropertyChangedFor(nameof(ExportPageIsActive))]
    [NotifyPropertyChangedFor(nameof(BackupsPageIsActive))]
    [NotifyPropertyChangedFor(nameof(SettingsPageIsActive))]
    private PageViewModel _currentPageViewModel = new();

    #endregion

    #region PRIVATE METHODS

    private void OnSidebarUpdate(UpdateSidebarMessage message)
    {
        // Read the comment in UpdateSidebarMessage.cs to understand the necessity of this mediator.
        if (CurrentPageViewModel is not TransactionsViewModel vm) return;

        vm.CurrentTransactionType = message.TransactionType;
        OnPropertyChanged(nameof(IncomePageIsActive));
        OnPropertyChanged(nameof(PaymentsPageIsActive));
    }

    #endregion
}