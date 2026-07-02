using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace SmartFileSplit.Core;

/// <summary>
/// Читает CSV через CsvHelper и отдаёт сырые поля каждой строки как string[].
/// Разделитель задаётся вызывающим кодом; заголовок не выделяется (это делает
/// уровень выше — <see cref="FileSplitter"/>).
///
/// Один экземпляр рассчитан на ОДИН проход: <see cref="ReadRows"/> и
/// <see cref="ReadRawRecords"/> тянут из общего парсера, поэтому не следует
/// вызывать их повторно или оба на одном экземпляре — создавайте новый.
/// </summary>
public sealed class CsvTableReader : ITableReader
{
    private readonly StreamReader _stream;
    private readonly CsvParser _parser;

    public CsvTableReader(string path, char delimiter)
    {
        // StreamReader сам распознаёт и пропускает BOM.
        _stream = new StreamReader(path);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter.ToString(),
            HasHeaderRecord = false,
        };
        _parser = new CsvParser(_stream, config);
    }

    public IEnumerable<string[]> ReadRows()
    {
        while (_parser.Read())
        {
            // parser.Record переиспользует внутренний буфер — отдаём копию.
            yield return (string[])_parser.Record!.Clone();
        }
    }

    /// <summary>
    /// Перечисляет сырые записи (исходный текст строки CSV вместе с завершающим
    /// переводом строки), без разбора и переэкранирования полей. Нужен для
    /// дословного копирования CSV→CSV, чтобы сохранить исходное квотирование.
    /// </summary>
    public IEnumerable<string> ReadRawRecords()
    {
        while (_parser.Read())
            yield return _parser.RawRecord;
    }

    public void Dispose()
    {
        _parser.Dispose();
        _stream.Dispose();
    }
}