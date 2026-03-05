using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILoggerService _logger;
    private readonly IDialogService _dialogService;

    // Database Settings
    private DatabaseProviderType _databaseProvider;
    private string _liteDbPath = "";
    private string _serverHost = "";
    private int _serverPort;
    private string _apiKey = "";
    private string _databaseName = "";
    private bool _useSsl;

    // Import/Export Settings
    private string _defaultExportPath = "";
    private string _defaultImportPath = "";

    // General Settings
    private string _culture = "en-US";
    private bool _autoCheckForUpdates = true;
    private bool _enableAutoCategorization = true;
    private float _autoCategorizationThreshold = 0.8f;

    // Notifications
    private bool _enablePriceDropAlerts = true;
    private bool _enableExpirationAlerts = true;
    private int _alertCheckIntervalHours = 24;

    // Connection Settings
    private int _connectionTimeout = 30;
    private int _retryCount = 3;

    // View State
    private string _selectedCategory = "General";
    private ObservableCollection<string> _categories = new()
    {
        "General",
        "Database",
        "Import/Export",
        "Notifications",
        "ML",
        "About"
    };

    // Provider Switching State
    private DatabaseProviderType _originalProviderType;

    public SettingsViewModel(
        ISettingsService settingsService,
        ILoggerService logger,
        IDialogService dialogService)
    {
        _settingsService = settingsService;
        _logger = logger;
        _dialogService = dialogService;

        SaveCommand = new RelayCommand(SaveSettingsAsync, CanSave);
        ResetCommand = new RelayCommand(ResetSettings);
        BrowseLiteDbPathCommand = new RelayCommand(BrowseLiteDbPath);
        BrowseExportPathCommand = new RelayCommand(BrowseExportPath);
        BrowseImportPathCommand = new RelayCommand(BrowseImportPath);
        TestConnectionCommand = new RelayCommand(TestConnection);
        ExportDataCommand = new RelayCommand(ShowExportDataDialog);

        LoadSettings();
    }

    #region Properties

    public ObservableCollection<string> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    // General Settings
    public string Culture
    {
        get => _culture;
        set
        {
            if (SetProperty(ref _culture, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public List<string> AvailableCultures => new() { "en-US", "en-GB", "en-AU", "zh-CN", "zh-TW", "ja-JP" };

    public bool AutoCheckForUpdates
    {
        get => _autoCheckForUpdates;
        set
        {
            if (SetProperty(ref _autoCheckForUpdates, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public bool EnableAutoCategorization
    {
        get => _enableAutoCategorization;
        set
        {
            if (SetProperty(ref _enableAutoCategorization, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public float AutoCategorizationThreshold
    {
        get => _autoCategorizationThreshold;
        set
        {
            if (SetProperty(ref _autoCategorizationThreshold, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    // Database Settings
    public DatabaseProviderType DatabaseProvider
    {
        get => _databaseProvider;
        set
        {
            if (SetProperty(ref _databaseProvider, value))
            {
                OnPropertyChanged(nameof(IsLiteDbSelected));
                OnPropertyChanged(nameof(IsNoSqlSelected));
                OnPropertyChanged(nameof(IsProviderChanged));
                OnPropertyChanged(nameof(ProviderChangeMessage));
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Indicates whether the database provider has been changed from its original value.
    /// When true, a restart is required for the change to take effect.
    /// </summary>
    public bool IsProviderChanged => _databaseProvider != _originalProviderType;

    /// <summary>
    /// Returns a message indicating the provider change and restart requirement.
    /// </summary>
    public string ProviderChangeMessage => IsProviderChanged 
        ? $"Database provider changed from {_originalProviderType} to {_databaseProvider}. Application restart is required for this change to take effect."
        : string.Empty;

    public List<DatabaseProviderType> AvailableDatabaseProviders => new() { DatabaseProviderType.LiteDB, DatabaseProviderType.AdvGenNoSQLServer };

    public string LiteDbPath
    {
        get => _liteDbPath;
        set
        {
            if (SetProperty(ref _liteDbPath, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public string ServerHost
    {
        get => _serverHost;
        set
        {
            if (SetProperty(ref _serverHost, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public int ServerPort
    {
        get => _serverPort;
        set
        {
            if (SetProperty(ref _serverPort, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public string ApiKey
    {
        get => _apiKey;
        set
        {
            if (SetProperty(ref _apiKey, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public string DatabaseName
    {
        get => _databaseName;
        set
        {
            if (SetProperty(ref _databaseName, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public bool UseSsl
    {
        get => _useSsl;
        set
        {
            if (SetProperty(ref _useSsl, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public bool IsLiteDbSelected => DatabaseProvider == DatabaseProviderType.LiteDB;
    public bool IsNoSqlSelected => DatabaseProvider == DatabaseProviderType.AdvGenNoSQLServer;

    // Import/Export Settings
    public string DefaultExportPath
    {
        get => _defaultExportPath;
        set
        {
            if (SetProperty(ref _defaultExportPath, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public string DefaultImportPath
    {
        get => _defaultImportPath;
        set
        {
            if (SetProperty(ref _defaultImportPath, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    // Notification Settings
    public bool EnablePriceDropAlerts
    {
        get => _enablePriceDropAlerts;
        set
        {
            if (SetProperty(ref _enablePriceDropAlerts, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public bool EnableExpirationAlerts
    {
        get => _enableExpirationAlerts;
        set
        {
            if (SetProperty(ref _enableExpirationAlerts, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public int AlertCheckIntervalHours
    {
        get => _alertCheckIntervalHours;
        set
        {
            if (SetProperty(ref _alertCheckIntervalHours, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    // Connection Settings
    public int ConnectionTimeout
    {
        get => _connectionTimeout;
        set
        {
            if (SetProperty(ref _connectionTimeout, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public int RetryCount
    {
        get => _retryCount;
        set
        {
            if (SetProperty(ref _retryCount, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    // About Information
    public string ApplicationVersion => "1.0.0";
    public string BuildDate => "February 2026";
    public string DotNetVersion => ".NET 9.0";
    public string DatabaseVersion => "LiteDB 5.x";

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand BrowseLiteDbPathCommand { get; }
    public ICommand BrowseExportPathCommand { get; }
    public ICommand BrowseImportPathCommand { get; }
    public ICommand TestConnectionCommand { get; }
    public ICommand ExportDataCommand { get; }

    #endregion

    #region Methods

    private void LoadSettings()
    {
        try
        {
            // Copy settings from service to local properties
            DatabaseProvider = _settingsService.DatabaseProviderType;
            
            // Store the original provider type to detect changes
            _originalProviderType = _settingsService.DatabaseProviderType;
            OnPropertyChanged(nameof(IsProviderChanged));
            OnPropertyChanged(nameof(ProviderChangeMessage));
            
            LiteDbPath = _settingsService.LiteDbPath ?? "";
            ServerHost = _settingsService.ServerHost ?? "";
            ServerPort = _settingsService.ServerPort;
            ApiKey = _settingsService.ApiKey ?? "";
            DatabaseName = _settingsService.DatabaseName ?? "";
            UseSsl = _settingsService.UseSsl;
            
            DefaultExportPath = _settingsService.DefaultExportPath ?? "";
            DefaultImportPath = _settingsService.DefaultImportPath ?? "";
            
            Culture = _settingsService.Culture ?? "en-US";
            AutoCheckForUpdates = _settingsService.AutoCheckForUpdates;
            EnableAutoCategorization = _settingsService.EnableAutoCategorization;
            AutoCategorizationThreshold = _settingsService.AutoCategorizationThreshold;
            
            EnablePriceDropAlerts = _settingsService.EnablePriceDropAlerts;
            EnableExpirationAlerts = _settingsService.EnableExpirationAlerts;
            AlertCheckIntervalHours = _settingsService.AlertCheckIntervalHours;
            
            ConnectionTimeout = _settingsService.ConnectionTimeout;
            RetryCount = _settingsService.RetryCount;

            _logger.LogInfo("Settings loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load settings", ex);
            _dialogService.ShowError("Failed to load settings. Default values will be used.");
        }
    }

    private bool CanSave()
    {
        if (DatabaseProvider == DatabaseProviderType.LiteDB)
        {
            return !string.IsNullOrWhiteSpace(LiteDbPath);
        }
        else
        {
            return !string.IsNullOrWhiteSpace(ServerHost) && ServerPort > 0;
        }
    }

    private async void SaveSettingsAsync()
    {
        try
        {
            // Check if provider type has changed
            bool providerChanged = DatabaseProvider != _originalProviderType;
            
            if (providerChanged)
            {
                // Show restart confirmation dialog
                var restartConfirmed = _dialogService.ShowConfirmation(
                    $"You have changed the database provider from {_originalProviderType} to {DatabaseProvider}.\n\n" +
                    "This change requires the application to restart to take effect.\n\n" +
                    "All current data connections will be closed and re-initialized with the new provider.\n\n" +
                    "Do you want to save and restart now?",
                    "Database Provider Change - Restart Required");

                if (!restartConfirmed)
                {
                    // User cancelled, revert provider change
                    DatabaseProvider = _originalProviderType;
                    _logger.LogInfo("User cancelled provider change - reverted to original provider");
                    return;
                }
            }

            // Copy local properties to service
            _settingsService.DatabaseProviderType = DatabaseProvider;
            _settingsService.LiteDbPath = LiteDbPath;
            _settingsService.ServerHost = ServerHost;
            _settingsService.ServerPort = ServerPort;
            _settingsService.ApiKey = ApiKey;
            _settingsService.DatabaseName = DatabaseName;
            _settingsService.UseSsl = UseSsl;
            
            _settingsService.DefaultExportPath = DefaultExportPath;
            _settingsService.DefaultImportPath = DefaultImportPath;
            
            _settingsService.Culture = Culture;
            _settingsService.AutoCheckForUpdates = AutoCheckForUpdates;
            _settingsService.EnableAutoCategorization = EnableAutoCategorization;
            _settingsService.AutoCategorizationThreshold = AutoCategorizationThreshold;
            
            _settingsService.EnablePriceDropAlerts = EnablePriceDropAlerts;
            _settingsService.EnableExpirationAlerts = EnableExpirationAlerts;
            _settingsService.AlertCheckIntervalHours = AlertCheckIntervalHours;
            
            _settingsService.ConnectionTimeout = ConnectionTimeout;
            _settingsService.RetryCount = RetryCount;

            await _settingsService.SaveSettingsAsync();
            _logger.LogInfo("Settings saved successfully");

            if (providerChanged)
            {
                _logger.LogInfo($"Database provider changed from {_originalProviderType} to {DatabaseProvider}. Initiating restart...");
                _dialogService.ShowInfo("Settings saved. The application will now restart to apply the database provider change.");
                
                // Trigger application restart
                RestartApplication();
            }
            else
            {
                _dialogService.ShowSuccess("Settings saved successfully!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save settings", ex);
            _dialogService.ShowError($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Restarts the application to apply database provider changes.
    /// </summary>
    private void RestartApplication()
    {
        try
        {
            // Get the executable path
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName 
                ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
            
            _logger.LogInfo($"Restarting application: {exePath}");
            
            // Start new instance
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            };
            
            System.Diagnostics.Process.Start(startInfo);
            
            // Shutdown current instance
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to restart application", ex);
            _dialogService.ShowError(
                "Settings were saved but the application could not restart automatically.\n\n" +
                "Please restart the application manually to apply the database provider change.");
        }
    }

    private void ResetSettings()
    {
        var result = _dialogService.ShowConfirmation(
            "Are you sure you want to reset all settings to default values?",
            "Reset Settings");

        if (result)
        {
            _settingsService.ResetToDefaults();
            LoadSettings();
            _logger.LogInfo("Settings reset to defaults");
            _dialogService.ShowInfo("Settings reset to defaults.");
        }
    }

    private void BrowseLiteDbPath()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "LiteDB files (*.db)|*.db|All files (*.*)|*.*",
            DefaultExt = "db",
            FileName = "GroceryPrices.db"
        };

        if (dialog.ShowDialog() == true)
        {
            LiteDbPath = dialog.FileName;
        }
    }

    private void BrowseExportPath()
    {
        // Use OpenFileDialog with a folder selection workaround (selecting a file in the folder)
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Export Folder (select any file in the folder)",
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Select Folder"
        };

        if (dialog.ShowDialog() == true)
        {
            var folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(folderPath))
            {
                DefaultExportPath = folderPath;
            }
        }
    }

    private void BrowseImportPath()
    {
        // Use OpenFileDialog with a folder selection workaround (selecting a file in the folder)
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Import Folder (select any file in the folder)",
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Select Folder"
        };

        if (dialog.ShowDialog() == true)
        {
            var folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(folderPath))
            {
                DefaultImportPath = folderPath;
            }
        }
    }

    private void TestConnection()
    {
        _dialogService.ShowInfo("Testing connection... This feature will be implemented when database provider switching is fully supported.");
    }

    private void ShowExportDataDialog()
    {
        _dialogService.ShowExportDataDialog();
    }

    #endregion
}
