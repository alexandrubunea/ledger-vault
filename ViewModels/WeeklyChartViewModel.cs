using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public partial class WeeklyChartViewModel(StatsRepository statsRepository) : ChartViewModel
{
    #region PUBLIC PROPERTIES

    public ChartType WeeklyChartType
    {
        set
        {
            if (value == ChartType.Undefined)
                return;

            if (value != ChartType.WeeklyExpenses && value != ChartType.WeeklyIncome)
                throw new Exception(
                    $"{nameof(WeeklyChartViewModel)}: the chart type needs to be {nameof(ChartType.WeeklyExpenses)} or {nameof(ChartType.WeeklyIncome)}.");

            _chartType = value;

            ChartTitle = "Your last week's " + (_chartType == ChartType.WeeklyIncome ? "income" : "expenses");

            _ = Task.Run(async () => { await Dispatcher.UIThread.InvokeAsync(LoadStats); });
        }
    }

    #endregion

    #region PRIVATE PROPERTIES

    [ObservableProperty] private ISeries[] _lastWeekData = [];

    [ObservableProperty] private ICartesianAxis[] _lastWeekDataXAxes =
        [new Axis { MinLimit = null, MaxLimit = null }];

    [ObservableProperty] private ICartesianAxis[] _lastWeekDataYAxes =
        [new Axis { MinLimit = null, MaxLimit = null }];

    [ObservableProperty] private bool _noDataWeek;
    [ObservableProperty] private bool _containsDataWeek;

    [ObservableProperty] private string _chartTitle = "";

    private ChartType _chartType = ChartType.Undefined;

    #endregion

    #region PRIVATE METHODS

    private void LoadStats()
    {
        if (_chartType != ChartType.WeeklyExpenses && _chartType != ChartType.WeeklyIncome)
            throw new Exception(
                $"{nameof(WeeklyChartViewModel)}: the chart type needs to be {nameof(ChartType.WeeklyExpenses)} or {nameof(ChartType.WeeklyIncome)}.");

        _ = Task.Run(async () =>
        {
            List<Tuple<string, decimal>> weeklyData = _chartType == ChartType.WeeklyIncome
                ? await statsRepository.GetWeeklyIncome()
                : await statsRepository.GetWeeklyExpenses();

            List<string> days = [];
            List<double> transactions = [];
            
            weeklyData.ForEach(dailyData =>
            {
                days.Add(dailyData.Item1);
                transactions.Add(Convert.ToDouble(Math.Abs(dailyData.Item2)));
            });

            if (transactions.Count == 0)
            {
                NoDataWeek = true;
                ContainsDataWeek = false;
                return;
            }

            NoDataWeek = false;
            ContainsDataWeek = true;

            double maxAmount = transactions.Max();

            LastWeekData =
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
                    Values = new ReadOnlyCollection<double>(transactions),
                    Stroke = null,
                    Fill = new SolidColorPaint(_chartType == ChartType.WeeklyIncome
                        ? SKColors.MediumPurple
                        : SKColors.IndianRed),
                    IgnoresBarPosition = true
                }
            ];

            LastWeekDataXAxes =
            [
                new Axis
                {
                    MinLimit = null, MaxLimit = null,
                    Labels = days,
                }
            ];

            LastWeekDataYAxes =
            [
                new Axis
                {
                    MinLimit = 0, MaxLimit = maxAmount,
                    Labeler = value => value.ToString("N")
                }
            ];
        });
    }

    #endregion
}