using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.Views;

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
    void ShowExportDataDialog();
    void ShowImportFromUrlDialog();
    void ShowIllusoryDiscountDetectionDialog();
    void ShowServerDataTransferDialog();
    void ShowBestPricesDialog();
    void ShowEditPlaceDialog(Core.Models.Place place);
    void ShowTripOptimizerDialog();
    void ShowPriceAlertsDialog();
    void ShowWeeklySpecialsImportDialog();
    void ShowCloudSyncDialog();
    void ShowStaticPeerConfigDialog();
    
    // Progress Dialogs
    ExportProgressWindow ShowExportProgressDialog(string title = "Exporting Data...");
    ImportProgressWindow ShowImportProgressDialog(string title = "Importing Data...");
}
