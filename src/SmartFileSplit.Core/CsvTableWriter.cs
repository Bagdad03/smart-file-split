using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace SmartFileSplit.Core;

/// <summary>
/// Пишет строки в CSV через CsvHelper: UTF-8 с BOM (дружелюбно к Excel),
/// разделитель задаётся вызывающим кодом. Экранирование значений с
/// разделителями/кавычками/переводами строк берёт на себя CsvHelper.
/// </summary>
public sealed class CsvTableWriter : ITableWriter
{
    private readonly StreamWriter _stream;
    private readonly CsvWriter _csv;

    public CsvTableWriter(string path, char delimiter)
    {
        // UTF-8 с BOM, чтобы Excel корректно распознавал кодировку.
        _stream = new StreamWriter(path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter.ToString(),
            HasHeaderRecord = false,
        };
        _csv = new CsvWriter(_stream, config);
    }

    public void WriteRow(string[] row)
    {
        foreach (var field in row)
            _csv.WriteField(field);
        _csv.NextRecord();
    }

    public void Dispose()
    {
        _csv.Dispose();
        _stream.Dispose();
    }
}