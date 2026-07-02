namespace SmartFileSplit.Core.Tests;

/// <summary>
/// Синхронный <see cref="IProgress{T}"/>: вызывает колбэк прямо в потоке
/// <c>Report</c>, в отличие от <see cref="Progress{T}"/>, который доставляет
/// асинхронно через SynchronizationContext. Нужен для детерминированной
/// проверки значений прогресса в тестах.
/// </summary>
internal sealed class SynchronousProgress : IProgress<int>
{
    private readonly Action<int> _onReport;

    public SynchronousProgress(Action<int> onReport) => _onReport = onReport;

    public void Report(int value) => _onReport(value);
}