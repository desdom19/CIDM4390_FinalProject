namespace CampaignPerformanceAnalytics.Models;

/// <summary>
/// What the dashboard shows: the KPI totals over every Daily Result.
/// </summary>
public class DashboardResult
{
    public required KpiTotals Totals { get; init; }
}

/// <summary>
/// The raw KPI figures, each summed over every Daily Result of every Campaign and Channel.
/// Counts are <see cref="long"/> because the sum of an int column can go past the int range.
/// </summary>
public record KpiTotals(
    decimal TotalSpend,
    decimal TotalRevenue,
    long TotalLeads,
    long TotalImpressions,
    long TotalClicks);
