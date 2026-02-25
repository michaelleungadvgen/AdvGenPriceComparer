using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

public partial class GlobalSearchWindow : Window
{
    public GlobalSearchViewModel ViewModel { get; }
    public SearchResult? SelectedResult { get; private set; }

    public GlobalSearchWindow(GlobalSearchViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        
        // Focus search box when window opens
        Loaded += (s, e) =>
        {
            if (FindName("SearchTextBox") is TextBox searchBox)
            {
                searchBox.Focus();
            }
        };
    }

    private void ResultBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is SearchResult result)
        {
            SelectedResult = result;
            ViewModel.SelectResultCommand.Execute(result);
            
            // Set dialog result and close
            DialogResult = true;
            Close();
        }
    }
}

/// <summary>
/// Converts relevance score to opacity (higher relevance = more opaque)
/// </summary>
public class RelevanceToOpacityConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double relevance)
        {
            // Minimum opacity of 0.6, maximum of 1.0
            return 0.6 + (relevance * 0.4);
        }
        return 1.0;
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new System.NotImplementedException();
    }
}
