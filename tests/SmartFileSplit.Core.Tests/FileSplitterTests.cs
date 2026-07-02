using SmartFileSplit.Core;

namespace SmartFileSplit.Core.Tests;

public class FileSplitterTests
{
    // --- Хелперы ---

    /// <summary>Записывает строки во входной CSV-файл.</summary>
    private static void WriteCsv(string path, IEnumerable<string[]> rows)
    {
        using var w = new CsvTableWriter(path, ',');
        foreach (var row in rows)
            w.WriteRow(row);
    }

    /// <summary>Читает все строки CSV-файла обратно.</summary>
    private static string[][] ReadCsv(string path)
    {
        using var r = new CsvTableReader(path, ',');
        return r.ReadRows().ToArray();
    }

    /// <summary>Генерирует <paramref name="count"/> строк данных вида ["i", "row-i"].</summary>
    private static string[][] DataRows(int count) =>
        Enumerable.Range(1, count).Select(i => new[] { i.ToString(), $"row-{i}" }).ToArray();

    private static readonly string[] Header = { "id", "name" };

    // --- ByRowCount ---

    [Fact]
    public void ByRowCount_10_rows_limit_4_gives_4_4_2()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, DataRows(10));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 4,
            HasHeader = false,
            OutputFormat = OutputFormat.Csv,
        });

        Assert.Equal(3, result.PartCount);
        Assert.Equal(10, result.DataRowCount);
        Assert.Equal(new[] { 4, 4, 2 }, result.OutputFiles.Select(f => ReadCsv(f).Length));
    }

    // --- ByFileCount ---

    [Fact]
    public void ByFileCount_10_rows_K3_gives_4_4_2()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, DataRows(10));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByFileCount,
            Value = 3,
            HasHeader = false,
            OutputFormat = OutputFormat.Csv,
        });

        // ceil(10/3) = 4 строки на файл → 4/4/2 (3 файла).
        Assert.Equal(3, result.PartCount);
        Assert.Equal(new[] { 4, 4, 2 }, result.OutputFiles.Select(f => ReadCsv(f).Length));
    }

    [Fact]
    public void ByFileCount_ceil_can_yield_fewer_files_than_requested()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, DataRows(10));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByFileCount,
            Value = 6,
            HasHeader = false,
            OutputFormat = OutputFormat.Csv,
        });

        // ceil(10/6) = 2 строки на файл → 5 файлов, а не запрошенные 6 (это нормально).
        Assert.Equal(5, result.PartCount);
        Assert.All(result.OutputFiles, f => Assert.Equal(2, ReadCsv(f).Length));
    }

    // --- Заголовок ---

    [Fact]
    public void Header_on_is_copied_to_every_file_and_not_counted()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, new[] { Header }.Concat(DataRows(10)));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 4,
            HasHeader = true,
            OutputFormat = OutputFormat.Csv,
        });

        Assert.Equal(3, result.PartCount);
        Assert.Equal(10, result.DataRowCount);
        foreach (var f in result.OutputFiles)
        {
            var rows = ReadCsv(f);
            Assert.Equal(Header, rows[0]); // заголовок первым в каждом файле
        }
        // Данных с учётом заголовка: 5/5/3 строк в файлах.
        Assert.Equal(new[] { 5, 5, 3 }, result.OutputFiles.Select(f => ReadCsv(f).Length));
    }

    [Fact]
    public void Header_off_treats_first_row_as_data()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, new[] { Header }.Concat(DataRows(3))); // 4 строки, все — данные

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 10,
            HasHeader = false,
            OutputFormat = OutputFormat.Csv,
        });

        Assert.Equal(1, result.PartCount);
        Assert.Equal(4, result.DataRowCount);
        Assert.Equal(Header, ReadCsv(result.OutputFiles[0])[0]); // первая строка — обычные данные
    }

    // --- Граничные случаи ---

    [Fact]
    public void No_data_rows_produces_no_files()
    {
        using var tmp = new TempDir();
        var input = tmp.File("empty.csv");
        WriteCsv(input, new[] { Header }); // только заголовок

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 5,
            HasHeader = true,
            OutputFormat = OutputFormat.Csv,
        });

        Assert.True(result.NoData);
        Assert.Empty(result.OutputFiles);
    }

    [Fact]
    public void Excel_input_with_only_header_produces_no_files()
    {
        using var tmp = new TempDir();
        var input = tmp.File("header_only.xlsx");
        XlsxFixture.Create(input, ("Sheet1", new[] { Header })); // только заголовок

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 5,
            HasHeader = true,
            OutputFormat = OutputFormat.Csv,
        });

        Assert.True(result.NoData);
        Assert.Empty(result.OutputFiles);
    }

    [Fact]
    public void Fewer_rows_than_limit_gives_single_file()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, DataRows(3));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 100,
            HasHeader = false,
            OutputFormat = OutputFormat.Csv,
        });

        Assert.Equal(1, result.PartCount);
    }

    [Fact]
    public void FileCount_greater_than_rows_gives_fewer_files()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, DataRows(2));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByFileCount,
            Value = 5,
            HasHeader = false,
            OutputFormat = OutputFormat.Csv,
        });

        Assert.Equal(2, result.PartCount); // не 5 — данных меньше
    }

    // --- Имена файлов ---

    [Fact]
    public void Names_are_zero_padded_to_part_count()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, DataRows(10));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 1,
            HasHeader = false,
            OutputFormat = OutputFormat.Csv,
        });

        Assert.Equal(10, result.PartCount);
        Assert.Equal("src_01.csv", Path.GetFileName(result.OutputFiles[0]));
        Assert.Equal("src_10.csv", Path.GetFileName(result.OutputFiles[9]));
    }

    // --- Прогресс ---

    [Fact]
    public void Progress_last_value_is_100_with_synchronous_sink()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        WriteCsv(input, DataRows(10));

        var reports = new List<int>();
        var sink = new SynchronousProgress(reports.Add);
        new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 3,
            HasHeader = false,
            OutputFormat = OutputFormat.Csv,
        }, sink);

        Assert.Equal(100, reports[^1]);
    }

    // --- Сквозной: xlsx вход → csv выход, формат независим ---

    [Fact]
    public void Xlsx_input_splits_to_csv_output_preserving_data()
    {
        using var tmp = new TempDir();
        var input = tmp.File("book.xlsx");
        XlsxFixture.Create(input, ("Sheet1", new[] { Header }.Concat(DataRows(5)).ToArray()));

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 2,
            HasHeader = true,
            OutputFormat = OutputFormat.Csv,
        });

        Assert.Equal(3, result.PartCount); // 5 данных по 2 → 2/2/1
        Assert.All(result.OutputFiles, f => Assert.EndsWith(".csv", f));

        // Склеиваем данные из всех файлов (без заголовков) и сверяем с исходными 5 строками.
        var data = result.OutputFiles.SelectMany(f => ReadCsv(f).Skip(1)).ToArray();
        Assert.Equal(DataRows(5), data);
    }
}