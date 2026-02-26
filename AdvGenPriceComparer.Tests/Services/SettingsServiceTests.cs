using System;
using System.IO;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Services;
using Xunit;

namespace AdvGenPriceComparer.Tests.Services
{
    // Collection to prevent parallel execution of these tests
    [CollectionDefinition("SettingsServiceTests", DisableParallelization = true)]
    public class SettingsServiceCollection : ICollectionFixture<object>
    {
    }

    [Collection("SettingsServiceTests")]
    public class SettingsServiceTests : IDisposable
    {
        private readonly string _testDir;
        private readonly string _originalAppData;

        public SettingsServiceTests()
        {
            // Create a temporary directory for test files
            _testDir = Path.Combine(Path.GetTempPath(), $"AdvGenSettingsTests_{Guid.NewGuid()}");
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

        private SettingsService CreateService()
        {
            var logger = new TestLoggerService();
            return new SettingsService(logger);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesDefaultValues()
        {
            // Act
            var service = CreateService();

            // Assert
            Assert.Equal(DatabaseProviderType.LiteDB, service.DatabaseProviderType);
            Assert.Equal("localhost", service.ServerHost);
            Assert.Equal(5000, service.ServerPort);
            Assert.Equal("GroceryPrices", service.DatabaseName);
            Assert.True(service.UseSsl);
            Assert.Equal("en-AU", service.Culture);
            Assert.True(service.AutoCheckForUpdates);
            Assert.Equal(0.7f, service.AutoCategorizationThreshold);
            Assert.True(service.EnableAutoCategorization);
            Assert.True(service.EnablePriceDropAlerts);
            Assert.True(service.EnableExpirationAlerts);
            Assert.Equal(24, service.AlertCheckIntervalHours);
            Assert.Equal(30, service.ConnectionTimeout);
            Assert.Equal(3, service.RetryCount);
        }

        [Fact]
        public void Constructor_SetsDefaultPaths()
        {
            // Act
            var service = CreateService();

            // Assert
            var appDataPath = Path.Combine(_testDir, "AdvGenPriceComparer");
            Assert.Contains(appDataPath, service.LiteDbPath);
            Assert.Contains("GroceryPrices.db", service.LiteDbPath);
            Assert.Contains(appDataPath, service.MLModelPath);
            Assert.Contains("category_model.zip", service.MLModelPath);
            Assert.Equal(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), service.DefaultExportPath);
            Assert.Equal(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), service.DefaultImportPath);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void DatabaseProviderType_CanChangeValue()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.DatabaseProviderType = DatabaseProviderType.AdvGenNoSQLServer;

            // Assert
            Assert.Equal(DatabaseProviderType.AdvGenNoSQLServer, service.DatabaseProviderType);
        }

        [Fact]
        public void AutoCategorizationThreshold_ClampsToValidRange()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert - Below minimum
            service.AutoCategorizationThreshold = -0.5f;
            Assert.Equal(0.0f, service.AutoCategorizationThreshold);

            // Act & Assert - Above maximum
            service.AutoCategorizationThreshold = 1.5f;
            Assert.Equal(1.0f, service.AutoCategorizationThreshold);

            // Act & Assert - Valid range
            service.AutoCategorizationThreshold = 0.5f;
            Assert.Equal(0.5f, service.AutoCategorizationThreshold);
        }

        [Fact]
        public void AlertCheckIntervalHours_EnforcesMinimumValue()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.AlertCheckIntervalHours = 0;

            // Assert
            Assert.Equal(1, service.AlertCheckIntervalHours);

            // Act
            service.AlertCheckIntervalHours = -5;

