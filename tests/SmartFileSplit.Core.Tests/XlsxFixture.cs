using ClosedXML.Excel;

namespace SmartFileSplit.Core.Tests;

/// <summary>
/// Хелпер для генерации xlsx-фикстур напрямую через ClosedXML,
/// чтобы тесты Excel-ридера/инспектора не зависели от XlsxTableWriter.
/// </summary>
internal static class XlsxFixture
{
    /// <summary>
    /// Создаёт книгу: на каждый элемент <paramref name="sheets"/> — лист с именем
    /// и набором строк (значения ячеек).
    /// </summary>
    public static void Create(string path, params (string Name, string[][] Rows)[] sheets)
    {
        using var wb = new XLWorkbook();
        foreach (var (name, rows) in sheets)
        {
            var ws = wb.AddWorksheet(name);
            for (var r = 0; r < rows.Length; r++)
                for (var c = 0; c < rows[r].Length; c++)
                    ws.Cell(r + 1, c + 1).Value = rows[r][c];
        }
        wb.SaveAs(path);
    }
}