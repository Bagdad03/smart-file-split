using SmartFileSplit.Core;

namespace SmartFileSplit.Core.Tests;

// Сквозные тесты FileSplitter с выводом в XLSX (вторая половина форматной
// матрицы) и с чтением ;-CSV / рваных строк.
public class CrossFormatSplitTests
{
    private static readonly string[] Header = { "id", "name" };

    private static string[][] DataRows(int count) =>
        Enumerable.Range(1, count).Select(i => new[] { i.ToString(), $"row-{i}" }).ToArray();

    private static void WriteCsv(string path, IEnumerable<string[]> rows, char delimiter = ',')
    {
        using var w = new CsvTableWriter(path, delimiter);
        foreach (var row in rows)
            w.WriteRow(row);
    }

    private static string[][] ReadXlsx(string path)
    {
        using var r = new ExcelTableReader(path, sheetIndex: 0);
        return r.ReadRows().ToArray();
    }

    [Fact]
    public void Csv_input_splits_to_xlsx_output_with_header_in_each_file()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, new[] { Header }.Concat(DataRows(5)));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 2,
            HasHeader = true,
            OutputFormat = OutputFormat.Xlsx,
        });

        Assert.Equal(3, result.PartCount); // 5 данных по 2 → 2/2/1
        Assert.All(result.OutputFiles, f => Assert.EndsWith(".xlsx", f));

        // Заголовок первым в каждом файле; данные из всех файлов совпадают с исходными.
        foreach (var f in result.OutputFiles)
            Assert.Equal(Header, ReadXlsx(f)[0]);
        var data = result.OutputFiles.SelectMany(f => ReadXlsx(f).Skip(1)).ToArray();
        Assert.Equal(DataRows(5), data);
    }

    [Fact]
    public void Xlsx_input_splits_to_xlsx_output_preserving_data()
    {
        using var tmp = new TempDir();
        var input = tmp.File("book.xlsx");
        XlsxFixture.Create(input, ("Sheet1", new[] { Header }.Concat(DataRows(4)).ToArray()));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByFileCount,
            Value = 2,
            HasHeader = true,
            OutputFormat = OutputFormat.Xlsx,
        });

        Assert.Equal(2, result.PartCount);
        Assert.All(result.OutputFiles, f => Assert.EndsWith(".xlsx", f));
        var data = result.OutputFiles.SelectMany(f => ReadXlsx(f).Skip(1)).ToArray();
        Assert.Equal(DataRows(4), data);
    }

    [Fact]
    public void Semicolon_csv_input_is_parsed_into_columns_when_writing_xlsx()
    {
        using var tmp = new TempDir();
        var input = tmp.File("ru.csv");
        // Точка с запятой как разделитель (русская локаль Excel).
        WriteCsv(input, new[]
        {
            new[] { "город", "значение" },
            new[] { "Москва", "1" },
            new[] { "Тверь", "2" },
        }, delimiter: ';');

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 10,
            HasHeader = true,
            OutputFormat = OutputFormat.Xlsx,
            CsvDelimiter = ';',
        });

        var rows = ReadXlsx(result.OutputFiles[0]);
        // Каждая строка разбита на 2 столбца, а не склеена в один.
        Assert.Equal(new[] { "город", "значение" }, rows[0]);
        Assert.Equal(new[] { "Москва", "1" }, rows[1]);
        Assert.Equal(new[] { "Тверь", "2" }, rows[2]);
    }

    [Fact]
    public void Ragged_rows_csv_to_xlsx_keeps_short_rows_short()
    {
        using var tmp = new TempDir();
        var input = tmp.File("ragged.csv");
        // Строки разной длины (типично для выгрузок с пустыми хвостовыми полями).
        File.WriteAllText(input, "a,b,c\n1,2,3\n4,5\n6\n");

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 10,
            HasHeader = true,
            OutputFormat = OutputFormat.Xlsx,
        });

        Assert.Single(result.OutputFiles);
        var rows = ReadXlsx(result.OutputFiles[0]);
        Assert.Equal(new[] { "a", "b", "c" }, rows[0]);
        // Короткие строки данных остаются короткими (не крэшат и не добиваются пустыми).
        Assert.Equal("1", rows[1][0]);
        Assert.Equal("4", rows[2][0]);
        Assert.Equal("5", rows[2][1]);
        Assert.Equal("6", rows[3][0]);
    }
}