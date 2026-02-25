namespace AdvGenPriceComparer.WPF.Services;

public interface IDialogService
{
    void ShowInfo(string message, string title = "Information");
    void ShowSuccess(string message, string title = "Success");
    void ShowWarning(string message, string title = "Warning");
    void ShowError(string message, string title = "Error");
    bool ShowConfirmation(string message, string title = "Confirmation");
    void ShowComparePricesDialog(string category = null);
}
