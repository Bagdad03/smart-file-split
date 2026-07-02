using ClosedXML.Excel;

namespace SmartFileSplit.Core;

/// <summary>
/// Пишет строки в один лист .xlsx через ClosedXML. Строки копятся в книге
/// в памяти и сохраняются на диск при <see cref="Dispose"/>.
/// Значения записываются как текст, чтобы round-trip сохранял их дословно.
/// </summary>
public sealed class XlsxTableWriter : ITableWriter
{
    private readonly string _path;
    private readonly XLWorkbook _workbook;
    private readonly IXLWorksheet _sheet;
    private int _rowIndex = 1;

    public XlsxTableWriter(string path)
    {
        _path = path;
        _workbook = new XLWorkbook();
        _sheet = _workbook.AddWorksheet("Sheet1");
    }

    public void WriteRow(string[] row)
    {
        for (var c = 0; c < row.Length; c++)
        {
            // SetValue<string> пишет значение как текст, без автоопределения типа.
            _sheet.Cell(_rowIndex, c + 1).SetValue(row[c]);
        }
        _rowIndex++;
    }

    public void Dispose()
    {
        _workbook.SaveAs(_path);
        _workbook.Dispose();
    }
}