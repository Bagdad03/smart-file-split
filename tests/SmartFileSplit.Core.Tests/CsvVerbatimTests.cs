using SmartFileSplit.Core;

namespace SmartFileSplit.Core.Tests;

public class CsvVerbatimTests
{
    [Fact]
    public void Csv_to_csv_keeps_original_quoting_verbatim()
    {
        using var tmp = new TempDir();
        var input = tmp.File("src.csv");
        // Поле code принудительно заключено в кавычки в исходнике.
        File.WriteAllText(input, "id,code\n1,\"123\"\n2,\"456\"\n");

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 1,
            HasHeader = true,
            OutputFormat = OutputFormat.Csv,
            CsvDelimiter = ',',
        });

        Assert.Equal(2, result.PartCount);

        // Кавычки сохранены дословно в каждом файле.
        Assert.Contains("\"123\"", File.ReadAllText(result.OutputFiles[0]));
        Assert.Contains("\"456\"", File.ReadAllText(result.OutputFiles[1]));

        // Заголовок на месте, значение при разборе — без кавычек (семантика CSV та же).
        var rows0 = ReadCsv(result.OutputFiles[0]);
        Assert.Equal(new[] { "id", "code" }, rows0[0]);
        Assert.Equal(new[] { "1", "123" }, rows0[1]);
    }

    [Fact]
    public void Csv_to_csv_preserves_unquoted_and_quoted_mix_and_special_chars()
    {
        using var tmp = new TempDir();
        var input = tmp.File("mix.csv");
        // Строка с запятой в кавычках, обычное поле и намеренно квотированное число.
        File.WriteAllText(input, "a,b,c\n\"x,y\",plain,\"007\"\n");

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 10,
            HasHeader = true,
            OutputFormat = OutputFormat.Csv,
            CsvDelimiter = ',',
        });

        var text = File.ReadAllText(result.OutputFiles[0]);
        Assert.Contains("\"x,y\"", text); // квотированное поле с запятой
        Assert.Contains("plain", text);   // обычное поле без кавычек
        Assert.Contains("\"007\"", text);  // намеренно квотированное число сохранено
    }

    [Fact]
    public void Csv_without_trailing_newline_does_not_glue_rows()
    {
        using var tmp = new TempDir();
        var input = tmp.File("no_eol.csv");
        // Файл БЕЗ финального перевода строки (типично для не-Excel источников).
        File.WriteAllText(input, "id,code\n1,\"123\"\n2,\"456\"");

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 1,
            HasHeader = true,
            OutputFormat = OutputFormat.Csv,
            CsvDelimiter = ',',
        });

        Assert.Equal(2, result.PartCount);

        // Каждый файл: ровно 2 строки (заголовок + 1 данные), ничего не слиплось.
        var rows0 = ReadCsv(result.OutputFiles[0]);
        var rows1 = ReadCsv(result.OutputFiles[1]);
        Assert.Equal(new[] { "id", "code" }, rows0[0]);
        Assert.Equal(new[] { "1", "123" }, rows0[1]);
        Assert.Equal(2, rows0.Length);
        Assert.Equal(new[] { "id", "code" }, rows1[0]);
        Assert.Equal(new[] { "2", "456" }, rows1[1]);
        Assert.Equal(2, rows1.Length);
    }

    [Fact]
    public void Csv_without_trailing_newline_multiple_rows_per_part()
    {
        using var tmp = new TempDir();
        var input = tmp.File("no_eol2.csv");
        // 5 строк данных без финального перевода строки, без заголовка, по 2 на файл.
        File.WriteAllText(input, "a\nb\nc\nd\ne");

        var result = new FileSplitter().Split(new SplitOptions
        {
            InputPath = input,
            OutputDirectory = tmp.Path,
            Mode = SplitMode.ByRowCount,
            Value = 2,
            HasHeader = false,
            OutputFormat = OutputFormat.Csv,
            CsvDelimiter = ',',
        });

        Assert.Equal(3, result.PartCount);
        Assert.Equal(new[] { "a", "b" }, ReadCsv(result.OutputFiles[0]).Select(r => r[0]));
        Assert.Equal(new[] { "c", "d" }, ReadCsv(result.OutputFiles[1]).Select(r => r[0]));
        Assert.Equal(new[] { "e" }, ReadCsv(result.OutputFiles[2]).Select(r => r[0]));
    }

    private static string[][] ReadCsv(string path)
    {
        using var r = new CsvTableReader(path, ',');
        return r.ReadRows().ToArray();
    }
}