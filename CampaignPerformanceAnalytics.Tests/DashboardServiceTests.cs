using System.Text;
using CampaignPerformanceAnalytics.Models;
using CampaignPerformanceAnalytics.Services;
using Xunit.Abstractions;

namespace CampaignPerformanceAnalytics.Tests;

/// <summary>
/// Integration tests through the DashboardService seam, against campaign_analytics_test.
/// Daily Results are loaded through CsvImportService. Expected figures come from the dashboard
/// spec and ticket (the known totals of the sample CSVs), not from the code.
/// </summary>
[Collection(TestDatabaseCollection.Name)]
public sealed class DashboardServiceTests(TestDatabase database, VerificationReport report, ITestOutputHelper output)
    : IAsyncLifetime
{
    private const string Header = "campaign_name,channel,date,spend,impressions,clicks,leads,revenue";

    // xUnit creates a new instance per test, so this empties the tables before every test.
    public Task InitializeAsync() => database.EmptyResultsAndCampaignsAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public Task Clean_sample_CSV_gives_the_six_tile_figures_and_their_display_text() =>
        RunAsync("01", "Clean sample CSV: the five totals and Overall ROI", async checks =>
        {
            await ImportSampleAsync("campaign_results_clean.csv");

            var totals = await GetTotalsAsync();

            checks.Equal("Total Spend", 45256.13m, totals.TotalSpend);
            checks.Equal("Total Revenue", 82078.31m, totals.TotalRevenue);
            checks.Equal("Total Leads", 1630L, totals.TotalLeads);
            checks.Equal("Total Impressions", 4995183L, totals.TotalImpressions);
            checks.Equal("Total Clicks", 37882L, totals.TotalClicks);

            checks.Equal("Total Spend tile", "$45,256.13", KpiFormatter.Money(totals.TotalSpend));
            checks.Equal("Total Revenue tile", "$82,078.31", KpiFormatter.Money(totals.TotalRevenue));
            checks.Equal("Total Leads tile", "1,630", KpiFormatter.Count(totals.TotalLeads));
            checks.Equal("Total Impressions tile", "4,995,183", KpiFormatter.Count(totals.TotalImpressions));
            checks.Equal("Total Clicks tile", "37,882", KpiFormatter.Count(totals.TotalClicks));

            // Ticket 02: (82,078.31 − 45,256.13) ÷ 45,256.13 = 0.813640…, checked to 4 decimals.
            checks.Equal("Overall ROI (to 4 decimals)", 0.8136m, RoundedTo4Decimals(totals.OverallRoi));
            checks.Equal("Overall ROI tile", "81.4%", KpiFormatter.Roi(totals.OverallRoi));
        });

    /// <summary>
    /// The zero-spend test's tile checks stay in this feature's report; ROI by Channel ticket 03
    /// extends the test with a second set of chart checks, written to that PBI's report. Both sets must match.
    /// </summary>
    [Fact]
    public async Task Zero_total_spend_gives_no_ROI_but_keeps_the_real_revenue()
    {
        var tileChecks = report.StartTest(ReportFeature.DashboardKpiTiles, "02",
            "Zero total spend: ROI N/A, revenue still shown", output);
        var chartChecks = report.StartTest(ReportFeature.RoiByChannelBarChart, "03",
            "Zero total spend: chart shows the no-spend message (extends PBI 5's zero-spend test)", output);
        try
        {
            await ImportLinesAsync(Header,
                "Cart Reminder Email,email,2026-09-01,0.00,500,20,2,120.50",
                "Cart Reminder Email,email,2026-09-02,0.00,450,18,1,79.25");

            var (totals, channels) = await GetDashboardAsync();

            tileChecks.Equal("Total Spend", 0m, totals.TotalSpend);
            tileChecks.Equal("Overall ROI", (decimal?)null, totals.OverallRoi);
            tileChecks.Equal("Overall ROI tile", "N/A (spend is 0)", KpiFormatter.Roi(totals.OverallRoi));
            tileChecks.Equal("Total Revenue tile", "$199.75", KpiFormatter.Money(totals.TotalRevenue));

            var chart = RoiChart.Build(channels, totals.HasDailyResults);

            chartChecks.Equal("state", RoiChartState.NoSpend, chart.State);
            chartChecks.Equal("bar count", 0, chart.Bars.Count);
            chartChecks.SequenceEqual("left out", ["Display", "Email", "Search", "Social"], chart.LeftOut);
        }
        catch (Exception ex)
        {
            tileChecks.Stopped(ex);
            chartChecks.Stopped(ex);
            throw;
        }
        tileChecks.AssertAll();
        chartChecks.AssertAll();
    }

    [Fact]
    public Task Overall_ROI_is_calculated_from_the_totals_not_averaged_over_Campaigns() =>
        RunAsync("02", "Overall ROI uses totals, not an average", async checks =>
        {
            // The glossary's ROI example: ROIs of +200% and −50% average +75%, but together
            // (5,300 − 10,100) ÷ 10,100 = −47.5%.
            await ImportLinesAsync(Header,
                "Brand Keywords Search,search,2026-09-01,100.00,1000,50,5,300.00",
                "Awareness Banner Display,display,2026-09-01,10000.00,90000,400,20,5000.00");

            var totals = await GetTotalsAsync();

            checks.Equal("Overall ROI tile", "-47.5%", KpiFormatter.Roi(totals.OverallRoi));
        });

    [Fact]
    public Task Empty_tables_give_zero_tiles_and_show_there_are_no_Daily_Results() =>
        RunAsync("03", "Empty tables: zero tiles, ROI N/A, no Daily Results", async checks =>
        {
            // InitializeAsync has already emptied results and campaigns.
            var totals = await GetTotalsAsync();

            checks.Equal("Has Daily Results", false, totals.HasDailyResults);

            checks.Equal("Total Spend", 0m, totals.TotalSpend);
            checks.Equal("Total Revenue", 0m, totals.TotalRevenue);
            checks.Equal("Total Leads", 0L, totals.TotalLeads);
            checks.Equal("Total Impressions", 0L, totals.TotalImpressions);
            checks.Equal("Total Clicks", 0L, totals.TotalClicks);
            checks.Equal("Overall ROI", (decimal?)null, totals.OverallRoi);

            checks.Equal("Total Spend tile", "$0.00", KpiFormatter.Money(totals.TotalSpend));
            checks.Equal("Total Revenue tile", "$0.00", KpiFormatter.Money(totals.TotalRevenue));
            checks.Equal("Total Leads tile", "0", KpiFormatter.Count(totals.TotalLeads));
            checks.Equal("Total Impressions tile", "0", KpiFormatter.Count(totals.TotalImpressions));
            checks.Equal("Total Clicks tile", "0", KpiFormatter.Count(totals.TotalClicks));
            checks.Equal("Overall ROI tile", "N/A (spend is 0)", KpiFormatter.Roi(totals.OverallRoi));
        });

    // ---- helpers ----

    /// <summary>
    /// Runs one test's body with its own set of recorded checks, then fails the test if any check
    /// didn't match. An exception is recorded in the report before it fails the test.
    /// </summary>
    /// <param name="sourceTicket">The dashboard-kpi-tiles ticket whose behaviour the test covers, e.g. "01".</param>
    private async Task RunAsync(string sourceTicket, string title, Func<TestChecks, Task> body)
    {
        var checks = report.StartTest(ReportFeature.DashboardKpiTiles, sourceTicket, title, output);
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

    /// <summary>Reads the totals, requiring the read to have succeeded.</summary>
    private async Task<KpiTotals> GetTotalsAsync() => (await GetDashboardAsync()).Totals;

    /// <summary>Reads the totals and the Channel totals, requiring the read to have succeeded.</summary>
    private async Task<(KpiTotals Totals, IReadOnlyList<ChannelTotals> Channels)> GetDashboardAsync()
    {
        var result = await database.CreateDashboardService().GetDashboardAsync();
        Assert.False(result.Failed, result.ErrorMessage);
        return (result.Totals, result.ChannelTotals);
    }

    private static decimal? RoundedTo4Decimals(decimal? value) => value is null ? null : Math.Round(value.Value, 4);

    private async Task ImportSampleAsync(string fileName)
    {
        await using var stream = File.OpenRead(RepositoryPaths.SampleData(fileName));
        var summary = await database.CreateImportService().ImportAsync(stream, fileName);
        // The totals only mean something if the file was imported.
        Assert.Null(summary.ErrorMessage);
    }

    /// <summary>Imports a small inline CSV, one string per line, and requires every row to be imported.</summary>
    private async Task ImportLinesAsync(params string[] lines)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n", lines) + "\n"));
        var summary = await database.CreateImportService().ImportAsync(stream, "test.csv");
        // The totals only mean something if every data row was imported.
        Assert.Null(summary.ErrorMessage);
        Assert.Equal(lines.Length - 1, summary.ImportedCount);
    }
}

