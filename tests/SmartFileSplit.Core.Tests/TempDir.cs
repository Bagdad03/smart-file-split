namespace SmartFileSplit.Core.Tests;

/// <summary>
/// Временный каталог для фикстур одного теста: создаётся в конструкторе,
/// рекурсивно удаляется в <see cref="Dispose"/>. Используется через
/// xUnit-паттерн `using var tmp = new TempDir();`.
/// </summary>
public sealed class TempDir : IDisposable
{
    public string Path { get; }

    public TempDir()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "sfs_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    /// <summary>Абсолютный путь к файлу с именем <paramref name="name"/> внутри каталога.</summary>
    public string File(string name) => System.IO.Path.Combine(Path, name);

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
        catch
        {
            // Уборка временных файлов не должна валить тест.
        }
    }
}