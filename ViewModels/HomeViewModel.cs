using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Services;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ledger_vault.ViewModels;

public partial class HomeViewModel : PageViewModel
{
    #region PUBLIC API

    public HomeViewModel(UserStateService userStateService, StatsRepository statsRepository)
    {
        PageName = ApplicationPages.Home;

        _statsRepository = statsRepository;

        UserFullName = userStateService.FullUserName;
        CurrentBalance = userStateService.Balance;
        _currencyId = userStateService.CurrencyId;

        _ = Task.Run(async () => { await Dispatcher.UIThread.InvokeAsync(LoadStats); });
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public HomeViewModel()
    {
        UserFullName = "John Doe";
    }
#pragma warning restore

    public string GetCurrency => Currencies[_currencyId][..3];
    public string GetFormattedBalance => CurrentBalance.ToString("N");

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

    private readonly StatsRepository _statsRepository;

    [ObservableProperty] private string _userFullName;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(GetFormattedBalance))]
    private decimal _currentBalance;

    private readonly short _currencyId;

    [ObservableProperty] private ISeries[] _lastWeekIncome = [];

    [ObservableProperty] private ICartesianAxis[] _lastWeekIncomeXAxes =
        [new Axis { MinLimit = null, MaxLimit = null }];

    [ObservableProperty] private bool _noDataWeekIncome;
    [ObservableProperty] private bool _containsDataWeekIncome;

    #endregion

    #region PRIVATE METHODS

    private void LoadStats()
    {
        _ = Task.Run(async () =>
        {
            Dictionary<string, decimal> weeklyExpenses = await _statsRepository.GetWeeklyIncome();

            List<string> days = weeklyExpenses.Keys.ToList();
            List<double> expenses = weeklyExpenses.Values.Select(Convert.ToDouble).ToList();

            if (expenses.Count == 0)
            {
                NoDataWeekIncome = true;
                ContainsDataWeekIncome = false;
                return;
            }

            NoDataWeekIncome = false;
            ContainsDataWeekIncome = true;

            double maxAmount = expenses.Max();

            LastWeekIncome =
            [
                new ColumnSeries<double>
                {
                    IsHoverable = false,
                    Values = new ReadOnlyCollection<double>(Enumerable.Repeat(maxAmount, days.Count).ToList()),
                    Stroke = null,
                    Fill = new SolidColorPaint(new SKColor(30, 30, 30, 30)),
                    IgnoresBarPosition = true
                },
                new ColumnSeries<double>
                {
                    Values = new ReadOnlyCollection<double>(expenses),
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.MediumPurple),
                    IgnoresBarPosition = true
                }
            ];

            LastWeekIncomeXAxes =
            [
                new Axis
                {
                    MinLimit = null, MaxLimit = null,
                    Labels = days
                }
            ];
        });
    }

    #endregion
}