/// <summary>
/// The dashboard when the database can't be reached. These tests touch no database at all: there is
/// no table-emptying setup, and the service is pointed at a port where nothing listens. They stay in
/// the "Test database" collection only so that their checks are recorded in the dashboard report.
/// </summary>
[Collection(TestDatabaseCollection.Name)]
public sealed class DashboardServiceDatabaseFailureTests(VerificationReport report, ITestOutputHelper output)
{
    private const string UnreachableConnectionString =
        "Server=127.0.0.1;Port=1;Database=campaign_analytics_test;User ID=nobody;Password=none;Connection Timeout=2";

    [Fact]
    public async Task Unreachable_database_gives_an_error_result_with_no_totals()
    {
        var checks = report.StartTest(ReportFeature.DashboardKpiTiles, "03",
            "Database can't be reached: error message, no totals, no Channel totals", output);
        try
        {
            var result = await new DashboardService(UnreachableConnectionString).GetDashboardAsync();

            checks.Equal("Failed", true, result.Failed);
            checks.Equal("Error message",
                "The dashboard couldn't load because of a database error. Try again later.", result.ErrorMessage);
            checks.Equal("Totals", (KpiTotals?)null, result.Totals);
            // ROI by Channel ticket 03: the chart gets no figures either, so a failure can't show as empty bars.
            checks.Equal("Channel totals", (IReadOnlyList<ChannelTotals>?)null, result.ChannelTotals);
        }
        catch (Exception ex)
        {
            checks.Stopped(ex);
            throw;
        }
        checks.AssertAll();
    }
}
