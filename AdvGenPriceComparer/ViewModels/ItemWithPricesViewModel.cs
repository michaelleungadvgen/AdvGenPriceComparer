using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;
using Microsoft.UI.Xaml.Media;

namespace AdvGenPriceComparer.Desktop.WinUI.ViewModels;

public class ItemWithPricesViewModel : BaseViewModel
{
    private readonly Item _item;
    private readonly IGroceryDataService _groceryDataService;
    private string _currentPriceDisplay = "No price data";
    private string _priceTrendIcon = "📊";
    private string _priceTrendText = "";
    private SolidColorBrush _priceTrendColor = new(Microsoft.UI.Colors.Gray);

    public ItemWithPricesViewModel(Item item, IGroceryDataService groceryDataService = null, IEnumerable<PriceRecord> priceRecords = null)
    {
        _item = item;
        _groceryDataService = groceryDataService;
        PriceHistory = new ObservableCollection<PriceRecordViewModel>();
        
        if (priceRecords != null)
        {
            LoadPriceHistory(priceRecords);
        }
        
        UpdatePriceTrend();
    }

    // Item Properties
    public string Id => _item.Id ?? "";
    public string Name => _item.Name ?? "Unnamed Item";
    public string Brand => string.IsNullOrEmpty(_item.Brand) ? "Unknown Brand" : _item.Brand;
    public string Description => _item.Description ?? "";
    public string Category => _item.Category ?? "Uncategorized";
    public string SubCategory => _item.SubCategory ?? "";
    public string PackageSize => _item.PackageSize ?? "";
    public string Unit => _item.Unit ?? "";
    public string Barcode => _item.Barcode ?? "";

    // Price-related Properties
    public ObservableCollection<PriceRecordViewModel> PriceHistory { get; }

    public string CurrentPriceDisplay
    {
        get => _currentPriceDisplay;
        private set => SetProperty(ref _currentPriceDisplay, value);
    }

    public string PriceTrendIcon
    {
        get => _priceTrendIcon;
        private set => SetProperty(ref _priceTrendIcon, value);
    }

    public string PriceTrendText
    {
        get => _priceTrendText;
        private set => SetProperty(ref _priceTrendText, value);
    }

    public SolidColorBrush PriceTrendColor
    {
        get => _priceTrendColor;
        private set => SetProperty(ref _priceTrendColor, value);
    }

    public int PriceHistoryCount => PriceHistory.Count;
    public bool HasNoPriceHistory => PriceHistory.Count == 0;

    // Methods
    public void LoadPriceHistory(IEnumerable<PriceRecord> priceRecords)
    {
        PriceHistory.Clear();
        
        var orderedRecords = priceRecords
            .Where(pr => pr.ItemId == _item.Id)
            .OrderByDescending(pr => pr.DateRecorded)
            .Take(20); // Limit to most recent 20 records

        foreach (var record in orderedRecords)
        {
            PriceHistory.Add(new PriceRecordViewModel(record, _groceryDataService));
        }

        UpdatePriceTrend();
        OnPropertyChanged(nameof(PriceHistoryCount));
        OnPropertyChanged(nameof(HasNoPriceHistory));
    }

    public void AddPriceRecord(PriceRecord record)
    {
        var viewModel = new PriceRecordViewModel(record, _groceryDataService);
        PriceHistory.Insert(0, viewModel); // Add to beginning (most recent)
        
        // Keep only most recent 20 records
        while (PriceHistory.Count > 20)
        {
            PriceHistory.RemoveAt(PriceHistory.Count - 1);
        }

        UpdatePriceTrend();
        OnPropertyChanged(nameof(PriceHistoryCount));
        OnPropertyChanged(nameof(HasNoPriceHistory));
    }

