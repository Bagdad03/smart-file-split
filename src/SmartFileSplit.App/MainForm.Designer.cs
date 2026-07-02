namespace SmartFileSplit.App;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblInput;
    private TextBox txtInput;
    private Button btnBrowseInput;

    private Label lblSheet;
    private ComboBox cmbSheet;

    private GroupBox grpMode;
    private RadioButton rbByRows;
    private RadioButton rbByFiles;
    private NumericUpDown numValue;

    private CheckBox chkHeader;

    private Label lblFormat;
    private ComboBox cmbFormat;
    private Label lblDelimiter;
    private ComboBox cmbDelimiter;

    private Label lblOutput;
    private TextBox txtOutput;
    private Button btnBrowseOutput;

    private Button btnSplit;
    private ProgressBar progress;
    private TextBox txtStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        lblInput = new Label();
        txtInput = new TextBox();
        btnBrowseInput = new Button();
        lblSheet = new Label();
        cmbSheet = new ComboBox();
        grpMode = new GroupBox();
        rbByRows = new RadioButton();
        rbByFiles = new RadioButton();
        numValue = new NumericUpDown();
        chkHeader = new CheckBox();
        lblFormat = new Label();
        cmbFormat = new ComboBox();
        lblDelimiter = new Label();
        cmbDelimiter = new ComboBox();
        lblOutput = new Label();
        txtOutput = new TextBox();
        btnBrowseOutput = new Button();
        btnSplit = new Button();
        progress = new ProgressBar();
        txtStatus = new TextBox();

        grpMode.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numValue).BeginInit();
        SuspendLayout();

        // lblInput
        lblInput.AutoSize = true;
        lblInput.Location = new Point(12, 15);
        lblInput.Text = "Входной файл:";

        // txtInput
        txtInput.Location = new Point(120, 12);
        txtInput.ReadOnly = true;
        txtInput.Size = new Size(320, 23);

        // btnBrowseInput
        btnBrowseInput.Location = new Point(450, 11);
        btnBrowseInput.Size = new Size(90, 25);
        btnBrowseInput.Text = "Обзор…";
        btnBrowseInput.Click += btnBrowseInput_Click;

        // lblSheet
        lblSheet.AutoSize = true;
        lblSheet.Location = new Point(12, 48);
        lblSheet.Text = "Лист:";

        // cmbSheet
        cmbSheet.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbSheet.Location = new Point(120, 45);
        cmbSheet.Size = new Size(320, 23);
        cmbSheet.Enabled = false;

        // grpMode
        grpMode.Controls.Add(rbByRows);
        grpMode.Controls.Add(rbByFiles);
        grpMode.Controls.Add(numValue);
        grpMode.Location = new Point(12, 80);
        grpMode.Size = new Size(528, 78);
        grpMode.Text = "Режим разбиения";

        // rbByRows
        rbByRows.AutoSize = true;
        rbByRows.Checked = true;
        rbByRows.Location = new Point(15, 22);
        rbByRows.Text = "Максимум строк на файл";

        // rbByFiles
        rbByFiles.AutoSize = true;
        rbByFiles.Location = new Point(15, 48);
        rbByFiles.Text = "Число выходных файлов";

        // numValue
        numValue.Location = new Point(360, 33);
        numValue.Size = new Size(150, 23);
        numValue.Minimum = 1;
        numValue.Maximum = 100000000;
        numValue.Value = 1000;
        numValue.ThousandsSeparator = true;

        // chkHeader
        chkHeader.AutoSize = true;
        chkHeader.Checked = true;
        chkHeader.Location = new Point(12, 168);
        chkHeader.Text = "Первая строка — заголовок (копируется в каждый файл)";

        // lblFormat
        lblFormat.AutoSize = true;
        lblFormat.Location = new Point(12, 202);
        lblFormat.Text = "Формат вывода:";

        // cmbFormat
        cmbFormat.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFormat.Location = new Point(120, 199);
        cmbFormat.Size = new Size(110, 23);
        cmbFormat.Items.AddRange(new object[] { "CSV", "XLSX" });
        cmbFormat.SelectedIndex = 0;
        cmbFormat.SelectedIndexChanged += cmbFormat_SelectedIndexChanged;

        // lblDelimiter
        lblDelimiter.AutoSize = true;
        lblDelimiter.Location = new Point(250, 202);
        lblDelimiter.Text = "Разделитель CSV:";

        // cmbDelimiter
        cmbDelimiter.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbDelimiter.Location = new Point(370, 199);
        cmbDelimiter.Size = new Size(70, 23);
        cmbDelimiter.Items.AddRange(new object[] { ",", ";" });
        cmbDelimiter.SelectedIndex = 0;

        // lblOutput
        lblOutput.AutoSize = true;
        lblOutput.Location = new Point(12, 238);
        lblOutput.Text = "Папка вывода:";

        // txtOutput
        txtOutput.Location = new Point(120, 235);
        txtOutput.ReadOnly = true;
        txtOutput.Size = new Size(320, 23);

        // btnBrowseOutput
        btnBrowseOutput.Location = new Point(450, 234);
        btnBrowseOutput.Size = new Size(90, 25);
        btnBrowseOutput.Text = "Обзор…";
        btnBrowseOutput.Click += btnBrowseOutput_Click;

        // btnSplit
        btnSplit.Location = new Point(12, 275);
        btnSplit.Size = new Size(120, 32);
        btnSplit.Text = "Разделить";
        btnSplit.Click += btnSplit_Click;

        // progress
        progress.Location = new Point(140, 280);
        progress.Size = new Size(400, 23);

        // txtStatus
        txtStatus.Location = new Point(12, 320);
        txtStatus.Multiline = true;
        txtStatus.ReadOnly = true;
        txtStatus.ScrollBars = ScrollBars.Vertical;
        txtStatus.Size = new Size(528, 100);

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(552, 435);
        Controls.Add(lblInput);
        Controls.Add(txtInput);
        Controls.Add(btnBrowseInput);
        Controls.Add(lblSheet);
        Controls.Add(cmbSheet);
        Controls.Add(grpMode);
        Controls.Add(chkHeader);
        Controls.Add(lblFormat);
        Controls.Add(cmbFormat);
        Controls.Add(lblDelimiter);
        Controls.Add(cmbDelimiter);
        Controls.Add(lblOutput);
        Controls.Add(txtOutput);
        Controls.Add(btnBrowseOutput);
        Controls.Add(btnSplit);
        Controls.Add(progress);
        Controls.Add(txtStatus);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SmartFileSplit — разбиение таблиц";

        grpMode.ResumeLayout(false);
        grpMode.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numValue).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}