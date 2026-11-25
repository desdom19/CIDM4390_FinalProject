using System.Globalization;

namespace CampaignPerformanceAnalytics.Services;

/// <summary>
/// Turns KPI figures into the text the dashboard tiles show. The invariant culture and explicit
/// patterns make the text the same on every machine, whatever the server's regional settings;
/// culture-dependent currency formatting ("C") is deliberately not used, as it can show another currency sign.
/// </summary>
public static class KpiFormatter
{
    /// <summary>Dollars with commas and two decimals, e.g. $45,256.13.</summary>
    public static string Money(decimal value) =>
        "$" + value.ToString("#,##0.00", CultureInfo.InvariantCulture);

    /// <summary>A whole number with commas, e.g. 4,995,183.</summary>
    public static string Count(long value) =>
        value.ToString("#,##0", CultureInfo.InvariantCulture);

    /// <summary>Shown instead of an ROI when total spend is 0.</summary>
    public const string RoiNotAvailable = "N/A (spend is 0)";

    /// <summary>
    /// A percentage with commas and one decimal, e.g. 81.4%, or <see cref="RoiNotAvailable"/> for null.
    /// Percent format ("P") is not used, as the invariant culture puts a space before the %.
    /// A custom pattern on a decimal drops the sign of a value that rounds to zero, so -0.04% shows
    /// as 0.0%, never -0.0%; KpiFormatterTests guards this.
    /// </summary>
    public static string Roi(decimal? value) =>
        value is null
            ? RoiNotAvailable
            : (value.Value * 100).ToString("#,##0.0", CultureInfo.InvariantCulture) + "%";
}
