using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service implementation for checking and managing application updates
/// </summary>
public class UpdateService : IUpdateService
{
    private readonly ISettingsService _settingsService;
    private readonly ILoggerService _logger;
    private readonly HttpClient _httpClient;
    private readonly string _currentVersion;

    /// <inheritdoc />
    public string UpdateInfoUrl { get; set; } = "https://raw.githubusercontent.com/advgen/pricecomparer/main/updates.json";

    /// <inheritdoc />
    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <inheritdoc />
    public event EventHandler<UpdateErrorEventArgs>? UpdateCheckFailed;

    /// <summary>
    /// Creates a new instance of UpdateService
    /// </summary>
    public UpdateService(ISettingsService settingsService, ILoggerService logger)
    {
        _settingsService = settingsService;
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AdvGenPriceComparer/" + GetCurrentVersion());
        _currentVersion = GetCurrentVersion();
    }

    /// <inheritdoc />
    public async Task<UpdateCheckResult> CheckForUpdateAsync()
    {
        try
        {
            _logger.LogInfo($"Checking for updates from: {UpdateInfoUrl}");

            var response = await _httpClient.GetAsync(UpdateInfoUrl);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Failed to check for updates: HTTP {(int)response.StatusCode}";
                _logger.LogError(errorMsg);
                return new UpdateCheckResult
                {
                    IsSuccessful = false,
                    ErrorMessage = errorMsg
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (updateInfo?.LatestVersion == null)
            {
                var errorMsg = "Invalid update information received";
                _logger.LogError(errorMsg);
                return new UpdateCheckResult
                {
                    IsSuccessful = false,
                    ErrorMessage = errorMsg
                };
            }

            var currentVersion = new Version(_currentVersion);
            var latestVersion = new Version(updateInfo.LatestVersion);

            var isUpdateAvailable = latestVersion > currentVersion;

            var result = new UpdateCheckResult
            {
                IsSuccessful = true,
                IsUpdateAvailable = isUpdateAvailable,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                DownloadUrl = updateInfo.DownloadUrl ?? string.Empty,
                ReleaseNotes = updateInfo.ReleaseNotes ?? string.Empty,
                IsMandatory = updateInfo.IsMandatory,
                FileSize = updateInfo.FileSize,
                ReleaseDate = updateInfo.ReleaseDate,
                FileHash = updateInfo.FileHash ?? string.Empty
            };

            if (isUpdateAvailable)
            {
                _logger.LogInfo($"Update available: {currentVersion} -> {latestVersion}");
                UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs { Result = result });
            }
            else
            {
                _logger.LogInfo($"No update available. Current version: {currentVersion}");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            var errorMsg = $"Network error checking for updates: {ex.Message}";
            _logger.LogError(errorMsg, ex);
            UpdateCheckFailed?.Invoke(this, new UpdateErrorEventArgs { ErrorMessage = errorMsg, Exception = ex });
            return new UpdateCheckResult { IsSuccessful = false, ErrorMessage = errorMsg };
        }
        catch (JsonException ex)
        {
            var errorMsg = $"Invalid update data format: {ex.Message}";
            _logger.LogError(errorMsg, ex);
            UpdateCheckFailed?.Invoke(this, new UpdateErrorEventArgs { ErrorMessage = errorMsg, Exception = ex });
            return new UpdateCheckResult { IsSuccessful = false, ErrorMessage = errorMsg };
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error checking for updates: {ex.Message}";
            _logger.LogError(errorMsg, ex);
            UpdateCheckFailed?.Invoke(this, new UpdateErrorEventArgs { ErrorMessage = errorMsg, Exception = ex });
            return new UpdateCheckResult { IsSuccessful = false, ErrorMessage = errorMsg };
        }
    }

    /// <inheritdoc />
    public async Task CheckForUpdateOnStartupAsync()
    {
        // Respect the user's auto-check setting
        if (!_settingsService.AutoCheckForUpdates)
        {
            _logger.LogInfo("Auto-check for updates is disabled");
            return;
        }

        // Check if we should skip the check (e.g., already checked today)
        if (ShouldSkipCheck())
        {
            _logger.LogInfo("Skipping update check - already checked recently");
            return;
        }

        // Perform the check
        var result = await CheckForUpdateAsync();

        // Record that we performed a check
        RecordCheckPerformed();

        // If update is available, show notification (unless silent)
        if (result.IsUpdateAvailable && result.IsSuccessful)
        {
            // This will be handled by the event handler in MainWindow
            // or we can show the window directly
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowUpdateNotification(result);
            });
        }
    }

    /// <inheritdoc />
    public async Task<bool> DownloadUpdateAsync(string downloadUrl)
    {
        try
        {
            _logger.LogInfo($"Starting download from: {downloadUrl}");

            // For MSI installers, we download to temp and execute
            var tempPath = Path.Combine(Path.GetTempPath(), "AdvGenPriceComparer_Update.msi");

            var response = await _httpClient.GetAsync(downloadUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to download update: HTTP {(int)response.StatusCode}");
                return false;
            }

            var data = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(tempPath, data);

            _logger.LogInfo($"Download completed: {tempPath}");

            // Execute the installer
            Process.Start(new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true,
                Verb = "open"
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to download update", ex);
            return false;
        }
    }

    /// <inheritdoc />
    public void OpenDownloadPage(string url)
    {
        try
        {
            _logger.LogInfo($"Opening download page: {url}");
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to open download page", ex);
        }
    }

    /// <summary>
    /// Shows the update notification window
    /// </summary>
    private void ShowUpdateNotification(UpdateCheckResult result)
    {
        try
        {
            var window = new Views.UpdateNotificationWindow(result, this);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to show update notification", ex);
        }
    }

    /// <summary>
    /// Gets the current application version from assembly
    /// </summary>
    private string GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString(3) ?? "1.0.0"; // Major.Minor.Build
    }

    /// <summary>
    /// Determines if we should skip the update check (checked within last 24 hours)
    /// </summary>
    private bool ShouldSkipCheck()
    {
        try
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AdvGenPriceComparer");
            var checkFile = Path.Combine(appDataPath, "last_update_check.txt");

            if (!File.Exists(checkFile))
                return false;

            var lastCheckText = File.ReadAllText(checkFile);
            if (DateTime.TryParse(lastCheckText, out var lastCheck))
            {
                // Only check once per day
                return (DateTime.Now - lastCheck).TotalHours < 24;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error reading last check time", ex);
        }

        return false;
    }

    /// <summary>
    /// Records that an update check was performed
    /// </summary>
    private void RecordCheckPerformed()
    {
        try
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AdvGenPriceComparer");
            Directory.CreateDirectory(appDataPath);
            var checkFile = Path.Combine(appDataPath, "last_update_check.txt");

            File.WriteAllText(checkFile, DateTime.Now.ToString("O"));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error recording check time", ex);
        }
    }
}

/// <summary>
/// Update information from remote JSON
/// </summary>
internal class UpdateInfo
{
    public string? LatestVersion { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ReleaseNotes { get; set; }
    public bool IsMandatory { get; set; }
    public long FileSize { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? FileHash { get; set; }
}
