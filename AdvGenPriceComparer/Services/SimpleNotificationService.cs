using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AdvGenPriceComparer.Desktop.WinUI.Services;

public class SimpleNotificationService : INotificationService
{
    private XamlRoot? _xamlRoot;

    public void Initialize(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public async Task ShowSuccessAsync(string message)
    {
        await ShowNotificationAsync("Success", message, "✅");
    }

    public async Task ShowErrorAsync(string message)
    {
        await ShowNotificationAsync("Error", message, "❌");
    }

    public async Task ShowWarningAsync(string message)
    {
        await ShowNotificationAsync("Warning", message, "⚠️");
    }

    public async Task ShowInfoAsync(string message)
    {
        await ShowNotificationAsync("Information", message, "ℹ️");
    }

    private async Task ShowNotificationAsync(string title, string message, string icon)
    {
        if (_xamlRoot == null) return;

        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        content.Children.Add(new TextBlock
        {
            Text = icon,
            FontSize = 18,
            VerticalAlignment = VerticalAlignment.Center
        });

        content.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            MaxWidth = 400
        });

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }
}