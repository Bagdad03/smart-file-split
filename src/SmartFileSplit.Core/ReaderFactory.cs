namespace SmartFileSplit.Core;

/// <summary>
/// Выбирает реализацию <see cref="ITableReader"/> по расширению ВХОДНОГО файла.
/// </summary>
public static class ReaderFactory
{
    public static ITableReader Create(SplitOptions options)
    {
        var ext = Path.GetExtension(options.InputPath).ToLowerInvariant();
        return ext switch
        {
            ".csv" => new CsvTableReader(options.InputPath, options.CsvDelimiter),
            ".xls" or ".xlsx" => new ExcelTableReader(options.InputPath, options.SheetIndex),
            _ => throw new NotSupportedException($"Формат входного файла не поддерживается: {ext}"),
        };
    }
}