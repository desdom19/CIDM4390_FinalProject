using System.Globalization;
using CampaignPerformanceAnalytics.Services;
using Xunit.Abstractions;

namespace CampaignPerformanceAnalytics.Tests;

/// <summary>
/// Pure unit tests of KpiFormatter; they touch no database. They sit in the "Test database"
/// collection only to share its VerificationReport. Expected text comes from the dashboard spec's table.
/// </summary>
[Collection(TestDatabaseCollection.Name)]
public sealed class KpiFormatterTests(VerificationReport report, ITestOutputHelper output)
{
    // A decimal can't be an attribute argument, so money inputs are written as invariant-culture text.
    [Theory]
    [InlineData("1234567.5", "$1,234,567.50")]
    [InlineData("0", "$0.00")]
    public void Money_is_dollars_with_commas_and_two_decimals(string input, string expected)
    {
        var value = decimal.Parse(input, CultureInfo.InvariantCulture);
        Check("01", $"Money {input}", expected, KpiFormatter.Money(value));
    }

    [Theory]
    [InlineData(0L, "0")]
    [InlineData(4995183L, "4,995,183")]
    public void Count_is_a_whole_number_with_commas(long input, string expected) =>
        Check("01", $"Count {input}", expected, KpiFormatter.Count(input));

    /// <param name="sourceTicket">The dashboard-kpi-tiles ticket whose behaviour the test covers, e.g. "01".</param>
    private void Check(string sourceTicket, string title, string expected, string actual)
    {
        var checks = report.StartTest(ReportFeature.DashboardKpiTiles, sourceTicket, $"Formatter: {title}", output);
        checks.Equal("display text", expected, actual);
        checks.AssertAll();
    }
}
