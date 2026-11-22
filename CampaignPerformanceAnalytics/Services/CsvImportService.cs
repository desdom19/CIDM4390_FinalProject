using System.Globalization;
using CampaignPerformanceAnalytics.Models;
using CsvHelper;
using MySqlConnector;

namespace CampaignPerformanceAnalytics.Services;

/// <summary>
/// Imports a CSV file of Daily Results into the campaign_analytics database.
/// All database work for one import runs in a single transaction.
/// </summary>
public class CsvImportService
{
    private readonly string _connectionString;

    public CsvImportService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<ImportSummary> ImportAsync(Stream csvStream, string fileName)
    {
        var summary = new ImportSummary { FileName = fileName };

        // GetString(name) matches header names ignoring case and surrounding whitespace.
        var options = new CsvReaderOptions { TrimWhitespace = true };
        await using var csv = CsvReader.Create(csvStream, options, leaveOpen: true);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var affectedCampaignIds = new HashSet<int>();

        while (await csv.ReadAsync())
        {
            summary.RowsRead++;

            var campaignName = csv.GetString("campaign_name");
            var channelName = csv.GetString("channel");
            var resultDate = DateOnly.ParseExact(csv.GetString("date"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var spend = decimal.Parse(csv.GetString("spend"), NumberStyles.Number, CultureInfo.InvariantCulture);
            var impressions = int.Parse(csv.GetString("impressions"), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var clicks = int.Parse(csv.GetString("clicks"), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var leads = int.Parse(csv.GetString("leads"), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var revenue = decimal.Parse(csv.GetString("revenue"), NumberStyles.Number, CultureInfo.InvariantCulture);

            var channelId = await FindChannelIdAsync(connection, transaction, channelName)
                ?? throw new InvalidOperationException($"Channel '{channelName}' is not a seeded Channel.");

            var campaignId = await FindCampaignIdAsync(connection, transaction, campaignName)
                ?? await CreateCampaignAsync(connection, transaction, campaignName, channelId, resultDate);

            await InsertDailyResultAsync(connection, transaction, campaignId, resultDate,
                spend, impressions, clicks, leads, revenue);

            affectedCampaignIds.Add(campaignId);
            summary.ImportedCount++;
        }

        foreach (var campaignId in affectedCampaignIds)
        {
            await RecalculateCampaignDatesAsync(connection, transaction, campaignId);
        }

        await transaction.CommitAsync();
        return summary;
    }

    // The channel_name column's case-insensitive collation decides the match.
    private static async Task<int?> FindChannelIdAsync(
        MySqlConnection connection, MySqlTransaction transaction, string channelName)
    {
        await using var command = new MySqlCommand(
            "SELECT channel_id FROM channels WHERE channel_name = @name LIMIT 1;",
            connection, transaction);
        command.Parameters.AddWithValue("@name", channelName);
        var result = await command.ExecuteScalarAsync();
        return result is null ? null : Convert.ToInt32(result);
    }

    // The campaign_name column's case-insensitive collation decides the match.
    // Runs inside the transaction, so it also finds Campaigns created earlier in this import.
    private static async Task<int?> FindCampaignIdAsync(
        MySqlConnection connection, MySqlTransaction transaction, string campaignName)
    {
        await using var command = new MySqlCommand(
            "SELECT campaign_id FROM campaigns WHERE campaign_name = @name LIMIT 1;",
            connection, transaction);
        command.Parameters.AddWithValue("@name", campaignName);
        var result = await command.ExecuteScalarAsync();
        return result is null ? null : Convert.ToInt32(result);
    }

    // start_date and end_date are NOT NULL, so a new Campaign starts with its first row's date;
    // both are recalculated from its Daily Results at the end of the import.
    private static async Task<int> CreateCampaignAsync(
        MySqlConnection connection, MySqlTransaction transaction,
        string campaignName, int channelId, DateOnly firstDate)
    {
        await using var command = new MySqlCommand(
            """
            INSERT INTO campaigns (campaign_name, start_date, end_date, roi_goal, channel_id)
            VALUES (@name, @date, @date, NULL, @channelId);
            """,
            connection, transaction);
        command.Parameters.AddWithValue("@name", campaignName);
        command.Parameters.AddWithValue("@date", firstDate);
        command.Parameters.AddWithValue("@channelId", channelId);
        await command.ExecuteNonQueryAsync();
        return checked((int)command.LastInsertedId);
    }

    private static async Task InsertDailyResultAsync(
        MySqlConnection connection, MySqlTransaction transaction, int campaignId, DateOnly resultDate,
        decimal spend, int impressions, int clicks, int leads, decimal revenue)
    {
        await using var command = new MySqlCommand(
            """
            INSERT INTO results (result_date, spend, impressions, clicks, leads, revenue, campaign_id)
            VALUES (@date, @spend, @impressions, @clicks, @leads, @revenue, @campaignId);
            """,
            connection, transaction);
        command.Parameters.AddWithValue("@date", resultDate);
        command.Parameters.AddWithValue("@spend", spend);
        command.Parameters.AddWithValue("@impressions", impressions);
        command.Parameters.AddWithValue("@clicks", clicks);
        command.Parameters.AddWithValue("@leads", leads);
        command.Parameters.AddWithValue("@revenue", revenue);
        command.Parameters.AddWithValue("@campaignId", campaignId);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task RecalculateCampaignDatesAsync(
        MySqlConnection connection, MySqlTransaction transaction, int campaignId)
    {
        await using var command = new MySqlCommand(
            """
            UPDATE campaigns
               SET start_date = (SELECT MIN(result_date) FROM results WHERE campaign_id = @campaignId),
                   end_date   = (SELECT MAX(result_date) FROM results WHERE campaign_id = @campaignId)
             WHERE campaign_id = @campaignId;
            """,
            connection, transaction);
        command.Parameters.AddWithValue("@campaignId", campaignId);
        await command.ExecuteNonQueryAsync();
    }
}
