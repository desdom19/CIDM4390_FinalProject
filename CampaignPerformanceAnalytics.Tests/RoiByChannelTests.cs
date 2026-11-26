using System.Text;
using CampaignPerformanceAnalytics.Models;
using CampaignPerformanceAnalytics.Services;
using Xunit.Abstractions;

namespace CampaignPerformanceAnalytics.Tests;

/// <summary>
/// Integration tests of the ROI by Channel figures through the DashboardService seam, against
/// campaign_analytics_test; the returned Channel totals are also passed through RoiChart. Daily
/// Results are loaded through CsvImportService. Expected figures come from the ROI by Channel spec
/// and ticket (the known per-Channel totals of the clean sample CSV), not from the code.
/// </summary>
[Collection(TestDatabaseCollection.Name)]
public sealed class RoiByChannelTests(TestDatabase database, VerificationReport report, ITestOutputHelper output)
    : IAsyncLifetime
{
    private const string Header = "campaign_name,channel,date,spend,impressions,clicks,leads,revenue";

    // xUnit creates a new instance per test, so this empties the tables before every test.
    public Task InitializeAsync() => database.EmptyResultsAndCampaignsAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public Task Clean_sample_CSV_gives_each_Channels_totals_ROI_and_label() =>
        RunAsync("01", "Clean sample CSV: each Channel's spend, revenue, ROI and label", async checks =>
        {
            await ImportSampleAsync("campaign_results_clean.csv");

            var (totals, channels) = await GetDashboardAsync();
            var byName = channels.ToDictionary(c => c.Name);

            CheckChannel(checks, byName, "email", 2875.19m, 7333.83m, 1.5507m);
            CheckChannel(checks, byName, "social", 13750.29m, 17155.23m, 0.2476m);
            CheckChannel(checks, byName, "search", 18451.80m, 42691.58m, 1.3137m);
            CheckChannel(checks, byName, "display", 10178.85m, 14897.67m, 0.4636m);

            // Same snapshot: the Channel spends add up to the Total Spend tile's figure.
            checks.Equal("Total Spend", 45256.13m, totals.TotalSpend);
            checks.Equal("Sum of Channel spends", 45256.13m, channels.Sum(c => c.TotalSpend));

            var bars = RoiChart.Build(channels).Bars.ToDictionary(b => b.Name, b => b.Label);
            checks.Equal("Email bar", "155.1%", bars.GetValueOrDefault("Email"));
            checks.Equal("Social bar", "24.8%", bars.GetValueOrDefault("Social"));
            checks.Equal("Search bar", "131.4%", bars.GetValueOrDefault("Search"));
            checks.Equal("Display bar", "46.4%", bars.GetValueOrDefault("Display"));
        });

    [Fact]
    public Task Channel_ROI_is_calculated_from_its_totals_not_averaged_over_Campaigns() =>
        RunAsync("01", "Channel ROI uses totals, not an average of its Campaigns", async checks =>
        {
            // The glossary's ROI example on one Channel: Campaign ROIs of +200% and −50% average +75%,
            // but the Channel's totals give (5,300 − 10,100) ÷ 10,100 = −47.5%.
            await ImportLinesAsync(Header,
                "Brand Keywords Search,search,2026-09-01,100.00,1000,50,5,300.00",
                "Generic Keywords Search,search,2026-09-01,10000.00,90000,400,20,5000.00");

            var (_, channels) = await GetDashboardAsync();
            var search = channels.Single(c => c.Name == "search");

            checks.Equal("search spend", 10100m, search.TotalSpend);
            checks.Equal("search revenue", 5300m, search.TotalRevenue);

            var bar = RoiChart.Build(channels).Bars.Single(b => b.Name == "Search");
            checks.Equal("Search bar", "-47.5%", bar.Label);
        });

    // ---- helpers ----

    private static void CheckChannel(TestChecks checks, IReadOnlyDictionary<string, ChannelTotals> byName,
        string name, decimal spend, decimal revenue, decimal roiTo4Decimals)
    {
        var channel = byName.GetValueOrDefault(name);
        checks.Equal($"{name} spend", (decimal?)spend, channel?.TotalSpend);
        checks.Equal($"{name} revenue", (decimal?)revenue, channel?.TotalRevenue);
        checks.Equal($"{name} ROI (to 4 decimals)", (decimal?)roiTo4Decimals,
            channel?.Roi is { } roi ? Math.Round(roi, 4) : null);
    }

    /// <summary>
    /// Runs one test's body with its own set of recorded checks, then fails the test if any check
    /// didn't match. An exception is recorded in the report before it fails the test.
    /// </summary>
    /// <param name="sourceTicket">The roi-by-channel-bar-chart ticket whose behaviour the test covers, e.g. "01".</param>
    private async Task RunAsync(string sourceTicket, string title, Func<TestChecks, Task> body)
    {
        var checks = report.StartTest(ReportFeature.RoiByChannelBarChart, sourceTicket, title, output);
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

    /// <summary>Reads the dashboard, requiring the read to have succeeded.</summary>
    private async Task<(KpiTotals Totals, IReadOnlyList<ChannelTotals> Channels)> GetDashboardAsync()
    {
        var result = await database.CreateDashboardService().GetDashboardAsync();
        Assert.False(result.Failed, result.ErrorMessage);
        return (result.Totals, result.ChannelTotals);
    }

    private async Task ImportSampleAsync(string fileName)
    {
        await using var stream = File.OpenRead(RepositoryPaths.SampleData(fileName));
        var summary = await database.CreateImportService().ImportAsync(stream, fileName);
        // The figures only mean something if the file was imported.
        Assert.Null(summary.ErrorMessage);
    }

    /// <summary>Imports a small inline CSV, one string per line, and requires every row to be imported.</summary>
    private async Task ImportLinesAsync(params string[] lines)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n", lines) + "\n"));
        var summary = await database.CreateImportService().ImportAsync(stream, "test.csv");
        // The figures only mean something if every data row was imported.
        Assert.Null(summary.ErrorMessage);
        Assert.Equal(lines.Length - 1, summary.ImportedCount);
    }
}
