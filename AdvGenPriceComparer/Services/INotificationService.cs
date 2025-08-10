using System.Threading.Tasks;

namespace AdvGenPriceComparer.Desktop.WinUI.Services;

public interface INotificationService
{
    Task ShowSuccessAsync(string message);
    Task ShowErrorAsync(string message);
    Task ShowWarningAsync(string message);
    Task ShowInfoAsync(string message);
}