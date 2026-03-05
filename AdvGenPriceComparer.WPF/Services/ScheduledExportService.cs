using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for managing scheduled automatic exports of price data
/// </summary>
public class ScheduledExportService : IDisposable
{
    private readonly StaticDataExporter _exporter;
    private readonly ISettingsService _settingsService;
    private readonly ILoggerService _logger;
    private readonly string _scheduleConfigPath;
    private readonly System.Timers.Timer _checkTimer;
    private readonly object _lockObject = new();
    
    private ScheduledExportConfig _config;
    private bool _isRunning;
    private DateTime _lastCheckTime;

    /// <summary>
    /// Event raised when a scheduled export is completed
    /// </summary>
    public event EventHandler<ScheduledExportCompletedEventArgs>? ExportCompleted;

    /// <summary>
    /// Event raised when a scheduled export fails
    /// </summary>
    public event EventHandler<ScheduledExportFailedEventArgs>? ExportFailed;

    /// <summary>
    /// Current configuration for scheduled exports
    /// </summary>
    public ScheduledExportConfig Config => _config;

    /// <summary>
    /// Whether the scheduled export service is currently enabled and running
    /// </summary>
    public bool IsEnabled => _config?.IsEnabled ?? false;

    /// <summary>
    /// The last time a check was performed for due exports
    /// </summary>
    public DateTime LastCheckTime => _lastCheckTime;

    public ScheduledExportService(
        StaticDataExporter exporter,
        ISettingsService settingsService,
        ILoggerService logger)
    {
        _exporter = exporter;
        _settingsService = settingsService;
        _logger = logger;

        // Set up configuration path
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer");
        _scheduleConfigPath = Path.Combine(appDataPath, "scheduled_export_config.json");

        // Load or create default config
        _config = LoadConfigAsync().GetAwaiter().GetResult() ?? CreateDefaultConfig();

        // Set up timer to check for scheduled exports every minute
        _checkTimer = new System.Timers.Timer(60000); // 1 minute
        _checkTimer.Elapsed += async (s, e) => await CheckAndRunExportAsync();
        _checkTimer.AutoReset = true;

        _logger.LogInfo("ScheduledExportService initialized");
    }

    /// <summary>
    /// Start the scheduled export service
    /// </summary>
    public void Start()
    {
        if (_config.IsEnabled && !_isRunning)
        {
            lock (_lockObject)
            {
                if (_config.IsEnabled && !_isRunning)
                {
                    _checkTimer.Start();
                    _isRunning = true;
                    _logger.LogInfo("Scheduled export service started");
                }
            }
        }
    }

    /// <summary>
    /// Stop the scheduled export service
    /// </summary>
    public void Stop()
    {
        if (_isRunning)
        {
            lock (_lockObject)
            {
                if (_isRunning)
                {
                    _checkTimer.Stop();
                    _isRunning = false;
                    _logger.LogInfo("Scheduled export service stopped");
                }
            }
        }
    }

    /// <summary>
    /// Update the scheduled export configuration
    /// </summary>
    public async Task UpdateConfigAsync(ScheduledExportConfig newConfig)
    {
        _config = newConfig;
        await SaveConfigAsync();

        // Restart if needed
        if (_isRunning && !_config.IsEnabled)
        {
            Stop();
        }
        else if (!_isRunning && _config.IsEnabled)
        {
            Start();
        }

        _logger.LogInfo($"Scheduled export configuration updated. Enabled: {_config.IsEnabled}, Frequency: {_config.Frequency}");
    }

