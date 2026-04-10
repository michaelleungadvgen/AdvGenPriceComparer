using System;
using System.Collections.Generic;

namespace AdvGenPriceComparer.Core.Models;

/// <summary>
/// Information about directory size and contents
/// </summary>
public class DirectorySizeInfo
{
    /// <summary>
    /// Directory path
    /// </summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Total size of all files in bytes
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Total size formatted as human-readable string
    /// </summary>
    public string TotalSizeFormatted => FormatBytes(TotalSizeBytes);

    /// <summary>
    /// Number of files in directory
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Number of subdirectories
    /// </summary>
    public int SubdirectoryCount { get; set; }

    /// <summary>
    /// Oldest file timestamp
    /// </summary>
    public DateTime? OldestFileDate { get; set; }

    /// <summary>
    /// Newest file timestamp
    /// </summary>
    public DateTime? NewestFileDate { get; set; }

    /// <summary>
    /// Average file size in bytes
    /// </summary>
    public long AverageFileSizeBytes => FileCount > 0 ? TotalSizeBytes / FileCount : 0;

    /// <summary>
    /// Largest file size in bytes
    /// </summary>
    public long LargestFileSizeBytes { get; set; }

    /// <summary>
    /// Free disk space on the drive in bytes
    /// </summary>
    public long AvailableFreeSpaceBytes { get; set; }

    /// <summary>
    /// Free disk space formatted as human-readable string
    /// </summary>
    public string AvailableFreeSpaceFormatted => FormatBytes(AvailableFreeSpaceBytes);

    /// <summary>
    /// List of largest files
    /// </summary>
    public List<ExportFileInfo> LargestFiles { get; set; } = new();

    /// <summary>
    /// Format bytes to human-readable string
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;
        const long TB = GB * 1024;

        return bytes switch
        {
            >= TB => $"{bytes / (double)TB:F2} TB",
            >= GB => $"{bytes / (double)GB:F2} GB",
            >= MB => $"{bytes / (double)MB:F2} MB",
            >= KB => $"{bytes / (double)KB:F2} KB",
            _ => $"{bytes} B"
        };
    }
}

/// <summary>
/// Information about a file for size monitoring
/// </summary>
public class ExportFileInfo
{
    /// <summary>
    /// Full file path
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// File size formatted as human-readable string
    /// </summary>
    public string SizeFormatted => FormatBytes(SizeBytes);

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// File extension
    /// </summary>
    public string Extension => System.IO.Path.GetExtension(FullPath);

    /// <summary>
    /// Format bytes to human-readable string
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

        return bytes switch
        {
            >= GB => $"{bytes / (double)GB:F2} GB",
            >= MB => $"{bytes / (double)MB:F2} MB",
            >= KB => $"{bytes / (double)KB:F2} KB",
            _ => $"{bytes} B"
        };
    }
}

/// <summary>
/// Result of checking size before export
/// </summary>
public class SizeCheckResult
{
    /// <summary>
    /// Whether the export can proceed
    /// </summary>
    public bool CanExport { get; set; }

    /// <summary>
    /// Current directory size in bytes
    /// </summary>
    public long CurrentSizeBytes { get; set; }

    /// <summary>
    /// Expected size after export in bytes
    /// </summary>
    public long ExpectedSizeAfterExportBytes { get; set; }

    /// <summary>
    /// Size limit in bytes
    /// </summary>
    public long SizeLimitBytes { get; set; }

    /// <summary>
    /// Space that needs to be freed in bytes
    /// </summary>
    public long SpaceNeededBytes { get; set; }

    /// <summary>
    /// Recommendation message for user
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Whether cleanup is recommended
    /// </summary>
    public bool CleanupRecommended { get; set; }

    /// <summary>
    /// Whether limits will be exceeded
    /// </summary>
    public bool WillExceedLimits => ExpectedSizeAfterExportBytes > SizeLimitBytes;

    /// <summary>
    /// Percentage of limit that will be used after export
    /// </summary>
    public double PercentageOfLimit => SizeLimitBytes > 0 
        ? (ExpectedSizeAfterExportBytes / (double)SizeLimitBytes) * 100 
        : 0;
}

