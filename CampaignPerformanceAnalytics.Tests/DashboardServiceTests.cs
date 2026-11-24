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
    // xUnit creates a new instance per test, so this empties the tables before every test.
    public Task InitializeAsync() => database.EmptyResultsAndCampaignsAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public Task Clean_sample_CSV_gives_the_five_totals_and_their_display_text() =>
        RunAsync("01", "Clean sample CSV: the five totals", async checks =>
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

    private async Task ImportSampleAsync(string fileName)
    {
        await using var stream = File.OpenRead(RepositoryPaths.SampleData(fileName));
        var summary = await database.CreateImportService().ImportAsync(stream, fileName);
        // The totals only mean something if the file was imported.
        Assert.Null(summary.ErrorMessage);
    }
}
