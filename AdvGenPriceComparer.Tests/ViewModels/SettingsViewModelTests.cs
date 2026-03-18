using System;
using System.IO;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.ViewModels;
using Xunit;

namespace AdvGenPriceComparer.Tests.ViewModels;

/// <summary>
/// Unit tests for SettingsViewModel database provider switching workflow
/// </summary>
public class SettingsViewModelTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _originalAppData;

    public SettingsViewModelTests()
    {
        // Create a temporary directory for test files
        _testDir = Path.Combine(Path.GetTempPath(), $"AdvGenSettingsVMTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);

        // Store original APPDATA environment variable
        _originalAppData = Environment.GetEnvironmentVariable("APPDATA") ?? string.Empty;

        // Set APPDATA to test directory for isolation
        Environment.SetEnvironmentVariable("APPDATA", _testDir);
    }

    public void Dispose()
    {
        // Restore original APPDATA
        Environment.SetEnvironmentVariable("APPDATA", _originalAppData);

        // Cleanup temporary files
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private SettingsService CreateSettingsService()
    {
        var logger = new TestLoggerService();
        return new SettingsService(logger);
    }

    private SettingsViewModel CreateViewModel(ISettingsService settingsService, IDialogService dialogService, IThemeService? themeService = null, ILocalizationService? localizationService = null)
    {
        var logger = new TestLoggerService();
        return new SettingsViewModel(settingsService, logger, dialogService, themeService ?? new TestThemeService(), localizationService ?? new TestLocalizationService());
    }

    #region Provider Change Detection Tests

    [Fact]
    public void Constructor_LoadsOriginalProviderType()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        settingsService.DatabaseProviderType = DatabaseProviderType.LiteDB;

        // Act
        var viewModel = CreateViewModel(settingsService, new TestDialogService());

        // Assert
        Assert.Equal(DatabaseProviderType.LiteDB, viewModel.DatabaseProvider);
        Assert.False(viewModel.IsProviderChanged);
        Assert.Equal(string.Empty, viewModel.ProviderChangeMessage);
    }

    [Fact]
    public void DatabaseProvider_WhenChanged_SetsIsProviderChanged()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        settingsService.DatabaseProviderType = DatabaseProviderType.LiteDB;
        var viewModel = CreateViewModel(settingsService, new TestDialogService());

        // Act
        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;

        // Assert
        Assert.True(viewModel.IsProviderChanged);
        Assert.Contains("LiteDB", viewModel.ProviderChangeMessage);
        Assert.Contains("AdvGenNoSQLServer", viewModel.ProviderChangeMessage);
        Assert.Contains("restart", viewModel.ProviderChangeMessage.ToLower());
    }

    [Fact]
    public void DatabaseProvider_WhenChangedBack_ClearsIsProviderChanged()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        settingsService.DatabaseProviderType = DatabaseProviderType.LiteDB;
        var viewModel = CreateViewModel(settingsService, new TestDialogService());

        // Act - change then change back
        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;
        viewModel.DatabaseProvider = DatabaseProviderType.LiteDB;

        // Assert
        Assert.False(viewModel.IsProviderChanged);
        Assert.Equal(string.Empty, viewModel.ProviderChangeMessage);
    }

    [Fact]
    public void IsLiteDbSelected_WhenProviderIsLiteDB_ReturnsTrue()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var viewModel = CreateViewModel(settingsService, new TestDialogService());

        // Act
        viewModel.DatabaseProvider = DatabaseProviderType.LiteDB;

        // Assert
        Assert.True(viewModel.IsLiteDbSelected);
        Assert.False(viewModel.IsNoSqlSelected);
    }

    [Fact]
    public void IsNoSqlSelected_WhenProviderIsAdvGenNoSQLServer_ReturnsTrue()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var viewModel = CreateViewModel(settingsService, new TestDialogService());

        // Act
        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;

        // Assert
        Assert.False(viewModel.IsLiteDbSelected);
        Assert.True(viewModel.IsNoSqlSelected);
    }

    #endregion

    #region CanSave Tests

    [Fact]
    public void CanSave_WithLiteDBAndValidPath_ReturnsTrue()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var viewModel = CreateViewModel(settingsService, new TestDialogService());
        viewModel.DatabaseProvider = DatabaseProviderType.LiteDB;
        viewModel.LiteDbPath = @"C:\Test\GroceryPrices.db";

        // Act & Assert
        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void CanSave_WithLiteDBAndEmptyPath_ReturnsFalse()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var viewModel = CreateViewModel(settingsService, new TestDialogService());
        viewModel.DatabaseProvider = DatabaseProviderType.LiteDB;
        viewModel.LiteDbPath = string.Empty;

        // Act & Assert
        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void CanSave_WithNoSqlAndValidHost_ReturnsTrue()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var viewModel = CreateViewModel(settingsService, new TestDialogService());
        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;
        viewModel.ServerHost = "localhost";
        viewModel.ServerPort = 5000;

        // Act & Assert
        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void CanSave_WithNoSqlAndEmptyHost_ReturnsFalse()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var viewModel = CreateViewModel(settingsService, new TestDialogService());
        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;
        viewModel.ServerHost = string.Empty;
        viewModel.ServerPort = 5000;

        // Act & Assert
        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void CanSave_WithNoSqlAndInvalidPort_ReturnsFalse()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var viewModel = CreateViewModel(settingsService, new TestDialogService());
        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;
        viewModel.ServerHost = "localhost";
        viewModel.ServerPort = 0;

        // Act & Assert
        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    #endregion

    #region Provider Switching Save Tests

    [Fact]
    public async Task SaveSettingsAsync_WithProviderChangeAndUserConfirms_SavesSettings()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        settingsService.DatabaseProviderType = DatabaseProviderType.LiteDB;
        await settingsService.SaveSettingsAsync();

        var dialogService = new TestDialogService { ConfirmResult = true };
        var viewModel = CreateViewModel(settingsService, dialogService);

        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;
        viewModel.ServerHost = "test.example.com";
        viewModel.ServerPort = 8080;

        // Act
        viewModel.SaveCommand.Execute(null);

        // Wait for async operation
        await Task.Delay(100);

        // Assert
        Assert.True(dialogService.ShowConfirmationCalled);
        Assert.Contains("database provider", dialogService.LastConfirmationMessage.ToLower());
        Assert.Contains("restart", dialogService.LastConfirmationMessage.ToLower());
    }

    [Fact]
    public void SaveSettingsAsync_WithProviderChangeAndUserCancels_RevertsProvider()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        settingsService.DatabaseProviderType = DatabaseProviderType.LiteDB;

        var dialogService = new TestDialogService { ConfirmResult = false };
        var viewModel = CreateViewModel(settingsService, dialogService);

        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        Assert.True(dialogService.ShowConfirmationCalled);
        Assert.Equal(DatabaseProviderType.LiteDB, viewModel.DatabaseProvider);
        Assert.False(viewModel.IsProviderChanged);
    }

    [Fact]
    public async Task SaveSettingsAsync_WithoutProviderChange_NoConfirmationDialog()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        settingsService.DatabaseProviderType = DatabaseProviderType.LiteDB;
        await settingsService.SaveSettingsAsync();

        var dialogService = new TestDialogService();
        var viewModel = CreateViewModel(settingsService, dialogService);

        // Make a change that doesn't affect provider
        viewModel.Culture = "zh-TW";

        // Act
        viewModel.SaveCommand.Execute(null);

        // Wait for async operation
        await Task.Delay(100);

        // Assert
        Assert.False(dialogService.ShowConfirmationCalled);
        Assert.True(dialogService.ShowSuccessCalled);
    }

    #endregion

    #region Reset Settings Tests

    [Fact]
    public void ResetSettings_WithUserConfirmation_ResetsToDefaults()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        settingsService.DatabaseProviderType = DatabaseProviderType.AdvGenNoSQLServer;
        settingsService.Culture = "zh-TW";
        settingsService.AutoCategorizationThreshold = 0.5f;

        var dialogService = new TestDialogService { ConfirmResult = true };
        var viewModel = CreateViewModel(settingsService, dialogService);

        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;
        viewModel.Culture = "zh-TW";

        // Act
        viewModel.ResetCommand.Execute(null);

        // Assert
        Assert.True(dialogService.ShowConfirmationCalled);
        Assert.Equal(DatabaseProviderType.LiteDB, viewModel.DatabaseProvider);
        Assert.Equal("en-AU", viewModel.Culture);
        Assert.Equal(0.7f, viewModel.AutoCategorizationThreshold);
    }

    [Fact]
    public void ResetSettings_WithoutUserConfirmation_KeepsCurrentValues()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var dialogService = new TestDialogService { ConfirmResult = false };
        var viewModel = CreateViewModel(settingsService, dialogService);

        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;
        viewModel.Culture = "zh-TW";

        // Act
        viewModel.ResetCommand.Execute(null);

        // Assert
        Assert.True(dialogService.ShowConfirmationCalled);
        Assert.Equal(DatabaseProviderType.AdvGenNoSQLServer, viewModel.DatabaseProvider);
        Assert.Equal("zh-TW", viewModel.Culture);
    }

    #endregion

    #region Property Change Notification Tests

    [Fact]
    public void DatabaseProvider_WhenChanged_RaisesPropertyChanged()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var viewModel = CreateViewModel(settingsService, new TestDialogService());
        var changedProperties = new System.Collections.Generic.List<string>();

        viewModel.PropertyChanged += (s, e) =>
        {
            changedProperties.Add(e.PropertyName!);
        };

        // Act
        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;

        // Assert - Verify that DatabaseProvider is among the changed properties
        Assert.Contains(nameof(SettingsViewModel.DatabaseProvider), changedProperties);
    }

    [Fact]
    public void DatabaseProvider_WhenChanged_RaisesDependentPropertyChanged()
    {
        // Arrange
        var settingsService = CreateSettingsService();
        var viewModel = CreateViewModel(settingsService, new TestDialogService());
        var changedProperties = new System.Collections.Generic.List<string>();

        viewModel.PropertyChanged += (s, e) =>
        {
            changedProperties.Add(e.PropertyName!);
        };

        // Act
        viewModel.DatabaseProvider = DatabaseProviderType.AdvGenNoSQLServer;

        // Assert
        Assert.Contains(nameof(SettingsViewModel.DatabaseProvider), changedProperties);
        Assert.Contains(nameof(SettingsViewModel.IsLiteDbSelected), changedProperties);
        Assert.Contains(nameof(SettingsViewModel.IsNoSqlSelected), changedProperties);
        Assert.Contains(nameof(SettingsViewModel.IsProviderChanged), changedProperties);
        Assert.Contains(nameof(SettingsViewModel.ProviderChangeMessage), changedProperties);
    }

    #endregion

    #region Test Helpers

    private class TestLoggerService : ILoggerService
    {
        public void LogDebug(string message) { }
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? exception = null) { }
        public void LogCritical(string message, Exception? exception = null) { }
        public string GetLogFilePath() => string.Empty;
    }

    private class TestThemeService : IThemeService
    {
        public ApplicationTheme CurrentTheme { get; private set; } = ApplicationTheme.System;
        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        public void ApplyTheme(ApplicationTheme theme)
        {
            var oldTheme = CurrentTheme;
            CurrentTheme = theme;
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs { OldTheme = oldTheme, NewTheme = theme });
        }
    }

    private class TestLocalizationService : ILocalizationService
    {
        public string CurrentCulture { get; private set; } = "en-US";
        public IReadOnlyList<Core.Interfaces.CultureInfo> AvailableCultures => new List<Core.Interfaces.CultureInfo>
        {
            new Core.Interfaces.CultureInfo("en-US", "English (US)", "English", "US"),
            new Core.Interfaces.CultureInfo("zh-CN", "Chinese (Simplified)", "Chinese (Simplified)", "CN")
        };

        public event EventHandler<CultureChangedEventArgs>? CultureChanged;

        public bool ChangeCulture(string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(cultureCode))
                return false;

            var oldCulture = CurrentCulture;
            CurrentCulture = cultureCode;
            CultureChanged?.Invoke(this, new CultureChangedEventArgs(oldCulture, cultureCode));
            return true;
        }

        public string GetString(string key) => key;
        public string GetString(string key, string cultureCode) => key;
        public string GetFormattedString(string key, params object[] args) => string.Format(key, args);
    }

    private class TestDialogService : IDialogService
    {
        public bool ConfirmResult { get; set; } = true;
        public bool ShowConfirmationCalled { get; private set; }
        public bool ShowSuccessCalled { get; private set; }
        public bool ShowInfoCalled { get; private set; }
        public bool ShowErrorCalled { get; private set; }
        public string LastConfirmationMessage { get; private set; } = string.Empty;

        public bool ShowConfirmation(string message, string title)
        {
            ShowConfirmationCalled = true;
            LastConfirmationMessage = message;
            return ConfirmResult;
        }

        public void ShowError(string message, string title = "Error")
        {
            ShowErrorCalled = true;
        }

        public void ShowInfo(string message, string title = "Information")
        {
            ShowInfoCalled = true;
        }

        public void ShowSuccess(string message, string title = "Success")
        {
            ShowSuccessCalled = true;
        }

        public void ShowWarning(string message, string title = "Warning") { }
        public void ShowComparePricesDialog(string? category = null) { }
        public SearchResult? ShowGlobalSearchDialog() => null;
        public void ShowBarcodeScannerDialog() { }
        public void ShowPriceDropNotificationsDialog() { }
        public void ShowFavoritesDialog() { }
        public void ShowDealExpirationRemindersDialog() { }
        public void ShowWeeklySpecialsDigestDialog() { }
        public void ShowShoppingListsDialog() { }
        public void ShowSettingsDialog() { }
        public void ShowMLModelManagementDialog() { }
        public void ShowPriceForecastDialog() { }
        public void ShowChatDialog() { }
        public void ShowExportDataDialog() { }
        public void ShowImportFromUrlDialog() { }
        public void ShowIllusoryDiscountDetectionDialog() { }
        public void ShowServerDataTransferDialog() { }
        public void ShowBestPricesDialog() { }
        public void ShowEditPlaceDialog(Core.Models.Place place) { }
        public void ShowTripOptimizerDialog() { }
        public void ShowPriceAlertsDialog() { }
        public void ShowWeeklySpecialsImportDialog() { }
        public void ShowCloudSyncDialog() { }
        public void ShowStaticPeerConfigDialog() { }
        public bool ShowQuestion(string title, string message) => ConfirmResult;
    }

    #endregion
}
