using System.Windows;
using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.Views;
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
        var dataService = ((App)Application.Current).Services.GetRequiredService<Core.Interfaces.IGroceryDataService>();
        var viewModel = new ViewModels.PriceComparisonViewModel(dataService, category);
        var window = new ComparePricesWindow(viewModel) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    public SearchResult? ShowGlobalSearchDialog()
    {
        var searchService = ((App)Application.Current).Services.GetRequiredService<IGlobalSearchService>();
        var logger = ((App)Application.Current).Services.GetRequiredService<ILoggerService>();
        var viewModel = new ViewModels.GlobalSearchViewModel(searchService, this, logger);
        var window = new GlobalSearchWindow(viewModel) { Owner = Application.Current.MainWindow };
        
        var result = window.ShowDialog();
        
        if (result == true)
        {
            return window.SelectedResult;
        }
        
        return null;
    }

    public void ShowBarcodeScannerDialog()
    {
        var barcodeService = ((App)Application.Current).Services.GetRequiredService<IBarcodeService>();
        var logger = ((App)Application.Current).Services.GetRequiredService<ILoggerService>();
        var viewModel = new ViewModels.ScanBarcodeViewModel(barcodeService, this, logger);
        var window = new ScanBarcodeWindow(viewModel) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowPriceDropNotificationsDialog()
    {
        var notificationService = ((App)Application.Current).Services.GetRequiredService<IPriceDropNotificationService>();
        var groceryData = ((App)Application.Current).Services.GetRequiredService<Core.Interfaces.IGroceryDataService>();
        var viewModel = new ViewModels.PriceDropNotificationViewModel(notificationService, groceryData);
        var window = new PriceDropNotificationsWindow(viewModel) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowFavoritesDialog()
    {
        var favoritesService = ((App)Application.Current).Services.GetRequiredService<IFavoritesService>();
        var logger = ((App)Application.Current).Services.GetRequiredService<ILoggerService>();
        var viewModel = new ViewModels.FavoritesViewModel(favoritesService, this, logger);
        var window = new FavoritesWindow(viewModel) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowDealExpirationRemindersDialog()
    {
        var dealExpirationService = ((App)Application.Current).Services.GetRequiredService<IDealExpirationService>();
        var viewModel = new ViewModels.DealExpirationReminderViewModel(dealExpirationService);
        var window = new DealExpirationRemindersWindow(viewModel) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowWeeklySpecialsDigestDialog()
    {
        var weeklySpecialsService = ((App)Application.Current).Services.GetRequiredService<IWeeklySpecialsService>();
        var dialogService = ((App)Application.Current).Services.GetRequiredService<IDialogService>();
        var viewModel = new ViewModels.WeeklySpecialsDigestViewModel(weeklySpecialsService, dialogService);
        var window = new WeeklySpecialsDigestWindow(viewModel) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowShoppingListsDialog()
    {
        var shoppingListService = ((App)Application.Current).Services.GetRequiredService<Core.Interfaces.IShoppingListService>();
        var dialogService = ((App)Application.Current).Services.GetRequiredService<IDialogService>();
        var viewModel = new ViewModels.ShoppingListViewModel(shoppingListService, dialogService);
        var window = new ShoppingListWindow(viewModel) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowSettingsDialog()
    {
        var settingsService = ((App)Application.Current).Services.GetRequiredService<ISettingsService>();
        var logger = ((App)Application.Current).Services.GetRequiredService<ILoggerService>();
        var dialogService = ((App)Application.Current).Services.GetRequiredService<IDialogService>();
        var viewModel = new ViewModels.SettingsViewModel(settingsService, logger, dialogService);
        var window = new SettingsWindow { DataContext = viewModel, Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    public void ShowMLModelManagementDialog()
    {
        var dataService = ((App)Application.Current).Services.GetRequiredService<Core.Interfaces.IGroceryDataService>();
        var dialogService = ((App)Application.Current).Services.GetRequiredService<IDialogService>();
        var logger = ((App)Application.Current).Services.GetRequiredService<ILoggerService>();
        var settingsService = ((App)Application.Current).Services.GetRequiredService<ISettingsService>();
        
        // Use MLModelPath from settings service
        var modelPath = settingsService.MLModelPath;
        
        var viewModel = new ViewModels.MLModelManagementViewModel(dataService, dialogService, logger, modelPath);
        var window = new MLModelManagementWindow(viewModel) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }
}
