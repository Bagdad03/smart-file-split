using System.Text.RegularExpressions;
using SmartFileSplit.Core;

namespace SmartFileSplit.App;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        UpdateDelimiterState();
    }

    /// <summary>Выбор входного файла + заполнение списка листов для Excel.</summary>
    private void btnBrowseInput_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Таблицы (*.xlsx;*.xls;*.csv)|*.xlsx;*.xls;*.csv|Все файлы (*.*)|*.*",
            Title = "Выберите входной файл",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        txtInput.Text = dialog.FileName;
        PopulateSheets(dialog.FileName);
        UpdateDelimiterState();

        // Папку вывода по умолчанию подставляем рядом с входным файлом.
        if (string.IsNullOrEmpty(txtOutput.Text))
            txtOutput.Text = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
    }

    /// <summary>Заполняет выпадающий список листов; для CSV список отключается.</summary>
    private void PopulateSheets(string path)
    {
        cmbSheet.Items.Clear();
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext != ".xls" && ext != ".xlsx")
        {
            cmbSheet.Enabled = false;
            return;
        }

        try
        {
            var sheets = WorkbookInspector.GetSheets(path);
            foreach (var sheet in sheets)
                cmbSheet.Items.Add(sheet.Name);

            cmbSheet.Enabled = cmbSheet.Items.Count > 0;
            if (cmbSheet.Items.Count > 0)
                cmbSheet.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            cmbSheet.Enabled = false;
            MessageBox.Show(this, $"Не удалось прочитать листы файла:\n{ex.Message}",
                "Ошибка чтения", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void cmbFormat_SelectedIndexChanged(object? sender, EventArgs e) => UpdateDelimiterState();

    /// <summary>
    /// Разделитель CSV релевантен, если CSV участвует в операции: либо вход —
    /// .csv (нужен для чтения), либо вывод — CSV (нужен для записи).
    /// </summary>
    private void UpdateDelimiterState()
    {
        var inputIsCsv = Path.GetExtension(txtInput.Text).ToLowerInvariant() == ".csv";
        var outputIsCsv = (string?)cmbFormat.SelectedItem == "CSV";
        var relevant = inputIsCsv || outputIsCsv;
        cmbDelimiter.Enabled = relevant;
        lblDelimiter.Enabled = relevant;
    }

    private void btnBrowseOutput_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog { Description = "Выберите папку для выходных файлов" };
        if (!string.IsNullOrEmpty(txtOutput.Text))
            dialog.SelectedPath = txtOutput.Text;
        if (dialog.ShowDialog(this) == DialogResult.OK)
            txtOutput.Text = dialog.SelectedPath;
    }

    private async void btnSplit_Click(object? sender, EventArgs e)
    {
        if (!TryBuildOptions(out var options))
            return;

        // Защита от перезаписи и очистка «хвоста» от прошлого прогона.
        if (!ConfirmAndClearPreviousParts(options))
            return;

        SetBusy(true);
        progress.Value = 0;
        AppendStatus($"Разбиение «{Path.GetFileName(options.InputPath)}»…");

        var reporter = new Progress<int>(p => progress.Value = Math.Clamp(p, 0, 100));

        try
        {
            var result = await Task.Run(() => new FileSplitter().Split(options, reporter));

            if (result.NoData)
            {
                AppendStatus("В файле нет строк данных для разбиения.");
                MessageBox.Show(this, "В файле нет строк данных для разбиения.",
                    "Нет данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                progress.Value = 100;
                AppendStatus($"Готово: создано файлов — {result.PartCount}, строк данных — {result.DataRowCount}.");
                AppendStatus($"Папка: {options.OutputDirectory}");
            }
        }
        catch (Exception ex)
        {
            AppendStatus($"Ошибка: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Ошибка при разбиении",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Валидирует ввод и собирает <see cref="SplitOptions"/>.</summary>
    private bool TryBuildOptions(out SplitOptions options)
    {
        options = null!;

        if (string.IsNullOrWhiteSpace(txtInput.Text) || !File.Exists(txtInput.Text))
        {
            Warn("Выберите существующий входной файл.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtOutput.Text) || !Directory.Exists(txtOutput.Text))
        {
            Warn("Выберите существующую папку вывода.");
            return false;
        }

        var format = (string?)cmbFormat.SelectedItem == "XLSX" ? OutputFormat.Xlsx : OutputFormat.Csv;
        var delimiter = ((string?)cmbDelimiter.SelectedItem ?? ",")[0];

        options = new SplitOptions
        {
            InputPath = txtInput.Text,
            OutputDirectory = txtOutput.Text,
            Mode = rbByRows.Checked ? SplitMode.ByRowCount : SplitMode.ByFileCount,
            Value = (int)numValue.Value,
            HasHeader = chkHeader.Checked,
            SheetIndex = cmbSheet.SelectedIndex < 0 ? 0 : cmbSheet.SelectedIndex,
            OutputFormat = format,
            CsvDelimiter = delimiter,
        };
        return true;
    }

    /// <summary>
    /// Если в папке уже есть части с таким именем — спрашиваем подтверждение и,
    /// после согласия, удаляем их. Это убирает «хвост» прошлого прогона (когда
    /// новых частей меньше или у номера другая ширина, например _09 → _9).
    /// </summary>
    private bool ConfirmAndClearPreviousParts(SplitOptions options)
    {
        var existing = FindPreviousParts(options);
        if (existing.Length == 0)
            return true;

        var answer = MessageBox.Show(this,
            $"В папке уже есть {existing.Length} файл(ов) от прошлого разбиения.\nОни будут удалены и заменены новыми. Продолжить?",
            "Файлы уже существуют", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
        if (answer != DialogResult.OK)
            return false;

        foreach (var file in existing)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Не удалось удалить прежний файл «{Path.GetFileName(file)}»:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Ищет файлы прошлого прогона строго по формату имён <c>{basename}_{цифры}.{ext}</c>.
    /// Точное совпадение (а не glob <c>_*</c>) исключает удаление посторонних файлов
    /// вроде <c>src_backup.csv</c> и корректно обрабатывает спецсимволы в имени.
    ///
    /// Важная гарантия: сам входной файл под этот паттерн не подпадает — его имя равно
    /// <c>{basename}.{ext}</c>, а паттерн требует дополнительный суффикс <c>_{цифры}</c>.
    /// Поэтому даже когда папка вывода совпадает с папкой входа, входной файл не будет
    /// удалён до чтения. Не ослабляйте паттерн (например до <c>_*</c>), не пересмотрев это.
    /// </summary>
    private static string[] FindPreviousParts(SplitOptions options)
    {
        var basename = Path.GetFileNameWithoutExtension(options.InputPath);
        var ext = WriterFactory.ExtensionFor(options.OutputFormat);
        var pattern = new Regex($@"^{Regex.Escape(basename)}_\d+\.{Regex.Escape(ext)}$", RegexOptions.IgnoreCase);

        return Directory.EnumerateFiles(options.OutputDirectory)
            .Where(f => pattern.IsMatch(Path.GetFileName(f)))
            .ToArray();
    }

    private void SetBusy(bool busy)
    {
        btnSplit.Enabled = !busy;
        btnBrowseInput.Enabled = !busy;
        btnBrowseOutput.Enabled = !busy;
        UseWaitCursor = busy;
    }

    private void Warn(string message) =>
        MessageBox.Show(this, message, "Проверьте параметры", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private void AppendStatus(string message) =>
        txtStatus.AppendText(message + Environment.NewLine);
}