using AdvGenPriceComparer.WPF.Models;

namespace AdvGenPriceComparer.WPF.Services;

public interface IDialogService
{
    void ShowInfo(string message, string title = "Information");
    void ShowSuccess(string message, string title = "Success");
    void ShowWarning(string message, string title = "Warning");
    void ShowError(string message, string title = "Error");
    bool ShowConfirmation(string message, string title = "Confirmation");
    bool ShowQuestion(string message, string title = "Question");
    void ShowComparePricesDialog(string category = null);
    SearchResult? ShowGlobalSearchDialog();
    void ShowBarcodeScannerDialog();
    void ShowPriceDropNotificationsDialog();
    void ShowFavoritesDialog();
    void ShowDealExpirationRemindersDialog();
    void ShowWeeklySpecialsDigestDialog();
    void ShowShoppingListsDialog();
    void ShowSettingsDialog();
    void ShowMLModelManagementDialog();
    void ShowPriceForecastDialog();
    void ShowChatDialog();
}
