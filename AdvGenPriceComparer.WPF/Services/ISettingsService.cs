using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for managing application settings with persistence
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Database provider type (LiteDB or AdvGenNoSQLServer)
    /// </summary>
    DatabaseProviderType DatabaseProviderType { get; set; }

    /// <summary>
    /// Path to LiteDB database file
    /// </summary>
    string LiteDbPath { get; set; }

    /// <summary>
    /// AdvGenNoSQLServer host
    /// </summary>
    string ServerHost { get; set; }

    /// <summary>
    /// AdvGenNoSQLServer port
    /// </summary>
    int ServerPort { get; set; }

    /// <summary>
    /// API key for AdvGenNoSQLServer
    /// </summary>
    string ApiKey { get; set; }

    /// <summary>
    /// Database name on AdvGenNoSQLServer
    /// </summary>
    string DatabaseName { get; set; }

    /// <summary>
    /// Use SSL/TLS for AdvGenNoSQLServer connection
    /// </summary>
    bool UseSsl { get; set; }

    /// <summary>
    /// Default path for exporting data
    /// </summary>
    string DefaultExportPath { get; set; }

    /// <summary>
    /// Default path for importing data
    /// </summary>
    string DefaultImportPath { get; set; }

    /// <summary>
    /// Application culture/language
    /// </summary>
    string Culture { get; set; }

    /// <summary>
    /// Automatically check for updates on startup
    /// </summary>
    bool AutoCheckForUpdates { get; set; }

    /// <summary>
    /// Path to ML model file
    /// </summary>
    string MLModelPath { get; set; }

    /// <summary>
    /// Confidence threshold for auto-categorization (0.0 - 1.0)
    /// </summary>
    float AutoCategorizationThreshold { get; set; }

    /// <summary>
    /// Enable auto-categorization during import
    /// </summary>
    bool EnableAutoCategorization { get; set; }

    /// <summary>
    /// Enable price drop alerts
    /// </summary>
    bool EnablePriceDropAlerts { get; set; }

    /// <summary>
    /// Enable deal expiration alerts
    /// </summary>
    bool EnableExpirationAlerts { get; set; }

    /// <summary>
    /// Alert check interval in hours
    /// </summary>
    int AlertCheckIntervalHours { get; set; }

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    int ConnectionTimeout { get; set; }

    /// <summary>
    /// Number of retry attempts for failed connections
    /// </summary>
    int RetryCount { get; set; }

    /// <summary>
    /// Load settings from disk
    /// </summary>
    Task LoadSettingsAsync();

    /// <summary>
    /// Save settings to disk
    /// </summary>
    Task SaveSettingsAsync();

    /// <summary>
    /// Reset settings to defaults
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Event fired when settings are loaded or saved
    /// </summary>
    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;
}

/// <summary>
/// Event args for settings changed events
/// </summary>
public class SettingsChangedEventArgs : EventArgs
{
    public bool Loaded { get; init; }
    public bool Saved { get; init; }
    public bool Reset { get; init; }
}
