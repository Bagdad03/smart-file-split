namespace SmartFileSplit.Core;

/// <summary>
/// Записывает строки в один выходной файл. Своя реализация на каждый
/// выходной формат (CSV, XLSX). Финальная запись/сохранение на диск
/// происходит при <see cref="IDisposable.Dispose"/>.
/// </summary>
public interface ITableWriter : IDisposable
{
    /// <summary>Дописывает одну строку (значения ячеек) в выходной файл.</summary>
    void WriteRow(string[] row);
}