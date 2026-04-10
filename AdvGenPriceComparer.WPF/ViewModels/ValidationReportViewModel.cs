using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for displaying and interacting with validation reports
/// </summary>
public class ValidationReportViewModel : ViewModelBase
{
    private ValidationReport? _report;
    private ObservableCollection<ValidationEntry> _filteredEntries = new();
    private ObservableCollection<DuplicateGroup> _duplicateGroups = new();
    private ValidationSeverity? _selectedSeverity;
    private ValidationEntryType? _selectedEntryType;
    private string _searchText = string.Empty;
    private bool _showOnlyDuplicates;
    private int _selectedTabIndex;

    public ValidationReportViewModel()
    {
        CloseCommand = new RelayCommand(Close);
        ExportReportCommand = new RelayCommand<ValidationReportFormat>(ExportReport);
        FilterCommand = new RelayCommand(ApplyFilters);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ViewEntryDetailsCommand = new RelayCommand<ValidationEntry>(ViewEntryDetails);
    }

    #region Properties

    public ValidationReport? Report
    {
        get => _report;
        set
        {
            _report = value;
            OnPropertyChanged();
            RefreshDisplay();
        }
    }

    public ObservableCollection<ValidationEntry> FilteredEntries
    {
        get => _filteredEntries;
        set
        {
            _filteredEntries = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DuplicateGroup> DuplicateGroups
    {
        get => _duplicateGroups;
        set
        {
            _duplicateGroups = value;
            OnPropertyChanged();
        }
    }

    public ValidationSeverity? SelectedSeverity
    {
        get => _selectedSeverity;
        set
        {
            _selectedSeverity = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public ValidationEntryType? SelectedEntryType
    {
        get => _selectedEntryType;
        set
        {
            _selectedEntryType = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public bool ShowOnlyDuplicates
    {
        get => _showOnlyDuplicates;
        set
        {
            _showOnlyDuplicates = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            _selectedTabIndex = value;
            OnPropertyChanged();
        }
    }

    // Summary Properties
    public int TotalItems => Report?.Summary.TotalItems ?? 0;
    public int ValidItems => Report?.Summary.ValidItems ?? 0;
    public int ItemsWithWarnings => Report?.Summary.ItemsWithWarnings ?? 0;
    public int InvalidItems => Report?.Summary.InvalidItems ?? 0;
    public int TotalErrors => Report?.Summary.TotalErrors ?? 0;
    public int TotalWarnings => Report?.Summary.TotalWarnings ?? 0;
    public double SuccessRate => Report?.Summary.SuccessRate ?? 0;

    public bool HasErrors => TotalErrors > 0;
    public bool HasWarnings => TotalWarnings > 0;
    public bool HasDuplicates => DuplicateGroups.Count > 0;

    public string StatusText => Report?.Status switch
    {
        ValidationStatus.Valid => "✅ Valid",
        ValidationStatus.ValidWithWarnings => "⚠️ Valid with Warnings",
        ValidationStatus.Invalid => "❌ Invalid",
        _ => "Unknown"
    };

    public string SummaryText
    {
        get
        {
            if (Report == null) return string.Empty;
            
            var parts = new List<string>();
            if (ValidItems > 0) parts.Add($"{ValidItems} valid");
            if (ItemsWithWarnings > 0) parts.Add($"{ItemsWithWarnings} with warnings");
            if (InvalidItems > 0) parts.Add($"{InvalidItems} invalid");
            
            return string.Join(", ", parts);
        }
    }

    #endregion

    #region Commands

    public ICommand CloseCommand { get; }
    public ICommand ExportReportCommand { get; }
    public ICommand FilterCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ViewEntryDetailsCommand { get; }

    #endregion

    #region Methods

    private void RefreshDisplay()
    {
        if (Report == null) return;

        // Update filtered entries
        ApplyFilters();

        // Update duplicate groups
        DuplicateGroups.Clear();
        foreach (var group in Report.DuplicateDetection.DuplicateGroups)
        {
            DuplicateGroups.Add(group);
        }

        // Notify summary property changes
        OnPropertyChanged(nameof(TotalItems));
        OnPropertyChanged(nameof(ValidItems));
        OnPropertyChanged(nameof(ItemsWithWarnings));
        OnPropertyChanged(nameof(InvalidItems));
        OnPropertyChanged(nameof(TotalErrors));
        OnPropertyChanged(nameof(TotalWarnings));
        OnPropertyChanged(nameof(SuccessRate));
        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(HasWarnings));
        OnPropertyChanged(nameof(HasDuplicates));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(SummaryText));
    }

    private void ApplyFilters()
    {
        if (Report?.Entries == null)
        {
            FilteredEntries.Clear();
            return;
        }

        var query = Report.Entries.AsEnumerable();

        // Filter by severity
        if (SelectedSeverity.HasValue)
        {
            query = query.Where(e => e.Severity >= SelectedSeverity.Value);
        }

        // Filter by entry type
        if (SelectedEntryType.HasValue)
        {
            query = query.Where(e => e.EntryType == SelectedEntryType.Value);
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            query = query.Where(e => 
                (e.Message?.ToLowerInvariant().Contains(searchLower) == true) ||
                (e.EntityName?.ToLowerInvariant().Contains(searchLower) == true) ||
                (e.FieldName?.ToLowerInvariant().Contains(searchLower) == true) ||
                (e.EntityId?.ToLowerInvariant().Contains(searchLower) == true));
        }

        // Filter to show only duplicates
        if (ShowOnlyDuplicates)
        {
            var duplicateIds = Report.DuplicateDetection.DuplicateGroups
                .SelectMany(g => g.Items)
                .Select(i => i.EntityId)
                .ToHashSet();
            
            query = query.Where(e => duplicateIds.Contains(e.EntityId ?? string.Empty));
        }

        FilteredEntries.Clear();
        foreach (var entry in query.OrderByDescending(e => e.Severity).ThenBy(e => e.Timestamp))
        {
            FilteredEntries.Add(entry);
        }
    }

    private void ClearFilters()
    {
        SelectedSeverity = null;
        SelectedEntryType = null;
        SearchText = string.Empty;
        ShowOnlyDuplicates = false;
        ApplyFilters();
    }

    private void ExportReport(ValidationReportFormat format)
    {
        // This will be handled by the dialog service or code-behind
        RequestClose?.Invoke(this, new ValidationReportExportEventArgs(format));
    }

    private void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void ViewEntryDetails(ValidationEntry? entry)
    {
        if (entry != null)
        {
            SelectedEntry = entry;
        }
    }

    private ValidationEntry? _selectedEntry;
    public ValidationEntry? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            _selectedEntry = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Event raised when the dialog should close
    /// </summary>
    public event EventHandler? RequestClose;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a validation report from a StaticImportResult
    /// </summary>
    public static ValidationReport CreateFromImportResult(Services.StaticImportResult result, string sourceName)
    {
        var report = new ValidationReport
        {
            ReportType = ValidationReportType.ImportValidation,
            SourceName = sourceName,
            SourcePath = result.SourceUrl ?? result.ArchivePath,
            Status = result.Success 
                ? (result.Errors.Count > 0 ? ValidationStatus.ValidWithWarnings : ValidationStatus.Valid)
                : ValidationStatus.Invalid,
            Summary = new ValidationSummary
            {
                TotalItems = result.StoresImported + result.StoresSkipped + result.ProductsImported + 
                            result.ProductsSkipped + result.PricesImported + result.PricesSkipped,
                ValidItems = result.StoresImported + result.ProductsImported + result.PricesImported,
                ItemsWithWarnings = result.Errors.Count > 0 ? result.Errors.Count : 0,
                InvalidItems = result.Success ? 0 : 1,
                TotalErrors = result.Errors.Count + result.ChecksumErrors.Count,
                TotalWarnings = 0
            }
        };

        // Add entries for errors
        foreach (var error in result.Errors)
        {
            report.Entries.Add(new ValidationEntry
            {
                EntryType = ValidationEntryType.Schema,
                Severity = ValidationSeverity.Error,
                FieldName = "Import",
                Message = error,
                Timestamp = result.ImportedAt
            });
        }

        foreach (var error in result.ChecksumErrors)
        {
            report.Entries.Add(new ValidationEntry
            {
                EntryType = ValidationEntryType.Checksum,
                Severity = ValidationSeverity.Critical,
                FieldName = "Checksum",
                Message = error,
                Timestamp = result.ImportedAt
            });
        }

        // If there was an error message, add it
        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            report.Entries.Add(new ValidationEntry
            {
                EntryType = ValidationEntryType.Schema,
                Severity = ValidationSeverity.Critical,
                FieldName = "Import",
                Message = result.ErrorMessage,
                Timestamp = result.ImportedAt
            });
        }

        return report;
    }

    #endregion
}

/// <summary>
/// Event args for validation report export
/// </summary>
public class ValidationReportExportEventArgs : EventArgs
{
    public ValidationReportFormat Format { get; }

    public ValidationReportExportEventArgs(ValidationReportFormat format)
    {
        Format = format;
    }
}
