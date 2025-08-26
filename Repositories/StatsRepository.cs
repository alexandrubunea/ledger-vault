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
                                   WITH days_of_week (day_num, day_name) AS (
                                     SELECT '0', 'Sunday' UNION ALL
                                     SELECT '1', 'Monday' UNION ALL
                                     SELECT '2', 'Tuesday' UNION ALL
                                     SELECT '3', 'Wednesday' UNION ALL
                                     SELECT '4', 'Thursday' UNION ALL
                                     SELECT '5', 'Friday' UNION ALL
                                     SELECT '6', 'Saturday'
                                   )
                                   SELECT
                                     t1.day_name AS DayOfWeek,
                                     COALESCE(SUM(t2.Amount), 0) AS TotalAmount
                                   FROM days_of_week AS t1
                                   LEFT JOIN transactions AS t2
                                     ON t1.day_num = strftime('%w', t2.DateTime)
                                     AND t2.DateTime >= date('now', '-7 days')
                                     AND {amountCondition}
                                   GROUP BY
                                     t1.day_num, t1.day_name
                                   ORDER BY
                                     (strftime('%w', 'now') - t1.day_num + 7) % 7;
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