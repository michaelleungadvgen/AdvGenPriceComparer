using System.Windows;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.Views;
using AdvGenPriceComparer.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.WPF.Services;

public class SimpleDialogService : IDialogService
{
    public void ShowInfo(string message, string title = "Information")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowSuccess(string message, string title = "Success")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowWarning(string message, string title = "Warning")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowError(string message, string title = "Error")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool ShowConfirmation(string message, string title = "Confirmation")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public bool ShowQuestion(string message, string title = "Question")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public void ShowComparePricesDialog(string category = null)
    {
        var mediator = ((App)System.Windows.Application.Current).Services.GetRequiredService<AdvGenFlow.IMediator>();
        var viewModel = new ViewModels.PriceComparisonViewModel(mediator, category);
        var window = new ComparePricesWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public SearchResult? ShowGlobalSearchDialog()
    {
        var searchService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IGlobalSearchService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        var viewModel = new ViewModels.GlobalSearchViewModel(searchService, this, logger);
        var window = new GlobalSearchWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        
        var result = window.ShowDialog();
        
        if (result == true)
        {
            return window.SelectedResult;
        }
        
        return null;
    }

    public void ShowBarcodeScannerDialog()
    {
        var barcodeService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IBarcodeService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        var viewModel = new ViewModels.ScanBarcodeViewModel(barcodeService, this, logger);
        var window = new ScanBarcodeWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowPriceDropNotificationsDialog()
    {
        var notificationService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IPriceDropNotificationService>();
        var groceryData = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IGroceryDataService>();
        var viewModel = new ViewModels.PriceDropNotificationViewModel(notificationService, groceryData);
        var window = new PriceDropNotificationsWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowFavoritesDialog()
    {
        var favoritesService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IFavoritesService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        var viewModel = new ViewModels.FavoritesViewModel(favoritesService, this, logger);
        var window = new FavoritesWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowDealExpirationRemindersDialog()
    {
        var dealExpirationService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDealExpirationService>();
        var viewModel = new ViewModels.DealExpirationReminderViewModel(dealExpirationService);
        var window = new DealExpirationRemindersWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowWeeklySpecialsDigestDialog()
    {
        var weeklySpecialsService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IWeeklySpecialsService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var viewModel = new ViewModels.WeeklySpecialsDigestViewModel(weeklySpecialsService, dialogService);
        var window = new WeeklySpecialsDigestWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowShoppingListsDialog()
    {
        var shoppingListService = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IShoppingListService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var viewModel = new ViewModels.ShoppingListViewModel(shoppingListService, dialogService);
        var window = new ShoppingListWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowSettingsDialog()
    {
        var settingsService = ((App)System.Windows.Application.Current).Services.GetRequiredService<ISettingsService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var themeService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IThemeService>();
        var localizationService = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILocalizationService>();
        var viewModel = new ViewModels.SettingsViewModel(settingsService, logger, dialogService, themeService, localizationService);
        var window = new SettingsWindow { DataContext = viewModel, Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowMLModelManagementDialog()
    {
        var dataService = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IGroceryDataService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        var settingsService = ((App)System.Windows.Application.Current).Services.GetRequiredService<ISettingsService>();
        
        // Use MLModelPath from settings service
        var modelPath = settingsService.MLModelPath;
        
        var viewModel = new ViewModels.MLModelManagementViewModel(dataService, dialogService, logger, modelPath);
        var window = new MLModelManagementWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowPriceForecastDialog()
    {
        var priceRecordRepository = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IPriceRecordRepository>();
        var itemRepository = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IItemRepository>();
        var placeRepository = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IPlaceRepository>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        
        var viewModel = new ViewModels.PriceForecastViewModel(
            priceRecordRepository, 
            itemRepository, 
            placeRepository, 
            dialogService, 
            logger);
        var window = new PriceForecastWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowChatDialog()
    {
        var window = new Chat.PriceChatWindow { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowExportDataDialog()
    {
        var exportService = ((App)System.Windows.Application.Current).Services.GetRequiredService<ExportService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var viewModel = new ExportDataViewModel(exportService, dialogService);
        var window = new ExportDataWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowImportFromUrlDialog()
    {
        var staticDataImporter = ((App)System.Windows.Application.Current).Services.GetRequiredService<StaticDataImporter>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var viewModel = new ImportFromUrlViewModel(staticDataImporter, logger, dialogService);
        var window = new ImportFromUrlWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowIllusoryDiscountDetectionDialog()
    {
        var forecastingService = ((App)System.Windows.Application.Current).Services.GetRequiredService<AdvGenPriceComparer.ML.Services.PriceForecastingService>();
        var itemRepository = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IItemRepository>();
        var priceRecordRepository = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IPriceRecordRepository>();
        var placeRepository = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IPlaceRepository>();
        
        var viewModel = new ViewModels.IllusoryDiscountDetectionViewModel(
            forecastingService, 
            itemRepository, 
            priceRecordRepository, 
            placeRepository);
        var window = new IllusoryDiscountDetectionWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowServerDataTransferDialog()
    {
        var settingsService = ((App)System.Windows.Application.Current).Services.GetRequiredService<ISettingsService>();
        var groceryDataService = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IGroceryDataService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        
        var viewModel = new ViewModels.ServerDataTransferViewModel(
            settingsService, 
            groceryDataService, 
            this, 
            logger);
        var window = new ServerDataTransferWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowBestPricesDialog()
    {
        var bestPriceService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IBestPriceService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        
        var viewModel = new ViewModels.BestPricesViewModel(bestPriceService, logger);
        var window = new BestPricesWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowEditPlaceDialog(Core.Models.Place place)
    {
        var mediator = ((App)System.Windows.Application.Current).Services.GetRequiredService<AdvGenFlow.IMediator>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();

        var viewModel = new ViewModels.AddStoreViewModel(mediator, dialogService)
        {
            StoreId = place.Id,
            StoreName = place.Name,
            Chain = place.Chain ?? string.Empty,
            Address = place.Address ?? string.Empty,
            Suburb = place.Suburb ?? string.Empty,
            State = place.State ?? "QLD",
            Postcode = place.Postcode ?? string.Empty,
            Phone = place.Phone ?? string.Empty
        };
        
        var window = new AddStoreWindow(viewModel)
        {
            Owner = System.Windows.Application.Current.MainWindow,
            Title = "Edit Store"
        };
        
        window.ShowDialog();
    }

    public void ShowTripOptimizerDialog()
    {
        var tripOptimizerService = ((App)System.Windows.Application.Current).Services.GetRequiredService<ITripOptimizerService>();
        var groceryDataService = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IGroceryDataService>();
        var shoppingListService = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IShoppingListService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        
        var viewModel = new ViewModels.TripOptimizerViewModel(tripOptimizerService, groceryDataService, shoppingListService, logger, dialogService);
        var window = new TripOptimizerWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowPriceAlertsDialog()
    {
        var priceAlertService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IPriceAlertService>();
        var mediator = ((App)System.Windows.Application.Current).Services.GetRequiredService<AdvGenFlow.IMediator>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        
        var viewModel = new ViewModels.PriceAlertViewModel(priceAlertService, mediator, dialogService, logger);
        var window = new PriceAlertWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowWeeklySpecialsImportDialog()
    {
        var importService = ((App)System.Windows.Application.Current).Services.GetRequiredService<Core.Interfaces.IWeeklySpecialsImportService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        
        var viewModel = new ViewModels.WeeklySpecialsImportViewModel(importService, dialogService, logger);
        var window = new WeeklySpecialsImportWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowCloudSyncDialog()
    {
        var cloudSyncService = ((App)System.Windows.Application.Current).Services.GetRequiredService<ICloudSyncService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        
        var viewModel = new ViewModels.CloudSyncViewModel(cloudSyncService, dialogService, logger);
        var window = new CloudSyncStatusWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowStaticPeerConfigDialog()
    {
        var peerDiscoveryService = ((App)System.Windows.Application.Current).Services.GetRequiredService<PeerDiscoveryService>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();
        var dialogService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IDialogService>();
        
        var viewModel = new ViewModels.StaticPeerConfigViewModel(peerDiscoveryService, logger, dialogService);
        var window = new StaticPeerConfigWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }

    public ExportProgressWindow ShowExportProgressDialog(string title = "Exporting Data...")
    {
        var window = new ExportProgressWindow
        {
            Owner = System.Windows.Application.Current.MainWindow,
            Title = title
        };
        return window;
    }

    public ImportProgressWindow ShowImportProgressDialog(string title = "Importing Data...")
    {
        var window = new ImportProgressWindow
        {
            Owner = System.Windows.Application.Current.MainWindow,
            Title = title
        };
        return window;
    }

    public void ShowValidationReportDialog(Core.Models.ValidationReport report)
    {
        var viewModel = new ValidationReportViewModel
        {
            Report = report
        };
        var window = new ValidationReportWindow(viewModel) { Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
    }
}
