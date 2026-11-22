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
        // CsvHelper allows rows with a different field count by default (FieldCountMismatch = Allow),
        // so each such row reaches ValidateRowAsync and is rejected on its own.
        var options = new CsvReaderOptions { TrimWhitespace = true, TrackLineNumbers = true };
        await using var csv = CsvReader.Create(csvStream, options, leaveOpen: true);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var affectedCampaignIds = new HashSet<int>();

        while (await csv.ReadAsync())
        {
            summary.RowsRead++;

            var reasons = new List<string>();
            var dailyResult = await ValidateRowAsync(csv, connection, transaction, reasons);
            if (dailyResult is null)
            {
                summary.RejectedRows.Add(new RejectedRow(checked((int)csv.LineNumber), reasons));
                continue;
            }

            // Only a valid row reaches here, so a rejected row is never counted as a duplicate
            // and a Campaign is never created from a rejected row.
            var existingCampaignId = await FindCampaignIdAsync(connection, transaction, dailyResult.CampaignName);

            // Earlier accepted rows from this file are already inserted in this transaction,
            // so one lookup catches duplicates from the database and from the same file.
            // The first Daily Result stands, even if the duplicate's figures differ.
            if (existingCampaignId is not null
                && await DailyResultExistsAsync(connection, transaction, existingCampaignId.Value, dailyResult.ResultDate))
            {
                summary.DuplicateCount++;
                continue;
            }

            var campaignId = existingCampaignId
                ?? await CreateCampaignAsync(connection, transaction, dailyResult.CampaignName, dailyResult.ChannelId, dailyResult.ResultDate);

            await InsertDailyResultAsync(connection, transaction, campaignId, dailyResult.ResultDate,
                dailyResult.Spend, dailyResult.Impressions, dailyResult.Clicks, dailyResult.Leads, dailyResult.Revenue);

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

    private const int ExpectedFieldCount = 8;

    private record ParsedDailyResult(
        string CampaignName, int ChannelId, DateOnly ResultDate,
        decimal Spend, int Impressions, int Clicks, int Leads, decimal Revenue);

    /// <summary>
    /// Trims every field and checks the row against every validation rule.
    /// Returns the parsed row, or null with every failure added to <paramref name="reasons"/>.
    /// Plausibility (clicks ≤ impressions, leads ≤ clicks, revenue 0 when leads are 0) is not checked.
    /// </summary>
    private static async Task<ParsedDailyResult?> ValidateRowAsync(
        CsvReader csv, MySqlConnection connection, MySqlTransaction transaction, List<string> reasons)
    {
        // With the wrong field count the values can't be matched to their columns,
        // so any further reasons would be misleading.
        if (csv.FieldCount != ExpectedFieldCount)
        {
            reasons.Add($"Row has {csv.FieldCount} fields; expected {ExpectedFieldCount}.");
            return null;
        }

        string ReadRequiredField(string column)
        {
            var value = csv.GetString(column).Trim();
            if (value.Length == 0)
            {
                reasons.Add($"{column} is empty.");
            }
            return value;
        }

        var campaignName = ReadRequiredField("campaign_name");
        var channelName = ReadRequiredField("channel");
        var dateText = ReadRequiredField("date");
        var spendText = ReadRequiredField("spend");
        var impressionsText = ReadRequiredField("impressions");
        var clicksText = ReadRequiredField("clicks");
        var leadsText = ReadRequiredField("leads");
        var revenueText = ReadRequiredField("revenue");

        var resultDate = ParseDate(dateText, reasons);
        var spend = ParseAmount("spend", spendText, reasons);
        var impressions = ParseCount("impressions", impressionsText, reasons);
        var clicks = ParseCount("clicks", clicksText, reasons);
        var leads = ParseCount("leads", leadsText, reasons);
        var revenue = ParseAmount("revenue", revenueText, reasons);

        int? channelId = null;
        if (channelName.Length > 0)
        {
            channelId = await FindChannelIdAsync(connection, transaction, channelName);
            if (channelId is null)
            {
                reasons.Add($"channel '{channelName}' is not a known Channel.");
            }
        }

        if (reasons.Count > 0)
        {
            return null;
        }

        return new ParsedDailyResult(campaignName, channelId!.Value, resultDate!.Value,
            spend!.Value, impressions!.Value, clicks!.Value, leads!.Value, revenue!.Value);
    }

    // Empty values return null without a reason; ReadRequiredField already reported them.
    private static DateOnly? ParseDate(string value, List<string> reasons)
    {
        if (value.Length == 0) return null;
        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }
        reasons.Add($"date '{value}' is not a valid date in yyyy-MM-dd form.");
        return null;
    }

    private static decimal? ParseAmount(string column, string value, List<string> reasons)
    {
        if (value.Length == 0) return null;
        if (!decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var amount))
        {
            reasons.Add($"{column} '{value}' is not a decimal number.");
            return null;
        }
        if (amount < 0)
        {
            reasons.Add($"{column} '{value}' must be 0 or more.");
            return null;
        }
        return amount;
    }

    private static int? ParseCount(string column, string value, List<string> reasons)
    {
        if (value.Length == 0) return null;
        if (!int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var count))
        {
            reasons.Add($"{column} '{value}' is not a whole number.");
            return null;
        }
        if (count < 0)
        {
            reasons.Add($"{column} '{value}' must be 0 or more.");
            return null;
        }
        return count;
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

    // A Duplicate Daily Result is keyed on Campaign + date only; the figures don't matter.
    private static async Task<bool> DailyResultExistsAsync(
        MySqlConnection connection, MySqlTransaction transaction, int campaignId, DateOnly resultDate)
    {
        await using var command = new MySqlCommand(
            "SELECT 1 FROM results WHERE campaign_id = @campaignId AND result_date = @date LIMIT 1;",
            connection, transaction);
        command.Parameters.AddWithValue("@campaignId", campaignId);
        command.Parameters.AddWithValue("@date", resultDate);
        return await command.ExecuteScalarAsync() is not null;
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
