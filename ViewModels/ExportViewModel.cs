using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ledger_vault.Data;
using ledger_vault.Services;
using System.Text.Json;
using System.Text;
using System.Xml.Linq;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Linq;
using ledger_vault.Models;

namespace ledger_vault.ViewModels;

public partial class ExportViewModel : PageViewModel
{
    #region PUBLIC API

    public bool HaveBeenExported => ExportPath.Length > 0;

    public ExportViewModel(TransactionService transactionService)
    {
        PageName = ApplicationPages.Export;

        _transactionService = transactionService;
    }

#pragma warning disable
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ExportViewModel()
    {
    }
#pragma warning restore

    #endregion

    #region PRIVATE PROPERTIES

    [ObservableProperty] private int _selectedExportFormat;
    private static readonly List<string> ExportFormats = ["PDF", "CSV", "XML", "JSON", "XLSX"];

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HaveBeenExported))]
    private string _exportPath = string.Empty;

    [ObservableProperty] private bool _isExporting;

    [ObservableProperty] private int _exportProgress;

    [ObservableProperty] private string _exportStatusText = string.Empty;

    private readonly TransactionService _transactionService;

    #endregion

    #region PRIVATE METHODS

    [RelayCommand]
    private void ExportTransactions()
    {
        Task.Run(async () =>
        {
            try
            {
                IsExporting = true;
                ExportProgress = 0;
                ExportStatusText = "Starting export...";

                await Task.Delay(100); // Small delay to ensure UI updates

                ExportStatusText = "Fetching transactions...";
                ExportProgress = 10;
                List<Transaction> transactions = await _transactionService.GetTransactionsAsync();

                ExportStatusText = "Processing data...";
                ExportProgress = 30;

                string? fileContent = ExportFormats[SelectedExportFormat] switch
                {
                    "PDF" => await Task.Run(() => ExportTransactionsToPdf(transactions)),
                    "CSV" => await Task.Run(() => ExportTransactionsToCsv(transactions)),
                    "XML" => await Task.Run(() => ExportTransactionsToXml(transactions)),
                    "JSON" => await Task.Run(() => ExportTransactionsToJson(transactions)),
                    "XLSX" => await Task.Run(() => ExportTransactionsToXlsx(transactions)),
                    _ => null
                };

                if (fileContent == null)
                    throw new NullReferenceException($"{nameof(ExportTransactions)}: fileContent is null");

                ExportStatusText = "Creating export file...";
                ExportProgress = 70;

                string fileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "LedgerVault");
                Directory.CreateDirectory(fileDir);

                string filePath = Path.Combine(fileDir,
                    "Transactions_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." +
                    ExportFormats[SelectedExportFormat].ToLower());

                ExportStatusText = "Writing file...";
                ExportProgress = 90;

                if (ExportFormats[SelectedExportFormat] == "XLSX" || ExportFormats[SelectedExportFormat] == "PDF")
                {
                    // For binary formats, we need to write bytes, not text
                    await File.WriteAllBytesAsync(filePath, Convert.FromBase64String(fileContent));
                }
                else
                {
                    await File.WriteAllTextAsync(filePath, fileContent);
                }

                ExportProgress = 100;
                ExportStatusText = "Export completed successfully!";
                ExportPath = filePath;

                // Keep the completion message visible for a moment
                await Task.Delay(1500);
            }
            catch (Exception ex)
            {
                ExportStatusText = $"Export failed: {ex.Message}";
                ExportProgress = 0;

                // Keep error message visible longer
                await Task.Delay(3000);
            }
            finally
            {
                IsExporting = false;
                ExportStatusText = string.Empty;
                ExportProgress = 0;
            }
        });
    }

    private string ExportTransactionsToPdf(IEnumerable<Transaction> transactions)
    {
        const float titleFontSize = 20;
        const float headingFontSize = 10;
        const float marginBottom = 20;
        const int numColumns = 9;

        try
        {
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add title and generation date
            document.Add(new Paragraph("Transaction Report")
                .SetFontSize(titleFontSize)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(marginBottom));

            document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                .SetFontSize(headingFontSize)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginBottom(marginBottom));

            // Create table with the correct number of columns
            var table = new Table(UnitValue.CreatePercentArray(numColumns)).UseAllAvailableWidth();

            // Add headers
            string[] headers =
                ["ID", "Date", "Counterparty", "Description", "Amount", "Tags", "Reverted", "Verified", "Signed"];
            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(header).SimulateBold()));
            }

            // Add transaction data
            foreach (var transaction in transactions)
            {
                table.AddCell(new Cell().Add(new Paragraph(transaction.Id.ToString())));
                table.AddCell(new Cell().Add(new Paragraph(transaction.Timestamp.ToString("yyyy-MM-dd HH:mm"))));
                table.AddCell(new Cell().Add(new Paragraph(transaction.Counterparty)));
                table.AddCell(new Cell().Add(new Paragraph(transaction.Description)));
                table.AddCell(new Cell().Add(new Paragraph(transaction.Amount.ToString("N"))));
                table.AddCell(new Cell().Add(new Paragraph(string.Join(", ", transaction.Tags))));
                table.AddCell(new Cell().Add(new Paragraph(transaction.IsReverted ? "Yes" : "No")));
                table.AddCell(
                    new Cell().Add(new Paragraph(transaction.HashVerifiedStatus == HashStatus.Valid ? "Yes" : "No")));
                table.AddCell(new Cell().Add(new Paragraph(transaction.SignatureVerifiedStatus == SignatureStatus.Valid
                    ? "Yes"
                    : "No")));
            }

            document.Add(table);

            document.Close();

            return Convert.ToBase64String(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred during PDF export: {ex}");
        }
    }

    private string ExportTransactionsToCsv(IEnumerable<Transaction> transactions)
    {
        var csv = new StringBuilder();

        // Add header
        csv.AppendLine(
            "ID,Date,Counterparty,Description,Amount,Tags,Reverted,Verified,Signed");

        // Add data
        foreach (var transaction in transactions)
        {
            var line = $"{transaction.Id}," +
                       $"\"{transaction.Timestamp:yyyy-MM-dd HH:mm:ss}\"," +
                       $"\"{EscapeCsvField(transaction.Counterparty)}\"," +
                       $"\"{EscapeCsvField(transaction.Description)}\"," +
                       $"{transaction.Amount}," +
                       $"\"{string.Join("; ", transaction.Tags)}\"," +
                       $"{transaction.IsReverted}," +
                       $"\"{EscapeCsvField(transaction.HashVerifiedStatus == HashStatus.Valid ? "Yes" : "No")}\"," +
                       $"{EscapeCsvField(transaction.SignatureVerifiedStatus == SignatureStatus.Valid ? "Yes" : "No")}";
            csv.AppendLine(line);
        }

        return csv.ToString();
    }

    private string ExportTransactionsToXml(IEnumerable<Transaction> transactions)
    {
        var root = new XElement("TransactionData",
            new XElement("GeneratedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
            new XElement("Transactions",
                from transaction in transactions
                select new XElement("Transaction",
                    new XElement("Id", transaction.Id),
                    new XElement("Date", transaction.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")),
                    new XElement("Counterparty", transaction.Counterparty ?? ""),
                    new XElement("Description", transaction.Description ?? ""),
                    new XElement("Amount", transaction.Amount),
                    new XElement("Tags",
                        from tag in transaction.Tags
                        select new XElement("Tag", tag)
                    ),
                    new XElement("Reverted", transaction.IsReverted),
                    new XElement("Verified", transaction.HashVerifiedStatus == HashStatus.Valid ? "Yes" : "No"),
                    new XElement("Signed", transaction.SignatureVerifiedStatus == SignatureStatus.Valid ? "Yes" : "No")
                )
            )
        );

        return root.ToString();
    }

    private string ExportTransactionsToJson(IEnumerable<Transaction> transactions)
    {
        var exportData = new
        {
            GeneratedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Transactions = transactions
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(exportData, options);
    }

    private string ExportTransactionsToXlsx(IEnumerable<Transaction> transactions)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");

        // Add headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Date";
        worksheet.Cell(1, 3).Value = "Counterparty";
        worksheet.Cell(1, 4).Value = "Description";
        worksheet.Cell(1, 5).Value = "Amount";
        worksheet.Cell(1, 6).Value = "Tags";
        worksheet.Cell(1, 7).Value = "Reverted";
        worksheet.Cell(1, 8).Value = "Verified";
        worksheet.Cell(1, 9).Value = "Signed";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 12);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Add data
        int row = 2;
        foreach (var transaction in transactions)
        {
            worksheet.Cell(row, 1).Value = transaction.Id;
            worksheet.Cell(row, 2).Value = transaction.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 3).Value = transaction.Counterparty;
            worksheet.Cell(row, 4).Value = transaction.Description;
            worksheet.Cell(row, 5).Value = transaction.Amount;
            worksheet.Cell(row, 6).Value = string.Join(", ", transaction.Tags);
            worksheet.Cell(row, 7).Value = transaction.IsReverted ? "Yes" : "No";
            worksheet.Cell(row, 8).Value = transaction.HashVerifiedStatus == HashStatus.Valid ? "Yes" : "No";
            worksheet.Cell(row, 9).Value =
                transaction.SignatureVerifiedStatus == SignatureStatus.InProgress ? "Yes" : "No";
            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Save to memory stream and return as base64
        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static string EscapeCsvField(string? field)
    {
        // Replace quotes with double quotes and handle commas/newlines
        return string.IsNullOrEmpty(field) ? "" : field.Replace("\"", "\"\"");
    }

    #endregion PRIVATE METHODS
}