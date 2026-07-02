using SmartFileSplit.Core;

namespace SmartFileSplit.Core.Tests;

public class WorkbookInspectorTests
{
    [Fact]
    public void Lists_all_sheets_with_index_and_name()
    {
        using var tmp = new TempDir();
        var path = tmp.File("multi.xlsx");
        XlsxFixture.Create(path,
            ("Alpha", new[] { new[] { "a" } }),
            ("Beta", new[] { new[] { "b" } }),
            ("Gamma", new[] { new[] { "c" } }));

        var sheets = WorkbookInspector.GetSheets(path);

        Assert.Equal(3, sheets.Count);
        Assert.Equal(new SheetInfo(0, "Alpha"), sheets[0]);
        Assert.Equal(new SheetInfo(1, "Beta"), sheets[1]);
        Assert.Equal(new SheetInfo(2, "Gamma"), sheets[2]);
    }

    [Fact]
    public void Csv_has_no_sheets()
    {
        using var tmp = new TempDir();
        var path = tmp.File("data.csv");
        File.WriteAllText(path, "a,b\n1,2\n");

        var sheets = WorkbookInspector.GetSheets(path);

        Assert.Empty(sheets);
    }
}