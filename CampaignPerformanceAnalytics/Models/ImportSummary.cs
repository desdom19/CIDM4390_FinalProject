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

    /// <summary>
    /// Valid rows skipped as Duplicate Daily Results: their Campaign already had a Daily Result
    /// for that date, in the database or from an earlier accepted row in the same file.
    /// </summary>
    public int DuplicateCount { get; set; }

    /// <summary>Rows that failed validation. Each rejected row counts once, however many reasons it has.</summary>
    public int RejectedCount => RejectedRows.Count;

    public List<RejectedRow> RejectedRows { get; } = [];
}

/// <summary>A row that failed validation, with every reason it failed.</summary>
/// <param name="LineNumber">The file line number, with the header as line 1.</param>
public record RejectedRow(int LineNumber, IReadOnlyList<string> Reasons);
