using System.Text;

namespace SmartFileSplit.Core;

/// <summary>
/// Ядро алгоритма разбиения: читает входную таблицу потоком через
/// <see cref="ITableReader"/> и раскладывает строки данных по нескольким
/// выходным файлам через <see cref="ITableWriter"/>. Не знает про конкретные
/// форматы — они выбираются фабриками.
///
/// Особый случай CSV→CSV обрабатывается дословно (см. <see cref="SplitCsvVerbatim"/>):
/// сырые строки копируются как есть, чтобы сохранить исходное квотирование
/// (поле, бывшее в кавычках, остаётся в кавычках).
/// </summary>
public sealed class FileSplitter
{
    // Алгоритм делает два прохода по входному файлу: первый считает строки
    // (нужно для partSize при ByFileCount и для ширины зеро-паддинга имён),
    // второй раскладывает данные. Допущение: файл не меняется между проходами.
    // Циклы раздачи управляются enumerator'ом, поэтому даже при расхождении
    // проходов не будет ни зацикливания, ни потери строк (в худшем случае —
    // косметически неверная ширина номера).
    public SplitResult Split(SplitOptions options, IProgress<int>? progress = null)
    {
        return IsVerbatimCsv(options)
            ? SplitCsvVerbatim(options, progress)
            : SplitGeneric(options, progress);
    }

    /// <summary>Дословный режим применим, когда и вход, и выход — CSV.</summary>
    private static bool IsVerbatimCsv(SplitOptions options) =>
        Path.GetExtension(options.InputPath).ToLowerInvariant() == ".csv"
        && options.OutputFormat == OutputFormat.Csv;

    // --- Общий путь (любые форматы через фабрики) ---

    private static SplitResult SplitGeneric(SplitOptions options, IProgress<int>? progress)
    {
        var total = CountDataRows(options);
        if (total == 0)
        {
            progress?.Report(100);
            return new SplitResult { OutputFiles = Array.Empty<string>(), DataRowCount = 0 };
        }

        var (partSize, _, width) = Plan(total, options);
        var basename = Path.GetFileNameWithoutExtension(options.InputPath);
        var ext = WriterFactory.ExtensionFor(options.OutputFormat);
        var outputs = new List<string>();

        using var reader = ReaderFactory.Create(options);
        using var rows = reader.ReadRows().GetEnumerator();

        string[]? header = null;
        if (options.HasHeader && rows.MoveNext())
            header = rows.Current;

        // Цикл управляется состоянием enumerator'а (а не значением total): если
        // второй проход даст иное число строк, чем первый, это не приведёт к
        // зацикливанию/пустым файлам или потере строк.
        var reporter = new PercentReporter(progress, total);
        var written = 0;
        var partIndex = 0;
        var hasMore = rows.MoveNext();
        while (hasMore)
        {
            partIndex++;
            var path = PartPath(options, basename, partIndex, width, ext);
            using (var writer = WriterFactory.Create(path, options))
            {
                if (header is not null)
                    writer.WriteRow(header);

                var inPart = 0;
                while (inPart < partSize && hasMore)
                {
                    writer.WriteRow(rows.Current);
                    inPart++;
                    reporter.Advance(++written);
                    hasMore = rows.MoveNext();
                }
            }
            outputs.Add(path);
        }

        progress?.Report(100);
        return new SplitResult { OutputFiles = outputs, DataRowCount = written };
    }

    // --- Дословный путь CSV→CSV (сохраняет исходное квотирование) ---

    private static SplitResult SplitCsvVerbatim(SplitOptions options, IProgress<int>? progress)
    {
        var total = CountCsvRecords(options);
        if (total == 0)
        {
            progress?.Report(100);
            return new SplitResult { OutputFiles = Array.Empty<string>(), DataRowCount = 0 };
        }

        var (partSize, _, width) = Plan(total, options);
        var basename = Path.GetFileNameWithoutExtension(options.InputPath);
        var outputs = new List<string>();

        using var reader = new CsvTableReader(options.InputPath, options.CsvDelimiter);
        using var records = reader.ReadRawRecords().GetEnumerator();

        // Заголовок — сырая первая запись; копируется в начало каждого файла.
        string? header = null;
        if (options.HasHeader && records.MoveNext())
            header = records.Current;

        // Как и в общем пути, цикл управляется enumerator'ом, а не total.
        var reporter = new PercentReporter(progress, total);
        var written = 0;
        var partIndex = 0;
        var hasMore = records.MoveNext();
        while (hasMore)
        {
            partIndex++;
            var path = PartPath(options, basename, partIndex, width, "csv");

            // UTF-8 с BOM — как в CsvTableWriter (дружелюбно к Excel).
            using (var w = new StreamWriter(path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
            {
                if (header is not null)
                    w.Write(header);

                var inPart = 0;
                while (inPart < partSize && hasMore)
                {
                    w.Write(records.Current);
                    inPart++;
                    reporter.Advance(++written);
                    hasMore = records.MoveNext();
                }
            }
            outputs.Add(path);
        }

        progress?.Report(100);
        return new SplitResult { OutputFiles = outputs, DataRowCount = written };
    }

    // --- Общие хелперы ---

    /// <summary>Размер части, число частей и ширина зеро-паддинга номера.</summary>
    private static (int PartSize, int PartCount, int Width) Plan(int total, SplitOptions options)
    {
        var partSize = options.Mode == SplitMode.ByFileCount
            ? (int)Math.Ceiling((double)total / options.Value)
            : options.Value;
        var partCount = (int)Math.Ceiling((double)total / partSize);
        return (partSize, partCount, partCount.ToString().Length);
    }

    private static string PartPath(SplitOptions options, string basename, int index, int width, string ext) =>
        Path.Combine(options.OutputDirectory, $"{basename}_{index.ToString().PadLeft(width, '0')}.{ext}");

    /// <summary>
    /// Докладывает прогресс (0..100) в <see cref="IProgress{T}"/>, но только когда
    /// целочисленный процент изменился — иначе для файлов на сотни тысяч строк
    /// очередь <see cref="Progress{T}"/> заваливается лишними сообщениями.
    /// </summary>
    private sealed class PercentReporter
    {
        private readonly IProgress<int>? _progress;
        private readonly int _total;
        private int _lastPercent = -1;

        public PercentReporter(IProgress<int>? progress, int total)
        {
            _progress = progress;
            _total = total;
        }

        public void Advance(int written)
        {
            if (_progress is null || _total <= 0)
                return;
            var percent = (int)(written * 100L / _total);
            if (percent != _lastPercent)
            {
                _lastPercent = percent;
                _progress.Report(percent);
            }
        }
    }

    /// <summary>Считает строки данных общего ридера (вычитая заголовок, если он есть).</summary>
    private static int CountDataRows(SplitOptions options)
    {
        using var reader = ReaderFactory.Create(options);
        var count = 0;
        foreach (var _ in reader.ReadRows())
            count++;

        if (options.HasHeader && count > 0)
            count--;

        return count;
    }

    /// <summary>Считает записи CSV дословного ридера (вычитая заголовок, если он есть).</summary>
    private static int CountCsvRecords(SplitOptions options)
    {
        using var reader = new CsvTableReader(options.InputPath, options.CsvDelimiter);
        var count = 0;
        foreach (var _ in reader.ReadRawRecords())
            count++;

        if (options.HasHeader && count > 0)
            count--;

        return count;
    }
}