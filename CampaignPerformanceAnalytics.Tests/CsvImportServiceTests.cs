using System.Text;
using CampaignPerformanceAnalytics.Models;
using Xunit.Abstractions;

namespace CampaignPerformanceAnalytics.Tests;

/// <summary>
/// Integration tests through the one seam, CsvImportService, against campaign_analytics_test.
/// Expected figures come from the ticket and spec (the known contents of the sample CSVs), not from the code.
/// </summary>
[Collection(TestDatabaseCollection.Name)]
public sealed class CsvImportServiceTests(TestDatabase database, VerificationReport report, ITestOutputHelper output)
    : IAsyncLifetime
{
    private const string Header = "campaign_name,channel,date,spend,impressions,clicks,leads,revenue";

    private static readonly DateOnly September1 = new(2026, 9, 1);
    private static readonly DateOnly September2 = new(2026, 9, 2);
    private static readonly DateOnly September10 = new(2026, 9, 10);
    private static readonly DateOnly September30 = new(2026, 9, 30);

    /// <summary>The eight Campaigns in both sample CSVs, in campaign_name order.</summary>
    private static readonly string[] SampleCampaigns =
    [
        "Awareness Banner Display", "Back to School Social", "Brand Keywords Search", "Cart Reminder Email",
        "Competitor Keywords Search", "Fall Newsletter Email", "Influencer Promo Social", "Retargeting Display",
    ];

    // xUnit creates a new instance per test, so this empties the tables before every test.
    public Task InitializeAsync() => database.EmptyResultsAndCampaignsAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public Task Clean_sample_CSV_imports_all_240_Daily_Results_into_8_Campaigns() =>
        RunAsync("01", "Clean sample CSV into empty tables", async checks =>
        {
            var summary = await ImportSampleAsync("campaign_results_clean.csv");

            CheckCounts(checks, summary, rowsRead: 240, imported: 240, duplicates: 0, rejected: 0);
            checks.Equal("Daily Results stored", 240L, await database.CountDailyResultsAsync());
            await CheckSampleCampaignsAsync(checks);
        });

    [Fact]
    public Task Campaign_name_in_different_letter_case_matches_the_existing_Campaign() =>
        RunAsync("01", "Campaign named in different letter case is matched, not created again", async checks =>
        {
            var first = await ImportLinesAsync(Header,
                "Brand Keywords Search,search,2026-09-01,100.00,1000,50,5,200.00");
            var second = await ImportLinesAsync(Header,
                "BRAND keywords search,search,2026-09-02,110.00,1100,55,6,220.00");

            checks.Equal("first import: imported", 1, first.ImportedCount);
            checks.Equal("second import: imported", 1, second.ImportedCount);
            checks.Equal("second import: rejected", 0, second.RejectedCount);

            var campaigns = await database.GetCampaignsAsync();
            checks.SequenceEqual("Campaigns stored (name as first written)", ["Brand Keywords Search"], campaigns.Select(c => c.Name));
            checks.Equal("Daily Results stored", 2L, await database.CountDailyResultsAsync());
            checks.Equal("Campaign start_date", September1, campaigns.FirstOrDefault()?.StartDate);
            checks.Equal("Campaign end_date", September2, campaigns.FirstOrDefault()?.EndDate);
        });

    [Fact]
    public Task Channel_in_different_letter_case_is_matched_and_an_unknown_Channel_is_rejected() =>
        RunAsync("02", "Channel letter case matched; unknown Channel rejected", async checks =>
        {
            var summary = await ImportLinesAsync(Header,
                "Brand Keywords Search,Search,2026-09-01,100.00,1000,50,5,200.00",   // line 2
                "Morning Radio Spot,radio,2026-09-01,90.00,900,40,4,150.00");        // line 3

            CheckCounts(checks, summary, rowsRead: 2, imported: 1, duplicates: 0, rejected: 1);
            checks.SequenceEqual("rejected lines", [3], summary.RejectedRows.Select(r => r.LineNumber));
            checks.Contains("line 3 reason", ReasonsFor(summary, 3), "radio", "Channel");

            var campaigns = await database.GetCampaignsAsync();
            checks.SequenceEqual("Campaigns stored", ["Brand Keywords Search"], campaigns.Select(c => c.Name));
            checks.Equal("Brand Keywords Search: Channel", "search", campaigns.FirstOrDefault()?.ChannelName);
            checks.Equal("Daily Results stored", 1L, await database.CountDailyResultsAsync());
        });

    [Fact]
    public Task Campaign_whose_rows_are_all_invalid_is_not_created() =>
        RunAsync("02", "Every row of one Campaign invalid", async checks =>
        {
            var summary = await ImportLinesAsync(Header,
                "Cart Reminder Email,email,2026-09-01,50.00,500,20,2,80.00",       // line 2: valid
                "Broken Campaign,display,2026-09-31,60.00,600,30,3,90.00",         // line 3: invalid date
                "Broken Campaign,display,2026-09-02,-5.00,600,30,3,90.00",         // line 4: negative spend
                "Cart Reminder Email,email,2026-09-02,55.00,550,22,2,85.00",       // line 5: valid
                "Broken Campaign,display,2026-09-03,60.00,600,abc,3,90.00");       // line 6: clicks not a whole number

            CheckCounts(checks, summary, rowsRead: 5, imported: 2, duplicates: 0, rejected: 3);
            checks.SequenceEqual("rejected lines", [3, 4, 6], summary.RejectedRows.Select(r => r.LineNumber));
            checks.Contains("line 3 reason", ReasonsFor(summary, 3), "2026-09-31");
            checks.Contains("line 4 reason", ReasonsFor(summary, 4), "spend", "-5.00");
            checks.Contains("line 6 reason", ReasonsFor(summary, 6), "clicks", "abc");

            var campaigns = await database.GetCampaignsAsync();
            checks.SequenceEqual("Campaigns stored", ["Cart Reminder Email"], campaigns.Select(c => c.Name));
            checks.Equal("Daily Results stored", 2L, await database.CountDailyResultsAsync());
        });

    [Fact]
    public Task Reimporting_the_clean_sample_CSV_skips_all_240_as_duplicates() =>
        RunAsync("03", "Re-import of the clean sample CSV", async checks =>
        {
            var first = await ImportSampleAsync("campaign_results_clean.csv");
            checks.Equal("first import: imported", 240, first.ImportedCount);

            var second = await ImportSampleAsync("campaign_results_clean.csv");
            CheckCounts(checks, second, rowsRead: 240, imported: 0, duplicates: 240, rejected: 0);
            checks.Equal("Daily Results stored after both imports", 240L, await database.CountDailyResultsAsync());
        });

    [Fact]
    public Task Error_sample_CSV_imports_236_and_skips_1_duplicate_and_4_rejected_rows() =>
        RunAsync("03", "Error sample CSV into empty tables", async checks =>
        {
            var summary = await ImportSampleAsync("campaign_results_errors.csv");

            CheckCounts(checks, summary, rowsRead: 241, imported: 236, duplicates: 1, rejected: 4);
            checks.Equal("Daily Results stored", 236L, await database.CountDailyResultsAsync());
            await CheckSampleCampaignsAsync(checks);
        });

    [Fact]
    public Task Error_sample_CSV_rejects_exactly_lines_45_126_169_and_221() =>
        RunAsync("03", "Error sample CSV: rejected rows and reasons", async checks =>
        {
            var summary = await ImportSampleAsync("campaign_results_errors.csv");

            checks.SequenceEqual("rejected lines", [45, 126, 169, 221], summary.RejectedRows.Select(r => r.LineNumber));
            checks.Contains("line 45 reason (invalid date)", ReasonsFor(summary, 45), "date", "2026-09-31");
            checks.Contains("line 126 reason (invalid date)", ReasonsFor(summary, 126), "date", "2026-13-05");
            checks.Contains("line 169 reason (empty spend)", ReasonsFor(summary, 169), "spend", "empty");
            checks.Contains("line 221 reason (empty clicks)", ReasonsFor(summary, 221), "clicks", "empty");
            foreach (var row in summary.RejectedRows)
            {
                checks.Equal($"line {row.LineNumber}: number of reasons", 1, row.Reasons.Count);
            }
        });

    [Fact]
    public Task Error_sample_CSV_skips_line_202_as_a_duplicate_of_line_71() =>
        RunAsync("03", "Error sample CSV: duplicate at line 202 skipped", async checks =>
        {
            // Line 202 repeats line 71 exactly (Brand Keywords Search, 2026-09-10), so the figures can't tell
            // them apart. Brand Keywords Search has 31 rows in the file and none rejected, so if its 2026-09-10
            // Daily Result is stored once, with line 71's figures, the one row skipped is line 202.
            var summary = await ImportSampleAsync("campaign_results_errors.csv");
            checks.Equal("duplicates skipped", 1, summary.DuplicateCount);

            var dailyResults = await database.GetDailyResultsAsync("Brand Keywords Search");
            checks.Equal("Brand Keywords Search: Daily Results stored", 30, dailyResults.Count);
            checks.Equal("Brand Keywords Search: distinct dates", 30, dailyResults.Select(r => r.Date).Distinct().Count());

            var onSeptember10 = dailyResults.Where(r => r.Date == September10).ToList();
            checks.Equal("Daily Results on 2026-09-10", 1, onSeptember10.Count);
            checks.Equal("2026-09-10 figures (line 71)",
                new StoredDailyResult(September10, 256.76m, 4009, 235, 11, 491.33m),
                onSeptember10.FirstOrDefault());
        });

    [Fact]
    public Task Header_with_a_missing_column_is_refused_and_nothing_is_written() =>
        RunAsync("04", "Header with a missing column refused", async checks =>
        {
            // No leads column; the data rows would otherwise be valid.
            var summary = await ImportLinesAsync(
                "campaign_name,channel,date,spend,impressions,clicks,revenue",
                "Brand Keywords Search,search,2026-09-01,100.00,1000,50,200.00");

            checks.Equal("upload refused", true, summary.Failed);
            checks.Contains("refusal message", summary.ErrorMessage, "missing column", "'leads'");
            await CheckTablesEmptyAsync(checks);
        });

    [Fact]
    public Task Empty_file_is_refused_and_nothing_is_written() =>
        RunAsync("04", "Empty file refused", async checks =>
        {
            var summary = await ImportLinesAsync();

            checks.Equal("upload refused", true, summary.Failed);
            checks.Contains("refusal message", summary.ErrorMessage, "empty");
            await CheckTablesEmptyAsync(checks);
        });

    [Fact]
    public Task Reordered_differently_cased_header_imports_normally() =>
        RunAsync("04", "Reordered, differently-cased header imports", async checks =>
        {
            var summary = await ImportLinesAsync(
                "Revenue,LEADS,clicks,Impressions,SPEND,Date,Channel,Campaign_Name",
                "200.00,5,50,1000,100.00,2026-09-01,search,Brand Keywords Search",
                "220.00,6,55,1100,110.00,2026-09-02,search,Brand Keywords Search");

            CheckCounts(checks, summary, rowsRead: 2, imported: 2, duplicates: 0, rejected: 0);
            checks.Equal("Daily Results stored", 2L, await database.CountDailyResultsAsync());
            checks.Equal("2026-09-01 figures, read by column name",
                new StoredDailyResult(September1, 100.00m, 1000, 50, 5, 200.00m),
                (await database.GetDailyResultsAsync("Brand Keywords Search")).FirstOrDefault());
        });

    [Fact]
    public Task Row_naming_a_Campaign_on_a_different_Channel_is_rejected() =>
        RunAsync("05", "Channel mismatch rejected (optional)", async checks =>
        {
            // Brand Keywords Search is already stored on search; Cart Reminder Email is created on email
            // by line 3 of the second import. A later row naming either on another Channel is rejected.
            await ImportLinesAsync(Header,
                "Brand Keywords Search,search,2026-09-01,100.00,1000,50,5,200.00");
            var summary = await ImportLinesAsync(Header,
                "Brand Keywords Search,email,2026-09-02,110.00,1100,55,6,220.00",    // line 2: differs from the stored Channel
                "Cart Reminder Email,email,2026-09-01,50.00,500,20,2,80.00",         // line 3: creates the Campaign on email
                "Cart Reminder Email,search,2026-09-02,55.00,550,22,2,85.00",        // line 4: differs from line 3's Channel
                "Brand Keywords Search,search,2026-09-02,110.00,1100,55,6,220.00");  // line 5: right Channel

            CheckCounts(checks, summary, rowsRead: 4, imported: 2, duplicates: 0, rejected: 2);
            checks.SequenceEqual("rejected lines", [2, 4], summary.RejectedRows.Select(r => r.LineNumber));
            checks.Contains("line 2 reason names both Channels", ReasonsFor(summary, 2), "'search'", "'email'");
            checks.Contains("line 4 reason names both Channels", ReasonsFor(summary, 4), "'email'", "'search'");

            var campaigns = await database.GetCampaignsAsync();
            checks.SequenceEqual("Campaigns stored", ["Brand Keywords Search", "Cart Reminder Email"], campaigns.Select(c => c.Name));
            checks.SequenceEqual("Campaign Channels", ["search", "email"], campaigns.Select(c => c.ChannelName));
            checks.Equal("Daily Results stored", 3L, await database.CountDailyResultsAsync());
        });

    // ---- helpers ----

    /// <summary>
    /// Runs one test's body with its own set of recorded checks, then fails the test if any check
    /// didn't match. An exception is recorded in the report before it fails the test.
    /// </summary>
    /// <param name="sourceTicket">The csv-import ticket whose behaviour the test covers, e.g. "03".</param>
    private async Task RunAsync(string sourceTicket, string title, Func<TestChecks, Task> body)
    {
        var checks = report.StartTest(ReportFeature.CsvImport, sourceTicket, title, output);
        try
        {
            await body(checks);
        }
        catch (Exception ex)
        {
            checks.Stopped(ex);
            throw;
        }
        checks.AssertAll();
    }

    private async Task<ImportSummary> ImportSampleAsync(string fileName)
    {
        await using var stream = File.OpenRead(RepositoryPaths.SampleData(fileName));
        return await database.CreateImportService().ImportAsync(stream, fileName);
    }

    private async Task<ImportSummary> ImportLinesAsync(params string[] lines)
    {
        var text = lines.Length == 0 ? "" : string.Join("\n", lines) + "\n";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        return await database.CreateImportService().ImportAsync(stream, "test.csv");
    }

    /// <summary>All reasons recorded for one rejected line, or null if that line wasn't rejected.</summary>
    private static string? ReasonsFor(ImportSummary summary, int lineNumber)
    {
        var row = summary.RejectedRows.SingleOrDefault(r => r.LineNumber == lineNumber);
        return row is null ? null : string.Join(" ", row.Reasons);
    }

    private static void CheckCounts(TestChecks checks, ImportSummary summary,
        int rowsRead, int imported, int duplicates, int rejected)
    {
        checks.Equal("file-level error", (string?)null, summary.ErrorMessage);
        checks.Equal("rows read", rowsRead, summary.RowsRead);
        checks.Equal("imported", imported, summary.ImportedCount);
        checks.Equal("duplicates skipped", duplicates, summary.DuplicateCount);
        checks.Equal("rejected", rejected, summary.RejectedCount);
    }

    private async Task CheckTablesEmptyAsync(TestChecks checks)
    {
        checks.Equal("Daily Results stored", 0L, await database.CountDailyResultsAsync());
        checks.Equal("Campaigns stored", 0, (await database.GetCampaignsAsync()).Count);
    }

    /// <summary>Both sample CSVs leave the same 8 Campaigns, each dated 2026-09-01 to 2026-09-30 with no ROI Goal.</summary>
    private async Task CheckSampleCampaignsAsync(TestChecks checks)
    {
        var campaigns = await database.GetCampaignsAsync();
        checks.SequenceEqual("Campaigns stored", SampleCampaigns, campaigns.Select(c => c.Name));
        foreach (var campaign in campaigns)
        {
            checks.Equal($"{campaign.Name}: start_date", September1, campaign.StartDate);
            checks.Equal($"{campaign.Name}: end_date", September30, campaign.EndDate);
            checks.Equal($"{campaign.Name}: roi_goal", (decimal?)null, campaign.RoiGoal);
        }
    }
}
