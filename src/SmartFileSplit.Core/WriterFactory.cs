namespace SmartFileSplit.Core;

/// <summary>
/// Выбирает реализацию <see cref="ITableWriter"/> по ВЫБРАННОМУ формату вывода
/// (независимо от формата входа) и сообщает расширение выходных файлов.
/// </summary>
public static class WriterFactory
{
    public static ITableWriter Create(string outputPath, SplitOptions options) => options.OutputFormat switch
    {
        OutputFormat.Csv => new CsvTableWriter(outputPath, options.CsvDelimiter),
        OutputFormat.Xlsx => new XlsxTableWriter(outputPath),
        _ => throw new NotSupportedException($"Формат вывода не поддерживается: {options.OutputFormat}"),
    };

    public static string ExtensionFor(OutputFormat format) => format switch
    {
        OutputFormat.Csv => "csv",
        OutputFormat.Xlsx => "xlsx",
        _ => throw new NotSupportedException($"Формат вывода не поддерживается: {format}"),
    };
}