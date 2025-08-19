using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ledger_vault.Services;

public class StatsRepository(DatabaseManagerService databaseManagerService)
{
    #region PUBLIC API

    public async Task<List<Tuple<string, decimal>>> GetWeeklyExpenses()
    {
        return await GetWeeklyTransactions(isExpense: true, "Error trying to get the weekly expenses from the db");
    }

    public async Task<List<Tuple<string, decimal>>> GetWeeklyIncome()
    {
        return await GetWeeklyTransactions(isExpense: false, "Error trying to get the weekly income from the db");
    }

    public async Task<List<Tuple<string, int>>> GetExpensesByTags()
    {
        return await GetTransactionsByTags(isExpense: true, "Error trying to get the expenses tags from the db");
    }

    public async Task<List<Tuple<string, int>>> GetIncomeByTags()
    {
        return await GetTransactionsByTags(isExpense: false, "Error trying to get the income tags from the db");
    }

    #endregion

    #region PRIVATE METHODS

    private async Task<List<Tuple<string, int>>> GetTransactionsByTags(bool isExpense, string errorMessage)
    {
        List<Tuple<string, int>> result = [];
        var conn = databaseManagerService.GetConnection();
        var command = conn.CreateCommand();

        try
        {
            string amountCondition = isExpense ? "tr.Amount <= 0" : "tr.Amount > 0";

            command.CommandText = $"""
                                   SELECT
                                       CASE
                                           WHEN j.value IS NULL OR TRIM(j.value) = '' THEN 'untagged'
                                           ELSE TRIM(j.value)
                                           END AS tag,
                                       COUNT(tr.id) AS count
                                   FROM transactions AS tr
                                            LEFT JOIN json_each('["' || REPLACE(tr.tags, ',', '","') || '"]') AS j
                                   WHERE {amountCondition}
                                   GROUP BY
                                       tag
                                   ORDER BY
                                       count DESC
                                   LIMIT 5;
                                   """;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string dayName = reader.GetString(0);
                int amount = reader.GetInt32(1);

                result.Add(new Tuple<string, int>(dayName, amount));
            }
        }
        catch (Exception ex)
        {
            throw new Exception(errorMessage, ex);
        }

        return result;
    }

    private async Task<List<Tuple<string, decimal>>> GetWeeklyTransactions(bool isExpense, string errorMessage)
    {
        List<Tuple<string, decimal>> result = [];

        var conn = databaseManagerService.GetConnection();
        var command = conn.CreateCommand();

        try
        {
            string amountCondition = isExpense ? "Amount <= 0" : "Amount > 0";

            command.CommandText = $"""
                                   SELECT
                                     CASE strftime('%w', DateTime)
                                       WHEN '0' THEN 'Sunday'
                                       WHEN '1' THEN 'Monday'
                                       WHEN '2' THEN 'Tuesday'
                                       WHEN '3' THEN 'Wednesday'
                                       WHEN '4' THEN 'Thursday'
                                       WHEN '5' THEN 'Friday'
                                       WHEN '6' THEN 'Saturday'
                                     END AS DayOfWeek,
                                     SUM(Amount) AS TotalAmount
                                   FROM transactions
                                   WHERE
                                     DateTime >= date('now', '-7 days') AND {amountCondition}
                                   GROUP BY
                                     strftime('%w', DateTime)
                                   ORDER BY
                                     abs(strftime('%w', 'now') - strftime('%w', DateTime)) % 7 DESC;
                                   """;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string dayName = reader.GetString(0);
                decimal amount = reader.GetDecimal(1);

                result.Add(new Tuple<string, decimal>(dayName, amount));
            }
        }
        catch (Exception ex)
        {
            throw new Exception(errorMessage, ex);
        }

        return result;
    }

    #endregion
}