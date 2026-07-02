using SmartFileSplit.Core;

namespace SmartFileSplit.Core.Tests;

public class ExcelReaderTests
{
    [Fact]
    public void Reads_rows_of_the_selected_sheet()
    {
        using var tmp = new TempDir();
        var path = tmp.File("two.xlsx");
        XlsxFixture.Create(path,
            ("First", new[] { new[] { "x" }, new[] { "1" } }),
            ("Second", new[]
            {
                new[] { "id", "name" },
                new[] { "1", "Alice" },
                new[] { "2", "Bob" },
            }));

        string[][] rows;
        using (var reader = new ExcelTableReader(path, sheetIndex: 1))
            rows = reader.ReadRows().ToArray();

        Assert.Equal(new[]
        {
            new[] { "id", "name" },
            new[] { "1", "Alice" },
            new[] { "2", "Bob" },
        }, rows);
    }

    [Fact]
    public void Empty_cells_become_empty_strings()
    {
        using var tmp = new TempDir();
        var path = tmp.File("gaps.xlsx");
        XlsxFixture.Create(path,
            ("S", new[]
            {
                new[] { "a", "b", "c" },
                new[] { "1", "", "3" },
            }));

        string[][] rows;
        using (var reader = new ExcelTableReader(path, sheetIndex: 0))
            rows = reader.ReadRows().ToArray();

        Assert.Equal(new[] { "1", "", "3" }, rows[1]);
    }
}