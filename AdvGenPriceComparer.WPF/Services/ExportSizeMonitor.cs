using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for monitoring export directory size and enforcing limits
/// </summary>
public class ExportSizeMonitor : IFileSizeMonitor
{
    private readonly ILoggerService _logger;
    private readonly string _defaultExportDirectory;

    /// <summary>
    /// Create a new ExportSizeMonitor
    /// </summary>
    public ExportSizeMonitor(ILoggerService logger, ISettingsService settingsService)
    {
        _logger = logger;
        _defaultExportDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer",
            "Exports");
    }

    /// <inheritdoc />
    public async Task<DirectorySizeInfo> GetDirectorySizeInfoAsync(string? directoryPath = null)
    {
        var path = directoryPath ?? _defaultExportDirectory;
        
        if (!Directory.Exists(path))
        {
            return new DirectorySizeInfo
            {
                DirectoryPath = path,
                TotalSizeBytes = 0,
                FileCount = 0,
                AvailableFreeSpaceBytes = await GetAvailableFreeSpaceAsync(path)
            };
        }

        var files = await Task.Run(() => Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
        var directories = await Task.Run(() => Directory.GetDirectories(path, "*", SearchOption.AllDirectories));
        
        long totalSize = 0;
        long largestFileSize = 0;
        DateTime? oldestDate = null;
        DateTime? newestDate = null;
        var largestFiles = new List<ExportFileInfo>();

        foreach (var file in files)
        {
            try
            {
                var fileInfo = new System.IO.FileInfo(file);
                if (!fileInfo.Exists) continue;

                totalSize += fileInfo.Length;

                if (fileInfo.Length > largestFileSize)
                {
                    largestFileSize = fileInfo.Length;
                }

                var lastWriteTime = fileInfo.LastWriteTimeUtc;
                if (oldestDate == null || lastWriteTime < oldestDate)
                    oldestDate = lastWriteTime;
                if (newestDate == null || lastWriteTime > newestDate)
                    newestDate = lastWriteTime;

                largestFiles.Add(new ExportFileInfo
                {
                    FullPath = file,
                    FileName = Path.GetFileName(file),
                    SizeBytes = fileInfo.Length,
                    LastModified = lastWriteTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error reading file info for {file}: {ex.Message}");
            }
        }

        // Sort and take top 10 largest files
        largestFiles = largestFiles
            .OrderByDescending(f => f.SizeBytes)
            .Take(10)
            .ToList();

        return new DirectorySizeInfo
        {
            DirectoryPath = path,
            TotalSizeBytes = totalSize,
            FileCount = files.Length,
            SubdirectoryCount = directories.Length,
            OldestFileDate = oldestDate,
            NewestFileDate = newestDate,
            LargestFileSizeBytes = largestFileSize,
            LargestFiles = largestFiles,
            AvailableFreeSpaceBytes = await GetAvailableFreeSpaceAsync(path)
        };
    }

    /// <inheritdoc />
    public async Task<SizeCheckResult> CheckSizeBeforeExportAsync(long expectedNewFileSize, string? directoryPath = null)
    {
        var path = directoryPath ?? _defaultExportDirectory;
        var options = new FileSizeMonitorOptions();
        var dirInfo = await GetDirectorySizeInfoAsync(path);
        
        var expectedTotal = dirInfo.TotalSizeBytes + expectedNewFileSize;
        var spaceNeeded = expectedTotal > options.MaxDirectorySizeBytes 
            ? expectedTotal - options.MaxDirectorySizeBytes 
            : 0;

        var result = new SizeCheckResult
        {
            CanExport = expectedTotal <= options.MaxDirectorySizeBytes,
            CurrentSizeBytes = dirInfo.TotalSizeBytes,
            ExpectedSizeAfterExportBytes = expectedTotal,
            SizeLimitBytes = options.MaxDirectorySizeBytes,
            SpaceNeededBytes = spaceNeeded,
            CleanupRecommended = expectedTotal > options.MaxDirectorySizeBytes * (options.CleanupThresholdPercentage / 100.0)
        };

        if (!result.CanExport)
        {
            result.Recommendation = $"Export would exceed size limit. Need to free {FormatBytes(spaceNeeded)}. " +
                $"Consider running cleanup to remove old exports.";
        }
        else if (result.CleanupRecommended)
        {
            result.Recommendation = $"Export directory is approaching size limit ({result.PercentageOfLimit:F1}% used). " +
                $"Consider cleaning up old exports to maintain optimal performance.";
        }
        else
        {
            result.Recommendation = "Export can proceed. Sufficient space available.";
        }

        _logger.LogInfo($"Size check for export: Current={FormatBytes(dirInfo.TotalSizeBytes)}, " +
            $"Expected={FormatBytes(expectedTotal)}, Limit={FormatBytes(options.MaxDirectorySizeBytes)}, " +
            $"CanExport={result.CanExport}");

        return result;
    }

    /// <inheritdoc />
    public async Task<CleanupResult> CleanupOldExportsAsync(long targetFreeSpace, CleanupOptions? options = null, string? directoryPath = null)
    {
        var path = directoryPath ?? _defaultExportDirectory;
        var cleanupOptions = options ?? new CleanupOptions();
        var result = new CleanupResult { Success = true };

        if (!Directory.Exists(path))
        {
            result.Message = "Directory does not exist.";
            return result;
        }

        var files = await GetDeletableFilesAsync(path, cleanupOptions);
        
        if (cleanupOptions.Strategy == CleanupStrategy.LargestFirst)
        {
            files = files.OrderByDescending(f => f.Length).ToList();
        }
        else if (cleanupOptions.Strategy == CleanupStrategy.LeastRecentlyAccessed)
        {
            files = files.OrderBy(f => f.LastAccessTimeUtc).ToList();
        }
        else // OldestFirst (default)
        {
            files = files.OrderBy(f => f.LastWriteTimeUtc).ToList();
        }

        long spaceFreed = 0;
        int filesKept = 0;

        foreach (var file in files)
        {
            // Ensure we keep minimum number of files
            if (files.Count - result.FilesDeleted <= cleanupOptions.MinFilesToKeep && spaceFreed < targetFreeSpace)
            {
                filesKept++;
                continue;
            }

            // Stop if we've freed enough space
            if (spaceFreed >= targetFreeSpace && result.FilesDeleted > 0)
            {
                break;
            }

            try
            {
                if (!cleanupOptions.DryRun)
                {
                    file.Delete();
                }
                
                result.FilesDeleted++;
                spaceFreed += file.Length;
                result.DeletedFiles.Add(file.FullName);
                
                _logger.LogInfo($"{(cleanupOptions.DryRun ? "[DRY RUN] Would delete" : "Deleted")}: {file.FullName} ({FormatBytes(file.Length)})");
            }
            catch (Exception ex)
            {
                result.FailedDeletions.Add(new DeleteFailure
                {
                    FilePath = file.FullName,
                    ErrorMessage = ex.Message
                });
                _logger.LogWarning($"Failed to delete {file.FullName}: {ex.Message}");
            }
        }

        result.SpaceFreedBytes = spaceFreed;
        result.Success = result.FailedDeletions.Count == 0 || result.FilesDeleted > 0;
        
        var action = cleanupOptions.DryRun ? "would be" : "was";
        result.Message = $"Cleanup completed: {result.FilesDeleted} files {action} deleted, " +
            $"{result.SpaceFreedFormatted} {action} freed. " +
            $"{(filesKept > 0 ? $"{filesKept} files kept to meet minimum retention." : "")}";

        _logger.LogInfo(result.Message);

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> PerformAutoCleanupAsync(FileSizeMonitorOptions? options = null, string? directoryPath = null)
    {
        var path = directoryPath ?? _defaultExportDirectory;
        var monitorOptions = options ?? new FileSizeMonitorOptions();

        if (!monitorOptions.AutoCleanupEnabled)
        {
            _logger.LogInfo("Auto-cleanup is disabled.");
            return false;
        }

        var dirInfo = await GetDirectorySizeInfoAsync(path);
        var usagePercentage = (dirInfo.TotalSizeBytes / (double)monitorOptions.MaxDirectorySizeBytes) * 100;

        if (usagePercentage < monitorOptions.CleanupThresholdPercentage)
        {
            _logger.LogInfo($"Directory size ({usagePercentage:F1}%) below cleanup threshold ({monitorOptions.CleanupThresholdPercentage}%). No cleanup needed.");
            return false;
        }

        var targetFreeSpace = (long)(monitorOptions.MaxDirectorySizeBytes * 0.2); // Free up 20% of limit
        
        _logger.LogInfo($"Auto-cleanup triggered: Usage is {usagePercentage:F1}%. Targeting to free {FormatBytes(targetFreeSpace)}.");

        var cleanupOptions = new CleanupOptions
        {
            Strategy = CleanupStrategy.OldestFirst,
            MinFileAge = monitorOptions.MinFileAgeForDeletion,
            MinFilesToKeep = monitorOptions.MinFilesToKeep,
            FileExtensions = monitorOptions.FileExtensions
        };

        var result = await CleanupOldExportsAsync(targetFreeSpace, cleanupOptions, path);
        
        return result.FilesDeleted > 0;
    }

    /// <inheritdoc />
    public async Task<int> GetFileCountAsync(string? directoryPath = null, string searchPattern = "*.*")
    {
        var path = directoryPath ?? _defaultExportDirectory;
        
        if (!Directory.Exists(path))
        {
            return 0;
        }

        return await Task.Run(() => Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories).Length);
    }

    /// <inheritdoc />
    public async Task<bool> AreLimitsExceededAsync(FileSizeMonitorOptions? options = null, string? directoryPath = null)
    {
        var path = directoryPath ?? _defaultExportDirectory;
        var monitorOptions = options ?? new FileSizeMonitorOptions();
        
        var dirInfo = await GetDirectorySizeInfoAsync(path);
        
        var sizeExceeded = dirInfo.TotalSizeBytes > monitorOptions.MaxDirectorySizeBytes;
        var countExceeded = dirInfo.FileCount > monitorOptions.MaxFileCount;
        
        if (sizeExceeded)
        {
            _logger.LogWarning($"Size limit exceeded: {FormatBytes(dirInfo.TotalSizeBytes)} > {FormatBytes(monitorOptions.MaxDirectorySizeBytes)}");
        }
        
        if (countExceeded)
        {
            _logger.LogWarning($"File count limit exceeded: {dirInfo.FileCount} > {monitorOptions.MaxFileCount}");
        }
        
        return sizeExceeded || countExceeded;
    }

    /// <summary>
    /// Get available free space on the drive
    /// </summary>
    private async Task<long> GetAvailableFreeSpaceAsync(string path)
    {
        return await Task.Run(() =>
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    // Get drive info for parent directory
                    var root = Path.GetPathRoot(path) ?? 
                        Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) ?? "C:\\";
                    var driveInfo = new DriveInfo(root);
                    return driveInfo.AvailableFreeSpace;
                }
                
                var rootPath = Path.GetPathRoot(directoryInfo.FullName);
                if (string.IsNullOrEmpty(rootPath))
                {
                    return 0;
                }
                
                var drive = new DriveInfo(rootPath);
                return drive.AvailableFreeSpace;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error getting available free space: {ex.Message}");
                return 0;
            }
        });
    }

    /// <summary>
    /// Get list of files that can be deleted based on cleanup options
    /// </summary>
    private async Task<List<FileInfo>> GetDeletableFilesAsync(string path, CleanupOptions options)
    {
        return await Task.Run(() =>
        {
            var files = new List<FileInfo>();
            var cutoffDate = DateTime.UtcNow.Subtract(options.MinFileAge);

            foreach (var filePath in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    // Skip excluded files
                    if (options.ExcludedFiles.Contains(filePath))
                    {
                        continue;
                    }

                    var fileInfo = new FileInfo(filePath);
                    if (!fileInfo.Exists) continue;

                    // Check file extension filter
                    if (options.FileExtensions?.Count > 0)
                    {
                        var extension = fileInfo.Extension.ToLowerInvariant();
                        if (!options.FileExtensions.Contains(extension))
                        {
                            continue;
                        }
                    }

                    // Check minimum age
                    if (fileInfo.LastWriteTimeUtc > cutoffDate)
                    {
                        continue;
                    }

                    files.Add(fileInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error checking file {filePath}: {ex.Message}");
                }
            }

            return files;
        });
    }

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
