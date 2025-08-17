using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Factories;
using ledger_vault.Messaging;
using ledger_vault.Models;
using ledger_vault.Services;

namespace ledger_vault.ViewModels;

public partial class TransactionsViewModel : PageViewModel, IDisposable
{
    #region PUBLIC PROPERTIES

    public TransactionType CurrentTransactionType { get; set; }

    #endregion

    #region PUBLIC API

    public string GetCurrency => Currencies[_currencyId][..3];
    public string GetFormattedBalance => CurrentBalance.ToString("N");
    public string PageTitle => CurrentTransactionType == TransactionType.Income ? "income" : "payments";

    public TransactionsViewModel(UserStateService userStateService, PageComponentFactory pageComponentFactory,
        MediatorService<ReturnFromTransactionFormMessage> formMediator,
        MediatorService<AddToTransactionListMessage> addToTransactionListMediator,
        MediatorService<ReverseTransactionMessage> reverseTransactionMediator,
        MediatorService<UpdateSidebarMessage> updateSidebarMediator)
    {
        PageName = ApplicationPages.Transaction;
        _pageComponentFactory = pageComponentFactory;
        _formMediator = formMediator;
        _addToTransactionListMediator = addToTransactionListMediator;
        _reverseTransactionMediator = reverseTransactionMediator;
        _sidebarUpdateMediator = updateSidebarMediator;

        _formMediator.Subscribe(OnCancelTransaction);
        _reverseTransactionMediator.Subscribe(OnReverseTransaction);

        CurrentBalance = userStateService.Balance;
        _currencyId = userStateService.CurrencyId;
    }

    public void SetActivePageComponent(PageComponents pageComponent)
    {
        PageComponentViewModel? vm = pageComponent == PageComponents.TransactionList
            ? _pageComponentFactory.GetComponentPageViewModel(PageComponents.TransactionList) as
                TransactionsListViewModel
            : _pageComponentFactory.GetComponentPageViewModel(PageComponents.TransactionForm) as
                TransactionFormViewModel;
        if (vm == null)
            throw new NullReferenceException($"{nameof(TransactionFormViewModel)} is null");

        vm.CurrentTransactionType = CurrentTransactionType;

        ActivePageComponent = vm;
    }

