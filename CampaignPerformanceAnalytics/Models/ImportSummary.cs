namespace CampaignPerformanceAnalytics.Models;

/// <summary>
/// The outcome of importing one CSV file of Daily Results.
/// </summary>
public class ImportSummary
{
    public required string FileName { get; init; }

    /// <summary>Data rows read from the file (the header is not counted).</summary>
    public int RowsRead { get; set; }

    /// <summary>Daily Results inserted into the results table.</summary>
    public int ImportedCount { get; set; }
}
