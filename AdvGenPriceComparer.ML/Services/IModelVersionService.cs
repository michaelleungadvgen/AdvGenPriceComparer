using AdvGenPriceComparer.ML.Models;

namespace AdvGenPriceComparer.ML.Services;

/// <summary>
/// Service for managing ML model versions with versioning, rollback, and retention policies
/// </summary>
public interface IModelVersionService
{
    /// <summary>
    /// Gets the retention settings
    /// </summary>
    ModelVersionRetentionSettings RetentionSettings { get; }

    /// <summary>
    /// Gets all model versions sorted by creation date (newest first)
    /// </summary>
    Task<IReadOnlyList<ModelVersionInfo>> GetAllVersionsAsync();

    /// <summary>
    /// Gets the currently active model version
    /// </summary>
    Task<ModelVersionInfo?> GetCurrentVersionAsync();

    /// <summary>
    /// Gets a specific version by its ID
    /// </summary>
    Task<ModelVersionInfo?> GetVersionByIdAsync(string versionId);

    /// <summary>
    /// Gets the latest version number
    /// </summary>
    Task<int> GetLatestVersionNumberAsync();

    /// <summary>
    /// Registers a new model version after training
    /// </summary>
    Task<ModelVersionInfo> RegisterNewVersionAsync(
        string modelPath,
        TrainingResult trainingResult,
        string description = "");

    /// <summary>
    /// Rolls back to a specific version
    /// </summary>
    Task<RollbackResult> RollbackToVersionAsync(string versionId);

    /// <summary>
    /// Rolls back to the previous version
    /// </summary>
    Task<RollbackResult> RollbackToPreviousVersionAsync();

    /// <summary>
    /// Gets the best performing version by accuracy
    /// </summary>
    Task<ModelVersionInfo?> GetBestVersionAsync();

    /// <summary>
    /// Deletes a specific version
    /// </summary>
    Task<bool> DeleteVersionAsync(string versionId);

    /// <summary>
    /// Cleans up old versions according to retention policy
    /// </summary>
    Task<CleanupResult> CleanupOldVersionsAsync();

    /// <summary>
    /// Gets a summary of model version history
    /// </summary>
    Task<ModelVersionSummary> GetVersionSummaryAsync();

    /// <summary>
    /// Sets a version as active (used internally by rollback)
    /// </summary>
    Task<bool> SetActiveVersionAsync(string versionId);

    /// <summary>
    /// Gets available rollback candidates (excluding current)
    /// </summary>
    Task<IReadOnlyList<ModelVersionInfo>> GetRollbackCandidatesAsync();

    /// <summary>
    /// Verifies integrity of all stored model files
    /// </summary>
    Task<IntegrityCheckResult> VerifyModelIntegrityAsync();

    /// <summary>
    /// Archives a version (keeps metadata but removes file)
    /// </summary>
    Task<bool> ArchiveVersionAsync(string versionId);

    /// <summary>
    /// Exports a version to a different location
    /// </summary>
    Task<bool> ExportVersionAsync(string versionId, string destinationPath);

    /// <summary>
    /// Imports a version from an external file
    /// </summary>
    Task<ModelVersionInfo?> ImportVersionAsync(string sourcePath, string description = "");

    /// <summary>
    /// Updates retention settings
    /// </summary>
    void UpdateRetentionSettings(ModelVersionRetentionSettings settings);

    /// <summary>
    /// Event fired when a new version is registered
    /// </summary>
    event EventHandler<ModelVersionInfo>? VersionRegistered;

    /// <summary>
    /// Event fired when rollback occurs
    /// </summary>
    event EventHandler<RollbackResult>? VersionRolledBack;

    /// <summary>
    /// Event fired when versions are cleaned up
    /// </summary>
    event EventHandler<CleanupResult>? VersionsCleanedUp;
}

/// <summary>
/// Result of a cleanup operation
/// </summary>
public class CleanupResult
{
    /// <summary>
    /// Number of versions deleted
    /// </summary>
    public int DeletedCount { get; set; }

    /// <summary>
    /// Number of versions archived
    /// </summary>
    public int ArchivedCount { get; set; }

    /// <summary>
    /// Storage space freed in bytes
    /// </summary>
    public long StorageFreedBytes { get; set; }

    /// <summary>
    /// IDs of deleted versions
    /// </summary>
    public List<string> DeletedVersionIds { get; set; } = new();

    /// <summary>
    /// Whether cleanup was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Result of an integrity check
/// </summary>
public class IntegrityCheckResult
{
    /// <summary>
    /// Number of models that passed integrity check
    /// </summary>
    public int PassedCount { get; set; }

    /// <summary>
    /// Number of models that failed integrity check
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Details of failed checks
    /// </summary>
    public List<IntegrityFailure> Failures { get; set; } = new();

    /// <summary>
    /// Whether all models passed
    /// </summary>
    public bool AllPassed => FailedCount == 0;

    /// <summary>
    /// Total number of models checked
    /// </summary>
    public int TotalChecked => PassedCount + FailedCount;
}

/// <summary>
/// Details of a single integrity failure
/// </summary>
public class IntegrityFailure
{
    /// <summary>
    /// Version ID that failed
    /// </summary>
    public string VersionId { get; set; } = string.Empty;

    /// <summary>
    /// Path to the model file
    /// </summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>
    /// Reason for failure
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
