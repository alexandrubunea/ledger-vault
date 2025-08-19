using System.Collections.Generic;

namespace ledger_vault.Models;

public class ChartData
{
    public List<string> Days { get; set; } = [];
    public List<double> PrimaryValues { get; set; } = [];
    public List<double> SecondaryValues { get; set; } = [];
    public double MaxAmount { get; set; }
    public bool IsEmpty { get; set; }

    public static ChartData Empty => new() { IsEmpty = true };
}