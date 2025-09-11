using AdvGenPriceComparer.Core.Helpers;
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

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = Services.GetRequiredService<MainWindow>();
        window.Activate();
    }

    private static IServiceProvider ConfigureServices()
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
        services.AddSingleton<IGroceryDataService>(new GroceryDataService(dbPath));
        services.AddSingleton<IDialogService, SimpleDialogService>();
        services.AddSingleton<INotificationService, SimpleNotificationService>();
        services.AddSingleton<ServerConfigService>(new ServerConfigService(serverConfigPath));
        services.AddSingleton<NetworkManager>();
        services.AddTransient<DemoDataService>();
        services.AddTransient<JsonImportService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ItemViewModel>();
        services.AddTransient<PlaceViewModel>();

        // Main Window
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }
}
