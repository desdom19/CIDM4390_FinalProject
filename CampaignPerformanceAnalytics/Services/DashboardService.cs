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
        SELECT COUNT(*)                                      AS daily_result_count,
               COALESCE(SUM(spend), 0)                       AS total_spend,
               COALESCE(SUM(revenue), 0)                     AS total_revenue,
               CAST(COALESCE(SUM(leads), 0) AS SIGNED)       AS total_leads,
               CAST(COALESCE(SUM(impressions), 0) AS SIGNED) AS total_impressions,
               CAST(COALESCE(SUM(clicks), 0) AS SIGNED)      AS total_clicks
          FROM results;
        """;

    private const string DatabaseFailureMessage =
        "The dashboard couldn't load because of a database error. Try again later.";

    private readonly string _connectionString;
    private readonly ILogger<DashboardService>? _logger;

    public DashboardService(string connectionString, ILogger<DashboardService>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Sums spend, revenue, leads, impressions and clicks over every Daily Result.
    /// A database error gives a failed result, never zero totals.
    /// </summary>
    public async Task<DashboardResult> GetKpiTotalsAsync()
    {
        try
        {
            return DashboardResult.Success(await ReadTotalsAsync());
        }
        catch (MySqlException ex)
        {
            _logger?.LogError(ex, "The dashboard's KPI totals couldn't be read.");
            return DashboardResult.Failure(DatabaseFailureMessage);
        }
    }

    private async Task<KpiTotals> ReadTotalsAsync()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(TotalsQuery, connection);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();

        return new KpiTotals(
            HasDailyResults: reader.GetInt64(0) > 0,
            TotalSpend: reader.GetDecimal(1),
            TotalRevenue: reader.GetDecimal(2),
            TotalLeads: reader.GetInt64(3),
            TotalImpressions: reader.GetInt64(4),
            TotalClicks: reader.GetInt64(5));
    }
}
