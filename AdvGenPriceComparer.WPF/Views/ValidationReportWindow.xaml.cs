using System;
using System.Windows;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for ValidationReportWindow.xaml
/// </summary>
public partial class ValidationReportWindow : Window
{
    private readonly ValidationReportViewModel _viewModel;

    public ValidationReportWindow(ValidationReportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        // Subscribe to close request
        _viewModel.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        // Handle export event
        if (e is ValidationReportExportEventArgs exportArgs)
        {
            ExportReport(exportArgs.Format);
        }
        
        Close();
    }

    private void ExportReport(ValidationReportFormat format)
    {
        if (_viewModel.Report == null) return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = format switch
            {
                ValidationReportFormat.Json => "JSON files (*.json)|*.json",
                ValidationReportFormat.Markdown => "Markdown files (*.md)|*.md",
                ValidationReportFormat.Csv => "CSV files (*.csv)|*.csv",
                ValidationReportFormat.Html => "HTML files (*.html)|*.html",
                _ => "All files (*.*)|*.*"
            },
            DefaultExt = format switch
            {
                ValidationReportFormat.Json => ".json",
                ValidationReportFormat.Markdown => ".md",
                ValidationReportFormat.Csv => ".csv",
                ValidationReportFormat.Html => ".html",
                _ => ".txt"
            },
            FileName = $"ValidationReport_{DateTime.Now:yyyyMMdd_HHmmss}{GetFileExtension(format)}"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var content = GenerateReportContent(format);
                System.IO.File.WriteAllText(dialog.FileName, content);
                MessageBox.Show($"Report exported to:\n{dialog.FileName}", "Export Successful", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export report:\n{ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private string GenerateReportContent(ValidationReportFormat format)
    {
        if (_viewModel.Report == null) return string.Empty;

        return format switch
        {
            ValidationReportFormat.Json => GenerateJsonReport(),
            ValidationReportFormat.Markdown => GenerateMarkdownReport(),
            ValidationReportFormat.Csv => GenerateCsvReport(),
            ValidationReportFormat.Html => GenerateHtmlReport(),
            _ => GenerateTextReport()
        };
    }

    private string GenerateJsonReport()
    {
        var options = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
        return System.Text.Json.JsonSerializer.Serialize(_viewModel.Report, options);
    }

    private string GenerateMarkdownReport()
    {
        var report = _viewModel.Report;
        if (report == null) return string.Empty;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Data Validation Report");
        sb.AppendLine();
        sb.AppendLine($"**Source:** {report.SourceName}");
        sb.AppendLine($"**Generated:** {report.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"**Status:** {report.Status}");
        sb.AppendLine();

        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"- **Total Items:** {report.Summary.TotalItems}");
        sb.AppendLine($"- **Valid:** {report.Summary.ValidItems}");
        sb.AppendLine($"- **With Warnings:** {report.Summary.ItemsWithWarnings}");
        sb.AppendLine($"- **Invalid:** {report.Summary.InvalidItems}");
        sb.AppendLine($"- **Total Errors:** {report.Summary.TotalErrors}");
        sb.AppendLine($"- **Total Warnings:** {report.Summary.TotalWarnings}");
        sb.AppendLine($"- **Success Rate:** {report.Summary.SuccessRate:F1}%");
        sb.AppendLine();

        if (report.Entries.Count > 0)
        {
            sb.AppendLine("## Validation Entries");
            sb.AppendLine();
            sb.AppendLine("| Severity | Type | Entity | Field | Message |");
            sb.AppendLine("|----------|------|--------|-------|----------|");

            foreach (var entry in report.Entries)
            {
                sb.AppendLine($"| {entry.Severity} | {entry.EntryType} | {entry.EntityName} | {entry.FieldName} | {entry.Message} |");
            }
            sb.AppendLine();
        }

        if (report.DuplicateDetection.DuplicateGroups.Count > 0)
        {
            sb.AppendLine("## Duplicates");
            sb.AppendLine();
            sb.AppendLine($"Total duplicates found: {report.DuplicateDetection.TotalDuplicatesFound}");
            sb.AppendLine();

            foreach (var group in report.DuplicateDetection.DuplicateGroups)
            {
                sb.AppendLine($"### {group.DuplicateKey}");
                sb.AppendLine();
                foreach (var item in group.Items)
                {
                    sb.AppendLine($"- {item.EntityName} ({item.Source})");
                }
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private string GenerateCsvReport()
    {
        var report = _viewModel.Report;
        if (report == null) return string.Empty;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Severity,Type,EntityType,EntityId,EntityName,FieldName,Message,CurrentValue,ExpectedValue,Timestamp");

        foreach (var entry in report.Entries)
        {
            sb.AppendLine($"\"{entry.Severity}\",\"{entry.EntryType}\",\"{entry.EntityType}\",\"{entry.EntityId}\",\"{entry.EntityName}\",\"{entry.FieldName}\",\"{entry.Message}\",\"{entry.CurrentValue}\",\"{entry.ExpectedValue}\",\"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}\"");
        }

        return sb.ToString();
    }

    private string GenerateHtmlReport()
    {
        var report = _viewModel.Report;
        if (report == null) return string.Empty;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<title>Validation Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
        sb.AppendLine(".container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; }");
        sb.AppendLine("h1 { color: #333; border-bottom: 2px solid #1976D2; padding-bottom: 10px; }");
        sb.AppendLine("h2 { color: #555; margin-top: 30px; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
        sb.AppendLine("th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }");
        sb.AppendLine("th { background: #1976D2; color: white; }");
        sb.AppendLine(".error { color: #DC3545; }");
        sb.AppendLine(".warning { color: #FFC107; }");
        sb.AppendLine(".info { color: #17A2B8; }");
        sb.AppendLine(".success { color: #28A745; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class='container'>");
        sb.AppendLine("<h1>Data Validation Report</h1>");
        sb.AppendLine($"<p><strong>Source:</strong> {report.SourceName}</p>");
        sb.AppendLine($"<p><strong>Generated:</strong> {report.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}</p>");
        sb.AppendLine($"<p><strong>Status:</strong> <span class='{(report.Status == ValidationStatus.Valid ? "success" : "error")}'>{report.Status}</span></p>");
        
        sb.AppendLine("<h2>Summary</h2>");
        sb.AppendLine("<ul>");
        sb.AppendLine($"<li><strong>Total Items:</strong> {report.Summary.TotalItems}</li>");
        sb.AppendLine($"<li><strong>Valid:</strong> <span class='success'>{report.Summary.ValidItems}</span></li>");
        sb.AppendLine($"<li><strong>With Warnings:</strong> <span class='warning'>{report.Summary.ItemsWithWarnings}</span></li>");
        sb.AppendLine($"<li><strong>Invalid:</strong> <span class='error'>{report.Summary.InvalidItems}</span></li>");
        sb.AppendLine($"<li><strong>Success Rate:</strong> {report.Summary.SuccessRate:F1}%</li>");
        sb.AppendLine("</ul>");

        if (report.Entries.Count > 0)
        {
            sb.AppendLine("<h2>Validation Entries</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Severity</th><th>Type</th><th>Entity</th><th>Field</th><th>Message</th></tr>");
            
            foreach (var entry in report.Entries)
            {
                var cssClass = entry.Severity switch
                {
                    ValidationSeverity.Error or ValidationSeverity.Critical => "error",
                    ValidationSeverity.Warning => "warning",
                    _ => "info"
                };
                sb.AppendLine($"<tr class='{cssClass}'>");
                sb.AppendLine($"<td>{entry.Severity}</td>");
                sb.AppendLine($"<td>{entry.EntryType}</td>");
                sb.AppendLine($"<td>{entry.EntityName}</td>");
                sb.AppendLine($"<td>{entry.FieldName}</td>");
                sb.AppendLine($"<td>{entry.Message}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
        }

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private string GenerateTextReport()
    {
        var report = _viewModel.Report;
        if (report == null) return string.Empty;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("DATA VALIDATION REPORT");
        sb.AppendLine("======================");
        sb.AppendLine();
        sb.AppendLine($"Source: {report.SourceName}");
        sb.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"Status: {report.Status}");
        sb.AppendLine();
        sb.AppendLine("SUMMARY");
        sb.AppendLine("-------");
        sb.AppendLine($"Total Items: {report.Summary.TotalItems}");
        sb.AppendLine($"Valid: {report.Summary.ValidItems}");
        sb.AppendLine($"With Warnings: {report.Summary.ItemsWithWarnings}");
        sb.AppendLine($"Invalid: {report.Summary.InvalidItems}");
        sb.AppendLine($"Success Rate: {report.Summary.SuccessRate:F1}%");
        sb.AppendLine();

        if (report.Entries.Count > 0)
        {
            sb.AppendLine("VALIDATION ENTRIES");
            sb.AppendLine("------------------");
            foreach (var entry in report.Entries)
            {
                sb.AppendLine($"[{entry.Severity}] {entry.EntryType} - {entry.EntityName} - {entry.FieldName}: {entry.Message}");
            }
        }

        return sb.ToString();
    }

    private static string GetFileExtension(ValidationReportFormat format)
    {
        return format switch
        {
            ValidationReportFormat.Json => ".json",
            ValidationReportFormat.Markdown => ".md",
            ValidationReportFormat.Csv => ".csv",
            ValidationReportFormat.Html => ".html",
            _ => ".txt"
        };
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.RequestClose -= OnRequestClose;
        base.OnClosed(e);
    }
}
