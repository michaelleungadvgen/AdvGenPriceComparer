using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using AdvGenPriceComparer.Desktop.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.Desktop.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        this.Closed += OnClosed;
        
        // Subscribe to store added event
        ViewModel.OnStoreAdded += () => ContentFrame.Navigate(typeof(Views.PlaceListView));
        
        // Initialize services with XamlRoot when content is loaded
        this.Activated += (s, e) => 
        {
            var dialogService = App.Services.GetRequiredService<IDialogService>() as SimpleDialogService;
            dialogService?.Initialize(this.Content.XamlRoot);
            
            var notificationService = App.Services.GetRequiredService<INotificationService>() as SimpleNotificationService;
            notificationService?.Initialize(this.Content.XamlRoot);
        };
    }

    private void OnClosed(object sender, WindowEventArgs e)
    {
        ViewModel.Dispose();
    }

    private void ItemsNav_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.ItemListView));
    }

    private void StoresNav_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.PlaceListView));
    }

    private void CategoriesNav_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.CategoryListView));
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.AddItemCommand.CanExecute(null))
            ViewModel.AddItemCommand.Execute(null);
    }

    private void AddPlace_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.AddPlaceCommand.CanExecute(null))
            ViewModel.AddPlaceCommand.Execute(null);
    }
}