    public void Dispose()
    {
        _formMediator.Unsubscribe(OnCancelTransaction);
        _reverseTransactionMediator.Unsubscribe(OnReverseTransaction);
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public TransactionsViewModel()
    {
    }
#pragma warning restore

    #endregion

    #region PRIVATE PROPERTIES

    private static readonly List<string> Currencies =
    [
        "AED - United Arab Emirates Dirham",
        "AFN - Afghan Afghani",
        "ALL - Albanian Lek",
        "AMD - Armenian Dram",
        "ANG - Netherlands Antillean Guilder",
        "AOA - Angolan Kwanza",
        "ARS - Argentine Peso",
        "AUD - Australian Dollar",
        "AWG - Aruban Florin",
        "AZN - Azerbaijani Manat",
        "BAM - Bosnia and Herzegovina Convertible Mark",
        "BBD - Barbadian Dollar",
        "BDT - Bangladeshi Taka",
        "BGN - Bulgarian Lev",
        "BHD - Bahraini Dinar",
        "BIF - Burundian Franc",
        "BMD - Bermudian Dollar",
        "BND - Brunei Dollar",
        "BOB - Bolivian Boliviano",
        "BRL - Brazilian Real",
        "BSD - Bahamian Dollar",
        "BTN - Bhutanese Ngultrum",
        "BWP - Botswana Pula",
        "BYN - Belarusian Ruble",
        "BZD - Belize Dollar",
        "CAD - Canadian Dollar",
        "CDF - Congolese Franc",
        "CHF - Swiss Franc",
        "CLP - Chilean Peso",
        "CNY - Chinese Yuan",
        "COP - Colombian Peso",
        "CRC - Costa Rican Colón",
        "CUC - Cuban Convertible Peso",
        "CUP - Cuban Peso",
        "CVE - Cape Verdean Escudo",
        "CZK - Czech Koruna",
        "DJF - Djiboutian Franc",
        "DKK - Danish Krone",
        "DOP - Dominican Peso",
        "DZD - Algerian Dinar",
        "EGP - Egyptian Pound",
        "ERN - Eritrean Nakfa",
        "ETB - Ethiopian Birr",
        "EUR - Euro",
        "FJD - Fijian Dollar",
        "FKP - Falkland Islands Pound",
        "FOK - Faroese Króna",
        "GBP - British Pound Sterling",
        "GEL - Georgian Lari",
        "GGP - Guernsey Pound",
        "GHS - Ghanaian Cedi",
        "GIP - Gibraltar Pound",
        "GMD - Gambian Dalasi",
        "GNF - Guinean Franc",
        "GTQ - Guatemalan Quetzal",
        "GYD - Guyanese Dollar",
        "HKD - Hong Kong Dollar",
        "HNL - Honduran Lempira",
        "HRK - Croatian Kuna",
        "HTG - Haitian Gourde",
        "HUF - Hungarian Forint",
        "IDR - Indonesian Rupiah",
        "ILS - Israeli New Shekel",
        "IMP - Isle of Man Pound",
        "INR - Indian Rupee",
        "IQD - Iraqi Dinar",
        "IRR - Iranian Rial",
        "ISK - Icelandic Króna",
        "JEP - Jersey Pound",
        "JMD - Jamaican Dollar",
        "JOD - Jordanian Dinar",
        "JPY - Japanese Yen",
        "KES - Kenyan Shilling",
        "KGS - Kyrgyzstani Som",
        "KHR - Cambodian Riel",
        "KID - Kiribati Dollar",
        "KMF - Comorian Franc",
        "KRW - South Korean Won",
        "KWD - Kuwaiti Dinar",
        "KYD - Cayman Islands Dollar",
        "KZT - Kazakhstani Tenge",
        "LAK - Lao Kip",
        "LBP - Lebanese Pound",
        "LKR - Sri Lankan Rupee",
        "LRD - Liberian Dollar",
        "LSL - Lesotho Loti",
        "LYD - Libyan Dinar",
        "MAD - Moroccan Dirham",
        "MDL - Moldovan Leu",
        "MGA - Malagasy Ariary",
        "MKD - Macedonian Denar",
        "MMK - Burmese Kyat",
        "MNT - Mongolian Tögrög",
        "MOP - Macanese Pataca",
        "MRU - Mauritanian Ouguiya",
        "MUR - Mauritian Rupee",
        "MVR - Maldivian Rufiyaa",
        "MWK - Malawian Kwacha",
        "MXN - Mexican Peso",
        "MYR - Malaysian Ringgit",
        "MZN - Mozambican Metical",
        "NAD - Namibian Dollar",
        "NGN - Nigerian Naira",
        "NIO - Nicaraguan Córdoba",
        "NOK - Norwegian Krone",
        "NPR - Nepalese Rupee",
        "NZD - New Zealand Dollar",
        "OMR - Omani Rial",
        "PAB - Panamanian Balboa",
        "PEN - Peruvian Sol",
        "PGK - Papua New Guinean Kina",
        "PHP - Philippine Peso",
        "PKR - Pakistani Rupee",
        "PLN - Polish Złoty",
        "PYG - Paraguayan Guaraní",
        "QAR - Qatari Riyal",
        "RON - Romanian Leu",
        "RSD - Serbian Dinar",
        "RUB - Russian Ruble",
        "RWF - Rwandan Franc",
        "SAR - Saudi Riyal",
        "SBD - Solomon Islands Dollar",
        "SCR - Seychellois Rupee",
        "SDG - Sudanese Pound",
        "SEK - Swedish Krona",
        "SGD - Singapore Dollar",
        "SHP - Saint Helena Pound",
        "SLE - Sierra Leonean Leone",
        "SLL - Sierra Leonean Leone (Old)",
        "SOS - Somali Shilling",
        "SRD - Surinamese Dollar",
        "SSP - South Sudanese Pound",
        "STN - São Tomé and Príncipe Dobra",
        "SYP - Syrian Pound",
        "SZL - Swazi Lilangeni",
        "THB - Thai Baht",
        "TJS - Tajikistani Somoni",
        "TMT - Turkmenistani Manat",
        "TND - Tunisian Dinar",
        "TOP - Tongan Paʻanga",
        "TRY - Turkish Lira",
        "TTD - Trinidad and Tobago Dollar",
        "TVD - Tuvaluan Dollar",
        "TWD - New Taiwan Dollar",
        "TZS - Tanzanian Shilling",
        "UAH - Ukrainian Hryvnia",
        "UGX - Ugandan Shilling",
        "USD - United States Dollar",
        "UYU - Uruguayan Peso",
        "UZS - Uzbekistani Soʻm",
        "VES - Venezuelan Bolívar",
        "VND - Vietnamese Đồng",
        "VUV - Vanuatu Vatu",
        "WST - Samoan Tālā",
        "XAF - Central African CFA Franc",
        "XCD - East Caribbean Dollar",
        "XDR - IMF Special Drawing Rights",
        "XOF - West African CFA Franc",
        "XPF - CFP Franc",
        "YER - Yemeni Rial",
        "ZAR - South African Rand",
        "ZMW - Zambian Kwacha",
        "ZWL - Zimbabwean Dollar"
    ];

    private readonly PageComponentFactory _pageComponentFactory;
    private readonly MediatorService<ReturnFromTransactionFormMessage> _formMediator;
    private readonly MediatorService<AddToTransactionListMessage> _addToTransactionListMediator;
    private readonly MediatorService<ReverseTransactionMessage> _reverseTransactionMediator;
    private readonly MediatorService<UpdateSidebarMessage> _sidebarUpdateMediator;

    [ObservableProperty] private PageComponentViewModel? _activePageComponent;

    [ObservableProperty] private bool _showIncomeMode = true;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(GetFormattedBalance))]
    private decimal _currentBalance;

    private readonly short _currencyId;

    #endregion

    #region PRIVATE METHODS

    [RelayCommand]
    private void SwitchMode()
    {
        ShowIncomeMode = !ShowIncomeMode;

        SetActivePageComponent(ShowIncomeMode ? PageComponents.TransactionList : PageComponents.TransactionForm);
    }

    private void OnCancelTransaction(ReturnFromTransactionFormMessage formMessage)
    {
        ShowIncomeMode = true;
        SetActivePageComponent(PageComponents.TransactionList);

        if (!formMessage.TransactionConfirmed) return;
        CurrentBalance += formMessage.TransactionAmount;

        if (formMessage.Transaction == null)
            return;

        _addToTransactionListMediator.Publish(new AddToTransactionListMessage
            { Transaction = formMessage.Transaction });
    }

    private void OnReverseTransaction(ReverseTransactionMessage reverseTransactionMessage)
    {
        if (reverseTransactionMessage.Transaction == null)
            return;

        Transaction tx = reverseTransactionMessage.Transaction;

        TransactionFormViewModel? form =
            _pageComponentFactory.GetComponentPageViewModel(PageComponents.TransactionForm) as TransactionFormViewModel;
        if (form == null)
            return;

        form.Description =
            $"Reverse of transaction #{tx.Id}. From counterparty: {tx.Counterparty}. Made at: {tx.Timestamp:dd MMM yyyy HH:mm:ss}.";

        form.Counterparty = tx.Counterparty;
        form.Tags = new ObservableCollection<string>(tx.Tags);
        form.ReverseTransactionId = tx.Id;
        form.Amount = Math.Abs(tx.Amount);

        ShowIncomeMode = false;
        CurrentTransactionType = CurrentTransactionType == TransactionType.Income
            ? TransactionType.Payment
            : TransactionType.Income;
        form.CurrentTransactionType = CurrentTransactionType;

        ActivePageComponent = form;
        _sidebarUpdateMediator.Publish(new UpdateSidebarMessage { TransactionType = CurrentTransactionType });
        OnPropertyChanged(nameof(PageTitle));
    }

    #endregion
}