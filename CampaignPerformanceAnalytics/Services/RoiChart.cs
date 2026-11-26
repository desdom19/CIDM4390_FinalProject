using System.Globalization;
using CampaignPerformanceAnalytics.Models;

namespace CampaignPerformanceAnalytics.Services;

/// <summary>
/// Works out the ROI by Channel bar chart from the Channel totals: everything the view draws,
/// so no chart logic sits in the markup.
/// </summary>
public static class RoiChart
{
    public static RoiChartModel Build(IReadOnlyList<ChannelTotals> channels)
    {
        // Only Channels with spend have an ROI to draw.
        var withRoi = channels
            .Where(c => c.Roi is not null)
            .Select(c => (Channel: c, Roi: c.Roi!.Value))
            .ToList();

        if (withRoi.Count == 0)
        {
            return new RoiChartModel([], ZeroLine: 0m);
        }

        var largestRoi = withRoi.Max(c => c.Roi);

        // Guard until ticket 02 draws negative bars: an ROI of 0 or below gets width 0 (a label with
        // no bar), so a loss is never drawn as a gain. A positive ROI means the largest is positive
        // too, so the division never divides by 0.
        var bars = withRoi
            .Select(c => new RoiBar(
                DisplayName(c.Channel.Name),
                KpiFormatter.Roi(c.Roi),
                c.Roi > 0 ? c.Roi / largestRoi * 100 : 0m))
            .ToList();

        // Every bar grows right from the left edge.
        return new RoiChartModel(bars, ZeroLine: 0m);
    }

    /// <summary>The stored name with its first letter capitalised, e.g. "email" → "Email".</summary>
    private static string DisplayName(string storedName) =>
        storedName.Length == 0
            ? storedName
            : char.ToUpperInvariant(storedName[0]) + storedName[1..];
}

/// <summary>The bars of the ROI by Channel chart, in the order they are drawn.</summary>
/// <param name="ZeroLine">Where the zero line sits, as a percentage across the chart track from its left edge.</param>
public record RoiChartModel(IReadOnlyList<RoiBar> Bars, decimal ZeroLine);

/// <summary>One Channel's bar.</summary>
/// <param name="Name">The Channel's display name, e.g. Email.</param>
/// <param name="Label">The ROI as the Overall ROI tile shows it, e.g. 155.1%.</param>
/// <param name="Width">The bar's length as a percentage of the chart track, from the zero line; 0 draws no bar.</param>
public record RoiBar(string Name, string Label, decimal Width)
{
    /// <summary>
    /// The width as CSS for an inline style, e.g. 33.33%. The invariant culture means the decimal
    /// point is never written as a comma, which a browser would reject.
    /// </summary>
    public string CssWidth => Width.ToString("0.##", CultureInfo.InvariantCulture) + "%";
}