    private void UpdatePriceTrend()
    {
        if (PriceHistory.Count == 0)
        {
            CurrentPriceDisplay = "No price data";
            PriceTrendIcon = "📊";
            PriceTrendText = "";
            PriceTrendColor = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            return;
        }

        var latestPrice = PriceHistory.First();
        CurrentPriceDisplay = $"${latestPrice.Price:F2}";

        if (PriceHistory.Count > 1)
        {
            var previousPrice = PriceHistory.Skip(1).FirstOrDefault();
            if (previousPrice != null)
            {
                var priceDiff = latestPrice.Price - previousPrice.Price;
                var percentChange = Math.Abs(priceDiff) / previousPrice.Price * 100;

                if (Math.Abs(priceDiff) < 0.01m) // Less than 1 cent difference
                {
                    PriceTrendIcon = "➖";
                    PriceTrendText = "No change";
                    PriceTrendColor = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                }
                else if (priceDiff > 0)
                {
                    PriceTrendIcon = "📈";
                    PriceTrendText = $"+${priceDiff:F2} ({percentChange:F1}%)";
                    PriceTrendColor = new SolidColorBrush(Microsoft.UI.Colors.Red);
                }
                else
                {
                    PriceTrendIcon = "📉";
                    PriceTrendText = $"-${Math.Abs(priceDiff):F2} ({percentChange:F1}%)";
                    PriceTrendColor = new SolidColorBrush(Microsoft.UI.Colors.Green);
                }
            }
        }
        else
        {
            PriceTrendIcon = "🆕";
            PriceTrendText = "New item";
            PriceTrendColor = new SolidColorBrush(Microsoft.UI.Colors.Blue);
        }
    }

    public Item GetItem() => _item;
}

public class PriceRecordViewModel : BaseViewModel
{
    private readonly PriceRecord _record;
    private readonly IGroceryDataService _groceryDataService;

    public PriceRecordViewModel(PriceRecord record, IGroceryDataService groceryDataService = null)
    {
        _record = record;
        _groceryDataService = groceryDataService;
        
        // Determine status based on age
        var daysSinceRecorded = (DateTime.Now - record.DateRecorded).Days;
        
        if (daysSinceRecorded == 0)
        {
            StatusText = "Today";
            StatusColor = new SolidColorBrush(Microsoft.UI.Colors.Green);
        }
        else if (daysSinceRecorded <= 7)
        {
            StatusText = "Recent";
            StatusColor = new SolidColorBrush(Microsoft.UI.Colors.Blue);
        }
        else if (daysSinceRecorded <= 30)
        {
            StatusText = "Current";
            StatusColor = new SolidColorBrush(Microsoft.UI.Colors.Orange);
        }
        else
        {
            StatusText = "Old";
            StatusColor = new SolidColorBrush(Microsoft.UI.Colors.Gray);
        }
    }

    public string Id => _record.Id ?? "";
    public decimal Price => _record.Price;
    public DateTime RecordedDate => _record.DateRecorded;
    public string StoreName
    {
        get
        {
            if (_groceryDataService != null && !string.IsNullOrEmpty(_record.PlaceId))
            {
                var place = _groceryDataService.GetPlaceById(_record.PlaceId);
                if (place != null)
                {
                    return place.Name;
                }
            }
            return "Store";
        }
    }
    public string StoreLocation => FormatStoreLocation();
    public string StatusText { get; }
    public SolidColorBrush StatusColor { get; }

    private string FormatStoreLocation()
    {
        if (_groceryDataService != null && !string.IsNullOrEmpty(_record.PlaceId))
        {
            var place = _groceryDataService.GetPlaceById(_record.PlaceId);
            if (place != null)
            {
                var locationParts = new List<string>();
                if (!string.IsNullOrEmpty(place.Suburb)) locationParts.Add(place.Suburb);
                if (!string.IsNullOrEmpty(place.State)) locationParts.Add(place.State);

                if (locationParts.Any())
                {
                    return string.Join(", ", locationParts);
                }
                return place.Address ?? "Location";
            }
        }
        return "Location";
    }

    public PriceRecord GetPriceRecord() => _record;
}