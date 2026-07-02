# SmartFileSplit

A Windows GUI utility that splits a single tabular file (`.xlsx`, `.xls`, `.csv`)
into several files. You choose **either** a maximum number of rows per file **or** a
desired number of output files, and the tool cuts the source into parts.

Stack: **C# / .NET 10, WinForms**. Distributed as a self-contained single `.exe`
(no .NET installation required on the target machine).

## Features

- Input: `.xlsx`, `.xls` (legacy BIFF, no Excel required), `.csv`.
- Two split modes: by max rows per file, or by number of output files.
- "First row is a header" option: the header is copied to the top of every output
  file and is not counted as a data row.
- Output format (**CSV** or **XLSX**) is chosen independently of the input format.
- Sheet selection for multi-sheet Excel workbooks.
- CSV: UTF-8 with BOM (Excel-friendly), delimiter `,` or `;` (for Russian locale).
- **CSV → CSV** is copied verbatim: values quoted in the source keep their quotes.
- **XLSX output** stores values as text, so leading zeros and long codes
  (`007`, contract numbers) are not turned into numbers by Excel.
- Background splitting with a progress bar, input validation, and overwrite protection.

## Requirements

- To **run the prebuilt `.exe`** — nothing (it is self-contained).
- To **build from source** — [.NET 10 SDK](https://dotnet.microsoft.com/download).

## Build and test

From the repository root:

```bash
dotnet build                                 # build the solution
dotnet test                                  # run the core unit tests
dotnet run --project src/SmartFileSplit.App  # launch the GUI
```

Build the release single-file `.exe` (runs without an installed .NET):

```bash
dotnet publish src/SmartFileSplit.App -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The resulting file appears at
`src/SmartFileSplit.App/bin/Release/net10.0-windows/win-x64/publish/SmartFileSplit.App.exe`.

## Usage

1. **Browse…** next to "Input file" — pick an `.xlsx`/`.xls`/`.csv`
   (for Excel a sheet list appears).
2. Choose the mode (max rows per file / number of files) and the number.
3. Optionally enable "First row is a header".
4. Choose the output format (CSV or XLSX) and, for CSV, the delimiter (`,` or `;`).
5. **Browse…** next to "Output folder".
6. Click **Split**.

Output files are named `{name}_{index}.{ext}`, starting at 1, zero-padded to the
part count (`_01`, `_02`, …).

## Architecture

The logic is deliberately separated from the UI so the core can be unit-tested
without a window.

- `src/SmartFileSplit.Core/` — UI-free library:
  - `ITableReader` / `ITableWriter` — read/write abstractions; `ReaderFactory`
    picks a reader by input extension, `WriterFactory` picks a writer by the chosen
    output format.
  - `CsvTableReader` / `CsvTableWriter` (CsvHelper), `ExcelTableReader`
    (ExcelDataReader), `XlsxTableWriter` (ClosedXML), `WorkbookInspector`.
  - `FileSplitter` — the splitting algorithm, reporting progress via `IProgress<int>`.
- `src/SmartFileSplit.App/` — WinForms application (`MainForm`).
- `tests/SmartFileSplit.Core.Tests/` — xUnit tests for the core.

## License

[MIT](LICENSE).