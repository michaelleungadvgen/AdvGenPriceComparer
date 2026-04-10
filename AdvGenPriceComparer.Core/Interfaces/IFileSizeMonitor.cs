using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Interface for monitoring export directory size and enforcing limits
/// </summary>
public interface IFileSizeMonitor
{
    /// <summary>
    /// Get current directory size information
    /// </summary>
    /// <param name="directoryPath">Path to monitor (null for default export directory)</param>
    /// <returns>Directory size information</returns>
    Task<DirectorySizeInfo> GetDirectorySizeInfoAsync(string? directoryPath = null);

    /// <summary>
    /// Check if a new export would exceed size limits
    /// </summary>
    /// <param name="expectedNewFileSize">Expected size of new export file in bytes</param>
    /// <param name="directoryPath">Path to check (null for default export directory)</param>
    /// <returns>Size check result with recommendation</returns>
    Task<SizeCheckResult> CheckSizeBeforeExportAsync(long expectedNewFileSize, string? directoryPath = null);

    /// <summary>
    /// Clean up old export files to free up space
    /// </summary>
    /// <param name="targetFreeSpace">Target space to free in bytes</param>
    /// <param name="options">Cleanup options</param>
    /// <param name="directoryPath">Path to clean (null for default export directory)</param>
    /// <returns>Cleanup result with details of deleted files</returns>
    Task<CleanupResult> CleanupOldExportsAsync(long targetFreeSpace, CleanupOptions? options = null, string? directoryPath = null);

    /// <summary>
    /// Perform automatic cleanup if size limits are exceeded
    /// </summary>
    /// <param name="options">Monitor options</param>
    /// <param name="directoryPath">Path to monitor (null for default export directory)</param>
    /// <returns>True if cleanup was performed</returns>
    Task<bool> PerformAutoCleanupAsync(FileSizeMonitorOptions? options = null, string? directoryPath = null);

    /// <summary>
    /// Get file count in directory
    /// </summary>
    /// <param name="directoryPath">Path to check (null for default export directory)</param>
    /// <param name="searchPattern">File search pattern (default: *.*)</param>
    /// <returns>Number of files</returns>
    Task<int> GetFileCountAsync(string? directoryPath = null, string searchPattern = "*.*");

    /// <summary>
    /// Check if size limits are currently exceeded
    /// </summary>
    /// <param name="options">Monitor options</param>
    /// <param name="directoryPath">Path to check (null for default export directory)</param>
    /// <returns>True if limits are exceeded</returns>
    Task<bool> AreLimitsExceededAsync(FileSizeMonitorOptions? options = null, string? directoryPath = null);
}
