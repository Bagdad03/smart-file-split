using System.Text;

namespace SmartFileSplit.Core;

/// <summary>
/// Регистрация провайдера устаревших кодовых страниц, которого требует
/// ExcelDataReader для чтения старых BIFF-файлов .xls. Регистрируется
/// один раз за процесс.
/// </summary>
internal static class ExcelEncoding
{
    private static bool _registered;
    private static readonly object Gate = new();

    public static void EnsureRegistered()
    {
        if (_registered)
            return;
        lock (Gate)
        {
            if (_registered)
                return;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _registered = true;
        }
    }
}