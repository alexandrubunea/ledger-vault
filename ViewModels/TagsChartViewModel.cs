using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;
using ledger_vault.Services;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ledger_vault.ViewModels;

public partial class TagsChartViewModel : ViewModelBase
{
    #region PUBLIC PROPERTIES

    public ChartType TagsChartType
    {
        get => _tagsChartType;
        set
        {
            if (value == ChartType.Undefined)
                return;

            if (value != ChartType.PopularIncomeTags && value != ChartType.PopularExpensesTags)
                throw new Exception(
                    $"{nameof(TagsChartViewModel)}: the chart type needs to be {nameof(ChartType.PopularIncomeTags)} or {nameof(ChartType.PopularExpensesTags)}.");

            _tagsChartType = value;

            SetChartTitle();
            _ = Task.Run(async () => { await Dispatcher.UIThread.InvokeAsync(LoadData); });
        }
    }

    #endregion

    #region PUBLIC API

    public TagsChartViewModel(StatsRepository statsRepository)
    {
        _statsRepository = statsRepository;
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly StatsRepository _statsRepository;

    private ChartType _tagsChartType = ChartType.Undefined;

    [ObservableProperty] string _chartTitle = string.Empty;

    [ObservableProperty] private ISeries[] _series =
    [
        new PolarLineSeries<int>
        {
            Values = [0],
            LineSmoothness = 0,
            GeometrySize = 0,
            Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(90))
        },
        new PolarLineSeries<int>
        {
            Values = [0],
            LineSmoothness = 1,
            GeometrySize = 0,
            Fill = new SolidColorPaint(SKColors.IndianRed.WithAlpha(90))
        }
    ];

    [ObservableProperty] private PolarAxis[] _angleAxes =
    [
        new()
        {
            LabelsRotation = LiveCharts.TangentAngle,
            LabelsPaint = new SolidColorPaint(new SKColor(43, 44, 46)),
            LabelsBackground = LvcColor.Empty,
            Labels = ["first", "second", "third", "forth", "fifth"]
        }
    ];

    [ObservableProperty] private PolarAxis[] _radialAxes =
    [
        new()
        {
            SeparatorsPaint = new SolidColorPaint(new SKColor(43, 44, 46).WithAlpha(100))
            {
                StrokeThickness = 1
            },
            LabelsBackground = LvcColor.Empty
        }
    ];

    [ObservableProperty] private bool _containsData;
    [ObservableProperty] private bool _noData;

    #endregion

    #region PRIVATE METHODS

    private void SetChartTitle()
    {
        if (TagsChartType != ChartType.PopularExpensesTags && TagsChartType != ChartType.PopularIncomeTags)
        {
            ChartTitle = "Invalid chart type";
            return;
        }

        ChartTitle = "Your most popular " + (TagsChartType == ChartType.PopularIncomeTags ? "income" : "expense") +
                     " by tags";
    }

    private async Task LoadData()
    {
        List<Tuple<string, int>> tagsData = TagsChartType == ChartType.PopularIncomeTags
            ? await _statsRepository.GetIncomeByTags()
            : await _statsRepository.GetExpensesByTags();

        if (tagsData.Count == 0)
        {
            NoData = true;
            ContainsData = false;

            return;
        }

        NoData = false;
        ContainsData = true;

        List<string> tags = tagsData.Select(x => x.Item1).ToList();
        List<int> amounts = tagsData.Select(x => x.Item2).ToList();

        Series =
        [
            new PolarLineSeries<int>
            {
                Values = amounts,
                LineSmoothness = 0.15,
                GeometrySize = 0,
                Fill = new SolidColorPaint(TagsChartType == ChartType.PopularIncomeTags
                    ? SKColors.MediumPurple.WithAlpha(90)
                    : SKColors.IndianRed.WithAlpha(90)),
                Stroke = new SolidColorPaint(TagsChartType == ChartType.PopularIncomeTags
                    ? SKColors.MediumPurple
                    : SKColors.IndianRed),
            },
        ];

        AngleAxes =
        [
            new PolarAxis
            {
                LabelsRotation = LiveCharts.TangentAngle,
                LabelsPaint = new SolidColorPaint(new SKColor(43, 44, 46))
                {
                    StrokeThickness = 1
                },
                LabelsBackground = LvcColor.Empty,
                Labels = tags,
                SeparatorsPaint = new SolidColorPaint(new SKColor(43, 44, 46).WithAlpha(100))
            }
        ];
    }

    #endregion
}