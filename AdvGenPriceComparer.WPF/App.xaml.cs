using System;
using System.IO;
using System.Windows;
using AdvGenFlow;
using AdvGenPriceComparer.Application;
using AdvGenPriceComparer.Application.Interfaces;
using AdvGenPriceComparer.Application.Services;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Services;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.ML.Models;
using AdvGenPriceComparer.ML.Services;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.ViewModels;
using AdvGenPriceComparer.WPF.Views;
using AdvGenPriceComparer.WPF.Chat.Services;
using AdvGenPriceComparer.WPF.Chat.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; private set; }
    public static IServiceProvider? ServiceProvider => (Current as App)?.Services;

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
            // Apply theme and localization from settings
            var settingsService = Services.GetRequiredService<ISettingsService>();
            var themeService = Services.GetRequiredService<IThemeService>();
            var localizationService = Services.GetRequiredService<ILocalizationService>();
            
            // Wait for settings to be loaded (they load async in background)
            // We'll apply the settings once confirmed loaded
            settingsService.SettingsChanged += (s, args) =>
            {
                if (args.Loaded)
                {
                    logger.LogInfo($"Applying theme from settings: {settingsService.ApplicationTheme}");
                    themeService.ApplyTheme(settingsService.ApplicationTheme);
                    
                    // Apply localization culture from settings
                    if (!string.IsNullOrEmpty(settingsService.Culture) && 
                        !settingsService.Culture.Equals(localizationService.CurrentCulture, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.LogInfo($"Applying culture from settings: {settingsService.Culture}");
                        localizationService.ChangeCulture(settingsService.Culture);
                    }
                }
            };

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
            services.AddSingleton<IP2PNetworkService, NetworkManager>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();

            // Peer Discovery Service for P2P static data sharing
            services.AddSingleton<PeerDiscoveryService>(provider =>
            {
                var logger = provider.GetRequiredService<ILoggerService>();
                var serverConfig = provider.GetRequiredService<ServerConfigService>();
                var cachePath = Path.Combine(appDataPath, "peer_discovery_cache.json");
                return new PeerDiscoveryService(logger, serverConfig, cachePath);
            });

            // Settings Service - Create service, load settings in background
            services.AddSingleton<ISettingsService>(provider =>
            {
                var logger = provider.GetRequiredService<ILoggerService>();
                logger.LogInfo("Creating SettingsService");
                var settingsService = new SettingsService(logger);

                // Load settings in background - don't block startup!
                _ = Task.Run(async () =>
                {
                    logger.LogInfo("Loading settings in background...");
                    try
                    {
                        await settingsService.LoadSettingsAsync();
                        logger.LogInfo("Settings loaded successfully in background");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Failed to load settings in background", ex);
                        // Settings service will use defaults
                    }
                });

                logger.LogInfo("SettingsService created (settings loading in background)");
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
            services.AddSingleton<IExportHistoryRepository>(provider => provider.GetRequiredService<IDatabaseProvider>().ExportHistories);
            services.AddSingleton<IImportHistoryRepository>(provider => provider.GetRequiredService<IDatabaseProvider>().ImportHistories);

            // Required: handlers inject ILogger<T>
            services.AddLogging();

            // Register Application layer mediator and all handlers (via reflection)
            services.AddApplicationServices();

            // Price History Tracking Service
            services.AddSingleton<IPriceHistoryTrackingService, PriceHistoryTrackingService>();
            
            // ML.NET Services
            services.AddSingleton<IModelVersionService>(provider =>
            {
                var settingsService = provider.GetRequiredService<ISettingsService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                var modelPath = settingsService.MLModelPath;
                
                return new ModelVersionService(
                    modelPath,
                    new ModelVersionRetentionSettings(),
                    msg => logger.LogInfo(msg),
                    (msg, ex) => logger.LogError(msg, ex),
                    msg => logger.LogWarning(msg));
            });
            services.AddSingleton<ModelTrainingService>(provider =>
            {
                var logger = provider.GetRequiredService<ILoggerService>();
                var versionService = provider.GetService<IModelVersionService>();
                return new ModelTrainingService(
                    versionService,
                    msg => logger.LogInfo(msg),
                    (msg, ex) => logger.LogError(msg, ex),
                    msg => logger.LogWarning(msg));
            });
            services.AddSingleton<CategoryPredictionService>(provider =>
            {
                var settingsService = provider.GetRequiredService<ISettingsService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                var modelPath = settingsService.MLModelPath;
                
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(modelPath)!);
                
                return new CategoryPredictionService(modelPath,
                    msg => logger.LogInfo(msg),
                    (msg, ex) => logger.LogError(msg, ex),
                    msg => logger.LogWarning(msg));
            });
            services.AddSingleton<ICategoryPredictionService>(provider => provider.GetRequiredService<CategoryPredictionService>());
            services.AddSingleton<DataPreparationService>();
            services.AddSingleton<AdvGenPriceComparer.ML.Services.PriceForecastingService>(provider =>
            {
                var settingsService = provider.GetRequiredService<ISettingsService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                var modelPath = Path.Combine(
                    Path.GetDirectoryName(settingsService.MLModelPath) ?? 
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdvGenPriceComparer", "MLModels"),
                    "price_forecast_models");
                
                Directory.CreateDirectory(modelPath);
                
                return new AdvGenPriceComparer.ML.Services.PriceForecastingService(
                    modelPath,
                    msg => logger.LogInfo(msg),
                    (msg, ex) => logger.LogError(msg, ex),
                    msg => logger.LogWarning(msg));
            });
            
            services.AddTransient<DemoDataService>();
            services.AddTransient<JsonImportService>(provider =>
            {
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var categoryPredictionService = provider.GetRequiredService<ICategoryPredictionService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                
                return new JsonImportService(itemRepo, placeRepo, priceRepo, categoryPredictionService,
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
            services.AddTransient<StaticDataExporter>(provider =>
            {
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var exportHistoryRepo = provider.GetRequiredService<IExportHistoryRepository>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new StaticDataExporter(itemRepo, placeRepo, priceRepo, exportHistoryRepo, logger);
            });
            services.AddTransient<StaticDataImporter>(provider =>
            {
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var importHistoryRepo = provider.GetRequiredService<IImportHistoryRepository>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new StaticDataImporter(itemRepo, placeRepo, priceRepo, importHistoryRepo, logger);
            });
            services.AddSingleton<ScheduledExportService>(provider =>
            {
                var exporter = provider.GetRequiredService<StaticDataExporter>();
                var settingsService = provider.GetRequiredService<ISettingsService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                var service = new ScheduledExportService(exporter, settingsService, logger);
                
                // Start the service if enabled
                if (service.IsEnabled)
                {
                    service.Start();
                    logger.LogInfo("ScheduledExportService started (enabled in config)");
                }
                else
                {
                    logger.LogInfo("ScheduledExportService created but not started (disabled in config)");
                }
                
                return service;
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
            services.AddSingleton<ICloudSyncService>(provider =>
            {
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var settingsService = provider.GetRequiredService<ISettingsService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new CloudSyncService(itemRepo, placeRepo, priceRepo, settingsService, logger);
            });
            services.AddSingleton<IPriceAlertService>(provider =>
            {
                var dbService = provider.GetRequiredService<AdvGenPriceComparer.Data.LiteDB.Services.DatabaseService>();
                var notificationService = provider.GetRequiredService<INotificationService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                var groceryData = provider.GetRequiredService<IGroceryDataService>();
                return new PriceAlertService(dbService, notificationService, logger, groceryData);
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
            services.AddSingleton<IWeeklySpecialsImportService>(provider =>
            {
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var logger = provider.GetRequiredService<ILoggerService>();
                var categoryPredictionService = provider.GetService<ICategoryPredictionService>();
                return new WeeklySpecialsImportService(itemRepo, placeRepo, priceRepo, logger, categoryPredictionService);
            });
            services.AddSingleton<IBestPriceService>(provider =>
            {
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new BestPriceService(itemRepo, placeRepo, priceRepo, logger);
            });
            services.AddSingleton<IReportGenerationService>(provider =>
            {
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                return new ReportGenerationService(priceRepo, itemRepo, placeRepo);
            });
            services.AddSingleton<IShoppingListRepository>(provider =>
            {
                var dbProvider = provider.GetRequiredService<IDatabaseProvider>();
                // Cast to LiteDbProvider to access the underlying database
                if (dbProvider is AdvGenPriceComparer.Data.LiteDB.Services.LiteDbProvider liteDbProvider)
                {
                    var database = liteDbProvider.GetDatabase();
                    if (database != null)
                    {
                        return new AdvGenPriceComparer.Data.LiteDB.Repositories.ShoppingListRepository(database);
                    }
                }
                throw new InvalidOperationException("Unable to create ShoppingListRepository - database not available");
            });
            services.AddSingleton<IShoppingListService>(provider =>
            {
                var repo = provider.GetRequiredService<IShoppingListRepository>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new ShoppingListService(repo, logger);
            });

            // Trip Optimizer Service
            services.AddSingleton<ITripOptimizerService>(provider =>
            {
                var groceryData = provider.GetRequiredService<IGroceryDataService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new TripOptimizerService(groceryData, logger);
            });

            // Update Service
            services.AddSingleton<IUpdateService>(provider =>
            {
                var settingsService = provider.GetRequiredService<ISettingsService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new UpdateService(settingsService, logger);
            });

            // SignalR Client Service for real-time price updates
            services.AddSingleton<IPriceUpdateClientService>(provider =>
            {
                var logger = provider.GetRequiredService<ILoggerService>();
                return new PriceUpdateClientService(logger);
            });

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ItemViewModel>();
            services.AddTransient<PlaceViewModel>();
            services.AddTransient<CloudSyncViewModel>(provider =>
            {
                var cloudSyncService = provider.GetRequiredService<ICloudSyncService>();
                var dialogService = provider.GetRequiredService<IDialogService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new CloudSyncViewModel(cloudSyncService, dialogService, logger);
            });
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
            services.AddTransient<ReportsViewModel>(provider =>
            {
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var reportService = provider.GetRequiredService<IReportGenerationService>();
                var dialogService = provider.GetRequiredService<IDialogService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new ReportsViewModel(priceRepo, itemRepo, placeRepo, reportService, dialogService, logger);
            });
            services.AddTransient<ShoppingListViewModel>(provider =>
            {
                var service = provider.GetRequiredService<IShoppingListService>();
                var dialogService = provider.GetRequiredService<IDialogService>();
                return new ShoppingListViewModel(service, dialogService);
            });
            services.AddTransient<ImportFromUrlViewModel>(provider =>
            {
                var staticDataImporter = provider.GetRequiredService<StaticDataImporter>();
                var logger = provider.GetRequiredService<ILoggerService>();
                var dialogService = provider.GetRequiredService<IDialogService>();
                return new ImportFromUrlViewModel(staticDataImporter, logger, dialogService);
            });

            // Views
            services.AddTransient<ItemsPage>();
            services.AddTransient<PriceHistoryPage>();
            services.AddTransient<ReportsPage>(provider =>
            {
                var viewModel = provider.GetRequiredService<ReportsViewModel>();
                return new ReportsPage(viewModel);
            });
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
            services.AddTransient<ShoppingListWindow>(provider =>
            {
                var viewModel = provider.GetRequiredService<ShoppingListViewModel>();
                return new ShoppingListWindow(viewModel);
            });
            services.AddTransient<ImportFromUrlWindow>(provider =>
            {
                var viewModel = provider.GetRequiredService<ImportFromUrlViewModel>();
                return new ImportFromUrlWindow(viewModel);
            });
            services.AddTransient<TripOptimizerWindow>(provider =>
            {
                var tripOptimizerService = provider.GetRequiredService<ITripOptimizerService>();
                var groceryData = provider.GetRequiredService<IGroceryDataService>();
                var shoppingListService = provider.GetRequiredService<IShoppingListService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                var dialogService = provider.GetRequiredService<IDialogService>();
                var viewModel = new TripOptimizerViewModel(tripOptimizerService, groceryData, shoppingListService, logger, dialogService);
                return new TripOptimizerWindow(viewModel);
            });

            // Chat Services
            services.AddSingleton<IOllamaService>(provider =>
            {
                var logger = provider.GetRequiredService<ILoggerService>();
                return new OllamaService(logger);
            });
            services.AddSingleton<IQueryRouterService>(provider =>
            {
                var groceryData = provider.GetRequiredService<IGroceryDataService>();
                var itemRepo = provider.GetRequiredService<IItemRepository>();
                var placeRepo = provider.GetRequiredService<IPlaceRepository>();
                var priceRepo = provider.GetRequiredService<IPriceRecordRepository>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new QueryRouterService(groceryData, itemRepo, placeRepo, priceRepo, logger);
            });
            services.AddTransient<ChatViewModel>(provider =>
            {
                var ollamaService = provider.GetRequiredService<IOllamaService>();
                var queryRouter = provider.GetRequiredService<IQueryRouterService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new ChatViewModel(ollamaService, queryRouter, logger);
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

