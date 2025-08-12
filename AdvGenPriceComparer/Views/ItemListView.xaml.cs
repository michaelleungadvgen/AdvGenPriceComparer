using Microsoft.UI.Xaml.Controls;

namespace AdvGenPriceComparer.Desktop.WinUI.Views
{
    public sealed partial class ItemListView : Page
    {
        public ItemListView()
        {
            this.InitializeComponent();
            LoadItems();
        }

        private void LoadItems()
        {
            // TODO: Load items from database
            // For now, this is a placeholder that will be connected to the database service
        }
    }
}