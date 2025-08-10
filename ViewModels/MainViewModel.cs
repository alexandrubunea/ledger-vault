using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Factories;

namespace ledger_vault.ViewModels;

public partial class MainViewModel : CoreViewModel
{
    #region PUBLIC PROPERTIES

    public bool HomePageIsActive => CurrentPageViewModel is HomeViewModel;

    public bool IncomePageIsActive => CurrentPageViewModel is TransactionsViewModel
    {
        TransactionType: TransactionType.Income
    };

    public bool PaymentsPageIsActive => CurrentPageViewModel is TransactionsViewModel
    {
        TransactionType: TransactionType.Payment
    };

    public bool CashFlowPageIsActive => CurrentPageViewModel is CashFlowViewModel;
    public bool VerifyIntegrityPageIsActive => CurrentPageViewModel is VerifyIntegrityViewModel;
    public bool ExportPageIsActive => CurrentPageViewModel is ExportViewModel;
    public bool BackupsPageIsActive => CurrentPageViewModel is BackupsViewModel;
    public bool SettingsPageIsActive => CurrentPageViewModel is SettingsViewModel;

    #endregion

    #region PUBLIC API

    public MainViewModel(PageFactory pageFactory)
    {
        ViewModelName = CoreViews.Main;

        _pageFactory = pageFactory;
        SwitchPageCommand(ApplicationPages.Home);
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

        vm.TransactionType = pageName == ApplicationPages.Income ? TransactionType.Income : TransactionType.Payment;
        CurrentPageViewModel = vm;
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly PageFactory _pageFactory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HomePageIsActive))]
    [NotifyPropertyChangedFor(nameof(IncomePageIsActive))]
    [NotifyPropertyChangedFor(nameof(PaymentsPageIsActive))]
    [NotifyPropertyChangedFor(nameof(CashFlowPageIsActive))]
    [NotifyPropertyChangedFor(nameof(VerifyIntegrityPageIsActive))]
    [NotifyPropertyChangedFor(nameof(ExportPageIsActive))]
    [NotifyPropertyChangedFor(nameof(BackupsPageIsActive))]
    [NotifyPropertyChangedFor(nameof(SettingsPageIsActive))]
    private PageViewModel _currentPageViewModel = new();

    #endregion
}