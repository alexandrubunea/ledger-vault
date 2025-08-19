using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ledger_vault.Services;

public class StatsRepository(DatabaseManagerService databaseManagerService)
{
    #region PUBLIC API

    public async Task<List<Tuple<string, decimal>>> GetWeeklyExpenses()
    {
        List<Tuple<string, decimal>> result = [];

        var conn = databaseManagerService.GetConnection();
        var command = conn.CreateCommand();

        try
        {
            command.CommandText = """
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
                                    DateTime >= date('now', '-7 days') AND Amount <= 0
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
            throw new Exception("Error trying to get the weekly income from the db", ex);
        }

        return result;
    }

    public async Task<List<Tuple<string, decimal>>> GetWeeklyIncome()
    {
        List<Tuple<string, decimal>> result = [];

        var conn = databaseManagerService.GetConnection();
        var command = conn.CreateCommand();

        try
        {
            command.CommandText = """
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
                                    DateTime >= date('now', '-7 days') AND Amount > 0
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
            throw new Exception("Error trying to get the weekly income from the db", ex);
        }

        return result;
    }

    #endregion
}