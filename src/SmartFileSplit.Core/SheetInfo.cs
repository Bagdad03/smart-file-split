namespace SmartFileSplit.Core;

/// <summary>A worksheet in an Excel workbook, identified by zero-based index and name.</summary>
public sealed record SheetInfo(int Index, string Name);
