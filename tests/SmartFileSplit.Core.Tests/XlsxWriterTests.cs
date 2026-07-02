using ClosedXML.Excel;
using SmartFileSplit.Core;

namespace SmartFileSplit.Core.Tests;

public class XlsxWriterTests
{
    [Fact]
    public void Numeric_looking_values_are_stored_as_text_so_excel_keeps_them_verbatim()
    {
        using var tmp = new TempDir();
        var path = tmp.File("text.xlsx");

        using (var writer = new XlsxTableWriter(path))
            writer.WriteRow(new[] { "123", "007", "abc" });

        using var wb = new XLWorkbook(path);
        var ws = wb.Worksheet(1);

        // Значения-числа хранятся как текст — Excel не превратит их в числа
        // и не потеряет ведущие нули.
        Assert.Equal(XLDataType.Text, ws.Cell(1, 1).DataType);
        Assert.Equal(XLDataType.Text, ws.Cell(1, 2).DataType);
        Assert.Equal("007", ws.Cell(1, 2).GetString());
    }


    [Fact]
    public void Writer_then_reader_round_trips_rows()
    {
        using var tmp = new TempDir();
        var path = tmp.File("out.xlsx");
        var rows = new[]
        {
            new[] { "id", "name" },
            new[] { "1", "Alice" },
            new[] { "2", "Bob" },
        };

        using (var writer = new XlsxTableWriter(path))
            foreach (var row in rows)
                writer.WriteRow(row);

        string[][] readBack;
        using (var reader = new ExcelTableReader(path, sheetIndex: 0))
            readBack = reader.ReadRows().ToArray();

        Assert.Equal(rows, readBack);
    }
}