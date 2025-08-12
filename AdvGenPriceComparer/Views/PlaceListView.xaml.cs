using Microsoft.UI.Xaml.Controls;

namespace AdvGenPriceComparer.Desktop.WinUI.Views
{
    public sealed partial class PlaceListView : Page
    {
        public PlaceListView()
        {
            this.InitializeComponent();
            LoadStores();
        }

        private void LoadStores()
        {
            // TODO: Load stores from database
            // For now, this is a placeholder that will be connected to the database service
        }
    }
}