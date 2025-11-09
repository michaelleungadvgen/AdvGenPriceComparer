using System.Threading.Tasks;

namespace AdvGenPriceComparer.WPF.Services;

public interface INotificationService
{
    Task ShowInfoAsync(string message);
    Task ShowSuccessAsync(string message);
    Task ShowWarningAsync(string message);
    Task ShowErrorAsync(string message);
}
