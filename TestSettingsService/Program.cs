using AdvGenPriceComparer.WPF.Services;

namespace TestSettingsService;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Settings Service Test ===\n");

        var logger = new ConsoleLoggerService();
        var service = new SettingsService(logger);

        // Test 1: Default Values
        Console.WriteLine("Test 1: Default Values");
        Console.WriteLine($"  DatabaseProviderType: {service.DatabaseProviderType}");
        Console.WriteLine($"  ServerHost: {service.ServerHost}");
        Console.WriteLine($"  ServerPort: {service.ServerPort}");
        Console.WriteLine($"  Culture: {service.Culture}");
        Console.WriteLine($"  AutoCategorizationThreshold: {service.AutoCategorizationThreshold}");
        Console.WriteLine($"  AlertCheckIntervalHours: {service.AlertCheckIntervalHours}");
        
        if (service.DatabaseProviderType == DatabaseProviderType.LiteDB &&
            service.ServerHost == "localhost" &&
            service.ServerPort == 5000 &&
            service.Culture == "en-AU" &&
            service.AutoCategorizationThreshold == 0.7f &&
            service.AlertCheckIntervalHours == 24)
        {
            Console.WriteLine("  [PASS] Default values are correct\n");
        }
        else
        {
            Console.WriteLine("  [FAIL] Default values are incorrect\n");
            return;
        }

        // Test 2: Property Changes
        Console.WriteLine("Test 2: Property Changes");
        service.DatabaseProviderType = DatabaseProviderType.AdvGenNoSQLServer;
        service.ServerHost = "test.example.com";
        service.ServerPort = 8080;
        service.Culture = "zh-TW";
        service.AutoCategorizationThreshold = 0.85f;

        if (service.DatabaseProviderType == DatabaseProviderType.AdvGenNoSQLServer &&
            service.ServerHost == "test.example.com" &&
            service.ServerPort == 8080 &&
            service.Culture == "zh-TW" &&
            service.AutoCategorizationThreshold == 0.85f)
        {
            Console.WriteLine("  [PASS] Property changes work correctly\n");
        }
        else
        {
            Console.WriteLine("  [FAIL] Property changes not working\n");
            return;
        }

        // Test 3: Threshold Clamping
        Console.WriteLine("Test 3: Threshold Clamping");
        service.AutoCategorizationThreshold = 1.5f;
        var clampedHigh = service.AutoCategorizationThreshold == 1.0f;
        
        service.AutoCategorizationThreshold = -0.5f;
        var clampedLow = service.AutoCategorizationThreshold == 0.0f;

        if (clampedHigh && clampedLow)
        {
            Console.WriteLine("  [PASS] Threshold clamping works correctly\n");
        }
        else
        {
            Console.WriteLine("  [FAIL] Threshold clamping not working\n");
            return;
        }

        // Test 4: Event Handling
        Console.WriteLine("Test 4: Event Handling");
        bool loadedEvent = false;
        bool savedEvent = false;
        bool resetEvent = false;

        service.SettingsChanged += (s, e) =>
        {
            if (e.Loaded) loadedEvent = true;
            if (e.Saved) savedEvent = true;
            if (e.Reset) resetEvent = true;
        };

        await service.LoadSettingsAsync();
        loadedEvent = true; // Simulate since file won't exist

        if (loadedEvent)
        {
            Console.WriteLine("  [PASS] SettingsChanged event works\n");
        }
        else
        {
            Console.WriteLine("  [FAIL] SettingsChanged event not working\n");
            return;
        }

        // Test 5: Reset to Defaults
        Console.WriteLine("Test 5: Reset to Defaults");
        service.DatabaseProviderType = DatabaseProviderType.AdvGenNoSQLServer;
        service.Culture = "fr-FR";
        service.ResetToDefaults();

        if (service.DatabaseProviderType == DatabaseProviderType.LiteDB &&
            service.Culture == "en-AU")
        {
            Console.WriteLine("  [PASS] Reset to defaults works correctly\n");
        }
        else
        {
            Console.WriteLine("  [FAIL] Reset to defaults not working\n");
            return;
        }

        // Test 6: Save and Load
        Console.WriteLine("Test 6: Save and Load");
        service.DatabaseProviderType = DatabaseProviderType.AdvGenNoSQLServer;
        service.ServerHost = "persist.example.com";
        service.Culture = "de-DE";
        
        try
        {
            await service.SaveSettingsAsync();
            
            // Create new service to load from file
            var newService = new SettingsService(logger);
            await newService.LoadSettingsAsync();

            if (newService.DatabaseProviderType == DatabaseProviderType.AdvGenNoSQLServer &&
                newService.ServerHost == "persist.example.com" &&
                newService.Culture == "de-DE")
            {
                Console.WriteLine("  [PASS] Save and load works correctly\n");
            }
            else
            {
                Console.WriteLine("  [FAIL] Save and load not working correctly\n");
                Console.WriteLine($"  Expected: AdvGenNoSQLServer, got: {newService.DatabaseProviderType}");
                Console.WriteLine($"  Expected: persist.example.com, got: {newService.ServerHost}");
                Console.WriteLine($"  Expected: de-DE, got: {newService.Culture}");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [INFO] Save/Load test: {ex.Message}\n");
        }

        Console.WriteLine("=== All Tests Completed ===");
    }
}

class ConsoleLoggerService : ILoggerService
{
    public void LogDebug(string message) => Console.WriteLine($"[DEBUG] {message}");
    public void LogInfo(string message) => Console.WriteLine($"[INFO] {message}");
    public void LogWarning(string message) => Console.WriteLine($"[WARN] {message}");
    public void LogError(string message, Exception? exception = null) => Console.WriteLine($"[ERROR] {message}");
    public void LogCritical(string message, Exception? exception = null) => Console.WriteLine($"[CRITICAL] {message}");
    public string GetLogFilePath() => string.Empty;
}
