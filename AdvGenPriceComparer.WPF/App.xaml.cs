using System;
using System.IO;
using System.Windows;
using AdvGenPriceComparer.Core.Helpers;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Services;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.ViewModels;
using AdvGenPriceComparer.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider Services { get; private set; }

    public App()
    {
        Services = ConfigureServices();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logger = Services.GetRequiredService<ILoggerService>();
        logger.LogInfo("OnStartup called");

        try
        {
            logger.LogInfo("Creating MainWindow");
            var mainWindow = Services.GetRequiredService<MainWindow>();
            logger.LogInfo("MainWindow created successfully");
            mainWindow.Show();
            logger.LogInfo("MainWindow shown");
        }
        catch (Exception ex)
        {
            logger.LogCritical("Application startup failed", ex);
            MessageBox.Show($"Application startup failed: {ex.Message}\n\nCheck logs at: {logger.GetLogFilePath()}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        try
        {
            var services = new ServiceCollection();

            // Database Path
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AdvGenPriceComparer");
            Directory.CreateDirectory(appDataPath);
            var dbPath = Path.Combine(appDataPath, "GroceryPrices.db");

            // Server Config Path
            var serverConfigPath = Path.Combine(appDataPath, "servers.json");
            if (!File.Exists(serverConfigPath))
            {
                var projectServerPath = Path.Combine(AppContext.BaseDirectory, "servers.json");
                if (File.Exists(projectServerPath))
                {
                    File.Copy(projectServerPath, serverConfigPath);
                }
            }

            // Core Services
            services.AddSingleton<ILoggerService, FileLoggerService>();
            services.AddSingleton<IDialogService, SimpleDialogService>();
            services.AddSingleton<INotificationService, SimpleNotificationService>();
            services.AddSingleton<ServerConfigService>(provider =>
                new ServerConfigService(serverConfigPath));
            services.AddSingleton<NetworkManager>();

            // Settings Service - Load settings on startup
            services.AddSingleton<ISettingsService>(provider =>
            {
                var logger = provider.GetRequiredService<ILoggerService>();
                var settingsService = new SettingsService(logger);
                settingsService.LoadSettingsAsync().Wait();
                return settingsService;
            });

            // Database Provider Factory
            services.AddSingleton<DatabaseProviderFactory>();

            // IDatabaseProvider - Created asynchronously using the factory
            services.AddSingleton<IDatabaseProvider>(provider =>
            {
                var factory = provider.GetRequiredService<DatabaseProviderFactory>();
                return factory.CreateProviderAsync().GetAwaiter().GetResult();
            });

            // Data Services - all use the shared IDatabaseProvider
            services.AddSingleton<IGroceryDataService>(provider =>
            {
                var dbProvider = provider.GetRequiredService<IDatabaseProvider>();
                return new ProviderGroceryDataService(dbProvider);
            });
            
            // Register Repositories from the provider
            services.AddSingleton<IPriceRecordRepository>(provider => provider.GetRequiredService<IDatabaseProvider>().PriceRecords);
            services.AddSingleton<IItemRepository>(provider => provider.GetRequiredService<IDatabaseProvider>().Items);
            services.AddSingleton<IPlaceRepository>(provider => provider.GetRequiredService<IDatabaseProvider>().Places);
            services.AddSingleton<IAlertRepository>(provider => provider.GetRequiredService<IDatabaseProvider>().Alerts);
            
            services.AddTransient<DemoDataService>();
            services.AddTransient<JsonImportService>(provider =>
            {
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var logger = provider.GetRequiredService<ILoggerService>();
                
                // Note: JsonImportService might need refactoring if it depends on DatabaseService directly
                // For now we'll assume it uses repositories or we'll refactor it next
                return new JsonImportService(itemRepo, placeRepo, priceRepo,
                    msg => logger.LogInfo(msg),
                    (msg, ex) => logger.LogError(msg, ex),
                    msg => logger.LogWarning(msg));
            });
            services.AddTransient<ExportService>(provider =>
            {
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new ExportService(itemRepo, placeRepo, priceRepo, logger);
            });
            services.AddSingleton<IGlobalSearchService>(provider =>
            {
                var dataService = provider.GetRequiredService<IGroceryDataService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new GlobalSearchService(dataService, logger);
            });
            services.AddSingleton<IBarcodeService>(provider =>
            {
                var logger = provider.GetRequiredService<ILoggerService>();
                return new BarcodeService(logger);
            });
            services.AddSingleton<IPriceDropNotificationService>(provider =>
            {
                var groceryData = provider.GetRequiredService<IGroceryDataService>();
                var notificationService = provider.GetRequiredService<INotificationService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new PriceDropNotificationService(groceryData, notificationService, logger);
            });
            services.AddSingleton<IFavoritesService>(provider =>
            {
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new FavoritesService(itemRepo, logger);
            });
            services.AddSingleton<IDealExpirationService>(provider =>
            {
                var groceryData = provider.GetRequiredService<IGroceryDataService>();
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AdvGenPriceComparer");
                return new DealExpirationService(groceryData, appDataPath);
            });
            services.AddSingleton<IWeeklySpecialsService>(provider =>
            {
                var groceryData = provider.GetRequiredService<IGroceryDataService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new WeeklySpecialsService(groceryData, logger);
            });

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ItemViewModel>();
            services.AddTransient<PlaceViewModel>();
            services.AddTransient<AddStoreViewModel>();
            services.AddTransient<ImportDataViewModel>();
            services.AddTransient<PriceHistoryViewModel>();
            services.AddTransient<AddPriceRecordViewModel>();
            services.AddTransient<ExportDataViewModel>(provider =>
            {
                var exportService = provider.GetRequiredService<ExportService>();
                var dialogService = provider.GetRequiredService<IDialogService>();
                return new ExportDataViewModel(exportService, dialogService);
            });
            services.AddTransient<PriceDropNotificationViewModel>(provider =>
            {
                var notificationService = provider.GetRequiredService<IPriceDropNotificationService>();
                var groceryData = provider.GetRequiredService<IGroceryDataService>();
                return new PriceDropNotificationViewModel(notificationService, groceryData);
            });
            services.AddTransient<DealExpirationReminderViewModel>(provider =>
            {
                var dealExpirationService = provider.GetRequiredService<IDealExpirationService>();
                return new DealExpirationReminderViewModel(dealExpirationService);
            });
            services.AddTransient<WeeklySpecialsDigestViewModel>(provider =>
            {
                var weeklySpecialsService = provider.GetRequiredService<IWeeklySpecialsService>();
                var dialogService = provider.GetRequiredService<IDialogService>();
                return new WeeklySpecialsDigestViewModel(weeklySpecialsService, dialogService);
            });

            // Views
            services.AddTransient<ItemsPage>();
            services.AddTransient<PriceHistoryPage>();
            services.AddTransient<DealExpirationRemindersWindow>(provider =>
            {
                var viewModel = provider.GetRequiredService<DealExpirationReminderViewModel>();
                return new DealExpirationRemindersWindow(viewModel);
            });
            services.AddTransient<WeeklySpecialsDigestWindow>(provider =>
            {
                var viewModel = provider.GetRequiredService<WeeklySpecialsDigestViewModel>();
                return new WeeklySpecialsDigestWindow(viewModel);
            });

            // Main Window
            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Service configuration failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
}

