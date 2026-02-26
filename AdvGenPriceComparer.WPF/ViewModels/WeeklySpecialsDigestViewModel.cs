using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for Weekly Specials Digest window
/// </summary>
public class WeeklySpecialsDigestViewModel : ViewModelBase
{
    private readonly IWeeklySpecialsService _weeklySpecialsService;
    private readonly IDialogService _dialogService;
    private WeeklyDigestReport? _currentReport;
    private string _selectedCategory = "All";
    private string _selectedStore = "All";
    private bool _isLoading;

    public WeeklySpecialsDigestViewModel(IWeeklySpecialsService weeklySpecialsService, IDialogService dialogService)
    {
        _weeklySpecialsService = weeklySpecialsService ?? throw new ArgumentNullException(nameof(weeklySpecialsService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        AllDeals = new ObservableCollection<WeeklySpecialItemViewModel>();
        Categories = new ObservableCollection<string>();
        Stores = new ObservableCollection<string>();

        GenerateDigestCommand = new RelayCommand(GenerateDigest);
        ExportMarkdownCommand = new RelayCommand(ExportToMarkdown);
        ExportTextCommand = new RelayCommand(ExportToText);
        CopyToClipboardCommand = new RelayCommand(CopyToClipboard);
        FilterDealsCommand = new RelayCommand(FilterDeals);

        GenerateDigest();
    }

    public ObservableCollection<WeeklySpecialItemViewModel> AllDeals { get; }
    public ObservableCollection<string> Categories { get; }
    public ObservableCollection<string> Stores { get; }

    public WeeklyDigestReport? CurrentReport
    {
        get => _currentReport;
        private set => SetProperty(ref _currentReport, value);
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                FilterDeals();
            }
        }
    }

    public string SelectedStore
    {
        get => _selectedStore;
        set
        {
            if (SetProperty(ref _selectedStore, value))
            {
                FilterDeals();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ReportTitle => CurrentReport != null 
        ? $"Weekly Specials: {CurrentReport.WeekStart:dd MMM} - {CurrentReport.WeekEnd:dd MMM yyyy}"
        : "Weekly Specials Digest";

    public string SummaryText => CurrentReport != null
        ? $"{CurrentReport.TotalDeals} deals • {CurrentReport.HalfPriceDeals} half-price 🔥"
        : "";

    public ICommand GenerateDigestCommand { get; }
    public ICommand ExportMarkdownCommand { get; }
    public ICommand ExportTextCommand { get; }
    public ICommand CopyToClipboardCommand { get; }
    public ICommand FilterDealsCommand { get; }

    private void GenerateDigest()
    {
        IsLoading = true;

        try
        {
            CurrentReport = _weeklySpecialsService.GenerateWeeklyDigest();
            
            // Update filter options
            Categories.Clear();
            Categories.Add("All");
            foreach (var category in CurrentReport.ByCategory.Keys.OrderBy(c => c))
            {
                Categories.Add(category);
            }

            Stores.Clear();
            Stores.Add("All");
            foreach (var store in CurrentReport.ByStore.Keys.OrderBy(s => s))
            {
                Stores.Add(store);
            }

            FilterDeals();

            OnPropertyChanged(nameof(ReportTitle));
            OnPropertyChanged(nameof(SummaryText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to generate digest: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterDeals()
    {
        if (CurrentReport == null) return;

        AllDeals.Clear();

        IEnumerable<WeeklySpecialItem> deals = CurrentReport.AllDeals;

        if (SelectedCategory != "All")
        {
            deals = deals.Where(d => d.Category == SelectedCategory);
        }

        if (SelectedStore != "All")
        {
            deals = deals.Where(d => d.StoreName == SelectedStore);
        }

        foreach (var deal in deals.OrderByDescending(d => d.SavingsPercentage))
        {
            AllDeals.Add(new WeeklySpecialItemViewModel(deal));
        }
    }

    private void ExportToMarkdown()
    {
        if (CurrentReport == null) return;

        try
        {
            var markdown = _weeklySpecialsService.ExportToMarkdown(CurrentReport);
            var fileName = $"WeeklySpecials_{CurrentReport.WeekStart:yyyyMMdd}.md";
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            File.WriteAllText(path, markdown);
            _dialogService.ShowSuccess($"Digest exported to:\n{path}");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Export failed: {ex.Message}");
        }
    }

    private void ExportToText()
    {
        if (CurrentReport == null) return;

        try
        {
            var text = _weeklySpecialsService.ExportToPlainText(CurrentReport);
            var fileName = $"WeeklySpecials_{CurrentReport.WeekStart:yyyyMMdd}.txt";
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            File.WriteAllText(path, text);
            _dialogService.ShowSuccess($"Digest exported to:\n{path}");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Export failed: {ex.Message}");
        }
    }

    private void CopyToClipboard()
    {
        if (CurrentReport == null) return;

        try
        {
            var markdown = _weeklySpecialsService.ExportToMarkdown(CurrentReport);
            Clipboard.SetText(markdown);
            _dialogService.ShowSuccess("Digest copied to clipboard!");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Copy failed: {ex.Message}");
        }
    }
}

/// <summary>
/// ViewModel wrapper for a weekly special item
/// </summary>
public class WeeklySpecialItemViewModel : ViewModelBase
{
    private readonly WeeklySpecialItem _item;

    public WeeklySpecialItemViewModel(WeeklySpecialItem item)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
    }

    public string ItemName => _item.ItemName;
    public string Brand => _item.Brand;
    public string Category => _item.Category;
    public string StoreName => _item.StoreName;
    public decimal Price => _item.Price;
    public decimal? OriginalPrice => _item.OriginalPrice;
    public decimal Savings => _item.Savings;
    public double SavingsPercentage => _item.SavingsPercentage;
    public bool IsHalfPrice => _item.IsHalfPrice;
    public DateTime ValidFrom => _item.ValidFrom;
    public DateTime ValidTo => _item.ValidTo;

    public string PriceDisplay => $"${Price:F2}";
    public string OriginalPriceDisplay => OriginalPrice.HasValue ? $"${OriginalPrice.Value:F2}" : "";
    public string SavingsDisplay => $"Save ${Savings:F2} ({SavingsPercentage:F0}%)";
    public string HalfPriceBadge => IsHalfPrice ? "🔥 HALF PRICE" : "";

    public string UrgencyColor
    {
        get
        {
            if (IsHalfPrice) return "#F44336"; // Red for half price
            if (SavingsPercentage >= 30) return "#FF9800"; // Orange for 30%+ savings
            if (SavingsPercentage >= 20) return "#FFC107"; // Amber for 20%+ savings
            return "#4CAF50"; // Green for other savings
        }
    }

    public string ValidityDisplay => $"Valid until {ValidTo:ddd, dd MMM}";
}
