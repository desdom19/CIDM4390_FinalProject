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
    /// <summary>The header must have exactly these columns, matched by name in any order and any letter case.</summary>
    private static readonly IReadOnlyList<string> RequiredColumns =
        ["campaign_name", "channel", "date", "spend", "impressions", "clicks", "leads", "revenue"];

    private const string DatabaseFailureMessage =
        "The import failed because of a database error. Nothing was saved, so it is safe to try again.";

    private readonly string _connectionString;
    private readonly ILogger<CsvImportService>? _logger;

    public CsvImportService(string connectionString, ILogger<CsvImportService>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<ImportSummary> ImportAsync(Stream csvStream, string fileName)
    {
        // GetString(name) matches header names ignoring case and surrounding whitespace.
        // CsvHelper allows rows with a different field count by default (FieldCountMismatch = Allow),
        // so each such row reaches ValidateRowAsync and is rejected on its own.
        // The separator is fixed so that a file using another separator fails the header check
        // instead of being guessed at.
        var options = new CsvReaderOptions { TrimWhitespace = true, TrackLineNumbers = true, Separator = ',' };
        await using var csv = CsvReader.Create(csvStream, options, leaveOpen: true);

        // File-level checks run before any database connection is opened, so a refused file writes nothing.
        var headerError = await CheckHeaderAsync(csv);
        if (headerError is not null)
        {
            return ImportSummary.Failure(fileName, headerError);
        }

        var summary = new ImportSummary { FileName = fileName };
        try
        {
            await ImportRowsAsync(csv, summary);
            return summary;
        }
        catch (MySqlException ex)
        {
            // The transaction was never committed, so disposing it rolled every change back.
            _logger?.LogError(ex, "CSV import of {FileName} failed and was rolled back.", fileName);
            return ImportSummary.Failure(fileName, DatabaseFailureMessage);
        }
    }

    /// <summary>
    /// Reads the header and returns a message describing why the file is refused, or null when the header is valid.
    /// </summary>
    private static async Task<string?> CheckHeaderAsync(CsvReader csv)
    {
        IReadOnlyList<string> headers;
        try
        {
            await csv.ReadHeaderAsync();
            headers = csv.Headers.Select(h => h.Trim()).ToList();
        }
        catch (CsvFormatException)
        {
            return "The header row could not be read. Check that the file is a comma-separated CSV.";
        }

        // A header of blank names comes from a file of blank lines or separators only.
        if (headers.Count == 0 || headers.All(h => h.Length == 0))
        {
            return "The file is empty. A Daily Results CSV must start with a header row.";
        }

        var missing = RequiredColumns
            .Where(required => !headers.Contains(required, StringComparer.OrdinalIgnoreCase))
            .ToList();

        // A column that isn't required, or a required column that appears a second time, is extra.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var extra = headers
            .Where(h => !RequiredColumns.Contains(h, StringComparer.OrdinalIgnoreCase) || !seen.Add(h))
            .Select(h => h.Length == 0 ? "(blank)" : h)
            .ToList();

        if (missing.Count == 0 && extra.Count == 0)
        {
            return null;
        }

        var problems = new List<string>();
        if (missing.Count > 0)
        {
            problems.Add($"missing {Plural(missing.Count, "column")} {Quote(missing)}");
        }
        if (extra.Count > 0)
        {
            problems.Add($"extra {Plural(extra.Count, "column")} {Quote(extra)}");
        }
        return $"The header is wrong: {string.Join("; ", problems)}. "
            + $"It must have exactly these columns, in any order: {string.Join(", ", RequiredColumns)}.";

        static string Plural(int count, string word) => count == 1 ? word : word + "s";
        static string Quote(IEnumerable<string> names) => string.Join(", ", names.Select(n => $"'{n}'"));
    }

    private async Task ImportRowsAsync(CsvReader csv, ImportSummary summary)
    {
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
            // Earlier accepted rows from this file are already inserted in this transaction,
            // so one lookup catches duplicates from the database and from the same file.
            // The first Daily Result stands, even if the duplicate's figures differ.
            if (dailyResult.ExistingCampaignId is { } existingCampaignId
                && await DailyResultExistsAsync(connection, transaction, existingCampaignId, dailyResult.ResultDate))
            {
                summary.DuplicateCount++;
                continue;
            }

            var campaignId = dailyResult.ExistingCampaignId
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
    }

    private static readonly int ExpectedFieldCount = RequiredColumns.Count;

    /// <param name="ExistingCampaignId">The Campaign's id if it already exists, otherwise null.</param>
    private record ValidatedDailyResult(
        string CampaignName, int ChannelId, int? ExistingCampaignId, DateOnly ResultDate,
        decimal Spend, int Impressions, int Clicks, int Leads, decimal Revenue);

    private record CampaignMatch(int CampaignId, string CampaignName, int ChannelId, string ChannelName);

    /// <summary>
    /// Trims every field and checks the row against every validation rule.
    /// Returns the validated row, or null with every failure added to <paramref name="reasons"/>.
    /// Plausibility (clicks ≤ impressions, leads ≤ clicks, revenue 0 when leads are 0) is not checked.
    /// </summary>
    private static async Task<ValidatedDailyResult?> ValidateRowAsync(
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

        // A Campaign always runs on exactly one Channel: the one stored with it, which its first
        // accepted row set. Campaigns created earlier in this import are found too, because the
        // lookup runs inside the import's transaction.
        CampaignMatch? campaign = null;
        if (campaignName.Length > 0)
        {
            campaign = await FindCampaignAsync(connection, transaction, campaignName);
            if (campaign is not null && channelId is not null && campaign.ChannelId != channelId)
            {
                reasons.Add($"Campaign '{campaign.CampaignName}' belongs to channel '{campaign.ChannelName}', not '{channelName}'.");
            }
        }

        if (reasons.Count > 0)
        {
            return null;
        }

        return new ValidatedDailyResult(campaignName, channelId!.Value, campaign?.CampaignId, resultDate!.Value,
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
    private static async Task<CampaignMatch?> FindCampaignAsync(
        MySqlConnection connection, MySqlTransaction transaction, string campaignName)
    {
        await using var command = new MySqlCommand(
            """
            SELECT c.campaign_id, c.campaign_name, c.channel_id, ch.channel_name
              FROM campaigns c
              JOIN channels ch ON ch.channel_id = c.channel_id
             WHERE c.campaign_name = @name
             LIMIT 1;
            """,
            connection, transaction);
        command.Parameters.AddWithValue("@name", campaignName);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new CampaignMatch(
            reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3));
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
