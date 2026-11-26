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
            // Highest first, by the exact ROI rather than the rounded label.
            .OrderByDescending(c => c.Roi)
            // Exact ties by stored name A to Z, so the order is always predictable.
            .ThenBy(c => c.Channel.Name, StringComparer.Ordinal)
            .ToList();

        if (withRoi.Count == 0)
        {
            return new RoiChartModel([], ZeroLine: 0m);
        }

        // Positive and negative bars share one scale: the largest gain plus the largest loss, so the
        // longest bar on each side reaches its edge of the track.
        var largestPositive = Math.Max(0m, withRoi.Max(c => c.Roi));
        var largestNegative = Math.Max(0m, -withRoi.Min(c => c.Roi));
        var scale = largestPositive + largestNegative;

        // The zero line sits where the negative side ends: 0 when nothing loses money, 100 when
        // everything does.
        var zeroLine = PercentOfScale(largestNegative, scale);

        var bars = withRoi
            .Select(c =>
            {
                var isNegative = c.Roi < 0;
                var width = PercentOfScale(Math.Abs(c.Roi), scale);
                // A positive bar starts at the zero line; a negative bar ends at it.
                var left = isNegative ? zeroLine - width : zeroLine;
                return new RoiBar(DisplayName(c.Channel.Name), KpiFormatter.Roi(c.Roi), isNegative, left, width);
            })
            .ToList();

        return new RoiChartModel(bars, zeroLine);
    }

    /// <summary>
    /// A value as a percentage of the scale. A scale of 0 means every ROI shown is exactly 0: the
    /// result is 0 (no bars, the zero line at the left edge) and nothing divides by 0.
    /// </summary>
    private static decimal PercentOfScale(decimal value, decimal scale) =>
        scale == 0 ? 0m : value / scale * 100;

    /// <summary>The stored name with its first letter capitalised, e.g. "email" → "Email".</summary>
    private static string DisplayName(string storedName) =>
        storedName.Length == 0
            ? storedName
            : char.ToUpperInvariant(storedName[0]) + storedName[1..];
}

/// <summary>The bars of the ROI by Channel chart, in the order they are drawn: highest ROI first.</summary>
/// <param name="ZeroLine">Where the zero line sits, as a percentage across the chart track from its left edge.</param>
public record RoiChartModel(IReadOnlyList<RoiBar> Bars, decimal ZeroLine)
{
    /// <summary>The zero line's position as CSS for an inline style, e.g. 20%; see <see cref="RoiBar.CssWidth"/>.</summary>
    public string CssZeroLine => CssPercent.Of(ZeroLine);

    /// <summary>Whether any bar grows left, so the chart needs room for labels left of the track.</summary>
    public bool HasNegativeBars => Bars.Any(b => b.IsNegative);

    /// <summary>
    /// The padding around the chart track as an inline style: room for the longest label past a
    /// full-length bar, so no label is clipped. Always on the right; on the left only when a bar
    /// grows left, so an all-positive chart keeps its full width.
    /// </summary>
    public string CssTrackPadding => HasNegativeBars ? "padding: 0 5.5rem" : "padding: 0 5.5rem 0 0";
}

/// <summary>One Channel's bar.</summary>
/// <param name="Name">The Channel's display name, e.g. Email.</param>
/// <param name="Label">The ROI as the Overall ROI tile shows it, e.g. 155.1%.</param>
/// <param name="IsNegative">Whether the Channel loses money: its bar ends at the zero line and grows left, with its label on its left.</param>
/// <param name="Left">The bar's left edge, as a percentage across the chart track: the zero line for a positive bar, the zero line minus the width for a negative one.</param>
/// <param name="Width">The bar's length as a percentage of the chart track, from the zero line; 0 draws no bar.</param>
public record RoiBar(string Name, string Label, bool IsNegative, decimal Left, decimal Width)
{
    /// <summary>
    /// The width as CSS for an inline style, e.g. 33.33%. The invariant culture means the decimal
    /// point is never written as a comma, which a browser would reject.
    /// </summary>
    public string CssWidth => CssPercent.Of(Width);

    /// <summary>The bar's position and length as an inline style, e.g. "left: 20%; width: 80%".</summary>
    public string CssBarStyle => $"left: {CssPercent.Of(Left)}; width: {CssWidth}";

    /// <summary>
    /// The label's position as an inline style, just past the bar's far end: a positive label
    /// starts at the bar's right end, a negative label ends at the bar's left end. An ROI of 0
    /// has no bar, so its label starts at the zero line.
    /// </summary>
    public string CssLabelStyle => IsNegative
        ? $"right: {CssPercent.Of(100 - Left)}; padding-right: 0.5rem"
        : $"left: {CssPercent.Of(Left + Width)}; padding-left: 0.5rem";
}

/// <summary>Writes a percentage for an inline CSS style, e.g. 33.33%, always with a decimal point.</summary>
internal static class CssPercent
{
    public static string Of(decimal percent) => percent.ToString("0.##", CultureInfo.InvariantCulture) + "%";
}
