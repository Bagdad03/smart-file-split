using SmartFileSplit.Core;

namespace SmartFileSplit.Core.Tests;

public class FactoryTests
{
    private static SplitOptions OptionsFor(string inputPath, OutputFormat outputFormat = OutputFormat.Csv) => new()
    {
        InputPath = inputPath,
        OutputDirectory = Path.GetDirectoryName(inputPath)!,
        Mode = SplitMode.ByRowCount,
        Value = 1,
        OutputFormat = outputFormat,
    };

    [Fact]
    public void ReaderFactory_picks_csv_reader_by_extension()
    {
        using var tmp = new TempDir();
        var path = tmp.File("data.csv");
        File.WriteAllText(path, "a,b\n");

        using var reader = ReaderFactory.Create(OptionsFor(path));

        Assert.IsType<CsvTableReader>(reader);
    }

    [Fact]
    public void ReaderFactory_picks_excel_reader_for_xlsx()
    {
        using var tmp = new TempDir();
        var path = tmp.File("data.xlsx");
        XlsxFixture.Create(path, ("S", new[] { new[] { "a" } }));

        using var reader = ReaderFactory.Create(OptionsFor(path));

        Assert.IsType<ExcelTableReader>(reader);
    }

    [Fact]
    public void ReaderFactory_throws_on_unknown_extension()
    {
        using var tmp = new TempDir();
        var path = tmp.File("data.txt");
        File.WriteAllText(path, "x");

        var ex = Assert.Throws<NotSupportedException>(() => ReaderFactory.Create(OptionsFor(path)));
        Assert.Contains(".txt", ex.Message);
    }

    [Fact]
    public void WriterFactory_picks_csv_writer_for_csv_format()
    {
        using var tmp = new TempDir();
        var path = tmp.File("out.csv");

        using var writer = WriterFactory.Create(path, OptionsFor(tmp.File("in.csv"), OutputFormat.Csv));

        Assert.IsType<CsvTableWriter>(writer);
    }

    [Fact]
    public void WriterFactory_picks_xlsx_writer_for_xlsx_format()
    {
        using var tmp = new TempDir();
        var path = tmp.File("out.xlsx");

        using var writer = WriterFactory.Create(path, OptionsFor(tmp.File("in.csv"), OutputFormat.Xlsx));

        Assert.IsType<XlsxTableWriter>(writer);
    }

    [Theory]
    [InlineData(OutputFormat.Csv, "csv")]
    [InlineData(OutputFormat.Xlsx, "xlsx")]
    public void WriterFactory_reports_extension_for_format(OutputFormat format, string expected)
    {
        Assert.Equal(expected, WriterFactory.ExtensionFor(format));
    }
}