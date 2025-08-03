using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Factories;

namespace ledger_vault.ViewModels;

public partial class MainViewModel : ViewModelBase
{
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

    // Related to styling active buttons
    public bool HomePageIsActive => CurrentPageViewModel is HomeViewModel;
    public bool IncomePageIsActive => CurrentPageViewModel is IncomeViewModel;
    public bool PaymentsPageIsActive => CurrentPageViewModel is PaymentsViewModel;
    public bool CashFlowPageIsActive => CurrentPageViewModel is CashFlowViewModel;
    public bool VerifyIntegrityPageIsActive => CurrentPageViewModel is VerifyIntegrityViewModel;
    public bool ExportPageIsActive => CurrentPageViewModel is ExportViewModel;
    public bool BackupsPageIsActive => CurrentPageViewModel is BackupsViewModel;
    public bool SettingsPageIsActive => CurrentPageViewModel is SettingsViewModel;

    public MainViewModel(PageFactory pageFactory)
    {
        _pageFactory = pageFactory;
        SwitchPage(ApplicationPages.Home);
    }

    [RelayCommand]
    public void SwitchPage(ApplicationPages pageName)
    {
        CurrentPageViewModel = _pageFactory.GetPageViewModel(pageName);
    }
}