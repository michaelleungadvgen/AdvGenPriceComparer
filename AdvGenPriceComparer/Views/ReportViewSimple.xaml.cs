using Microsoft.UI.Xaml.Controls;

namespace AdvGenPriceComparer.Desktop.WinUI.Views;

public sealed partial class ReportViewSimple : Page
{
    public ReportViewSimple()
    {
        this.InitializeComponent();
        SummaryText.Text = "Simple report page loaded successfully!";
    }
}