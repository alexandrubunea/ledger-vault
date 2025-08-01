using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace ledger_vault.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    /************************************************
     *                  Pages Logic                 *
     ************************************************/

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HomePageIsActive))]
    [NotifyPropertyChangedFor(nameof(IncomePageIsActive))]
    [NotifyPropertyChangedFor(nameof(PaymentsPageIsActive))]
    [NotifyPropertyChangedFor(nameof(CashFlowPageIsActive))]
    [NotifyPropertyChangedFor(nameof(VerifyIntegrityPageIsActive))]
    [NotifyPropertyChangedFor(nameof(ExportPageIsActive))]
    [NotifyPropertyChangedFor(nameof(BackupsPageIsActive))]
    [NotifyPropertyChangedFor(nameof(SettingsPageIsActive))]
    private ViewModelBase _currentPageViewModel = new HomeViewModel();

    // Related to styling active buttons
    public bool HomePageIsActive => CurrentPageViewModel is HomeViewModel;
    public bool IncomePageIsActive => CurrentPageViewModel is IncomeViewModel;
    public bool PaymentsPageIsActive => CurrentPageViewModel is PaymentsViewModel;
    public bool CashFlowPageIsActive => CurrentPageViewModel is CashFlowViewModel;
    public bool VerifyIntegrityPageIsActive => CurrentPageViewModel is VerifyIntegrityViewModel;
    public bool ExportPageIsActive => CurrentPageViewModel is ExportViewModel;
    public bool BackupsPageIsActive => CurrentPageViewModel is BackupsViewModel;
    public bool SettingsPageIsActive => CurrentPageViewModel is SettingsViewModel;

    public enum Pages : byte
    {
        Home,
        Income,
        Payments,
        CashFlow,
        VerifyIntegrity,
        Settings,
        Export,
        Backups,
    }

    // There is no need to reload the pages when switching between them
    private readonly Dictionary<Pages, Lazy<ViewModelBase>> _pageCache = new()
    {
        [Pages.Home] = new Lazy<ViewModelBase>(() => new HomeViewModel()),
        [Pages.Income] = new Lazy<ViewModelBase>(() => new IncomeViewModel()),
        [Pages.Payments] = new Lazy<ViewModelBase>(() => new PaymentsViewModel()),
        [Pages.CashFlow] = new Lazy<ViewModelBase>(() => new CashFlowViewModel()),
        [Pages.VerifyIntegrity] = new Lazy<ViewModelBase>(() => new VerifyIntegrityViewModel()),
        [Pages.Export] = new Lazy<ViewModelBase>(() => new ExportViewModel()),
        [Pages.Backups] = new Lazy<ViewModelBase>(() => new BackupsViewModel()),
        [Pages.Settings] = new Lazy<ViewModelBase>(() => new SettingsViewModel())
    };

    [RelayCommand]
    public void SwitchPage(Pages pageName)
    {
        if (_pageCache.TryGetValue(pageName, out var lazyPage))
            CurrentPageViewModel = lazyPage.Value;
    }
}