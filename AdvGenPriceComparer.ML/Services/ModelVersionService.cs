using System.Security.Cryptography;
using System.Text.Json;
using AdvGenPriceComparer.ML.Models;
using Microsoft.ML;

namespace AdvGenPriceComparer.ML.Services;

/// <summary>
/// Service for managing ML model versions with versioning, rollback, and retention policies
/// </summary>
public class ModelVersionService : IModelVersionService
{
    private readonly string _versionsDirectory;
    private readonly string _metadataFilePath;
    private readonly string _activeVersionFile;
    private readonly MLContext _mlContext;
    private readonly Action<string>? _logInfo;
    private readonly Action<string, Exception>? _logError;
    private readonly Action<string>? _logWarning;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<ModelVersionInfo> _versionsCache = new();
    private DateTime _cacheLastUpdated = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Retention settings for model versions
    /// </summary>
    public ModelVersionRetentionSettings RetentionSettings { get; private set; } = new();

    /// <summary>
    /// Event fired when a new version is registered
    /// </summary>
    public event EventHandler<ModelVersionInfo>? VersionRegistered;

    /// <summary>
    /// Event fired when rollback occurs
    /// </summary>
    public event EventHandler<RollbackResult>? VersionRolledBack;

    /// <summary>
    /// Event fired when versions are cleaned up
    /// </summary>
    public event EventHandler<CleanupResult>? VersionsCleanedUp;

    /// <summary>
    /// Creates a new instance of ModelVersionService
    /// </summary>
    public ModelVersionService(
        string baseModelPath,
        ModelVersionRetentionSettings? retentionSettings = null,
        Action<string>? logInfo = null,
        Action<string, Exception>? logError = null,
        Action<string>? logWarning = null)
    {
        _mlContext = new MLContext();
        _logInfo = logInfo;
        _logError = logError;
        _logWarning = logWarning;
        RetentionSettings = retentionSettings ?? new ModelVersionRetentionSettings();

        // Setup directory structure
        _versionsDirectory = Path.Combine(Path.GetDirectoryName(baseModelPath)!, "versions");
        _metadataFilePath = Path.Combine(_versionsDirectory, "versions_metadata.json");
        _activeVersionFile = Path.Combine(_versionsDirectory, "active_version.txt");

        Directory.CreateDirectory(_versionsDirectory);

        // Ensure default model exists in versions if not already tracked
        _ = EnsureDefaultModelTrackedAsync(baseModelPath);
    }

