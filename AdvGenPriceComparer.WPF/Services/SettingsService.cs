using System.IO;
using System.Text.Json;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Settings service that persists configuration to JSON file in AppData
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private readonly ILoggerService _logger;
    private readonly object _lockObject = new object();

    // Default values
    private const DatabaseProviderType DefaultProviderType = DatabaseProviderType.LiteDB;
    private const string DefaultCulture = "en-AU";
    private const float DefaultAutoCategorizationThreshold = 0.7f;
    private const int DefaultAlertCheckIntervalHours = 24;
    private const int DefaultConnectionTimeout = 30;
    private const int DefaultRetryCount = 3;

    // Backing fields
    private DatabaseProviderType _databaseProviderType = DefaultProviderType;
    private string _liteDbPath = string.Empty;
    private string _serverHost = "localhost";
    private int _serverPort = 5000;
    private string _apiKey = string.Empty;
    private string _databaseName = "GroceryPrices";
    private bool _useSsl = true;
    private string _defaultExportPath = string.Empty;
    private string _defaultImportPath = string.Empty;
    private string _culture = DefaultCulture;
    private bool _autoCheckForUpdates = true;
    private string _mlModelPath = string.Empty;
    private float _autoCategorizationThreshold = DefaultAutoCategorizationThreshold;
    private bool _enableAutoCategorization = true;
    private bool _enablePriceDropAlerts = true;
    private bool _enableExpirationAlerts = true;
    private int _alertCheckIntervalHours = DefaultAlertCheckIntervalHours;
    private int _connectionTimeout = DefaultConnectionTimeout;
    private int _retryCount = DefaultRetryCount;

    public DatabaseProviderType DatabaseProviderType
    {
        get => _databaseProviderType;
        set
        {
            _databaseProviderType = value;
            _logger?.LogInfo($"Settings: DatabaseProviderType changed to {value}");
        }
    }

    public string LiteDbPath
    {
        get => _liteDbPath;
        set
        {
            _liteDbPath = value;
            _logger?.LogDebug($"Settings: LiteDbPath changed to {value}");
        }
    }

    public string ServerHost
    {
        get => _serverHost;
        set
        {
            _serverHost = value;
            _logger?.LogDebug($"Settings: ServerHost changed to {value}");
        }
    }

    public int ServerPort
    {
        get => _serverPort;
        set
        {
            _serverPort = value;
            _logger?.LogDebug($"Settings: ServerPort changed to {value}");
        }
    }

    public string ApiKey
    {
        get => _apiKey;
        set
        {
            _apiKey = value;
            _logger?.LogDebug("Settings: ApiKey updated");
        }
    }

    public string DatabaseName
    {
        get => _databaseName;
        set
        {
            _databaseName = value;
            _logger?.LogDebug($"Settings: DatabaseName changed to {value}");
        }
    }

    public bool UseSsl
    {
        get => _useSsl;
        set
        {
            _useSsl = value;
            _logger?.LogDebug($"Settings: UseSsl changed to {value}");
        }
    }

    public string DefaultExportPath
    {
        get => _defaultExportPath;
        set
        {
            _defaultExportPath = value;
            _logger?.LogDebug($"Settings: DefaultExportPath changed to {value}");
        }
    }

    public string DefaultImportPath
    {
        get => _defaultImportPath;
        set
        {
            _defaultImportPath = value;
            _logger?.LogDebug($"Settings: DefaultImportPath changed to {value}");
        }
    }

    public string Culture
    {
        get => _culture;
        set
        {
            _culture = value;
            _logger?.LogInfo($"Settings: Culture changed to {value}");
        }
    }

    public bool AutoCheckForUpdates
    {
        get => _autoCheckForUpdates;
        set
        {
            _autoCheckForUpdates = value;
            _logger?.LogDebug($"Settings: AutoCheckForUpdates changed to {value}");
        }
    }

    public string MLModelPath
    {
        get => _mlModelPath;
        set
        {
            _mlModelPath = value;
            _logger?.LogDebug($"Settings: MLModelPath changed to {value}");
        }
    }

    public float AutoCategorizationThreshold
    {
        get => _autoCategorizationThreshold;
        set
        {
            _autoCategorizationThreshold = Math.Clamp(value, 0.0f, 1.0f);
            _logger?.LogDebug($"Settings: AutoCategorizationThreshold changed to {_autoCategorizationThreshold}");
        }
    }

    public bool EnableAutoCategorization
    {
        get => _enableAutoCategorization;
        set
        {
            _enableAutoCategorization = value;
            _logger?.LogDebug($"Settings: EnableAutoCategorization changed to {value}");
        }
    }

    public bool EnablePriceDropAlerts
    {
        get => _enablePriceDropAlerts;
        set
        {
            _enablePriceDropAlerts = value;
            _logger?.LogDebug($"Settings: EnablePriceDropAlerts changed to {value}");
        }
    }

    public bool EnableExpirationAlerts
    {
        get => _enableExpirationAlerts;
        set
        {
            _enableExpirationAlerts = value;
            _logger?.LogDebug($"Settings: EnableExpirationAlerts changed to {value}");
        }
    }

    public int AlertCheckIntervalHours
    {
        get => _alertCheckIntervalHours;
        set
        {
            _alertCheckIntervalHours = Math.Max(1, value);
            _logger?.LogDebug($"Settings: AlertCheckIntervalHours changed to {_alertCheckIntervalHours}");
        }
    }

    public int ConnectionTimeout
    {
        get => _connectionTimeout;
        set
        {
            _connectionTimeout = Math.Max(1, value);
            _logger?.LogDebug($"Settings: ConnectionTimeout changed to {_connectionTimeout}");
        }
    }

    public int RetryCount
    {
        get => _retryCount;
        set
        {
            _retryCount = Math.Max(0, value);
            _logger?.LogDebug($"Settings: RetryCount changed to {_retryCount}");
        }
    }

    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public SettingsService(ILoggerService logger)
    {
        _logger = logger;

        // Set default paths
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer");

        _settingsPath = Path.Combine(appDataPath, "settings.json");
        _liteDbPath = Path.Combine(appDataPath, "GroceryPrices.db");
        _mlModelPath = Path.Combine(appDataPath, "MLModels", "category_model.zip");

        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _defaultExportPath = documentsPath;
        _defaultImportPath = documentsPath;

        _logger.LogInfo("SettingsService initialized");
    }

    /// <summary>
    /// Load settings from JSON file
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                _logger.LogInfo("Settings file not found, using defaults");
                await SaveSettingsAsync(); // Create default settings file
                return;
            }

            var json = await File.ReadAllTextAsync(_settingsPath);
            var settingsData = JsonSerializer.Deserialize<SettingsData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (settingsData != null)
            {
                ApplySettings(settingsData);
                _logger.LogInfo("Settings loaded successfully");
            }

            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs { Loaded = true });
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load settings, using defaults", ex);
            ResetToDefaults();
        }
    }

    /// <summary>
    /// Save settings to JSON file
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        try
        {
            var settingsData = CreateSettingsData();

            lock (_lockObject)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(settingsData, options);
            await File.WriteAllTextAsync(_settingsPath, json);

            _logger.LogInfo("Settings saved successfully");
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs { Saved = true });
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save settings", ex);
            throw;
        }
    }

    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        _databaseProviderType = DefaultProviderType;
        _serverHost = "localhost";
        _serverPort = 5000;
        _apiKey = string.Empty;
        _databaseName = "GroceryPrices";
        _useSsl = true;
        _culture = DefaultCulture;
        _autoCheckForUpdates = true;
        _autoCategorizationThreshold = DefaultAutoCategorizationThreshold;
        _enableAutoCategorization = true;
        _enablePriceDropAlerts = true;
        _enableExpirationAlerts = true;
        _alertCheckIntervalHours = DefaultAlertCheckIntervalHours;
        _connectionTimeout = DefaultConnectionTimeout;
        _retryCount = DefaultRetryCount;

        // Reset paths
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer");

        _liteDbPath = Path.Combine(appDataPath, "GroceryPrices.db");
        _mlModelPath = Path.Combine(appDataPath, "MLModels", "category_model.zip");

        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _defaultExportPath = documentsPath;
        _defaultImportPath = documentsPath;

        _logger.LogInfo("Settings reset to defaults");
        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs { Reset = true });
    }

    /// <summary>
    /// Get the settings file path
    /// </summary>
    public string GetSettingsFilePath() => _settingsPath;

    private void ApplySettings(SettingsData data)
    {
        _databaseProviderType = data.DatabaseProviderType;
        _liteDbPath = data.LiteDbPath ?? _liteDbPath;
        _serverHost = data.ServerHost ?? _serverHost;
        _serverPort = data.ServerPort;
        _apiKey = data.ApiKey ?? _apiKey;
        _databaseName = data.DatabaseName ?? _databaseName;
        _useSsl = data.UseSsl;
        _defaultExportPath = data.DefaultExportPath ?? _defaultExportPath;
        _defaultImportPath = data.DefaultImportPath ?? _defaultImportPath;
        _culture = data.Culture ?? _culture;
        _autoCheckForUpdates = data.AutoCheckForUpdates;
        _mlModelPath = data.MLModelPath ?? _mlModelPath;
        _autoCategorizationThreshold = data.AutoCategorizationThreshold;
        _enableAutoCategorization = data.EnableAutoCategorization;
        _enablePriceDropAlerts = data.EnablePriceDropAlerts;
        _enableExpirationAlerts = data.EnableExpirationAlerts;
        _alertCheckIntervalHours = data.AlertCheckIntervalHours;
        _connectionTimeout = data.ConnectionTimeout;
        _retryCount = data.RetryCount;
    }

    private SettingsData CreateSettingsData()
    {
        return new SettingsData
        {
            DatabaseProviderType = _databaseProviderType,
            LiteDbPath = _liteDbPath,
            ServerHost = _serverHost,
            ServerPort = _serverPort,
            ApiKey = _apiKey,
            DatabaseName = _databaseName,
            UseSsl = _useSsl,
            DefaultExportPath = _defaultExportPath,
            DefaultImportPath = _defaultImportPath,
            Culture = _culture,
            AutoCheckForUpdates = _autoCheckForUpdates,
            MLModelPath = _mlModelPath,
            AutoCategorizationThreshold = _autoCategorizationThreshold,
            EnableAutoCategorization = _enableAutoCategorization,
            EnablePriceDropAlerts = _enablePriceDropAlerts,
            EnableExpirationAlerts = _enableExpirationAlerts,
            AlertCheckIntervalHours = _alertCheckIntervalHours,
            ConnectionTimeout = _connectionTimeout,
            RetryCount = _retryCount
        };
    }

    /// <summary>
    /// Internal DTO for JSON serialization
    /// </summary>
    private class SettingsData
    {
        public DatabaseProviderType DatabaseProviderType { get; set; }
        public string LiteDbPath { get; set; } = string.Empty;
        public string ServerHost { get; set; } = "localhost";
        public int ServerPort { get; set; } = 5000;
        public string ApiKey { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = "GroceryPrices";
        public bool UseSsl { get; set; } = true;
        public string DefaultExportPath { get; set; } = string.Empty;
        public string DefaultImportPath { get; set; } = string.Empty;
        public string Culture { get; set; } = "en-AU";
        public bool AutoCheckForUpdates { get; set; } = true;
        public string MLModelPath { get; set; } = string.Empty;
        public float AutoCategorizationThreshold { get; set; } = 0.7f;
        public bool EnableAutoCategorization { get; set; } = true;
        public bool EnablePriceDropAlerts { get; set; } = true;
        public bool EnableExpirationAlerts { get; set; } = true;
        public int AlertCheckIntervalHours { get; set; } = 24;
        public int ConnectionTimeout { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
    }
}
