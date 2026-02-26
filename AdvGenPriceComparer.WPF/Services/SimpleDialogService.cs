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
}
