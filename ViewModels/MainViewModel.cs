using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Factories;

namespace ledger_vault.ViewModels;

public partial class MainViewModel : CoreViewModel
{
    #region PUBLIC PROPERTIES

    public bool HomePageIsActive => CurrentPageViewModel is HomeViewModel;
    public bool IncomePageIsActive => CurrentPageViewModel is IncomeViewModel;
    public bool PaymentsPageIsActive => CurrentPageViewModel is PaymentsViewModel;
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

    public void SwitchPageCommand(ApplicationPages pageName)
    {
        if (CurrentPageViewModel is IDisposable disposable)
            disposable.Dispose();

        CurrentPageViewModel = _pageFactory.GetPageViewModel(pageName);
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