    /// <summary>
    /// Run an export immediately, regardless of schedule
    /// </summary>
    public async Task<ScheduledExportResult> RunExportNowAsync(
        string? customOutputPath = null,
        IProgress<StaticExportProgress>? progress = null)
    {
        var result = new ScheduledExportResult
        {
            StartedAt = DateTime.Now,
            IsManual = true
        };

        try
        {
            _logger.LogInfo("Starting manual scheduled export");

            var outputPath = customOutputPath ?? GetDefaultOutputPath();
            var exportOptions = CreateExportOptions();

            var exportResult = await _exporter.ExportStaticPackageAsync(
                exportOptions,
                outputPath,
                progress);

            result.CompletedAt = DateTime.Now;
            result.Success = exportResult.Success;
            result.ExportResult = exportResult;
            result.OutputPath = outputPath;

            if (exportResult.Success)
            {
                _config.LastExportTime = DateTime.Now;
                _config.LastExportPath = outputPath;
                _config.ExportCount++;
                await SaveConfigAsync();

                // Clean up old exports if retention is enabled
                if (_config.RetentionDays > 0)
                {
                    await CleanupOldExportsAsync();
                }

                ExportCompleted?.Invoke(this, new ScheduledExportCompletedEventArgs(result));
                _logger.LogInfo($"Manual export completed successfully. Output: {outputPath}");
            }
            else
            {
                result.ErrorMessage = exportResult.ErrorMessage;
                ExportFailed?.Invoke(this, new ScheduledExportFailedEventArgs(result, exportResult.ErrorMessage ?? "Unknown error"));
                _logger.LogError($"Manual export failed: {exportResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            result.CompletedAt = DateTime.Now;
            result.Success = false;
            result.ErrorMessage = ex.Message;
            ExportFailed?.Invoke(this, new ScheduledExportFailedEventArgs(result, ex.Message));
            _logger.LogError("Manual export failed with exception", ex);
        }

        return result;
    }

    /// <summary>
    /// Get the export history
    /// </summary>
    public Task<List<ExportHistoryEntry>> GetExportHistoryAsync(int count = 10)
    {
        var history = _config.ExportHistory
            .OrderByDescending(h => h.Timestamp)
            .Take(count)
            .ToList();
        
        return Task.FromResult(history);
    }

    /// <summary>
    /// Get the next scheduled export time
    /// </summary>
    public DateTime? GetNextScheduledTime()
    {
        if (!_config.IsEnabled)
            return null;

        var lastExport = _config.LastExportTime;
        var now = DateTime.Now;

        return _config.Frequency switch
        {
            ExportFrequency.Daily => lastExport.Date < now.Date 
                ? now.Date.Add(_config.ScheduledTime) 
                : now.Date.AddDays(1).Add(_config.ScheduledTime),
            ExportFrequency.Weekly => CalculateNextWeeklyExport(lastExport, now),
            ExportFrequency.Monthly => CalculateNextMonthlyExport(lastExport, now),
            _ => null
        };
    }

    private async Task CheckAndRunExportAsync()
    {
        _lastCheckTime = DateTime.Now;

        if (!_config.IsEnabled)
            return;

        try
        {
            var now = DateTime.Now;
            var shouldExport = _config.Frequency switch
            {
                ExportFrequency.Daily => ShouldRunDailyExport(now),
                ExportFrequency.Weekly => ShouldRunWeeklyExport(now),
                ExportFrequency.Monthly => ShouldRunMonthlyExport(now),
                _ => false
            };

            if (shouldExport)
            {
                _logger.LogInfo("Scheduled export is due, starting export...");
                await RunScheduledExportAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error checking for scheduled export", ex);
        }
    }

    private bool ShouldRunDailyExport(DateTime now)
    {
        // Check if we haven't exported today and it's past the scheduled time
        var lastExport = _config.LastExportTime;
        var scheduledToday = now.Date.Add(_config.ScheduledTime);
        
        return lastExport.Date < now.Date && now >= scheduledToday;
    }

    private bool ShouldRunWeeklyExport(DateTime now)
    {
        var lastExport = _config.LastExportTime;
        var scheduledThisWeek = GetScheduledDateTimeForWeek(now);
        
        // Check if we're on the right day and past the scheduled time
        return now.DayOfWeek == _config.DayOfWeek && 
               now >= scheduledThisWeek && 
               lastExport < scheduledThisWeek;
    }

    private bool ShouldRunMonthlyExport(DateTime now)
    {
        var lastExport = _config.LastExportTime;
        var scheduledThisMonth = new DateTime(now.Year, now.Month, Math.Min(_config.DayOfMonth, DateTime.DaysInMonth(now.Year, now.Month)))
            .Add(_config.ScheduledTime);
        
        return now.Date == scheduledThisMonth.Date && 
               now >= scheduledThisMonth && 
               lastExport < scheduledThisMonth;
    }

    private async Task RunScheduledExportAsync()
    {
        var result = new ScheduledExportResult
        {
            StartedAt = DateTime.Now,
            IsManual = false
        };

        try
        {
            var outputPath = GetDefaultOutputPath();
            var exportOptions = CreateExportOptions();

            var exportResult = await _exporter.ExportStaticPackageAsync(
                exportOptions,
                outputPath);

            result.CompletedAt = DateTime.Now;
            result.Success = exportResult.Success;
            result.ExportResult = exportResult;
            result.OutputPath = outputPath;

            if (exportResult.Success)
            {
                _config.LastExportTime = DateTime.Now;
                _config.LastExportPath = outputPath;
                _config.ExportCount++;
                
                // Add to history
                _config.ExportHistory.Add(new ExportHistoryEntry
                {
                    Timestamp = DateTime.Now,
                    Success = true,
                    ProductCount = exportResult.TotalProducts,
                    StoreCount = exportResult.TotalStores,
                    PriceRecordCount = exportResult.TotalPriceRecords,
                    OutputPath = outputPath,
                    ArchivePath = exportResult.ArchivePath
                });

                // Trim history if needed
                if (_config.ExportHistory.Count > 100)
                {
                    _config.ExportHistory = _config.ExportHistory
                        .OrderByDescending(h => h.Timestamp)
                        .Take(50)
                        .ToList();
                }

                await SaveConfigAsync();

                // Clean up old exports
                if (_config.RetentionDays > 0)
                {
                    await CleanupOldExportsAsync();
                }

                ExportCompleted?.Invoke(this, new ScheduledExportCompletedEventArgs(result));
                _logger.LogInfo($"Scheduled export completed successfully. Output: {outputPath}");
            }
            else
            {
                result.ErrorMessage = exportResult.ErrorMessage;
                
                _config.ExportHistory.Add(new ExportHistoryEntry
                {
                    Timestamp = DateTime.Now,
                    Success = false,
                    ErrorMessage = exportResult.ErrorMessage
                });
                await SaveConfigAsync();

                ExportFailed?.Invoke(this, new ScheduledExportFailedEventArgs(result, exportResult.ErrorMessage ?? "Unknown error"));
                _logger.LogError($"Scheduled export failed: {exportResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            result.CompletedAt = DateTime.Now;
            result.Success = false;
            result.ErrorMessage = ex.Message;

            _config.ExportHistory.Add(new ExportHistoryEntry
            {
                Timestamp = DateTime.Now,
                Success = false,
                ErrorMessage = ex.Message
            });
            await SaveConfigAsync();

            ExportFailed?.Invoke(this, new ScheduledExportFailedEventArgs(result, ex.Message));
            _logger.LogError("Scheduled export failed with exception", ex);
        }
    }

    private StaticExportOptions CreateExportOptions()
    {
        return new StaticExportOptions
        {
            ExportedBy = _config.ExportedBy,
            Description = $"Scheduled export on {DateTime.Now:yyyy-MM-dd HH:mm}",
            LocationSuburb = _config.LocationSuburb,
            LocationState = _config.LocationState,
            CreateCompressedPackage = _config.CreateCompressedPackage,
            GenerateDiscoveryFile = _config.GenerateDiscoveryFile,
            ValidTo = DateTime.UtcNow.AddDays(30)
        };
    }

    private string GetDefaultOutputPath()
    {
        var basePath = !string.IsNullOrEmpty(_config.OutputDirectory)
            ? _config.OutputDirectory
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AdvGenPriceComparer",
                "ScheduledExports");

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        return Path.Combine(basePath, $"export-{timestamp}");
    }

    private DateTime CalculateNextWeeklyExport(DateTime lastExport, DateTime now)
    {
        var daysUntilTarget = ((int)_config.DayOfWeek - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0 && now.TimeOfDay > _config.ScheduledTime)
        {
            daysUntilTarget = 7;
        }
        return now.Date.AddDays(daysUntilTarget).Add(_config.ScheduledTime);
    }

    private DateTime CalculateNextMonthlyExport(DateTime lastExport, DateTime now)
    {
        var targetDay = Math.Min(_config.DayOfMonth, DateTime.DaysInMonth(now.Year, now.Month));
        var targetDate = new DateTime(now.Year, now.Month, targetDay).Add(_config.ScheduledTime);
        
        if (targetDate <= now)
        {
            // Move to next month
            var nextMonth = now.AddMonths(1);
            targetDay = Math.Min(_config.DayOfMonth, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
            targetDate = new DateTime(nextMonth.Year, nextMonth.Month, targetDay).Add(_config.ScheduledTime);
        }
        
        return targetDate;
    }

    private DateTime GetScheduledDateTimeForWeek(DateTime now)
    {
        var daysDiff = (int)_config.DayOfWeek - (int)now.DayOfWeek;
        var scheduledDate = now.Date.AddDays(daysDiff);
        return scheduledDate.Add(_config.ScheduledTime);
    }

    private async Task CleanupOldExportsAsync()
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-_config.RetentionDays);
            var basePath = !string.IsNullOrEmpty(_config.OutputDirectory)
                ? _config.OutputDirectory
                : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AdvGenPriceComparer",
                    "ScheduledExports");

            if (!Directory.Exists(basePath))
                return;

            var directories = Directory.GetDirectories(basePath, "export-*");
            var deletedCount = 0;

            foreach (var dir in directories)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (dirInfo.CreationTime < cutoffDate)
                    {
                        Directory.Delete(dir, true);
                        deletedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to delete old export directory: {dir} - {ex.Message}");
                }
            }

            // Also clean up old zip files
            var zipFiles = Directory.GetFiles(basePath, "price-data-*.zip");
            foreach (var zipFile in zipFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(zipFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(zipFile);
                        deletedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to delete old export archive: {zipFile} - {ex.Message}");
                }
            }

            if (deletedCount > 0)
            {
                _logger.LogInfo($"Cleaned up {deletedCount} old export(s)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during old export cleanup", ex);
        }
    }

    private async Task<ScheduledExportConfig?> LoadConfigAsync()
    {
        try
        {
            if (!File.Exists(_scheduleConfigPath))
                return null;

            var json = await File.ReadAllTextAsync(_scheduleConfigPath);
            return JsonSerializer.Deserialize<ScheduledExportConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load scheduled export config", ex);
            return null;
        }
    }

    private async Task SaveConfigAsync()
    {
        try
        {
            var directory = Path.GetDirectoryName(_scheduleConfigPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(_scheduleConfigPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save scheduled export config", ex);
        }
    }

    private ScheduledExportConfig CreateDefaultConfig()
    {
        return new ScheduledExportConfig
        {
            IsEnabled = false,
            Frequency = ExportFrequency.Weekly,
            ScheduledTime = new TimeSpan(2, 0, 0), // 2:00 AM
            DayOfWeek = DayOfWeek.Sunday,
            DayOfMonth = 1,
            RetentionDays = 30,
            CreateCompressedPackage = true,
            GenerateDiscoveryFile = true,
            ExportedBy = Environment.UserName,
            ExportHistory = new List<ExportHistoryEntry>()
        };
    }

    public void Dispose()
    {
        Stop();
        _checkTimer?.Dispose();
    }
}

/// <summary>
/// Configuration for scheduled exports
/// </summary>
public class ScheduledExportConfig
{
    /// <summary>
    /// Whether scheduled exports are enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Export frequency (Daily, Weekly, Monthly)
    /// </summary>
    public ExportFrequency Frequency { get; set; }

    /// <summary>
    /// Time of day to run the export
    /// </summary>
    public TimeSpan ScheduledTime { get; set; }

    /// <summary>
    /// Day of week for weekly exports
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Day of month for monthly exports (1-31)
    /// </summary>
    public int DayOfMonth { get; set; }

    /// <summary>
    /// Output directory for exports (null = default)
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Number of days to retain exports (0 = keep all)
    /// </summary>
    public int RetentionDays { get; set; }

    /// <summary>
    /// Whether to create compressed packages
    /// </summary>
    public bool CreateCompressedPackage { get; set; }

    /// <summary>
    /// Whether to generate discovery files for P2P
    /// </summary>
    public bool GenerateDiscoveryFile { get; set; }

    /// <summary>
    /// Name of the exporter (metadata)
    /// </summary>
    public string? ExportedBy { get; set; }

    /// <summary>
    /// Location suburb for metadata
    /// </summary>
    public string? LocationSuburb { get; set; }

    /// <summary>
    /// Location state for metadata
    /// </summary>
    public string? LocationState { get; set; }

    /// <summary>
    /// Last time an export was performed
    /// </summary>
    public DateTime LastExportTime { get; set; }

    /// <summary>
    /// Path of the last export
    /// </summary>
    public string? LastExportPath { get; set; }

    /// <summary>
    /// Total number of exports performed
    /// </summary>
    public int ExportCount { get; set; }

    /// <summary>
    /// History of exports
    /// </summary>
    public List<ExportHistoryEntry> ExportHistory { get; set; } = new();
}

/// <summary>
/// Export frequency options
/// </summary>
public enum ExportFrequency
{
    Daily,
    Weekly,
    Monthly
}

/// <summary>
/// Entry in the export history
/// </summary>
public class ExportHistoryEntry
{
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public int ProductCount { get; set; }
    public int StoreCount { get; set; }
    public int PriceRecordCount { get; set; }
    public string? OutputPath { get; set; }
    public string? ArchivePath { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of a scheduled export operation
/// </summary>
public class ScheduledExportResult
{
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public bool Success { get; set; }
    public bool IsManual { get; set; }
    public string? OutputPath { get; set; }
    public string? ErrorMessage { get; set; }
    public StaticExportResult? ExportResult { get; set; }
}

/// <summary>
/// Event args for completed export
/// </summary>
public class ScheduledExportCompletedEventArgs : EventArgs
{
    public ScheduledExportResult Result { get; }

    public ScheduledExportCompletedEventArgs(ScheduledExportResult result)
    {
        Result = result;
    }
}

/// <summary>
/// Event args for failed export
/// </summary>
public class ScheduledExportFailedEventArgs : EventArgs
{
    public ScheduledExportResult Result { get; }
    public string ErrorMessage { get; }

    public ScheduledExportFailedEventArgs(ScheduledExportResult result, string errorMessage)
    {
        Result = result;
        ErrorMessage = errorMessage;
    }
}
