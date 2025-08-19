using System.Collections.Generic;

namespace ledger_vault.Models;

public class ChartData
{
    public List<string> Days { get; init; } = [];
    public List<double> PrimaryValues { get; init; } = [];
    public List<double> SecondaryValues { get; init; } = [];
    public double MaxAmount { get; init; }
    public bool IsEmpty { get; init; }

    public static ChartData Empty => new() { IsEmpty = true };
}