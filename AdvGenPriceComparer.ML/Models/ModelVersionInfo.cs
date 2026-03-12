namespace AdvGenPriceComparer.ML.Models;

/// <summary>
/// Represents a versioned ML model with metadata for tracking and rollback
/// </summary>
public class ModelVersionInfo
{
    /// <summary>
    /// Unique identifier for this model version
    /// </summary>
    public string VersionId { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Version number (e.g., 1, 2, 3)
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// When this model version was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Path to the model file
    /// </summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>
    /// Model accuracy (MacroAccuracy) if available
    /// </summary>
    public double? Accuracy { get; set; }

    /// <summary>
    /// Model micro accuracy if available
    /// </summary>
    public double? MicroAccuracy { get; set; }

    /// <summary>
    /// Number of items used for training
    /// </summary>
    public int TrainingItemCount { get; set; }

    /// <summary>
    /// Training duration
    /// </summary>
    public TimeSpan? TrainingDuration { get; set; }

    /// <summary>
    /// Description of what changed in this version
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the currently active model
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether this version can be rolled back to
    /// </summary>
    public bool IsRollbackCandidate { get; set; } = true;

    /// <summary>
    /// Source of training data (Database, CSV, etc.)
    /// </summary>
    public string TrainingSource { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Model file hash for integrity verification
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets a formatted display name for this version
    /// </summary>
    public string DisplayName => $"v{VersionNumber} ({CreatedAt:yyyy-MM-dd HH:mm})";

    /// <summary>
    /// Gets a detailed description of this version
    /// </summary>
    public string DetailedDescription =>
        $"Version {VersionNumber} - Created {CreatedAt:yyyy-MM-dd HH:mm}" +
        (Accuracy.HasValue ? $" - Accuracy: {Accuracy.Value:P1}" : "") +
        (TrainingItemCount > 0 ? $" - {TrainingItemCount} items" : "") +
        (IsActive ? " [ACTIVE]" : "");
}

/// <summary>
/// Settings for model version retention policy
/// </summary>
public class ModelVersionRetentionSettings
{
    /// <summary>
    /// Maximum number of model versions to keep (default: 10)
    /// </summary>
    public int MaxVersions { get; set; } = 10;

    /// <summary>
    /// Minimum number of versions to always keep (default: 3)
    /// </summary>
    public int MinVersions { get; set; } = 3;

    /// <summary>
    /// Days to keep old versions before cleanup (default: 30)
    /// </summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// Whether to automatically clean up old versions after training
    /// </summary>
    public bool AutoCleanup { get; set; } = true;

    /// <summary>
    /// Whether to keep versions with the best accuracy
    /// </summary>
    public bool KeepBestPerforming { get; set; } = true;

    /// <summary>
    /// Number of best performing versions to keep
    /// </summary>
    public int KeepBestCount { get; set; } = 3;
}

/// <summary>
/// Result of a rollback operation
/// </summary>
public class RollbackResult
{
    /// <summary>
    /// Whether rollback was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The version that was rolled back to
    /// </summary>
    public ModelVersionInfo? RolledBackTo { get; set; }

    /// <summary>
    /// The previous active version before rollback
    /// </summary>
    public ModelVersionInfo? PreviousVersion { get; set; }
}

/// <summary>
/// Summary of model version history
/// </summary>
public class ModelVersionSummary
{
    /// <summary>
    /// Total number of versions
    /// </summary>
    public int TotalVersions { get; set; }

    /// <summary>
    /// Number of active versions (not cleaned up)
    /// </summary>
    public int ActiveVersions { get; set; }

    /// <summary>
    /// Currently active version
    /// </summary>
    public ModelVersionInfo? CurrentVersion { get; set; }

    /// <summary>
    /// Best performing version by accuracy
    /// </summary>
    public ModelVersionInfo? BestVersion { get; set; }

    /// <summary>
    /// First version created
    /// </summary>
    public DateTime? FirstVersionDate { get; set; }

    /// <summary>
    /// Most recent version created
    /// </summary>
    public DateTime? LatestVersionDate { get; set; }

    /// <summary>
    /// Total storage used by all versions
    /// </summary>
    public long TotalStorageBytes { get; set; }

    /// <summary>
    /// Average accuracy across all versions with accuracy data
    /// </summary>
    public double? AverageAccuracy { get; set; }
}
