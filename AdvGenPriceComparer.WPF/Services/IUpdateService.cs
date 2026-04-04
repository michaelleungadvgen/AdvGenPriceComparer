using System;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for checking and managing application updates
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// The URL where update information is hosted (JSON file)
    /// </summary>
    string UpdateInfoUrl { get; set; }

    /// <summary>
    /// Check if an update is available
    /// </summary>
    /// <returns>Update check result with version information</returns>
    Task<UpdateCheckResult> CheckForUpdateAsync();

    /// <summary>
    /// Check for updates on startup if AutoCheckForUpdates is enabled
    /// </summary>
    Task CheckForUpdateOnStartupAsync();

    /// <summary>
    /// Download and install the update (if silent update is supported)
    /// </summary>
    /// <param name="updateResult">The update result containing the download URL and hash</param>
    /// <returns>True if download started successfully and verified</returns>
    Task<bool> DownloadUpdateAsync(UpdateCheckResult updateResult);

    /// <summary>
    /// Open the download page in browser
    /// </summary>
    /// <param name="url">URL to open</param>
    void OpenDownloadPage(string url);

    /// <summary>
    /// Event fired when an update is available
    /// </summary>
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <summary>
    /// Event fired when update check fails
    /// </summary>
    event EventHandler<UpdateErrorEventArgs>? UpdateCheckFailed;
}

/// <summary>
/// Result of an update check
/// </summary>
public class UpdateCheckResult
{
    /// <summary>
    /// Whether an update is available
    /// </summary>
    public bool IsUpdateAvailable { get; set; }

    /// <summary>
    /// Current installed version
    /// </summary>
    public Version? CurrentVersion { get; set; }

    /// <summary>
    /// Latest available version
    /// </summary>
    public Version? LatestVersion { get; set; }

    /// <summary>
    /// Download URL for the update
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Release notes for the update
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Whether the update is mandatory
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Size of the update file in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Date the update was published
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Hash of the update file for verification
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    /// <summary>
    /// Whether the check was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if check failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Event args for update available event
/// </summary>
public class UpdateAvailableEventArgs : EventArgs
{
    public UpdateCheckResult Result { get; set; } = new();
}

/// <summary>
/// Event args for update check error event
/// </summary>
public class UpdateErrorEventArgs : EventArgs
{
    public string ErrorMessage { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}
