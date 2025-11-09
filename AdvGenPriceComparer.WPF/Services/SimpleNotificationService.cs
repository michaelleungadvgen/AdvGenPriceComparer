using System.Threading.Tasks;
using System.Windows;

namespace AdvGenPriceComparer.WPF.Services;

public class SimpleNotificationService : INotificationService
{
    public Task ShowInfoAsync(string message)
    {
        MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task ShowSuccessAsync(string message)
    {
        MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task ShowWarningAsync(string message)
    {
        MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }
}
