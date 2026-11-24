using System.Text;
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

            var totals = (await database.CreateDashboardService().GetKpiTotalsAsync()).Totals;

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

            // Ticket 02: (82,078.31 âˆ’ 45,256.13) Ã· 45,256.13 = 0.813640â€¦, checked to 4 decimals.
            checks.Equal("Overall ROI (to 4 decimals)", 0.8136m, RoundedTo4Decimals(totals.OverallRoi));
            checks.Equal("Overall ROI tile", "81.4%", KpiFormatter.Roi(totals.OverallRoi));
        });

    [Fact]
    public Task Zero_total_spend_gives_no_ROI_but_keeps_the_real_revenue() =>
        RunAsync("02", "Zero total spend: ROI N/A, revenue still shown", async checks =>
        {
            await ImportLinesAsync(Header,
                "Cart Reminder Email,email,2026-09-01,0.00,500,20,2,120.50",
                "Cart Reminder Email,email,2026-09-02,0.00,450,18,1,79.25");

            var totals = (await database.CreateDashboardService().GetKpiTotalsAsync()).Totals;

            checks.Equal("Total Spend", 0m, totals.TotalSpend);
            checks.Equal("Overall ROI", (decimal?)null, totals.OverallRoi);
            checks.Equal("Overall ROI tile", "N/A (spend is 0)", KpiFormatter.Roi(totals.OverallRoi));
            checks.Equal("Total Revenue tile", "$199.75", KpiFormatter.Money(totals.TotalRevenue));
        });

    [Fact]
    public Task Overall_ROI_is_calculated_from_the_totals_not_averaged_over_Campaigns() =>
        RunAsync("02", "Overall ROI uses totals, not an average", async checks =>
        {
            // The glossary's ROI example: ROIs of +200% and âˆ’50% average +75%, but together
            // (5,300 âˆ’ 10,100) Ã· 10,100 = âˆ’47.5%.
            await ImportLinesAsync(Header,
                "Brand Keywords Search,search,2026-09-01,100.00,1000,50,5,300.00",
                "Awareness Banner Display,display,2026-09-01,10000.00,90000,400,20,5000.00");

            var totals = (await database.CreateDashboardService().GetKpiTotalsAsync()).Totals;

            checks.Equal("Overall ROI tile", "-47.5%", KpiFormatter.Roi(totals.OverallRoi));
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
