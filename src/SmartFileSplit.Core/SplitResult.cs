namespace SmartFileSplit.Core;

/// <summary>Outcome of a split operation.</summary>
public sealed class SplitResult
{
    /// <summary>Absolute paths of the files that were created, in order.</summary>
    public required IReadOnlyList<string> OutputFiles { get; init; }

    /// <summary>Number of data rows distributed across the output files (excludes the header).</summary>
    public required int DataRowCount { get; init; }

    /// <summary>True when the input contained no data rows, so nothing was written.</summary>
    public bool NoData => DataRowCount == 0;

    /// <summary>Number of output files produced.</summary>
    public int PartCount => OutputFiles.Count;
}
