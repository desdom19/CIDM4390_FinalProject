using System.Diagnostics.CodeAnalysis;

namespace CampaignPerformanceAnalytics.Models;

/// <summary>
/// What the dashboard shows: either the KPI totals over every Daily Result, or an error message
/// when they couldn't be read. Never both, so a failure can't be shown as zero tiles.
/// </summary>
public class DashboardResult
{
    private DashboardResult(KpiTotals? totals, string? errorMessage)
    {
        Totals = totals;
        ErrorMessage = errorMessage;
    }

    /// <summary>The totals; null when <see cref="Failed"/>.</summary>
    public KpiTotals? Totals { get; }

    /// <summary>Set when the totals couldn't be read; shown in place of the tiles.</summary>
    public string? ErrorMessage { get; }

    [MemberNotNullWhen(false, nameof(Totals))]
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool Failed => ErrorMessage is not null;

    public static DashboardResult Success(KpiTotals totals) => new(totals, null);

    public static DashboardResult Failure(string errorMessage) => new(null, errorMessage);
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
