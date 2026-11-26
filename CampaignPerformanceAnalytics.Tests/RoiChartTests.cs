using System.Globalization;
using CampaignPerformanceAnalytics.Models;
using CampaignPerformanceAnalytics.Services;
using Xunit.Abstractions;

namespace CampaignPerformanceAnalytics.Tests;

/// <summary>
/// Pure unit tests of RoiChart; they touch no database. They sit in the "Test database"
/// collection only to share its VerificationReport. Expected figures are worked out by hand
/// from the spec's rules (width = ROI ÷ largest ROI × 100, zero line at 0).
/// </summary>
[Collection(TestDatabaseCollection.Name)]
public sealed class RoiChartTests(VerificationReport report, ITestOutputHelper output)
{
    [Fact]
    public void Widths_are_proportional_to_the_ROI_with_the_largest_at_100() =>
        Run("01", "Chart: widths proportional to ROI, largest at 100", checks =>
        {
            // ROIs 200%, 100% and 50%: (300 − 100) ÷ 100, (200 − 100) ÷ 100, (150 − 100) ÷ 100.
            var chart = RoiChart.Build([
                new ChannelTotals("search", 100m, 300m),
                new ChannelTotals("email", 100m, 200m),
                new ChannelTotals("social", 100m, 150m),
            ]);

            checks.SequenceEqual("widths", [100m, 50m, 25m], chart.Bars.Select(b => b.Width));
        });

    [Fact]
    public void The_zero_line_is_at_the_left_edge() =>
        Run("01", "Chart: zero line at 0", checks =>
        {
            var chart = RoiChart.Build([
                new ChannelTotals("email", 100m, 250m),
                new ChannelTotals("search", 100m, 120m),
            ]);

            checks.Equal("zero line", 0m, chart.ZeroLine);
        });

    [Fact]
    public void An_ROI_of_exactly_0_has_width_0() =>
        Run("01", "Chart: ROI of exactly 0 has width 0", checks =>
        {
            // Display breaks even: (500 − 500) ÷ 500 = 0.
            var chart = RoiChart.Build([
                new ChannelTotals("email", 100m, 300m),
                new ChannelTotals("display", 500m, 500m),
            ]);

            var display = chart.Bars[1];
            checks.Equal("display width", 0m, display.Width);
            checks.Equal("display label", "0.0%", display.Label);
        });

    [Fact]
    public void Display_names_are_the_stored_names_capitalised() =>
        Run("01", "Chart: display names capitalised", checks =>
        {
            var chart = RoiChart.Build([
                new ChannelTotals("email", 100m, 200m),
                new ChannelTotals("social", 100m, 200m),
                new ChannelTotals("search", 100m, 200m),
                new ChannelTotals("display", 100m, 200m),
            ]);

            checks.SequenceEqual("display names", ["Email", "Social", "Search", "Display"], chart.Bars.Select(b => b.Name));
        });

    [Fact]
    public void Widths_are_the_same_whatever_the_current_culture() =>
        Run("01", "Chart: widths the same in a comma-decimal culture (de-DE)", checks =>
        {
            var original = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            try
            {
                // ROIs 300% and 100%: the second bar is a third of the first, 33.33…%.
                var chart = RoiChart.Build([
                    new ChannelTotals("email", 100m, 400m),
                    new ChannelTotals("social", 100m, 200m),
                ]);

                checks.SequenceEqual("CSS widths", ["100%", "33.33%"], chart.Bars.Select(b => b.CssWidth));
                checks.SequenceEqual("labels", ["300.0%", "100.0%"], chart.Bars.Select(b => b.Label));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        });

    [Fact]
    public void No_Channel_with_spend_gives_no_bars() =>
        Run("01", "Chart: no Channel with spend gives no bars (the empty dashboard still loads)", checks =>
        {
            var chart = RoiChart.Build([
                new ChannelTotals("email", 0m, 0m),
                new ChannelTotals("social", 0m, 0m),
                new ChannelTotals("search", 0m, 0m),
                new ChannelTotals("display", 0m, 0m),
            ]);

            checks.Equal("bar count", 0, chart.Bars.Count);
        });

    // Until ticket 02 draws negative bars growing left from a zero line, a Channel with an ROI of
    // 0 or below shows its name and label with no bar: never a crash, never a loss drawn as a gain.

    [Fact]
    public void Every_ROI_exactly_0_gives_width_0_and_no_division_error() =>
        Run("01", "Chart guard: every ROI exactly 0 gives no bars and no division error", checks =>
        {
            var chart = RoiChart.Build([
                new ChannelTotals("email", 100m, 100m),
                new ChannelTotals("social", 250m, 250m),
            ]);

            checks.SequenceEqual("widths", [0m, 0m], chart.Bars.Select(b => b.Width));
            checks.SequenceEqual("labels", ["0.0%", "0.0%"], chart.Bars.Select(b => b.Label));
        });

    [Fact]
    public void Every_ROI_negative_draws_no_bars_but_keeps_the_labels() =>
        Run("01", "Chart guard: every ROI negative draws no bars, labels kept", checks =>
        {
            // (52.50 − 100) ÷ 100 = −47.5%; (70 − 100) ÷ 100 = −30.0%.
            var chart = RoiChart.Build([
                new ChannelTotals("search", 100m, 52.50m),
                new ChannelTotals("display", 100m, 70m),
            ]);

            checks.SequenceEqual("widths", [0m, 0m], chart.Bars.Select(b => b.Width));
            checks.SequenceEqual("labels", ["-47.5%", "-30.0%"], chart.Bars.Select(b => b.Label));
        });

    [Fact]
    public void A_negative_ROI_beside_positive_ones_draws_no_bar_and_positives_scale_to_the_largest() =>
        Run("01", "Chart guard: a negative ROI beside positives draws no bar", checks =>
        {
            // ROIs 200%, 100% and −30%.
            var chart = RoiChart.Build([
                new ChannelTotals("email", 100m, 300m),
                new ChannelTotals("search", 100m, 200m),
                new ChannelTotals("display", 100m, 70m),
            ]);

            checks.SequenceEqual("widths", [100m, 50m, 0m], chart.Bars.Select(b => b.Width));
            checks.SequenceEqual("CSS widths", ["100%", "50%", "0%"], chart.Bars.Select(b => b.CssWidth));
            checks.Equal("display label", "-30.0%", chart.Bars[2].Label);
        });

    private void Run(string sourceTicket, string title, Action<TestChecks> body)
    {
        var checks = report.StartTest(ReportFeature.RoiByChannelBarChart, sourceTicket, title, output);
        try
        {
            body(checks);
        }
        catch (Exception ex)
        {
            checks.Stopped(ex);
            throw;
        }
        checks.AssertAll();
    }
}
