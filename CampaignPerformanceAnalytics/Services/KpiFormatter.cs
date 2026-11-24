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
}