            // Assert
            Assert.Equal(1, service.AlertCheckIntervalHours);
        }

        [Fact]
        public void ConnectionTimeout_EnforcesMinimumValue()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.ConnectionTimeout = 0;

            // Assert
            Assert.Equal(1, service.ConnectionTimeout);

            // Act
            service.ConnectionTimeout = -10;

            // Assert
            Assert.Equal(1, service.ConnectionTimeout);
        }

        [Fact]
        public void RetryCount_AllowsZero()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.RetryCount = 0;

            // Assert
            Assert.Equal(0, service.RetryCount);

            // Act
            service.RetryCount = -5;

            // Assert
            Assert.Equal(0, service.RetryCount);
        }

        #endregion

        #region LoadSettingsAsync Tests

        [Fact]
        public async Task LoadSettingsAsync_FileNotFound_CreatesDefaultSettingsFile()
        {
            // Arrange
            var service = CreateService();
            var settingsPath = Path.Combine(_testDir, "AdvGenPriceComparer", "settings.json");

            // Ensure file doesn't exist
            if (File.Exists(settingsPath))
                File.Delete(settingsPath);

            // Act
            await service.LoadSettingsAsync();

            // Assert
            Assert.True(File.Exists(settingsPath), "Settings file should be created with defaults");
        }

        [Fact]
        public async Task LoadSettingsAsync_ValidFile_LoadsAllSettings()
        {
            // Arrange
            var service = CreateService();
            var settingsPath = Path.Combine(_testDir, "AdvGenPriceComparer", "settings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);

            var testSettings = @"{
                ""databaseProviderType"": 1,
                ""liteDbPath"": ""/custom/path/db.db"",
                ""serverHost"": ""test.example.com"",
                ""serverPort"": 8080,
                ""apiKey"": ""test-api-key"",
                ""databaseName"": ""TestDB"",
                ""useSsl"": false,
                ""defaultExportPath"": ""/export/path"",
                ""defaultImportPath"": ""/import/path"",
                ""culture"": ""zh-TW"",
                ""autoCheckForUpdates"": false,
                ""mlModelPath"": ""/ml/model.zip"",
                ""autoCategorizationThreshold"": 0.85,
                ""enableAutoCategorization"": false,
                ""enablePriceDropAlerts"": false,
                ""enableExpirationAlerts"": false,
                ""alertCheckIntervalHours"": 12,
                ""connectionTimeout"": 60,
                ""retryCount"": 5
            }";

            File.WriteAllText(settingsPath, testSettings);

            // Act
            await service.LoadSettingsAsync();

            // Assert
            Assert.Equal(DatabaseProviderType.AdvGenNoSQLServer, service.DatabaseProviderType);
            Assert.Equal("/custom/path/db.db", service.LiteDbPath);
            Assert.Equal("test.example.com", service.ServerHost);
            Assert.Equal(8080, service.ServerPort);
            Assert.Equal("test-api-key", service.ApiKey);
            Assert.Equal("TestDB", service.DatabaseName);
            Assert.False(service.UseSsl);
            Assert.Equal("/export/path", service.DefaultExportPath);
            Assert.Equal("/import/path", service.DefaultImportPath);
            Assert.Equal("zh-TW", service.Culture);
            Assert.False(service.AutoCheckForUpdates);
            Assert.Equal("/ml/model.zip", service.MLModelPath);
            Assert.Equal(0.85f, service.AutoCategorizationThreshold);
            Assert.False(service.EnableAutoCategorization);
            Assert.False(service.EnablePriceDropAlerts);
            Assert.False(service.EnableExpirationAlerts);
            Assert.Equal(12, service.AlertCheckIntervalHours);
            Assert.Equal(60, service.ConnectionTimeout);
            Assert.Equal(5, service.RetryCount);
        }

        [Fact]
        public async Task LoadSettingsAsync_InvalidJson_UsesDefaults()
        {
            // Arrange
            var service = CreateService();
            var settingsPath = Path.Combine(_testDir, "AdvGenPriceComparer", "settings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
            File.WriteAllText(settingsPath, "invalid json content");

            // Act
            await service.LoadSettingsAsync();

            // Assert - Should use defaults after failed load
            Assert.Equal(DatabaseProviderType.LiteDB, service.DatabaseProviderType);
            Assert.Equal("en-AU", service.Culture);
        }

        [Fact]
        public async Task LoadSettingsAsync_EmptyFile_UsesDefaults()
        {
            // Arrange
            var service = CreateService();
            var settingsPath = Path.Combine(_testDir, "AdvGenPriceComparer", "settings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
            File.WriteAllText(settingsPath, "");

            // Act
            await service.LoadSettingsAsync();

            // Assert
            Assert.Equal(DatabaseProviderType.LiteDB, service.DatabaseProviderType);
            Assert.Equal("en-AU", service.Culture);
        }

        #endregion

        #region SaveSettingsAsync Tests

        [Fact]
        public async Task SaveSettingsAsync_CreatesFileWithCorrectContent()
        {
            // Arrange
            var service = CreateService();
            service.DatabaseProviderType = DatabaseProviderType.AdvGenNoSQLServer;
            service.ServerHost = "saved.example.com";
            service.ServerPort = 9090;

            // Act
            await service.SaveSettingsAsync();

            // Assert
            var settingsPath = Path.Combine(_testDir, "AdvGenPriceComparer", "settings.json");
            Assert.True(File.Exists(settingsPath), "Settings file should be created");

            var json = File.ReadAllText(settingsPath);
            Assert.Contains("\"databaseProviderType\": 1", json);
            Assert.Contains("\"serverHost\": \"saved.example.com\"", json);
            Assert.Contains("\"serverPort\": 9090", json);
        }

        [Fact]
        public async Task SaveSettingsAsync_UsesCamelCase()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.SaveSettingsAsync();

            // Assert
            var settingsPath = Path.Combine(_testDir, "AdvGenPriceComparer", "settings.json");
            var json = File.ReadAllText(settingsPath);

            Assert.Contains("\"databaseProviderType\"", json);
            Assert.Contains("\"liteDbPath\"", json);
            Assert.Contains("\"serverHost\"", json);
            Assert.Contains("\"autoCategorizationThreshold\"", json);
            Assert.Contains("\"enableAutoCategorization\"", json);
        }

        [Fact]
        public async Task SaveSettingsAsync_FileIsIndented()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.SaveSettingsAsync();

            // Assert
            var settingsPath = Path.Combine(_testDir, "AdvGenPriceComparer", "settings.json");
            var json = File.ReadAllText(settingsPath);

            Assert.Contains("{\n", json); // Should have newlines
            Assert.Contains("  \"", json); // Should have indentation
        }

        [Fact]
        public async Task SaveSettingsAsync_PreservesAllSettings()
        {
            // Arrange
            var service = CreateService();
            service.DatabaseProviderType = DatabaseProviderType.AdvGenNoSQLServer;
            service.LiteDbPath = "/custom/db.db";
            service.ServerHost = "host.example.com";
            service.ServerPort = 8080;
            service.ApiKey = "secret-key";
            service.DatabaseName = "MyDB";
            service.UseSsl = false;
            service.DefaultExportPath = "/export";
            service.DefaultImportPath = "/import";
            service.Culture = "zh-CN";
            service.AutoCheckForUpdates = false;
            service.MLModelPath = "/ml/model.zip";
            service.AutoCategorizationThreshold = 0.9f;
            service.EnableAutoCategorization = false;
            service.EnablePriceDropAlerts = false;
            service.EnableExpirationAlerts = false;
            service.AlertCheckIntervalHours = 6;
            service.ConnectionTimeout = 45;
            service.RetryCount = 2;

            // Act
            await service.SaveSettingsAsync();

            // Assert - Load into new service instance
            var newService = CreateService();
            await newService.LoadSettingsAsync();

            Assert.Equal(DatabaseProviderType.AdvGenNoSQLServer, newService.DatabaseProviderType);
            Assert.Equal("/custom/db.db", newService.LiteDbPath);
            Assert.Equal("host.example.com", newService.ServerHost);
            Assert.Equal(8080, newService.ServerPort);
            Assert.Equal("secret-key", newService.ApiKey);
            Assert.Equal("MyDB", newService.DatabaseName);
            Assert.False(newService.UseSsl);
            Assert.Equal("/export", newService.DefaultExportPath);
            Assert.Equal("/import", newService.DefaultImportPath);
            Assert.Equal("zh-CN", newService.Culture);
            Assert.False(newService.AutoCheckForUpdates);
            Assert.Equal("/ml/model.zip", newService.MLModelPath);
            Assert.Equal(0.9f, newService.AutoCategorizationThreshold);
            Assert.False(newService.EnableAutoCategorization);
            Assert.False(newService.EnablePriceDropAlerts);
            Assert.False(newService.EnableExpirationAlerts);
            Assert.Equal(6, newService.AlertCheckIntervalHours);
            Assert.Equal(45, newService.ConnectionTimeout);
            Assert.Equal(2, newService.RetryCount);
        }

        #endregion

        #region ResetToDefaults Tests

        [Fact]
        public void ResetToDefaults_RestoresAllDefaultValues()
        {
            // Arrange
            var service = CreateService();
            service.DatabaseProviderType = DatabaseProviderType.AdvGenNoSQLServer;
            service.ServerHost = "changed.example.com";
            service.ServerPort = 9999;
            service.Culture = "fr-FR";
            service.AutoCategorizationThreshold = 0.5f;

            // Act
            service.ResetToDefaults();

            // Assert
            Assert.Equal(DatabaseProviderType.LiteDB, service.DatabaseProviderType);
            Assert.Equal("localhost", service.ServerHost);
            Assert.Equal(5000, service.ServerPort);
            Assert.Equal("GroceryPrices", service.DatabaseName);
            Assert.True(service.UseSsl);
            Assert.Equal("en-AU", service.Culture);
            Assert.True(service.AutoCheckForUpdates);
            Assert.Equal(0.7f, service.AutoCategorizationThreshold);
            Assert.True(service.EnableAutoCategorization);
            Assert.True(service.EnablePriceDropAlerts);
            Assert.True(service.EnableExpirationAlerts);
            Assert.Equal(24, service.AlertCheckIntervalHours);
            Assert.Equal(30, service.ConnectionTimeout);
            Assert.Equal(3, service.RetryCount);
        }

        [Fact]
        public void ResetToDefaults_RestoresDefaultPaths()
        {
            // Arrange
            var service = CreateService();
            service.LiteDbPath = "/changed/db.db";
            service.MLModelPath = "/changed/model.zip";
            service.DefaultExportPath = "/changed/export";
            service.DefaultImportPath = "/changed/import";

            // Act
            service.ResetToDefaults();

            // Assert
            var appDataPath = Path.Combine(_testDir, "AdvGenPriceComparer");
            Assert.Contains(appDataPath, service.LiteDbPath);
            Assert.Contains(appDataPath, service.MLModelPath);
            Assert.Equal(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), service.DefaultExportPath);
            Assert.Equal(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), service.DefaultImportPath);
        }

        #endregion

        #region SettingsChanged Event Tests

        [Fact]
        public async Task LoadSettingsAsync_RaisesSettingsChangedEvent()
        {
            // Arrange
            var service = CreateService();
            bool eventRaised = false;
            SettingsChangedEventArgs? eventArgs = null;

            service.SettingsChanged += (s, e) =>
            {
                eventRaised = true;
                eventArgs = e;
            };

            // Act
            await service.LoadSettingsAsync();

            // Assert
            Assert.True(eventRaised);
            Assert.NotNull(eventArgs);
            Assert.True(eventArgs.Loaded);
            Assert.False(eventArgs.Saved);
            Assert.False(eventArgs.Reset);
        }

        [Fact]
        public async Task SaveSettingsAsync_RaisesSettingsChangedEvent()
        {
            // Arrange
            var service = CreateService();
            bool eventRaised = false;
            SettingsChangedEventArgs? eventArgs = null;

            service.SettingsChanged += (s, e) =>
            {
                eventRaised = true;
                eventArgs = e;
            };

            // Act
            await service.SaveSettingsAsync();

            // Assert
            Assert.True(eventRaised);
            Assert.NotNull(eventArgs);
            Assert.False(eventArgs.Loaded);
            Assert.True(eventArgs.Saved);
            Assert.False(eventArgs.Reset);
        }

        [Fact]
        public void ResetToDefaults_RaisesSettingsChangedEvent()
        {
            // Arrange
            var service = CreateService();
            bool eventRaised = false;
            SettingsChangedEventArgs? eventArgs = null;

            service.SettingsChanged += (s, e) =>
            {
                eventRaised = true;
                eventArgs = e;
            };

            // Act
            service.ResetToDefaults();

            // Assert
            Assert.True(eventRaised);
            Assert.NotNull(eventArgs);
            Assert.False(eventArgs.Loaded);
            Assert.False(eventArgs.Saved);
            Assert.True(eventArgs.Reset);
        }

        #endregion

        #region Persistence Tests

        [Fact]
        public async Task Persistence_RoundTrip_PreservesAllSettings()
        {
            // Arrange
            var originalService = CreateService();
            originalService.DatabaseProviderType = DatabaseProviderType.AdvGenNoSQLServer;
            originalService.LiteDbPath = "/roundtrip/db.db";
            originalService.ServerHost = "roundtrip.example.com";
            originalService.ServerPort = 7777;
            originalService.ApiKey = "roundtrip-key";
            originalService.DatabaseName = "RoundTripDB";
            originalService.UseSsl = false;
            originalService.DefaultExportPath = "/roundtrip/export";
            originalService.DefaultImportPath = "/roundtrip/import";
            originalService.Culture = "zh-TW";
            originalService.AutoCheckForUpdates = false;
            originalService.MLModelPath = "/roundtrip/model.zip";
            originalService.AutoCategorizationThreshold = 0.95f;
            originalService.EnableAutoCategorization = false;
            originalService.EnablePriceDropAlerts = false;
            originalService.EnableExpirationAlerts = false;
            originalService.AlertCheckIntervalHours = 8;
            originalService.ConnectionTimeout = 15;
            originalService.RetryCount = 1;

            // Act - Save and load
            await originalService.SaveSettingsAsync();

            var newService = CreateService();
            await newService.LoadSettingsAsync();

            // Assert
            Assert.Equal(originalService.DatabaseProviderType, newService.DatabaseProviderType);
            Assert.Equal(originalService.LiteDbPath, newService.LiteDbPath);
            Assert.Equal(originalService.ServerHost, newService.ServerHost);
            Assert.Equal(originalService.ServerPort, newService.ServerPort);
            Assert.Equal(originalService.ApiKey, newService.ApiKey);
            Assert.Equal(originalService.DatabaseName, newService.DatabaseName);
            Assert.Equal(originalService.UseSsl, newService.UseSsl);
            Assert.Equal(originalService.DefaultExportPath, newService.DefaultExportPath);
            Assert.Equal(originalService.DefaultImportPath, newService.DefaultImportPath);
            Assert.Equal(originalService.Culture, newService.Culture);
            Assert.Equal(originalService.AutoCheckForUpdates, newService.AutoCheckForUpdates);
            Assert.Equal(originalService.MLModelPath, newService.MLModelPath);
            Assert.Equal(originalService.AutoCategorizationThreshold, newService.AutoCategorizationThreshold);
            Assert.Equal(originalService.EnableAutoCategorization, newService.EnableAutoCategorization);
            Assert.Equal(originalService.EnablePriceDropAlerts, newService.EnablePriceDropAlerts);
            Assert.Equal(originalService.EnableExpirationAlerts, newService.EnableExpirationAlerts);
            Assert.Equal(originalService.AlertCheckIntervalHours, newService.AlertCheckIntervalHours);
            Assert.Equal(originalService.ConnectionTimeout, newService.ConnectionTimeout);
            Assert.Equal(originalService.RetryCount, newService.RetryCount);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void ApiKey_CanBeEmpty()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.ApiKey = string.Empty;

            // Assert
            Assert.Equal(string.Empty, service.ApiKey);
        }

        [Fact]
        public void ApiKey_CanContainSpecialCharacters()
        {
            // Arrange
            var service = CreateService();
            var specialKey = "abc123!@#$%^&*()_+-=[]{}|;':\",./<>?";

            // Act
            service.ApiKey = specialKey;

            // Assert
            Assert.Equal(specialKey, service.ApiKey);
        }

        [Fact]
        public void LiteDbPath_CanBeChanged()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.LiteDbPath = "/new/path/database.db";

            // Assert
            Assert.Equal("/new/path/database.db", service.LiteDbPath);
        }

        [Fact]
        public void MLModelPath_CanBeChanged()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.MLModelPath = "/new/ml/model.zip";

            // Assert
            Assert.Equal("/new/ml/model.zip", service.MLModelPath);
        }

        #endregion

        /// <summary>
        /// Simple test logger that doesn't write to disk
        /// </summary>
        private class TestLoggerService : ILoggerService
        {
            public void LogDebug(string message) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message, Exception? exception = null) { }
            public void LogCritical(string message, Exception? exception = null) { }
            public string GetLogFilePath() => string.Empty;
        }
    }
}
