using System.Text;
using ExcelDataReader;

namespace SmartFileSplit.Core;

/// <summary>
/// Читает один лист Excel-файла (.xls или .xlsx) через ExcelDataReader
/// и отдаёт строки как string[]. Работает без установленного Excel.
/// </summary>
public sealed class ExcelTableReader : ITableReader
{
    private readonly FileStream _stream;
    private readonly IExcelDataReader _reader;

    public ExcelTableReader(string path, int sheetIndex)
    {
        ExcelEncoding.EnsureRegistered();
        _stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _reader = ExcelReaderFactory.CreateReader(_stream);

        // Перематываем к нужному листу: каждый лист — отдельный "result set".
        for (var i = 0; i < sheetIndex; i++)
        {
            if (!_reader.NextResult())
                throw new ArgumentOutOfRangeException(
                    nameof(sheetIndex), sheetIndex, "В файле нет листа с таким индексом.");
        }
    }

    public IEnumerable<string[]> ReadRows()
    {
        while (_reader.Read())
        {
            var row = new string[_reader.FieldCount];
            for (var c = 0; c < _reader.FieldCount; c++)
                row[c] = _reader.GetValue(c)?.ToString() ?? string.Empty;
            yield return row;
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
        _stream.Dispose();
    }
}