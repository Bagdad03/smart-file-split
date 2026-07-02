using SmartFileSplit.Core;

namespace SmartFileSplit.Core.Tests;

public class CsvRoundTripTests
{
    [Fact]
    public void Writer_then_reader_round_trips_simple_rows()
    {
        using var tmp = new TempDir();
        var path = tmp.File("data.csv");
        var rows = new[]
        {
            new[] { "id", "name" },
            new[] { "1", "Alice" },
            new[] { "2", "Bob" },
        };

        using (var writer = new CsvTableWriter(path, ','))
            foreach (var row in rows)
                writer.WriteRow(row);

        string[][] readBack;
        using (var reader = new CsvTableReader(path, ','))
            readBack = reader.ReadRows().ToArray();

        Assert.Equal(rows, readBack);
    }

    [Fact]
    public void Round_trips_values_with_delimiters_quotes_and_newlines()
    {
        using var tmp = new TempDir();
        var path = tmp.File("tricky.csv");
        var rows = new[]
        {
            new[] { "plain", "with,comma" },
            new[] { "with \"quote\"", "line1\nline2" },
        };

        using (var writer = new CsvTableWriter(path, ','))
            foreach (var row in rows)
                writer.WriteRow(row);

        string[][] readBack;
        using (var reader = new CsvTableReader(path, ','))
            readBack = reader.ReadRows().ToArray();

        Assert.Equal(rows, readBack);
    }

    [Fact]
    public void Round_trips_with_semicolon_delimiter()
    {
        using var tmp = new TempDir();
        var path = tmp.File("ru.csv");
        var rows = new[]
        {
            new[] { "город", "значение" },
            new[] { "Москва", "1,5" },
        };

        using (var writer = new CsvTableWriter(path, ';'))
            foreach (var row in rows)
                writer.WriteRow(row);

        string[][] readBack;
        using (var reader = new CsvTableReader(path, ';'))
            readBack = reader.ReadRows().ToArray();

        Assert.Equal(rows, readBack);
    }

    [Fact]
    public void Writer_emits_utf8_bom()
    {
        using var tmp = new TempDir();
        var path = tmp.File("bom.csv");

        using (var writer = new CsvTableWriter(path, ','))
            writer.WriteRow(new[] { "a", "b" });

        var bytes = File.ReadAllBytes(path);
        Assert.True(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF,
            "Файл CSV должен начинаться с UTF-8 BOM (EF BB BF).");
    }
}