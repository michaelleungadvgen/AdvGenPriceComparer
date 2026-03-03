using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.Foundation;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace AdvGenPriceComparer.Desktop.WinUI.Views;

public sealed partial class ReportView : Page
{
    private readonly IGroceryDataService _groceryDataService;
    private ReportViewModel _viewModel;

    public ReportView()
    {
        this.InitializeComponent();
        _groceryDataService = App.Services.GetRequiredService<IGroceryDataService>();
        _viewModel = new ReportViewModel(_groceryDataService);
        this.DataContext = _viewModel;
        
        this.Loaded += ReportView_Loaded;
    }

    private async void ReportView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadReportData();
        DrawCharts();
    }

    private async System.Threading.Tasks.Task LoadReportData()
    {
        try
        {
            await _viewModel.LoadReportDataAsync();
            
            // Update summary cards
            TotalProductsText.Text = _viewModel.TotalProducts.ToString();
            AvgPriceChangeText.Text = $"{_viewModel.AvgPriceChange:+0.0;-0.0;0.0}%";
            BestStoreText.Text = _viewModel.BestStore ?? "N/A";
            PriceUpdatesText.Text = _viewModel.PriceUpdates.ToString();
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Error loading report data: {ex.Message}");
        }
    }

    private void DrawCharts()
    {
        DrawPriceTrendsChart();
        DrawStoreComparisonChart();
        DrawCategoryBreakdownChart();
        PopulateTopProductsList();
    }

    private void DrawPriceTrendsChart()
    {
        PriceTrendsCanvas.Children.Clear();
        
        if (_viewModel.PriceTrendsData == null || !_viewModel.PriceTrendsData.Any())
            return;

        var chartWidth = PriceTrendsChart.ActualWidth - 40;
        var chartHeight = PriceTrendsChart.ActualHeight - 40;
        
        if (chartWidth <= 0 || chartHeight <= 0)
        {
            chartWidth = 600;
            chartHeight = 260;
        }

        var data = _viewModel.PriceTrendsData.ToList();
        var maxPrice = data.SelectMany(d => new[] { d.AvgPrice, d.MinPrice, d.MaxPrice }).Max();
        var minPrice = data.SelectMany(d => new[] { d.AvgPrice, d.MinPrice, d.MaxPrice }).Min();
        var priceRange = maxPrice - minPrice;

        // Draw grid lines
        for (int i = 0; i <= 5; i++)
        {
            var y = 20 + (chartHeight - 40) * i / 5;
            var line = new Line
            {
                X1 = 20,
                X2 = chartWidth + 20,
                Y1 = y,
                Y2 = y,
                Stroke = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                StrokeThickness = 1
            };
            PriceTrendsCanvas.Children.Add(line);
            
            var priceLabel = minPrice + priceRange * (5 - i) / 5;
            var textBlock = new TextBlock
            {
                Text = $"${priceLabel:F2}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
            Canvas.SetLeft(textBlock, 0);
            Canvas.SetTop(textBlock, y - 8);
            PriceTrendsCanvas.Children.Add(textBlock);
        }

        // Draw lines
        DrawPriceLine(data, d => d.AvgPrice, Microsoft.UI.Colors.Blue, chartWidth, chartHeight, minPrice, priceRange);
        DrawPriceLine(data, d => d.MinPrice, Microsoft.UI.Colors.Green, chartWidth, chartHeight, minPrice, priceRange);
        DrawPriceLine(data, d => d.MaxPrice, Microsoft.UI.Colors.Orange, chartWidth, chartHeight, minPrice, priceRange);
    }

    private void DrawPriceLine(List<PriceTrendPoint> data, Func<PriceTrendPoint, decimal> priceSelector, 
        Windows.UI.Color color, double chartWidth, double chartHeight, decimal minPrice, decimal priceRange)
    {
        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            Points = new PointCollection()
        };

        for (int i = 0; i < data.Count; i++)
        {
            var x = 20 + (chartWidth - 40) * i / Math.Max(1, data.Count - 1);
            var price = priceSelector(data[i]);
            var y = 20 + (chartHeight - 40) * (1 - (double)((price - minPrice) / Math.Max(0.01m, priceRange)));
            polyline.Points.Add(new Point(x, y));
        }

        PriceTrendsCanvas.Children.Add(polyline);
    }

    private void DrawStoreComparisonChart()
    {
        StoreComparisonCanvas.Children.Clear();
        
        if (_viewModel.StoreComparisonData == null || !_viewModel.StoreComparisonData.Any())
            return;

        var chartWidth = StoreComparisonChart.ActualWidth - 40;
        var chartHeight = StoreComparisonChart.ActualHeight - 40;
        
        if (chartWidth <= 0 || chartHeight <= 0)
        {
            chartWidth = 600;
            chartHeight = 210;
        }

        var data = _viewModel.StoreComparisonData.ToList();
        var maxPrice = data.Max(d => d.AvgPrice);

        var barWidth = (chartWidth - 40) / data.Count * 0.8;
        var barSpacing = (chartWidth - 40) / data.Count * 0.2;

        var colors = new[] { Microsoft.UI.Colors.Blue, Microsoft.UI.Colors.Green, Microsoft.UI.Colors.Orange, Microsoft.UI.Colors.Red, Microsoft.UI.Colors.Purple };

        for (int i = 0; i < data.Count; i++)
        {
            var store = data[i];
            var barHeight = (double)(store.AvgPrice / maxPrice) * (chartHeight - 40);
            var x = 20 + i * (barWidth + barSpacing);
            var y = 20 + (chartHeight - 40) - barHeight;

            var rect = new Rectangle
            {
                Width = barWidth,
                Height = barHeight,
                Fill = new SolidColorBrush(colors[i % colors.Length])
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            StoreComparisonCanvas.Children.Add(rect);

            // Store name
            var storeLabel = new TextBlock
            {
                Text = store.StoreName,
                FontSize = 10,
                HorizontalTextAlignment = TextAlignment.Center,
                Width = barWidth
            };
            Canvas.SetLeft(storeLabel, x);
            Canvas.SetTop(storeLabel, chartHeight - 15);
            StoreComparisonCanvas.Children.Add(storeLabel);

            // Price label
            var priceLabel = new TextBlock
            {
                Text = $"${store.AvgPrice:F2}",
                FontSize = 9,
                HorizontalTextAlignment = TextAlignment.Center,
                Width = barWidth,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
            };
            Canvas.SetLeft(priceLabel, x);
            Canvas.SetTop(priceLabel, y + 5);
            StoreComparisonCanvas.Children.Add(priceLabel);
        }
    }

    private void DrawCategoryBreakdownChart()
    {
        CategoryBreakdownCanvas.Children.Clear();
        CategoryLegend.Children.Clear();
        
        if (_viewModel.CategoryBreakdownData == null || !_viewModel.CategoryBreakdownData.Any())
            return;

        var data = _viewModel.CategoryBreakdownData.ToList();
        var total = data.Sum(d => d.TotalSpent);
        
        var centerX = CategoryBreakdownChart.ActualWidth / 2;
        var centerY = CategoryBreakdownChart.ActualHeight / 2;
        var radius = Math.Min(centerX, centerY) - 20;

        if (centerX <= 0 || centerY <= 0)
        {
            centerX = 150;
            centerY = 150;
            radius = 130;
        }

        var colors = new[] { 
            Microsoft.UI.Colors.Blue, Microsoft.UI.Colors.Green, Microsoft.UI.Colors.Orange, 
            Microsoft.UI.Colors.Red, Microsoft.UI.Colors.Purple, Microsoft.UI.Colors.Teal,
            Microsoft.UI.Colors.Brown, Microsoft.UI.Colors.Pink 
        };

        double startAngle = 0;
        
        for (int i = 0; i < data.Count; i++)
        {
            var category = data[i];
            var percentage = (double)(category.TotalSpent / total);
            var sweepAngle = percentage * 360;

            // Create pie slice (simplified - using arc segments would be more complex)
            var ellipse = new Ellipse
            {
                Width = radius * 2 * percentage,
                Height = radius * 2 * percentage,
                Fill = new SolidColorBrush(colors[i % colors.Length])
            };
            
            // Position the slice (simplified positioning)
            var x = centerX + Math.Cos(startAngle * Math.PI / 180) * radius * 0.5;
            var y = centerY + Math.Sin(startAngle * Math.PI / 180) * radius * 0.5;
            
            Canvas.SetLeft(ellipse, x - ellipse.Width / 2);
            Canvas.SetTop(ellipse, y - ellipse.Height / 2);
            CategoryBreakdownCanvas.Children.Add(ellipse);

            // Add to legend
            var legendItem = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
            
            var colorRect = new Rectangle
            {
                Width = 16,
                Height = 16,
                Fill = new SolidColorBrush(colors[i % colors.Length]),
                Margin = new Thickness(0, 0, 8, 0)
            };
            legendItem.Children.Add(colorRect);
            
            var labelText = new TextBlock
            {
                Text = $"{category.Category} ({percentage:P1})",
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            legendItem.Children.Add(labelText);
            
            CategoryLegend.Children.Add(legendItem);

            startAngle += sweepAngle;
        }
    }

    private void PopulateTopProductsList()
    {
        TopProductsList.ItemsSource = _viewModel.TopProductsData;
    }

    private async void TimePeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            var selectedItem = TimePeriodComboBox.SelectedItem as ComboBoxItem;
            _viewModel.SelectedTimePeriod = selectedItem?.Content?.ToString() ?? "Last 7 Days";
            await LoadReportData();
            DrawCharts();
        }
    }

    private async void StoreChain_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            var selectedItem = StoreChainComboBox.SelectedItem as ComboBoxItem;
            _viewModel.SelectedStoreChain = selectedItem?.Content?.ToString() ?? "All Stores";
            await LoadReportData();
            DrawCharts();
        }
    }

    private async void RefreshData_Click(object sender, RoutedEventArgs e)
    {
        await LoadReportData();
        DrawCharts();
    }

    private async void ExportToCsv_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement CSV export
        var dialog = new ContentDialog
        {
            Title = "Export to CSV",
            Content = "CSV export functionality will be implemented soon.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async void ExportToPdf_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            savePicker.FileTypeChoices.Add("PDF Document", new List<string>() { ".pdf" });
            savePicker.SuggestedFileName = "PriceComparisonReport";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    // Create PDF document
                    Document document = new Document(PageSize.A4, 36, 36, 54, 36);
                    PdfWriter writer = PdfWriter.GetInstance(document, stream);
                    document.Open();

                    // Fonts
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                    // Title
                    document.Add(new Paragraph("Grocery Price Comparison Report", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 20 });
                    document.Add(new Paragraph($"Date: {DateTime.Now:d}", normalFont) { SpacingAfter = 10 });
                    document.Add(new Paragraph($"Time Period: {_viewModel.SelectedTimePeriod}", normalFont) { SpacingAfter = 20 });

                    // Summary Section
                    document.Add(new Paragraph("Summary", headerFont) { SpacingAfter = 10 });
                    var summaryTable = new PdfPTable(2) { WidthPercentage = 100 };
                    summaryTable.SetWidths(new float[] { 1f, 1f });

                    summaryTable.AddCell(new PdfPCell(new Phrase("Total Products:", boldFont)) { Border = 0 });
                    summaryTable.AddCell(new PdfPCell(new Phrase(_viewModel.TotalProducts.ToString(), normalFont)) { Border = 0 });

                    summaryTable.AddCell(new PdfPCell(new Phrase("Avg Price Change:", boldFont)) { Border = 0 });
                    summaryTable.AddCell(new PdfPCell(new Phrase($"{_viewModel.AvgPriceChange:+0.0;-0.0;0.0}%", normalFont)) { Border = 0 });

                    summaryTable.AddCell(new PdfPCell(new Phrase("Best Store:", boldFont)) { Border = 0 });
                    summaryTable.AddCell(new PdfPCell(new Phrase(_viewModel.BestStore ?? "N/A", normalFont)) { Border = 0 });

                    summaryTable.AddCell(new PdfPCell(new Phrase("Price Updates:", boldFont)) { Border = 0 });
                    summaryTable.AddCell(new PdfPCell(new Phrase(_viewModel.PriceUpdates.ToString(), normalFont)) { Border = 0 });

                    document.Add(summaryTable);
                    document.Add(new Paragraph(" ") { SpacingAfter = 10 }); // Spacing

                    // Store Comparison Section
                    if (_viewModel.StoreComparisonData != null && _viewModel.StoreComparisonData.Any())
                    {
                        document.Add(new Paragraph("Store Comparison", headerFont) { SpacingAfter = 10 });
                        var storeTable = new PdfPTable(3) { WidthPercentage = 100 };
                        storeTable.SetWidths(new float[] { 2f, 1f, 1f });

                        // Headers
                        storeTable.AddCell(new PdfPCell(new Phrase("Store", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                        storeTable.AddCell(new PdfPCell(new Phrase("Avg Price", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                        storeTable.AddCell(new PdfPCell(new Phrase("Products", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                        foreach (var store in _viewModel.StoreComparisonData)
                        {
                            storeTable.AddCell(new Phrase(store.StoreName, normalFont));
                            storeTable.AddCell(new Phrase($"${store.AvgPrice:F2}", normalFont));
                            storeTable.AddCell(new Phrase(store.ProductCount.ToString(), normalFont));
                        }
                        document.Add(storeTable);
                        document.Add(new Paragraph(" ") { SpacingAfter = 10 });
                    }

                    // Category Breakdown Section
                    if (_viewModel.CategoryBreakdownData != null && _viewModel.CategoryBreakdownData.Any())
                    {
                        document.Add(new Paragraph("Category Breakdown", headerFont) { SpacingAfter = 10 });
                        var categoryTable = new PdfPTable(3) { WidthPercentage = 100 };
                        categoryTable.SetWidths(new float[] { 2f, 1f, 1f });

                        // Headers
                        categoryTable.AddCell(new PdfPCell(new Phrase("Category", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                        categoryTable.AddCell(new PdfPCell(new Phrase("Total Spent", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                        categoryTable.AddCell(new PdfPCell(new Phrase("Products", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                        foreach (var category in _viewModel.CategoryBreakdownData)
                        {
                            categoryTable.AddCell(new Phrase(category.Category, normalFont));
                            categoryTable.AddCell(new Phrase($"${category.TotalSpent:F2}", normalFont));
                            categoryTable.AddCell(new Phrase(category.ProductCount.ToString(), normalFont));
                        }
                        document.Add(categoryTable);
                        document.Add(new Paragraph(" ") { SpacingAfter = 10 });
                    }

                    // Top Products Section
                    if (_viewModel.TopProductsData != null && _viewModel.TopProductsData.Any())
                    {
                        document.Add(new Paragraph("Top Products", headerFont) { SpacingAfter = 10 });
                        var productTable = new PdfPTable(4) { WidthPercentage = 100 };
                        productTable.SetWidths(new float[] { 2f, 1f, 1f, 1f });

                        // Headers
                        productTable.AddCell(new PdfPCell(new Phrase("Product", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                        productTable.AddCell(new PdfPCell(new Phrase("Avg Price", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                        productTable.AddCell(new PdfPCell(new Phrase("Min Price", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                        productTable.AddCell(new PdfPCell(new Phrase("Max Price", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                        foreach (var product in _viewModel.TopProductsData)
                        {
                            productTable.AddCell(new Phrase(product.ProductName, normalFont));
                            productTable.AddCell(new Phrase($"${product.AvgPrice:F2}", normalFont));
                            productTable.AddCell(new Phrase($"${product.MinPrice:F2}", normalFont));
                            productTable.AddCell(new Phrase($"${product.MaxPrice:F2}", normalFont));
                        }
                        document.Add(productTable);
                    }

                    document.Close();
                }

                var successDialog = new ContentDialog
                {
                    Title = "Export Successful",
                    Content = $"Report exported to {file.Name}.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Export Error",
                Content = $"An error occurred while exporting to PDF: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }

    private async void ShareReport_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement report sharing
        var dialog = new ContentDialog
        {
            Title = "Share Report",
            Content = "Report sharing functionality will be implemented soon.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}