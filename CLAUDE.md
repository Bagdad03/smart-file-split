# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project status

The **Core library is implemented and fully unit-tested** (27 xUnit tests green):
readers/writers (`CsvTableReader`/`CsvTableWriter`, `ExcelTableReader`/`XlsxTableWriter`),
`ReaderFactory`/`WriterFactory`, `WorkbookInspector`, and the `FileSplitter` algorithm.
The **App is still a WinForms stub** (`Form1`) — the UI (`MainForm`) and the single-file
`.exe` publish are the next piece of work. The originally approved design lives at
`C:\Users\lvv\.claude\plans\enumerated-juggling-curry.md`.

## What this is

A Windows desktop utility (GUI) that splits one tabular file (`.xlsx`, `.xls`, `.csv`)
into several files. The user picks **either** a maximum number of rows per file **or** a
desired number of output files. Stack: **C# / .NET 10 (LTS), WinForms**, edited in
VS Code (C# Dev Kit). Distributed as a self-contained single `.exe`.

## Commands

The .NET 10 SDK is required (installed: 10.0.301). Run from the repo root.

```bash
dotnet build                                   # build whole solution
dotnet test                                    # run all xUnit core tests
dotnet test --filter "FullyQualifiedName~ByRowCount"   # run a single test / subset
dotnet run --project src/SmartFileSplit.App    # launch the WinForms GUI
```

Release single-file `.exe` (runs without .NET installed on the target machine):

```bash
dotnet publish src/SmartFileSplit.App -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

## Architecture

The system deliberately separates a UI-free **Core** library from the **App** so the
splitting logic is unit-testable without a window.

- `src/SmartFileSplit.Core/` — all logic, no UI. Key seams:
  - **Reading** is abstracted behind `ITableReader` (streams rows as `string[]`).
    `ReaderFactory` chooses an implementation by file extension: `ExcelTableReader`
    (xls + xlsx, via **ExcelDataReader** — reads old BIFF `.xls` without Excel
    installed) or `CsvTableReader` (via **CsvHelper**).
  - **Writing** is abstracted behind `ITableWriter` (writes one output file).
    `WriterFactory` chooses by the **user-selected output format** (not the input
    format): `XlsxTableWriter` (**ClosedXML**) or `CsvTableWriter` (**CsvHelper**).
    Old `.xls` input is therefore re-emitted as `.xlsx` or `.csv`, never `.xls`.
  - `FileSplitter` is the core algorithm, decoupled from both ends via the reader/writer
    interfaces and reports progress through `IProgress<int>`.
  - `WorkbookInspector` lists sheets for Excel files (the UI lets the user pick one).
- `src/SmartFileSplit.App/` — WinForms `.exe` (`MainForm`, `Program.cs`). The UI builds
  a `SplitOptions` and runs `FileSplitter.Split` on a background thread (`Task.Run`),
  feeding `IProgress<int>` to a ProgressBar.
- `tests/SmartFileSplit.Core.Tests/` — xUnit tests against Core only.

### Splitting semantics (the part worth understanding before editing `FileSplitter`)

- **ByRowCount(N):** accumulate N *data* rows per output file; last file holds the
  remainder.
- **ByFileCount(K):** requires the total data-row count first, then part size =
  `ceil(total / K)` and proceeds like ByRowCount. When `total < K`, fewer than K files
  are produced (expected — surface this to the user rather than emitting empty files).
- **Header handling** is a user checkbox. When on, the first row is treated as a header:
  it is copied to the top of *every* output file and is **not** counted as a data row.
  When off, the first row is ordinary data. Keep this distinction central to the
  algorithm — most edge-case tests hinge on it.
- Output naming: `{basename}_{i}.{ext}` starting at 1, into a user-chosen folder, with
  the numeric suffix zero-padded to the part count (`_01`, `_02` once there are ≥10).

## Conventions

- CSV defaults: UTF-8 **with BOM** (Excel-friendly) and `,` separator, with a UI toggle
  to `;` for Russian Excel locales. The separator is part of `SplitOptions` and flows to
  both `CsvTableReader` and `CsvTableWriter` — keep read and write separators consistent.
- The output format is independent of the input format; never assume they match.