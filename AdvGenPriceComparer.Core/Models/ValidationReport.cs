using System;
using System.Collections.Generic;

namespace AdvGenPriceComparer.Core.Models;

/// <summary>
/// Represents a comprehensive validation report for import/export operations
/// </summary>
public class ValidationReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public ValidationReportType ReportType { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string? SourcePath { get; set; }
    
    /// <summary>
    /// Overall validation status
    /// </summary>
    public ValidationStatus Status { get; set; }
    
    /// <summary>
    /// Summary statistics for the validation
    /// </summary>
    public ValidationSummary Summary { get; set; } = new();
    
    /// <summary>
    /// Detailed validation entries
    /// </summary>
    public List<ValidationEntry> Entries { get; set; } = new();
    
    /// <summary>
    /// Schema validation results
    /// </summary>
    public SchemaValidationResult SchemaValidation { get; set; } = new();
    
    /// <summary>
    /// Data integrity check results
    /// </summary>
    public DataIntegrityResult DataIntegrity { get; set; } = new();
    
    /// <summary>
    /// Duplicate detection results
    /// </summary>
    public DuplicateDetectionResult DuplicateDetection { get; set; } = new();
}

/// <summary>
/// Type of validation report
/// </summary>
public enum ValidationReportType
{
    ImportValidation,
    ExportValidation,
    SyncValidation
}

/// <summary>
/// Overall validation status
/// </summary>
public enum ValidationStatus
{
    Valid,
    ValidWithWarnings,
    Invalid
}

/// <summary>
/// Summary statistics for validation
/// </summary>
public class ValidationSummary
{
    public int TotalItems { get; set; }
    public int ValidItems { get; set; }
    public int ItemsWithWarnings { get; set; }
    public int InvalidItems { get; set; }
    public int TotalErrors { get; set; }
    public int TotalWarnings { get; set; }
    
    /// <summary>
    /// Success rate as a percentage (0-100)
    /// </summary>
    public double SuccessRate => TotalItems > 0 ? (ValidItems / (double)TotalItems) * 100 : 0;
}

/// <summary>
/// Individual validation entry for a specific item or field
/// </summary>
public class ValidationEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ValidationEntryType EntryType { get; set; }
    public ValidationSeverity Severity { get; set; }
    public string EntityType { get; set; } = string.Empty; // Store, Product, Price
    public string? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? CurrentValue { get; set; }
    public string? ExpectedValue { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Type of entity being validated
/// </summary>
public enum ValidationEntryType
{
    Store,
    Product,
    PriceRecord,
    Manifest,
    Schema,
    Checksum
}

/// <summary>
/// Severity level of a validation issue
/// </summary>
public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Schema validation results
/// </summary>
public class SchemaValidationResult
{
    public bool IsValid { get; set; }
    public string? SchemaVersion { get; set; }
    public List<SchemaValidationError> Errors { get; set; } = new();
}

/// <summary>
/// Schema validation error details
/// </summary>
public class SchemaValidationError
{
    public string FileName { get; set; } = string.Empty;
    public string PropertyPath { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty; // MissingField, InvalidType, InvalidFormat
    public string Message { get; set; } = string.Empty;
    public string? InvalidValue { get; set; }
}

/// <summary>
/// Data integrity check results
/// </summary>
public class DataIntegrityResult
{
    public bool IsValid { get; set; }
    public int MissingRequiredFields { get; set; }
    public int InvalidNumericValues { get; set; }
    public int InvalidDateValues { get; set; }
    public int InvalidReferences { get; set; } // e.g., price referencing non-existent product
    public List<DataIntegrityIssue> Issues { get; set; } = new();
}

/// <summary>
/// Individual data integrity issue
/// </summary>
public class DataIntegrityIssue
{
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string IssueType { get; set; } = string.Empty; // MissingRequired, InvalidRange, InvalidReference
    public string FieldName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CurrentValue { get; set; }
    public string? SuggestedFix { get; set; }
}

/// <summary>
/// Duplicate detection results
/// </summary>
public class DuplicateDetectionResult
{
    public int TotalDuplicatesFound { get; set; }
    public int StoresDuplicated { get; set; }
    public int ProductsDuplicated { get; set; }
    public int PricesDuplicated { get; set; }
    public List<DuplicateGroup> DuplicateGroups { get; set; } = new();
}

/// <summary>
/// Group of duplicate entries
/// </summary>
public class DuplicateGroup
{
    public string GroupId { get; set; } = Guid.NewGuid().ToString();
    public string EntityType { get; set; } = string.Empty;
    public string DuplicateKey { get; set; } = string.Empty; // What field(s) caused the duplicate
    public List<DuplicateItem> Items { get; set; } = new();
    public DuplicateResolution Resolution { get; set; } = DuplicateResolution.Pending;
}

/// <summary>
/// Individual duplicate item
/// </summary>
public class DuplicateItem
{
    public string EntityId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // Import file or Existing database
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// How a duplicate was or will be resolved
/// </summary>
public enum DuplicateResolution
{
    Pending,
    Skipped,
    Updated,
    CreatedNew,
    Merged
}

/// <summary>
/// Filter options for validation reports
/// </summary>
public class ValidationReportFilter
{
    public ValidationSeverity? MinSeverity { get; set; }
    public ValidationEntryType? EntryType { get; set; }
    public string? SearchText { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool ShowOnlyDuplicates { get; set; }
}

/// <summary>
/// Export format for validation reports
/// </summary>
public enum ValidationReportFormat
{
    Json,
    Markdown,
    Csv,
    Html
}
