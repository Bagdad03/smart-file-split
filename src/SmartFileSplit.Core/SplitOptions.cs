namespace SmartFileSplit.Core;

/// <summary>
/// All parameters for one split operation. Built by the UI and consumed by <see cref="FileSplitter"/>.
/// </summary>
public sealed class SplitOptions
{
    /// <summary>Path to the input file (.xlsx, .xls or .csv).</summary>
    public required string InputPath { get; init; }

    /// <summary>Folder the output files are written into.</summary>
    public required string OutputDirectory { get; init; }

    /// <summary>Whether to split by rows-per-file or by number-of-files.</summary>
    public SplitMode Mode { get; init; }

    /// <summary>
    /// Meaning depends on <see cref="Mode"/>: max data rows per file (ByRowCount)
    /// or desired number of output files (ByFileCount). Must be &gt; 0.
    /// </summary>
    public int Value { get; init; }

    /// <summary>
    /// When true the first row is a header: copied to the top of every output file
    /// and not counted as a data row.
    /// </summary>
    public bool HasHeader { get; init; }

    /// <summary>Zero-based index of the Excel sheet to read. Ignored for CSV.</summary>
    public int SheetIndex { get; init; }

    /// <summary>Format of the produced files, independent of the input format.</summary>
    public OutputFormat OutputFormat { get; init; }

    /// <summary>Field separator used for reading and writing CSV.</summary>
    public char CsvDelimiter { get; init; } = ',';
}
