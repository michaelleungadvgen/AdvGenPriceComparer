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

            // Database Service - SINGLE instance shared across all services
            services.AddSingleton<DatabaseService>(provider =>
                new DatabaseService(dbPath));

            // Data Services - all use the shared DatabaseService
            services.AddSingleton<IGroceryDataService>(provider =>
            {
                var dbService = provider.GetRequiredService<DatabaseService>();
                return new GroceryDataService(dbService);
            });
            
            // Register Repositories
            services.AddSingleton<IPriceRecordRepository>(provider =>
            {
                var dbService = provider.GetRequiredService<DatabaseService>();
                return new AdvGenPriceComparer.Data.LiteDB.Repositories.PriceRecordRepository(dbService);
            });
            services.AddSingleton<IItemRepository>(provider =>
            {
                var dbService = provider.GetRequiredService<DatabaseService>();
                return new AdvGenPriceComparer.Data.LiteDB.Repositories.ItemRepository(dbService);
            });
            services.AddSingleton<IPlaceRepository>(provider =>
            {
                var dbService = provider.GetRequiredService<DatabaseService>();
                return new AdvGenPriceComparer.Data.LiteDB.Repositories.PlaceRepository(dbService);
            });
            
            services.AddTransient<DemoDataService>();
            services.AddTransient<JsonImportService>(provider =>
            {
                var dbService = provider.GetRequiredService<DatabaseService>();
                var logger = provider.GetRequiredService<ILoggerService>();
                return new JsonImportService(dbService, 
                    msg => logger.LogInfo(msg),
                    (msg, ex) => logger.LogError(msg, ex),
                    msg => logger.LogWarning(msg));
            });
            services.AddTransient<ExportService>(provider =>
            {
                var dbService = provider.GetRequiredService<DatabaseService>();
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

            // Views
            services.AddTransient<ItemsPage>();
            services.AddTransient<PriceHistoryPage>();

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

