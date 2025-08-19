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
using ledger_vault.Models;
using ledger_vault.Services;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ledger_vault.ViewModels;

public partial class WeeklyChartViewModel : ViewModelBase
{
    #region PUBLIC API

    public WeeklyChartViewModel(StatsRepository statsRepository)
    {
        _statsRepository = statsRepository;

        UpdateChartTitle();

        _ = Task.Run(async () => { await Dispatcher.UIThread.InvokeAsync(LoadStatsAndUpdateChart); });
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public WeeklyChartViewModel()
    {
    }
#pragma warning restore

    #endregion

    #region PRIVATE PROPERTIES

    private readonly StatsRepository _statsRepository;

    [ObservableProperty] private ISeries[] _lastWeekData = [];

    [ObservableProperty] private ICartesianAxis[] _lastWeekDataXAxes =
        [new Axis { MinLimit = null, MaxLimit = null }];

    [ObservableProperty] private ICartesianAxis[] _lastWeekDataYAxes =
        [new Axis { MinLimit = null, MaxLimit = null }];

    [ObservableProperty] private bool _noDataWeek;
    [ObservableProperty] private bool _containsDataWeek;

    [ObservableProperty] private string _chartTitle = "";

    private ChartType _chartType = ChartType.WeeklyIncome;

    private List<Tuple<string, decimal>> _weeklyIncome = [];
    private List<Tuple<string, decimal>> _weeklyExpenses = [];

    private ChartType WeeklyChartType
    {
        get => _chartType;
        set
        {
            if (value != ChartType.WeeklyExpenses && value != ChartType.WeeklyIncome &&
                value != ChartType.WeeklyCashFlow)
                throw new Exception(
                    $"{nameof(WeeklyChartViewModel)}: the chart type needs to be {nameof(ChartType.WeeklyExpenses)}, {nameof(ChartType.WeeklyIncome)} or {nameof(ChartType.WeeklyCashFlow)}.");

            _chartType = value;
            UpdateChartTitle();

            _ = Task.Run(async () => { await Dispatcher.UIThread.InvokeAsync(ChangeData); });
        }
    }

    #endregion

    #region PRIVATE METHODS

    [RelayCommand]
    private void NextChart()
    {
        // Would have been easier using a math modulo formula but maybe in the future will be changes in ChartType enum
        WeeklyChartType = WeeklyChartType switch
        {
            ChartType.WeeklyIncome => ChartType.WeeklyExpenses,
            ChartType.WeeklyExpenses => ChartType.WeeklyCashFlow,
            _ => ChartType.WeeklyIncome
        };
    }

    [RelayCommand]
    private void PreviousChart()
    {
        // Would have been easier using a math modulo formula but maybe in the future will be changes in ChartType enum
        WeeklyChartType = WeeklyChartType switch
        {
            ChartType.WeeklyIncome => ChartType.WeeklyCashFlow,
            ChartType.WeeklyExpenses => ChartType.WeeklyIncome,
            _ => ChartType.WeeklyExpenses
        };
    }

    private async Task LoadStatsAndUpdateChart()
    {
        if (WeeklyChartType != ChartType.WeeklyExpenses && WeeklyChartType != ChartType.WeeklyIncome &&
            WeeklyChartType != ChartType.WeeklyCashFlow)
            throw new Exception(
                $"{nameof(WeeklyChartViewModel)}: the chart type needs to be {nameof(ChartType.WeeklyExpenses)}, {nameof(ChartType.WeeklyIncome)} or {nameof(ChartType.WeeklyCashFlow)}.");

        _weeklyIncome = await _statsRepository.GetWeeklyIncome();
        _weeklyExpenses = await _statsRepository.GetWeeklyExpenses();

        ChangeData();
    }

    private void UpdateChartTitle()
    {
        ChartTitle = "Your last week's " + _chartType switch
        {
            ChartType.WeeklyIncome => "income",
            ChartType.WeeklyExpenses => "expenses",
            _ => "cash flow"
        };
    }

    private void ChangeData()
    {
        var chartData = WeeklyChartType switch
        {
            ChartType.WeeklyIncome => PrepareIncomeOrExpenseData(_weeklyIncome),
            ChartType.WeeklyExpenses => PrepareIncomeOrExpenseData(_weeklyExpenses),
            ChartType.WeeklyCashFlow => PrepareCashFlowData(),
            _ => throw new InvalidOperationException("Invalid chart type")
        };

        if (chartData.IsEmpty)
        {
            SetNoDataState();
            return;
        }

        SetChartState(chartData);
    }

    private ChartData PrepareIncomeOrExpenseData(List<Tuple<string, decimal>> data)
    {
        if (data.Count == 0)
            return ChartData.Empty;

        var days = data.Select(d => d.Item1).ToList();
        var amounts = data.Select(d => Convert.ToDouble(Math.Abs(d.Item2))).ToList();
        var maxAmount = amounts.Max();

        return new ChartData
        {
            Days = days,
            PrimaryValues = amounts,
            SecondaryValues = Enumerable.Repeat(maxAmount, days.Count).ToList(),
            MaxAmount = maxAmount,
            IsEmpty = false
        };
    }

    private ChartData PrepareCashFlowData()
    {
        List<string> allDays = GetOrderedUniqueDays(_weeklyIncome, _weeklyExpenses);

        if (allDays.Count == 0)
            return ChartData.Empty;

        Dictionary<string, decimal> incomeByDay = _weeklyIncome.ToDictionary(x => x.Item1, x => x.Item2);
        Dictionary<string, decimal> expensesByDay = _weeklyExpenses.ToDictionary(x => x.Item1, x => x.Item2);

        List<double> incomeValues = [];
        List<double> expenseValues = [];

        foreach (string day in allDays)
        {
            double incomeAmount = incomeByDay.TryGetValue(day, out decimal income)
                ? Convert.ToDouble(Math.Abs(income))
                : 0.0;
            double expenseAmount = expensesByDay.TryGetValue(day, out decimal expense)
                ? Convert.ToDouble(Math.Abs(expense))
                : 0.0;

            incomeValues.Add(incomeAmount);
            expenseValues.Add(expenseAmount);
        }

        var maxAmount = 0.0;
        if (incomeValues.Count > 0) maxAmount = Math.Max(maxAmount, incomeValues.Max());
        if (expenseValues.Count > 0) maxAmount = Math.Max(maxAmount, expenseValues.Max());

        return new ChartData
        {
            Days = allDays,
            PrimaryValues = incomeValues, // Income bars (purple)
            SecondaryValues = expenseValues, // Expense bars (red)
            MaxAmount = maxAmount,
            IsEmpty = false
        };
    }


    private List<string> GetOrderedUniqueDays(List<Tuple<string, decimal>> income,
        List<Tuple<string, decimal>> expenses)
    {
        // Both lists come from SQL already sorted chronologically (newest to oldest)
        // We need to merge them while preserving this chronological order

        List<string> result = [];
        int incomeIndex = 0, expenseIndex = 0;

        // Merge the two sorted lists, similar to merge sort
        while (incomeIndex < income.Count && expenseIndex < expenses.Count)
        {
            string incomeDay = income[incomeIndex].Item1;
            string expenseDay = expenses[expenseIndex].Item1;

            if (incomeDay == expenseDay)
            {
                // Same day in both lists, add once and advance both pointers
                if (!result.Contains(incomeDay))
                    result.Add(incomeDay);

                incomeIndex++;
                expenseIndex++;
            }
            else
            {
                // Different days - we need to determine which comes first chronologically
                // Since SQL returns oldest first, we need to check which day is more distant

                int incomeDistanceFromToday = CalculateDaysFromToday(incomeDay);
                int expenseDistanceFromToday = CalculateDaysFromToday(expenseDay);

                if (incomeDistanceFromToday >= expenseDistanceFromToday)
                {
                    // Income day is more distant (or equal distance), add it first
                    if (!result.Contains(incomeDay))
                        result.Add(incomeDay);

                    incomeIndex++;
                }
                else
                {
                    // Expense day is more distant, add it first
                    if (!result.Contains(expenseDay))
                        result.Add(expenseDay);
                    expenseIndex++;
                }
            }
        }

        // Add remaining days from whichever list still has items
        while (incomeIndex < income.Count)
        {
            string day = income[incomeIndex].Item1;
            if (!result.Contains(day))
                result.Add(day);

            incomeIndex++;
        }

        while (expenseIndex < expenses.Count)
        {
            string day = expenses[expenseIndex].Item1;
            if (!result.Contains(day))
                result.Add(day);

            expenseIndex++;
        }

        return result;
    }

    private int CalculateDaysFromToday(string dayWithDate)
    {
        var dayName = dayWithDate.Split(' ')[0];

        var dayOfWeek = dayName switch
        {
            "Sunday" => 0,
            "Monday" => 1,
            "Tuesday" => 2,
            "Wednesday" => 3,
            "Thursday" => 4,
            "Friday" => 5,
            "Saturday" => 6,
            _ => 0
        };

        var todayDayOfWeek = (int)DateTime.Now.DayOfWeek;
        return Math.Abs(todayDayOfWeek - dayOfWeek) % 7;
    }

    private void SetNoDataState()
    {
        NoDataWeek = true;
        ContainsDataWeek = false;
    }

    private void SetChartState(ChartData chartData)
    {
        NoDataWeek = false;
        ContainsDataWeek = true;

        var isIncomeChart = WeeklyChartType == ChartType.WeeklyIncome;
        var isCashFlowChart = WeeklyChartType == ChartType.WeeklyCashFlow;

        LastWeekData =
        [
            new ColumnSeries<double>
            {
                IsHoverable = isCashFlowChart,
                Values = new ReadOnlyCollection<double>(chartData.SecondaryValues),
                Stroke = null,
                Fill = new SolidColorPaint(isCashFlowChart
                    ? SKColors.IndianRed
                    : new SKColor(30, 30, 30, 30)),
                MaxBarWidth = 40,
                IgnoresBarPosition = true
            },
            new ColumnSeries<double>
            {
                Values = new ReadOnlyCollection<double>(chartData.PrimaryValues),
                Stroke = null,
                Fill = new SolidColorPaint(isIncomeChart || isCashFlowChart
                    ? SKColors.MediumPurple
                    : SKColors.IndianRed),
                MaxBarWidth = isCashFlowChart ? 30 : 40,
                IgnoresBarPosition = true
            }
        ];

        LastWeekDataXAxes =
        [
            new Axis
            {
                MinLimit = null,
                MaxLimit = null,
                Labels = chartData.Days
            }
        ];

        LastWeekDataYAxes =
        [
            new Axis
            {
                MinLimit = 0,
                MaxLimit = chartData.MaxAmount,
                Labeler = value => value.ToString("N")
            }
        ];
    }

    #endregion
}