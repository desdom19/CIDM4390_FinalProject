using System.Globalization;
using CampaignPerformanceAnalytics.Models;
using CampaignPerformanceAnalytics.Services;
using Xunit.Abstractions;

namespace CampaignPerformanceAnalytics.Tests;

/// <summary>
/// Pure unit tests of RoiChart; they touch no database. They sit in the "Test database"
/// collection only to share its VerificationReport. Expected figures are worked out by hand
/// from the spec's rules: scale = largest positive ROI + largest negative ROI's absolute value,
/// width = |ROI| ÷ scale × 100, zero line = largest negative ROI's absolute value ÷ scale × 100.
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

            // Every ROI is 100%, so ticket 02's tie rule orders them by stored name A to Z.
            checks.SequenceEqual("display names", ["Display", "Email", "Search", "Social"], chart.Bars.Select(b => b.Name));
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

    [Fact]
    public void Every_ROI_exactly_0_gives_the_zero_line_at_0_and_no_division_error() =>
        Run("02", "Chart: every ROI exactly 0 gives zero line 0, widths 0, no division error", checks =>
        {
            var chart = RoiChart.Build([
                new ChannelTotals("email", 100m, 100m),
                new ChannelTotals("social", 250m, 250m),
            ]);

            checks.Equal("zero line", 0m, chart.ZeroLine);
            checks.SequenceEqual("widths", [0m, 0m], chart.Bars.Select(b => b.Width));
            checks.SequenceEqual("negative", [false, false], chart.Bars.Select(b => b.IsNegative));
            checks.SequenceEqual("labels", ["0.0%", "0.0%"], chart.Bars.Select(b => b.Label));
        });

    // ---- ticket 02: sorting, mixed scaling and negative bars ----

    [Fact]
    public void Bars_are_sorted_by_exact_ROI_even_when_two_labels_round_the_same() =>
        Run("02", "Chart: sorted by exact ROI, highest first, even when two labels round the same", checks =>
        {
            // Social (112,345 − 100,000) ÷ 100,000 = 12.345% and email 12.340% both show 12.3%,
            // so only the exact ROI puts social above email. Search 50% is top, display 5% bottom.
            var chart = RoiChart.Build([
                new ChannelTotals("display", 100m, 105m),
                new ChannelTotals("email", 100000m, 112340m),
                new ChannelTotals("search", 100m, 150m),
                new ChannelTotals("social", 100000m, 112345m),
            ]);

            checks.SequenceEqual("order", ["Search", "Social", "Email", "Display"], chart.Bars.Select(b => b.Name));
            checks.SequenceEqual("labels", ["50.0%", "12.3%", "12.3%", "5.0%"], chart.Bars.Select(b => b.Label));
        });

    [Fact]
    public void Exact_ties_are_ordered_by_stored_name_A_to_Z() =>
        Run("02", "Chart: exact ROI ties ordered by stored name A to Z", checks =>
        {
            // Every Channel's ROI is exactly 100%, given Z to A.
            var chart = RoiChart.Build([
                new ChannelTotals("social", 100m, 200m),
                new ChannelTotals("search", 50m, 100m),
                new ChannelTotals("email", 10m, 20m),
                new ChannelTotals("display", 1m, 2m),
            ]);

            checks.SequenceEqual("order", ["Display", "Email", "Search", "Social"], chart.Bars.Select(b => b.Name));
        });

    [Fact]
    public void Mixed_values_put_the_zero_line_partway_across_on_one_shared_scale() =>
        Run("02", "Chart: mixed values put the zero line partway across, one shared scale", checks =>
        {
            // ROIs 200%, 100% and −50%. Scale = 2 + 0.5 = 2.5, so the widths are 2 ÷ 2.5 = 80,
            // 1 ÷ 2.5 = 40 and 0.5 ÷ 2.5 = 20, and the zero line sits at 0.5 ÷ 2.5 = 20.
            var chart = RoiChart.Build([
                new ChannelTotals("display", 100m, 50m),
                new ChannelTotals("email", 100m, 300m),
                new ChannelTotals("search", 100m, 200m),
            ]);

            checks.SequenceEqual("order", ["Email", "Search", "Display"], chart.Bars.Select(b => b.Name));
            checks.SequenceEqual("widths", [80m, 40m, 20m], chart.Bars.Select(b => b.Width));
            checks.SequenceEqual("negative", [false, false, true], chart.Bars.Select(b => b.IsNegative));
            checks.SequenceEqual("labels", ["200.0%", "100.0%", "-50.0%"], chart.Bars.Select(b => b.Label));
            checks.Equal("zero line", 20m, chart.ZeroLine);
            checks.Equal("CSS zero line", "20%", chart.CssZeroLine);
            checks.Equal("has negative bars", true, chart.HasNegativeBars);
        });

    [Fact]
    public void Every_ROI_negative_puts_the_zero_line_at_the_right_edge() =>
        Run("02", "Chart: every ROI negative puts the zero line at 100", checks =>
        {
            // (70 − 100) ÷ 100 = −30.0% and (52.50 − 100) ÷ 100 = −47.5%. Scale = 0 + 0.475, so the
            // widths are 0.3 ÷ 0.475 = 63.16% and 100%, and the zero line sits at 0.475 ÷ 0.475 = 100.
            var chart = RoiChart.Build([
                new ChannelTotals("search", 100m, 52.50m),
                new ChannelTotals("display", 100m, 70m),
            ]);

            checks.Equal("zero line", 100m, chart.ZeroLine);
            checks.SequenceEqual("order", ["Display", "Search"], chart.Bars.Select(b => b.Name));
            checks.SequenceEqual("labels", ["-30.0%", "-47.5%"], chart.Bars.Select(b => b.Label));
            checks.SequenceEqual("negative", [true, true], chart.Bars.Select(b => b.IsNegative));
            checks.SequenceEqual("CSS widths", ["63.16%", "100%"], chart.Bars.Select(b => b.CssWidth));
        });

    [Fact]
    public void The_longest_bar_on_each_side_reaches_its_edge() =>
        Run("02", "Chart: the longest bar on each side reaches its edge", checks =>
        {
            // ROIs 75%, 25%, −10% and −25%. Scale = 0.75 + 0.25 = 1, so the zero line is at 25:
            // the 75% bar runs from 25 to the right edge (25 + 75 = 100) and the −25% bar from the
            // zero line to the left edge (25 − 25 = 0).
            var chart = RoiChart.Build([
                new ChannelTotals("email", 100m, 175m),
                new ChannelTotals("social", 100m, 125m),
                new ChannelTotals("search", 100m, 90m),
                new ChannelTotals("display", 100m, 75m),
            ]);

            checks.Equal("zero line", 25m, chart.ZeroLine);
            checks.SequenceEqual("widths", [75m, 25m, 10m, 25m], chart.Bars.Select(b => b.Width));
            checks.Equal("right end of the longest positive bar", 100m, chart.ZeroLine + chart.Bars[0].Width);
            checks.Equal("left end of the longest negative bar", 0m, chart.ZeroLine - chart.Bars[3].Width);
        });

    [Fact]
    public void Positive_bars_start_at_the_zero_line_and_negative_bars_end_at_it() =>
        Run("02", "Chart: positive bars start at the zero line, negative bars end at it, labels past each end", checks =>
        {
            // ROIs 200%, 100% and −50%: zero line 20, widths 80, 40 and 20 (as in the mixed test).
            // Email spans 20 → 100 with its label from 100; search spans 20 → 60 with its label
            // from 60; display spans 0 → 20 with its label ending at 0, i.e. 100 from the right.
            var chart = RoiChart.Build([
                new ChannelTotals("display", 100m, 50m),
                new ChannelTotals("email", 100m, 300m),
                new ChannelTotals("search", 100m, 200m),
            ]);

            checks.SequenceEqual("bar left edges", [20m, 20m, 0m], chart.Bars.Select(b => b.Left));
            checks.SequenceEqual("bar styles",
                ["left: 20%; width: 80%", "left: 20%; width: 40%", "left: 0%; width: 20%"],
                chart.Bars.Select(b => b.CssBarStyle));
            checks.SequenceEqual("label styles",
                ["left: 100%; padding-left: 0.5rem", "left: 60%; padding-left: 0.5rem", "right: 100%; padding-right: 0.5rem"],
                chart.Bars.Select(b => b.CssLabelStyle));
        });

    [Fact]
    public void An_ROI_of_exactly_0_has_its_label_at_the_zero_line() =>
        Run("02", "Chart: an ROI of exactly 0 draws no bar, its label at the zero line", checks =>
        {
            // ROIs 100%, 0% and −100%: scale = 1 + 1 = 2, zero line at 1 ÷ 2 = 50.
            var chart = RoiChart.Build([
                new ChannelTotals("email", 100m, 200m),
                new ChannelTotals("search", 100m, 100m),
                new ChannelTotals("display", 100m, 0m),
            ]);

            var search = chart.Bars[1];
            checks.Equal("search label", "0.0%", search.Label);
            checks.Equal("search bar style", "left: 50%; width: 0%", search.CssBarStyle);
            checks.Equal("search label style", "left: 50%; padding-left: 0.5rem", search.CssLabelStyle);
        });

    [Fact]
    public void Negative_positions_are_the_same_whatever_the_current_culture() =>
        Run("02", "Chart: negative bar positions the same in a comma-decimal culture (de-DE)", checks =>
        {
            var original = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            try
            {
                // ROIs −30% and −47.5%: zero line 100, widths 63.157…% and 100%. Display spans
                // 36.84… → 100 with its label ending at 36.84…, i.e. 63.16 from the right; search
                // spans 0 → 100 with its label ending at the left edge, 100 from the right.
                var chart = RoiChart.Build([
                    new ChannelTotals("search", 100m, 52.50m),
                    new ChannelTotals("display", 100m, 70m),
                ]);

                checks.SequenceEqual("bar styles",
                    ["left: 36.84%; width: 63.16%", "left: 0%; width: 100%"],
                    chart.Bars.Select(b => b.CssBarStyle));
                checks.SequenceEqual("label styles",
                    ["right: 63.16%; padding-right: 0.5rem", "right: 100%; padding-right: 0.5rem"],
                    chart.Bars.Select(b => b.CssLabelStyle));
                checks.Equal("CSS zero line", "100%", chart.CssZeroLine);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        });

    [Fact]
    public void The_track_leaves_room_on_the_left_only_when_a_bar_is_negative() =>
        Run("02", "Chart: label room on the right always, on the left only when a bar is negative", checks =>
        {
            var allPositive = RoiChart.Build([
                new ChannelTotals("email", 100m, 200m),
                new ChannelTotals("search", 100m, 150m),
            ]);
            var mixed = RoiChart.Build([
                new ChannelTotals("email", 100m, 200m),
                new ChannelTotals("display", 100m, 70m),
            ]);

            checks.Equal("all positive", "padding: 0 5.5rem 0 0", allPositive.CssTrackPadding);
            checks.Equal("mixed", "padding: 0 5.5rem", mixed.CssTrackPadding);
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
