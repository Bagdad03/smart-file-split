namespace SmartFileSplit.Core;

/// <summary>How the input table is divided into output files.</summary>
public enum SplitMode
{
    /// <summary>Each output file holds at most a fixed number of data rows.</summary>
    ByRowCount,

    /// <summary>The data is divided into a fixed number of output files.</summary>
    ByFileCount,
}
