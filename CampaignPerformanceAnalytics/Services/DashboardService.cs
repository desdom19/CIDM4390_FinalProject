using CampaignPerformanceAnalytics.Models;
using MySqlConnector;

namespace CampaignPerformanceAnalytics.Services;

/// <summary>
/// Reads the dashboard's KPI totals from the campaign_analytics database.
/// </summary>
public class DashboardService
{
    // One query, so every total comes from the same snapshot of the data.
    // COALESCE turns the NULL that SUM gives over no rows into 0.
    // MySQL sums an int column as DECIMAL; CAST AS SIGNED reads the counts back as 64-bit integers.
    private const string TotalsQuery =
        """
        SELECT COALESCE(SUM(spend), 0)                       AS total_spend,
               COALESCE(SUM(revenue), 0)                     AS total_revenue,
               CAST(COALESCE(SUM(leads), 0) AS SIGNED)       AS total_leads,
               CAST(COALESCE(SUM(impressions), 0) AS SIGNED) AS total_impressions,
               CAST(COALESCE(SUM(clicks), 0) AS SIGNED)      AS total_clicks
          FROM results;
        """;

    private readonly string _connectionString;

    public DashboardService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>Sums spend, revenue, leads, impressions and clicks over every Daily Result.</summary>
    public async Task<DashboardResult> GetKpiTotalsAsync()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(TotalsQuery, connection);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();

        return new DashboardResult
        {
            Totals = new KpiTotals(
                TotalSpend: reader.GetDecimal(0),
                TotalRevenue: reader.GetDecimal(1),
                TotalLeads: reader.GetInt64(2),
                TotalImpressions: reader.GetInt64(3),
                TotalClicks: reader.GetInt64(4)),
        };
    }
}