    /// <summary>
    /// Gets all model versions sorted by creation date (newest first)
    /// </summary>
    public async Task<IReadOnlyList<ModelVersionInfo>> GetAllVersionsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await EnsureCacheValidAsync();
            return _versionsCache.OrderByDescending(v => v.CreatedAt).ToList().AsReadOnly();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets the currently active model version
    /// </summary>
    public async Task<ModelVersionInfo?> GetCurrentVersionAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await EnsureCacheValidAsync();
            return _versionsCache.FirstOrDefault(v => v.IsActive);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets a specific version by its ID
    /// </summary>
    public async Task<ModelVersionInfo?> GetVersionByIdAsync(string versionId)
    {
        await _lock.WaitAsync();
        try
        {
            await EnsureCacheValidAsync();
            return _versionsCache.FirstOrDefault(v => v.VersionId == versionId);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets the latest version number
    /// </summary>
    public async Task<int> GetLatestVersionNumberAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await EnsureCacheValidAsync();
            return _versionsCache.Count > 0 ? _versionsCache.Max(v => v.VersionNumber) : 0;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Registers a new model version after training
    /// </summary>
    public async Task<ModelVersionInfo> RegisterNewVersionAsync(
        string modelPath,
        TrainingResult trainingResult,
        string description = "")
    {
        await _lock.WaitAsync();
        try
        {
            await EnsureCacheValidAsync();

            var nextVersionNumber = _versionsCache.Count > 0 ? _versionsCache.Max(v => v.VersionNumber) + 1 : 1;
            var versionId = Guid.NewGuid().ToString("N")[..8];

            // Copy model to versions directory
            var versionedModelPath = Path.Combine(_versionsDirectory, $"model_v{nextVersionNumber}_{versionId}.zip");
            File.Copy(modelPath, versionedModelPath, overwrite: true);

            // Calculate file hash
            var fileHash = await CalculateFileHashAsync(versionedModelPath);
            var fileInfo = new FileInfo(versionedModelPath);

            var versionInfo = new ModelVersionInfo
            {
                VersionId = versionId,
                VersionNumber = nextVersionNumber,
                CreatedAt = DateTime.Now,
                ModelPath = versionedModelPath,
                Accuracy = trainingResult.Accuracy,
                MicroAccuracy = trainingResult.MicroAccuracy,
                TrainingItemCount = trainingResult.TrainingItemCount,
                TrainingDuration = trainingResult.Duration,
                Description = description,
                IsActive = true,
                TrainingSource = trainingResult.Message.Contains("CSV") ? "CSV Import" : "Database",
                FileSizeBytes = fileInfo.Length,
                FileHash = fileHash
            };

            // Deactivate previous version
            foreach (var v in _versionsCache)
            {
                v.IsActive = false;
            }

            _versionsCache.Add(versionInfo);
            await SaveMetadataAsync();
            await SaveActiveVersionAsync(versionId);

            _logInfo?.Invoke($"Registered new model version {versionInfo.DisplayName} with accuracy {versionInfo.Accuracy:P2}");

            // Trigger auto-cleanup if enabled
            if (RetentionSettings.AutoCleanup)
            {
                await CleanupOldVersionsInternalAsync();
            }

            VersionRegistered?.Invoke(this, versionInfo);
            return versionInfo;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Rolls back to a specific version
    /// </summary>
    public async Task<RollbackResult> RollbackToVersionAsync(string versionId)
    {
        await _lock.WaitAsync();
        try
        {
            var targetVersion = _versionsCache.FirstOrDefault(v => v.VersionId == versionId);
            if (targetVersion == null)
            {
                return new RollbackResult
                {
                    Success = false,
                    Message = $"Version {versionId} not found"
                };
            }

            if (!File.Exists(targetVersion.ModelPath))
            {
                return new RollbackResult
                {
                    Success = false,
                    Message = $"Model file for version {versionId} not found at {targetVersion.ModelPath}"
                };
            }

            var previousVersion = _versionsCache.FirstOrDefault(v => v.IsActive);

            // Deactivate all versions
            foreach (var v in _versionsCache)
            {
                v.IsActive = false;
            }

            // Activate target version
            targetVersion.IsActive = true;
            await SaveMetadataAsync();
            await SaveActiveVersionAsync(versionId);

            _logInfo?.Invoke($"Rolled back to model version {targetVersion.DisplayName}");

            var result = new RollbackResult
            {
                Success = true,
                Message = $"Successfully rolled back to version {targetVersion.VersionNumber}",
                RolledBackTo = targetVersion,
                PreviousVersion = previousVersion
            };

            VersionRolledBack?.Invoke(this, result);
            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Rolls back to the previous version
    /// </summary>
    public async Task<RollbackResult> RollbackToPreviousVersionAsync()
    {
        var versions = await GetAllVersionsAsync();
        var current = versions.FirstOrDefault(v => v.IsActive);
        if (current == null)
        {
            return new RollbackResult
            {
                Success = false,
                Message = "No active version found"
            };
        }

        var previous = versions
            .Where(v => v.VersionNumber < current.VersionNumber)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefault();

        if (previous == null)
        {
            return new RollbackResult
            {
                Success = false,
                Message = "No previous version available for rollback"
            };
        }

        return await RollbackToVersionAsync(previous.VersionId);
    }

    /// <summary>
    /// Gets the best performing version by accuracy
    /// </summary>
    public async Task<ModelVersionInfo?> GetBestVersionAsync()
    {
        var versions = await GetAllVersionsAsync();
        return versions
            .Where(v => v.Accuracy.HasValue && File.Exists(v.ModelPath))
            .OrderByDescending(v => v.Accuracy)
            .FirstOrDefault();
    }

    /// <summary>
    /// Deletes a specific version
    /// </summary>
    public async Task<bool> DeleteVersionAsync(string versionId)
    {
        await _lock.WaitAsync();
        try
        {
            var version = _versionsCache.FirstOrDefault(v => v.VersionId == versionId);
            if (version == null)
                return false;

            if (version.IsActive)
            {
                _logWarning?.Invoke($"Cannot delete active version {versionId}");
                return false;
            }

            if (File.Exists(version.ModelPath))
            {
                File.Delete(version.ModelPath);
            }

            _versionsCache.Remove(version);
            await SaveMetadataAsync();

            _logInfo?.Invoke($"Deleted model version {version.DisplayName}");
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Cleans up old versions according to retention policy
    /// </summary>
    public async Task<CleanupResult> CleanupOldVersionsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return await CleanupOldVersionsInternalAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets a summary of model version history
    /// </summary>
    public async Task<ModelVersionSummary> GetVersionSummaryAsync()
    {
        var versions = await GetAllVersionsAsync();
        var activeVersions = versions.Where(v => File.Exists(v.ModelPath)).ToList();
        var versionsWithAccuracy = activeVersions.Where(v => v.Accuracy.HasValue).ToList();

        var totalStorage = activeVersions.Sum(v => v.FileSizeBytes);
        var bestVersion = versionsWithAccuracy.OrderByDescending(v => v.Accuracy).FirstOrDefault();

        return new ModelVersionSummary
        {
            TotalVersions = versions.Count,
            ActiveVersions = activeVersions.Count,
            CurrentVersion = versions.FirstOrDefault(v => v.IsActive),
            BestVersion = bestVersion,
            FirstVersionDate = versions.Count > 0 ? versions.Min(v => v.CreatedAt) : null,
            LatestVersionDate = versions.Count > 0 ? versions.Max(v => v.CreatedAt) : null,
            TotalStorageBytes = totalStorage,
            AverageAccuracy = versionsWithAccuracy.Count > 0
                ? versionsWithAccuracy.Average(v => v.Accuracy!.Value)
                : null
        };
    }

    /// <summary>
    /// Sets a version as active
    /// </summary>
    public async Task<bool> SetActiveVersionAsync(string versionId)
    {
        return (await RollbackToVersionAsync(versionId)).Success;
    }

    /// <summary>
    /// Gets available rollback candidates (excluding current)
    /// </summary>
    public async Task<IReadOnlyList<ModelVersionInfo>> GetRollbackCandidatesAsync()
    {
        var versions = await GetAllVersionsAsync();
        var current = versions.FirstOrDefault(v => v.IsActive);
        return versions
            .Where(v => v.VersionId != current?.VersionId && File.Exists(v.ModelPath))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Verifies integrity of all stored model files
    /// </summary>
    public async Task<IntegrityCheckResult> VerifyModelIntegrityAsync()
    {
        var versions = await GetAllVersionsAsync();
        var result = new IntegrityCheckResult();

        foreach (var version in versions)
        {
            if (!File.Exists(version.ModelPath))
            {
                result.FailedCount++;
                result.Failures.Add(new IntegrityFailure
                {
                    VersionId = version.VersionId,
                    ModelPath = version.ModelPath,
                    Reason = "File not found"
                });
                continue;
            }

            var currentHash = await CalculateFileHashAsync(version.ModelPath);
            if (currentHash != version.FileHash)
            {
                result.FailedCount++;
                result.Failures.Add(new IntegrityFailure
                {
                    VersionId = version.VersionId,
                    ModelPath = version.ModelPath,
                    Reason = "Hash mismatch - file may be corrupted"
                });
            }
            else
            {
                // Try to load the model
                try
                {
                    using var stream = File.OpenRead(version.ModelPath);
                    var model = _mlContext.Model.Load(stream, out var _);
                    if (model != null)
                    {
                        result.PassedCount++;
                    }
                    else
                    {
                        result.FailedCount++;
                        result.Failures.Add(new IntegrityFailure
                        {
                            VersionId = version.VersionId,
                            ModelPath = version.ModelPath,
                            Reason = "Model loaded as null"
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Failures.Add(new IntegrityFailure
                    {
                        VersionId = version.VersionId,
                        ModelPath = version.ModelPath,
                        Reason = $"Failed to load: {ex.Message}"
                    });
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Archives a version (keeps metadata but removes file)
    /// </summary>
    public async Task<bool> ArchiveVersionAsync(string versionId)
    {
        await _lock.WaitAsync();
        try
        {
            var version = _versionsCache.FirstOrDefault(v => v.VersionId == versionId);
            if (version == null || version.IsActive)
                return false;

            if (File.Exists(version.ModelPath))
            {
                File.Delete(version.ModelPath);
            }

            version.IsRollbackCandidate = false;
            await SaveMetadataAsync();

            _logInfo?.Invoke($"Archived model version {version.DisplayName}");
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Exports a version to a different location
    /// </summary>
    public async Task<bool> ExportVersionAsync(string versionId, string destinationPath)
    {
        var version = await GetVersionByIdAsync(versionId);
        if (version == null || !File.Exists(version.ModelPath))
            return false;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(version.ModelPath, destinationPath, overwrite: true);
            return true;
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"Failed to export version {versionId}", ex);
            return false;
        }
    }

    /// <summary>
    /// Imports a version from an external file
    /// </summary>
    public async Task<ModelVersionInfo?> ImportVersionAsync(string sourcePath, string description = "")
    {
        if (!File.Exists(sourcePath))
            return null;

        try
        {
            // Validate it's a valid model
            using var stream = File.OpenRead(sourcePath);
            var model = _mlContext.Model.Load(stream, out var _);
            if (model == null)
                return null;

            var nextVersionNumber = await GetLatestVersionNumberAsync() + 1;
            var versionId = Guid.NewGuid().ToString("N")[..8];

            var versionedModelPath = Path.Combine(_versionsDirectory, $"model_v{nextVersionNumber}_{versionId}.zip");
            File.Copy(sourcePath, versionedModelPath, overwrite: true);

            var fileHash = await CalculateFileHashAsync(versionedModelPath);
            var fileInfo = new FileInfo(versionedModelPath);

            await _lock.WaitAsync();
            try
            {
                await EnsureCacheValidAsync();

                var versionInfo = new ModelVersionInfo
                {
                    VersionId = versionId,
                    VersionNumber = nextVersionNumber,
                    CreatedAt = DateTime.Now,
                    ModelPath = versionedModelPath,
                    Description = $"Imported: {description}",
                    IsActive = false, // Imported versions are not active by default
                    TrainingSource = "Import",
                    FileSizeBytes = fileInfo.Length,
                    FileHash = fileHash
                };

                _versionsCache.Add(versionInfo);
                await SaveMetadataAsync();

                _logInfo?.Invoke($"Imported model version {versionInfo.DisplayName}");
                return versionInfo;
            }
            finally
            {
                _lock.Release();
            }
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"Failed to import model from {sourcePath}", ex);
            return null;
        }
    }

    /// <summary>
    /// Updates retention settings
    /// </summary>
    public void UpdateRetentionSettings(ModelVersionRetentionSettings settings)
    {
        RetentionSettings = settings;
        _logInfo?.Invoke($"Updated retention settings: Max={settings.MaxVersions}, Min={settings.MinVersions}, Days={settings.RetentionDays}");
    }

    #region Private Methods

    private async Task EnsureCacheValidAsync()
    {
        if (DateTime.Now - _cacheLastUpdated > _cacheExpiration || !_versionsCache.Any())
        {
            await LoadMetadataAsync();
        }
    }

    private async Task LoadMetadataAsync()
    {
        if (!File.Exists(_metadataFilePath))
        {
            _versionsCache = new List<ModelVersionInfo>();
            _cacheLastUpdated = DateTime.Now;
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_metadataFilePath);
            var versions = JsonSerializer.Deserialize<List<ModelVersionInfo>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            _versionsCache = versions ?? new List<ModelVersionInfo>();
            _cacheLastUpdated = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logError?.Invoke("Failed to load version metadata", ex);
            _versionsCache = new List<ModelVersionInfo>();
        }
    }

    private async Task SaveMetadataAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_versionsCache, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_metadataFilePath, json);
            _cacheLastUpdated = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logError?.Invoke("Failed to save version metadata", ex);
        }
    }

    private async Task SaveActiveVersionAsync(string versionId)
    {
        try
        {
            await File.WriteAllTextAsync(_activeVersionFile, versionId);
        }
        catch (Exception ex)
        {
            _logError?.Invoke("Failed to save active version", ex);
        }
    }

    private async Task<string?> GetActiveVersionIdAsync()
    {
        if (!File.Exists(_activeVersionFile))
            return null;

        try
        {
            return await File.ReadAllTextAsync(_activeVersionFile);
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> CalculateFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hash);
    }

    private async Task<CleanupResult> CleanupOldVersionsInternalAsync()
    {
        var result = new CleanupResult { Success = true };
        var versionsToDelete = new List<ModelVersionInfo>();

        // Sort by creation date (newest first)
        var sortedVersions = _versionsCache
            .Where(v => !v.IsActive) // Never delete active version
            .OrderByDescending(v => v.CreatedAt)
            .ToList();

        // Keep minimum versions
        var candidatesForDeletion = sortedVersions.Skip(RetentionSettings.MinVersions).ToList();

        // Keep best performing versions
        if (RetentionSettings.KeepBestPerforming)
        {
            var bestVersions = sortedVersions
                .Where(v => v.Accuracy.HasValue)
                .OrderByDescending(v => v.Accuracy)
                .Take(RetentionSettings.KeepBestCount)
                .Select(v => v.VersionId)
                .ToHashSet();

            candidatesForDeletion = candidatesForDeletion
                .Where(v => !bestVersions.Contains(v.VersionId))
                .ToList();
        }

        // Apply max versions limit
        if (_versionsCache.Count > RetentionSettings.MaxVersions)
        {
            var excessCount = _versionsCache.Count - RetentionSettings.MaxVersions;
            var versionsByAge = candidatesForDeletion
                .Where(v => DateTime.Now - v.CreatedAt > TimeSpan.FromDays(RetentionSettings.RetentionDays))
                .Take(excessCount)
                .ToList();

            versionsToDelete.AddRange(versionsByAge);
        }

        // Delete versions
        foreach (var version in versionsToDelete)
        {
            try
            {
                if (File.Exists(version.ModelPath))
                {
                    result.StorageFreedBytes += version.FileSizeBytes;
                    File.Delete(version.ModelPath);
                }
                _versionsCache.Remove(version);
                result.DeletedVersionIds.Add(version.VersionId);
                result.DeletedCount++;
            }
            catch (Exception ex)
            {
                _logError?.Invoke($"Failed to delete version {version.VersionId}", ex);
            }
        }

        if (result.DeletedCount > 0)
        {
            await SaveMetadataAsync();
            result.Message = $"Cleaned up {result.DeletedCount} old versions, freed {result.StorageFreedBytes / 1024 / 1024} MB";
            _logInfo?.Invoke(result.Message);
            VersionsCleanedUp?.Invoke(this, result);
        }
        else
        {
            result.Message = "No versions needed cleanup";
        }

        return result;
    }

    private async Task EnsureDefaultModelTrackedAsync(string baseModelPath)
    {
        if (!File.Exists(baseModelPath))
            return;

        await _lock.WaitAsync();
        try
        {
            await LoadMetadataAsync();

            // Check if the default model is already tracked
            var alreadyTracked = _versionsCache.Any(v => v.ModelPath == baseModelPath);
            if (alreadyTracked || _versionsCache.Any())
                return;

            // Add the default model as version 1
            var fileHash = await CalculateFileHashAsync(baseModelPath);
            var fileInfo = new FileInfo(baseModelPath);

            var versionInfo = new ModelVersionInfo
            {
                VersionId = "default001",
                VersionNumber = 1,
                CreatedAt = fileInfo.LastWriteTime,
                ModelPath = baseModelPath,
                Description = "Initial model",
                IsActive = true,
                TrainingSource = "Initial",
                FileSizeBytes = fileInfo.Length,
                FileHash = fileHash
            };

            _versionsCache.Add(versionInfo);
            await SaveMetadataAsync();
            await SaveActiveVersionAsync(versionInfo.VersionId);

            _logInfo?.Invoke($"Tracked existing model as version {versionInfo.DisplayName}");
        }
        finally
        {
            _lock.Release();
        }
    }

    #endregion
}