/// <summary>
/// Result of cleanup operation
/// </summary>
public class CleanupResult
{
    /// <summary>
    /// Whether cleanup was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of files deleted
    /// </summary>
    public int FilesDeleted { get; set; }

    /// <summary>
    /// Total space freed in bytes
    /// </summary>
    public long SpaceFreedBytes { get; set; }

    /// <summary>
    /// Space freed formatted as human-readable string
    /// </summary>
    public string SpaceFreedFormatted => FormatBytes(SpaceFreedBytes);

    /// <summary>
    /// List of deleted files
    /// </summary>
    public List<string> DeletedFiles { get; set; } = new();

    /// <summary>
    /// List of files that could not be deleted
    /// </summary>
    public List<DeleteFailure> FailedDeletions { get; set; } = new();

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when cleanup was performed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Format bytes to human-readable string
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

        return bytes switch
        {
            >= GB => $"{bytes / (double)GB:F2} GB",
            >= MB => $"{bytes / (double)MB:F2} MB",
            >= KB => $"{bytes / (double)KB:F2} KB",
            _ => $"{bytes} B"
        };
    }
}

/// <summary>
/// Failed file deletion information
/// </summary>
public class DeleteFailure
{
    /// <summary>
    /// File path
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Options for file size monitoring
/// </summary>
public class FileSizeMonitorOptions
{
    /// <summary>
    /// Maximum directory size in bytes (default: 1 GB)
    /// </summary>
    public long MaxDirectorySizeBytes { get; set; } = 1024L * 1024L * 1024L; // 1 GB

    /// <summary>
    /// Maximum number of files allowed (default: 1000)
    /// </summary>
    public int MaxFileCount { get; set; } = 1000;

    /// <summary>
    /// Whether to automatically clean up when limits are exceeded
    /// </summary>
    public bool AutoCleanupEnabled { get; set; } = true;

    /// <summary>
    /// Percentage threshold to trigger cleanup (default: 90%)
    /// </summary>
    public double CleanupThresholdPercentage { get; set; } = 90.0;

    /// <summary>
    /// Minimum age of files before they can be deleted (default: 7 days)
    /// </summary>
    public TimeSpan MinFileAgeForDeletion { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Number of files to keep at minimum during cleanup
    /// </summary>
    public int MinFilesToKeep { get; set; } = 10;

    /// <summary>
    /// File extensions to include in monitoring (empty = all)
    /// </summary>
    public List<string> FileExtensions { get; set; } = new() { ".json", ".zip", ".gz", ".csv", ".md" };

    /// <summary>
    /// Whether to include subdirectories in size calculation
    /// </summary>
    public bool IncludeSubdirectories { get; set; } = true;
}

/// <summary>
/// Options for cleanup operations
/// </summary>
public class CleanupOptions
{
    /// <summary>
    /// Strategy for selecting files to delete
    /// </summary>
    public CleanupStrategy Strategy { get; set; } = CleanupStrategy.OldestFirst;

    /// <summary>
    /// Minimum age of files before they can be deleted
    /// </summary>
    public TimeSpan MinFileAge { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Minimum number of files to keep
    /// </summary>
    public int MinFilesToKeep { get; set; } = 10;

    /// <summary>
    /// File extensions to consider for cleanup (empty = all)
    /// </summary>
    public List<string>? FileExtensions { get; set; }

    /// <summary>
    /// Whether to simulate cleanup without actually deleting files
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Specific files to exclude from cleanup
    /// </summary>
    public List<string> ExcludedFiles { get; set; } = new();
}

/// <summary>
/// Strategy for selecting files to clean up
/// </summary>
public enum CleanupStrategy
{
    /// <summary>
    /// Delete oldest files first
    /// </summary>
    OldestFirst,

    /// <summary>
    /// Delete largest files first
    /// </summary>
    LargestFirst,

    /// <summary>
    /// Delete files that haven't been accessed recently
    /// </summary>
    LeastRecentlyAccessed,

    /// <summary>
    /// Delete files matching specific patterns first
    /// </summary>
    ByPattern
}
