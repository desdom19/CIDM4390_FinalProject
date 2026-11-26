using System.Diagnostics.CodeAnalysis;

namespace CampaignPerformanceAnalytics.Models;

/// <summary>
/// What the dashboard shows: either the KPI totals and the Channel totals, read from the same
/// snapshot of the data, or an error message when they couldn't be read. Never both, so a failure
/// can't be shown as zero tiles or empty bars.
/// </summary>
public class DashboardResult
{
    private DashboardResult(KpiTotals? totals, IReadOnlyList<ChannelTotals>? channelTotals, string? errorMessage)
    {
        Totals = totals;
        ChannelTotals = channelTotals;
        ErrorMessage = errorMessage;
    }

    /// <summary>The totals; null when <see cref="Failed"/>.</summary>
    public KpiTotals? Totals { get; }

    /// <summary>Every Channel's totals, one per Channel whether or not it has Daily Results; null when <see cref="Failed"/>.</summary>
    public IReadOnlyList<ChannelTotals>? ChannelTotals { get; }

    /// <summary>Set when the figures couldn't be read; shown in place of the tiles and the chart.</summary>
    public string? ErrorMessage { get; }

    [MemberNotNullWhen(false, nameof(Totals), nameof(ChannelTotals))]
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool Failed => ErrorMessage is not null;

    public static DashboardResult Success(KpiTotals totals, IReadOnlyList<ChannelTotals> channelTotals) =>
        new(totals, channelTotals, null);

    public static DashboardResult Failure(string errorMessage) => new(null, null, errorMessage);
}

/// <summary>
/// The raw KPI figures, each summed over every Daily Result of every Campaign and Channel.
/// Counts are <see cref="long"/> because the sum of an int column can go past the int range.
/// </summary>
/// <param name="HasDailyResults">Whether there is any Daily Result at all; the empty-state hint shows when there isn't.</param>
public record KpiTotals(
    bool HasDailyResults,
    decimal TotalSpend,
    decimal TotalRevenue,
    long TotalLeads,
    long TotalImpressions,
    long TotalClicks)
{
    /// <summary>
    /// ROI of all Campaigns together: (total revenue − total spend) ÷ total spend, from the same
    /// totals the spend and revenue tiles show; never an average of each Campaign's ROI.
    /// Null when total spend is exactly 0, whatever the revenue.
    /// </summary>
    public decimal? OverallRoi => TotalSpend == 0 ? null : (TotalRevenue - TotalSpend) / TotalSpend;
}

/// <summary>
/// One Channel's spend and revenue, each summed over every Daily Result of every Campaign on it.
/// </summary>
/// <param name="Name">The Channel's name as stored, e.g. "email".</param>
public record ChannelTotals(string Name, decimal TotalSpend, decimal TotalRevenue)
{
    /// <summary>
    /// The Channel's ROI: (total revenue − total spend) ÷ total spend, from its totals; never an
    /// average of its Campaigns' ROIs. Null when total spend is exactly 0, whatever the revenue.
    /// </summary>
    public decimal? Roi => TotalSpend == 0 ? null : (TotalRevenue - TotalSpend) / TotalSpend;
}
