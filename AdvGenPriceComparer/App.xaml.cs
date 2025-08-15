using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Core.Helpers;
using AdvGenPriceComparer.Core.Services;
using AdvGenPriceComparer.Desktop.WinUI.Services;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using System;
using System.IO;

namespace AdvGenPriceComparer.Desktop.WinUI;

public partial class App : Application
{
    public static ServiceProvider? Services { get; private set; }

    public App()
    {
        InitializeComponent();
        
        try
        {
            ConfigureServices();
        }
        catch (Exception ex)
        {
            // Log startup error
            System.Diagnostics.Debug.WriteLine($"Error configuring services: {ex.Message}");
        }
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        try
        {
            // Initialize database path
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdvGenPriceComparer");
            Directory.CreateDirectory(appDataPath);
            var dbPath = Path.Combine(appDataPath, "GroceryPrices.db");
            
            // Initialize server config path
            var serverConfigPath = Path.Combine(appDataPath, "servers.json");
            if (!File.Exists(serverConfigPath))
            {
                var projectServerPath = Path.Combine(Directory.GetCurrentDirectory(), "servers.json");
                if (File.Exists(projectServerPath))
                {
                    File.Copy(projectServerPath, serverConfigPath);
                }
            }

            // Register core services
            services.AddSingleton<IGroceryDataService>(provider => new GroceryDataService(dbPath));
            services.AddSingleton<ServerConfigService>(provider => new ServerConfigService(serverConfigPath));
            services.AddSingleton<NetworkManager>(provider => 
            {
                var groceryData = provider.GetRequiredService<IGroceryDataService>();
                var serverConfig = provider.GetRequiredService<ServerConfigService>();
                return new NetworkManager(groceryData, serverConfig);
            });

            // Register UI services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INotificationService, NotificationService>();

            // Register ViewModels
            services.AddTransient<MainWindowViewModel>();

            Services = services.BuildServiceProvider();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ConfigureServices: {ex.Message}");
            // Create minimal services if full setup fails
            services = new ServiceCollection();
            Services = services.BuildServiceProvider();
        }
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error launching window: {ex.Message}");
            // Try creating a simple window as fallback
            try
            {
                m_window = new MainWindow();
                m_window.Activate();
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback window creation failed: {fallbackEx.Message}");
            }
        }
    }

    private Window? m_window;
}
