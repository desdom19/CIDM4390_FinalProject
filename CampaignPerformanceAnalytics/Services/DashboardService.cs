using System.Data;
using CampaignPerformanceAnalytics.Models;
using MySqlConnector;

namespace CampaignPerformanceAnalytics.Services;

/// <summary>
/// Reads the dashboard's figures from the campaign_analytics database.
/// </summary>
public class DashboardService
{
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

    // Starts from channels and LEFT JOINs, so every Channel comes back, with 0 spend and revenue
    // when it has no Campaign or no Daily Result. Spend and revenue are summed per Channel here;
    // its ROI is worked out from these totals in C#, never averaged over Campaigns.
    private const string ChannelTotalsQuery =
        """
        SELECT ch.channel_name,
               COALESCE(SUM(r.spend), 0)   AS total_spend,
               COALESCE(SUM(r.revenue), 0) AS total_revenue
          FROM channels ch
          LEFT JOIN campaigns c ON c.channel_id = ch.channel_id
          LEFT JOIN results r   ON r.campaign_id = c.campaign_id
         GROUP BY ch.channel_id, ch.channel_name
         ORDER BY ch.channel_id;
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
    /// Reads everything the dashboard shows, in one read-only REPEATABLE READ transaction on one
    /// connection, so every figure comes from the same snapshot of the data.
    /// A database error gives a failed result, never zero totals.
    /// </summary>
    public async Task<DashboardResult> GetDashboardAsync()
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, isReadOnly: true);

            var totals = await ReadTotalsAsync(connection, transaction);
            var channelTotals = await ReadChannelTotalsAsync(connection, transaction);

            await transaction.CommitAsync();
            return DashboardResult.Success(totals, channelTotals);
        }
        catch (MySqlException ex)
        {
            _logger?.LogError(ex, "The dashboard's figures couldn't be read.");
            return DashboardResult.Failure(DatabaseFailureMessage);
        }
    }

    /// <summary>Sums spend, revenue, leads, impressions and clicks over every Daily Result.</summary>
    private static async Task<KpiTotals> ReadTotalsAsync(MySqlConnection connection, MySqlTransaction transaction)
    {
        await using var command = new MySqlCommand(TotalsQuery, connection, transaction);
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

    /// <summary>Sums spend and revenue per Channel over every Daily Result of its Campaigns.</summary>
    private static async Task<IReadOnlyList<ChannelTotals>> ReadChannelTotalsAsync(
        MySqlConnection connection, MySqlTransaction transaction)
    {
        await using var command = new MySqlCommand(ChannelTotalsQuery, connection, transaction);
        await using var reader = await command.ExecuteReaderAsync();

        var channels = new List<ChannelTotals>();
        while (await reader.ReadAsync())
        {
            channels.Add(new ChannelTotals(
                Name: reader.GetString(0),
                TotalSpend: reader.GetDecimal(1),
                TotalRevenue: reader.GetDecimal(2)));
        }
        return channels;
    }
}
