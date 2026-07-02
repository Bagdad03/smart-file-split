using ExcelDataReader;

namespace SmartFileSplit.Core;

/// <summary>
/// Перечисляет листы Excel-файла, чтобы UI дал пользователю выбрать один.
/// Для CSV листов нет — возвращается пустой список.
/// </summary>
public static class WorkbookInspector
{
    public static IReadOnlyList<SheetInfo> GetSheets(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext != ".xls" && ext != ".xlsx")
            return Array.Empty<SheetInfo>();

        ExcelEncoding.EnsureRegistered();
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var sheets = new List<SheetInfo>();
        var index = 0;
        do
        {
            sheets.Add(new SheetInfo(index, reader.Name));
            index++;
        } while (reader.NextResult());

        return sheets;
    }
}