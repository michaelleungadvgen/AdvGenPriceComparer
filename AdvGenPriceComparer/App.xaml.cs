using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Services;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Desktop.WinUI.Services;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace AdvGenPriceComparer.Desktop.WinUI;

public partial class App : Microsoft.UI.Xaml.Application
{
    public static IServiceProvider Services { get; private set; }
    public static MainWindow MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();

        // Initialize WinUI resources in code as workaround for resource loading issues
        try
        {
            var resources = new Microsoft.UI.Xaml.Controls.XamlControlsResources();
            this.Resources.MergedDictionaries.Add(resources);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Could not load XamlControlsResources: {ex.Message}");
        }

        Services = ConfigureServices();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            var window = Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            window.Activate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Application launch failed: {ex}");
            throw;
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        try
        {
            var services = new ServiceCollection();

            // Database Path
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdvGenPriceComparer");
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

            // Services
            services.AddSingleton<IGroceryDataService>(provider => new GroceryDataService(dbPath));
            services.AddSingleton<IDialogService, SimpleDialogService>();
            services.AddSingleton<INotificationService, SimpleNotificationService>();
            services.AddSingleton<ServerConfigService>(provider => new ServerConfigService(serverConfigPath));
            // Note: IP2PNetworkService is implemented by NetworkManager in WPF project
            // For WinUI, we would need a separate implementation or shared library
            // For now, commenting out to fix build - will need proper implementation
            // services.AddSingleton<IP2PNetworkService, NetworkManager>();
            services.AddTransient<DemoDataService>();
            services.AddTransient<AdvGenPriceComparer.Application.Services.JsonImportService>(provider =>
            {
                var dbService = new AdvGenPriceComparer.Data.LiteDB.Services.DatabaseService(dbPath);
                var itemRepo = new AdvGenPriceComparer.Data.LiteDB.Repositories.ItemRepository(dbService);
                var placeRepo = new AdvGenPriceComparer.Data.LiteDB.Repositories.PlaceRepository(dbService);
                var priceRepo = new AdvGenPriceComparer.Data.LiteDB.Repositories.PriceRecordRepository(dbService);
                return new AdvGenPriceComparer.Application.Services.JsonImportService(itemRepo, placeRepo, priceRepo);
            });

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ItemViewModel>();
            services.AddTransient<PlaceViewModel>();

            // Main Window
            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Service configuration failed: {ex}");
            throw;
        }
    }
}
