using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using AdvGenPriceComparer.ML.Services;
using AdvGenPriceComparer.WPF.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AdvGenPriceComparer.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for detecting illusory discounts (fake sales) using ML.NET price forecasting
    /// </summary>
    public class IllusoryDiscountDetectionViewModel : ViewModelBase
    {
        private readonly PriceForecastingService _forecastingService;
        private readonly IItemRepository _itemRepository;
        private readonly IPriceRecordRepository _priceRecordRepository;
        private readonly IPlaceRepository _placeRepository;

        private bool _isLoading;
        private string _loadingMessage = "";
        private string _title = "Illusory Discount Detection";
        private ObservableCollection<IllusoryDiscountInfo> _detectedDiscounts = new();
        private ObservableCollection<PriceAnomalyInfo> _allAnomalies = new();
        private int _totalItemsScanned;
        private int _illusoryDiscountsFound;
        private int _genuineDealsFound;
        private decimal _averageSavingsPercentage;
        private string _scanStatus = "Ready to scan";
        private Item? _selectedItem;
        private ObservableCollection<Item> _items = new();

        public IllusoryDiscountDetectionViewModel(
            PriceForecastingService forecastingService,
            IItemRepository itemRepository,
            IPriceRecordRepository priceRecordRepository,
            IPlaceRepository placeRepository)
        {
            _forecastingService = forecastingService;
            _itemRepository = itemRepository;
            _priceRecordRepository = priceRecordRepository;
            _placeRepository = placeRepository;

            ScanAllItemsCommand = new RelayCommand(async () => await ScanAllItemsAsync(), () => !IsLoading);
            ScanSelectedItemCommand = new RelayCommand(async () => await ScanSelectedItemAsync(), () => !IsLoading && SelectedItem != null);
            ClearResultsCommand = new RelayCommand(ClearResults, () => DetectedDiscounts.Any() || AllAnomalies.Any());
            ExportResultsCommand = new RelayCommand(ExportResults, () => DetectedDiscounts.Any());

            LoadItems();
        }

        #region Properties

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    // Refresh command can execute states
                    ((RelayCommand)ScanAllItemsCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ScanSelectedItemCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        }

        public string ScanStatus
        {
            get => _scanStatus;
            set => SetProperty(ref _scanStatus, value);
        }

        public ObservableCollection<Item> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public Item? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    ((RelayCommand)ScanSelectedItemCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<IllusoryDiscountInfo> DetectedDiscounts
        {
            get => _detectedDiscounts;
            set => SetProperty(ref _detectedDiscounts, value);
        }

        public ObservableCollection<PriceAnomalyInfo> AllAnomalies
        {
            get => _allAnomalies;
            set => SetProperty(ref _allAnomalies, value);
        }

        public int TotalItemsScanned
        {
            get => _totalItemsScanned;
            set => SetProperty(ref _totalItemsScanned, value);
        }

        public int IllusoryDiscountsFound
        {
            get => _illusoryDiscountsFound;
            set => SetProperty(ref _illusoryDiscountsFound, value);
        }

        public int GenuineDealsFound
        {
            get => _genuineDealsFound;
            set => SetProperty(ref _genuineDealsFound, value);
        }

        public decimal AverageSavingsPercentage
        {
            get => _averageSavingsPercentage;
            set => SetProperty(ref _averageSavingsPercentage, value);
        }

        public bool HasDetectedDiscounts => DetectedDiscounts.Any();
        public bool HasAnomalies => AllAnomalies.Any();
        public bool ShowNoResultsMessage => !IsLoading && !HasDetectedDiscounts && !HasAnomalies;

        #endregion

        #region Commands

        public ICommand ScanAllItemsCommand { get; }
        public ICommand ScanSelectedItemCommand { get; }
        public ICommand ClearResultsCommand { get; }
        public ICommand ExportResultsCommand { get; }

        #endregion

        #region Methods

        private void LoadItems()
        {
            try
            {
                var items = _itemRepository.GetAll().OrderBy(i => i.Name).ToList();
                Items = new ObservableCollection<Item>(items);
            }
            catch (Exception ex)
            {
                ScanStatus = $"Error loading items: {ex.Message}";
            }
        }

        private async Task ScanAllItemsAsync()
        {
            IsLoading = true;
            LoadingMessage = "Scanning all items for illusory discounts...";
            ScanStatus = "Scanning in progress...";
            DetectedDiscounts.Clear();
            AllAnomalies.Clear();

            try
            {
                var items = _itemRepository.GetAll().ToList();
                var places = _placeRepository.GetAll().ToDictionary(p => p.Id, p => p);
                TotalItemsScanned = 0;
                IllusoryDiscountsFound = 0;
                GenuineDealsFound = 0;
                decimal totalSavingsPercent = 0;
                int dealsCount = 0;

                foreach (var item in items)
                {
                    LoadingMessage = $"Scanning {item.Name} ({TotalItemsScanned + 1}/{items.Count})...";
                    
                    var priceRecords = _priceRecordRepository.GetByItem(item.Id).ToList();
                    
                    if (priceRecords.Count >= PriceForecastingService.MinimumDataPoints)
                    {
                        var currentSale = priceRecords
                            .Where(pr => pr.IsOnSale)
                            .OrderByDescending(pr => pr.DateRecorded)
                            .FirstOrDefault();

                        if (currentSale != null)
                        {
                            var history = PriceForecastingService.ConvertPriceRecords(
                                item.Id, 
                                item.Name, 
                                priceRecords,
                                places.TryGetValue(currentSale.PlaceId, out var place) ? place.Name : null,
                                item.Category);

                            // Detect illusory discounts
                            var illusoryDiscounts = _forecastingService.DetectIllusoryDiscounts(
                                item.Id,
                                item.Name,
                                history,
                                (float)currentSale.Price,
                                true);

                            if (illusoryDiscounts.Any())
                            {
                                foreach (var discount in illusoryDiscounts)
                                {
                                    var info = new IllusoryDiscountInfo
                                    {
                                        ItemId = item.Id,
                                        ItemName = item.Name,
                                        Category = item.Category ?? "Uncategorized",
                                        Brand = item.Brand ?? "",
                                        CurrentPrice = currentSale.Price,
                                        AveragePrice = (decimal)discount.ExpectedPrice,
                                        SavingsPercentage = (discount.ExpectedPrice - (float)currentSale.Price) / discount.ExpectedPrice * 100,
                                        IsIllusory = true,
                                        StoreName = places.TryGetValue(currentSale.PlaceId, out var p) ? p.Name : "Unknown",
                                        DetectionDate = DateTime.Now,
                                        Description = discount.Description,
                                        ConfidenceScore = discount.AnomalyScore
                                    };
                                    
                                    // Only add if it's truly illusory (savings < 5%)
                                    if (info.SavingsPercentage < 5)
                                    {
                                        DetectedDiscounts.Add(info);
                                        IllusoryDiscountsFound++;
                                    }
                                    else
                                    {
                                        GenuineDealsFound++;
                                        totalSavingsPercent += (decimal)info.SavingsPercentage;
                                        dealsCount++;
                                    }
                                }
                            }
                            else
                            {
                                // It's a genuine deal
                                var avgPrice = priceRecords.Where(pr => !pr.IsOnSale).Select(pr => pr.Price).DefaultIfEmpty(currentSale.Price).Average();
                                var savings = avgPrice - currentSale.Price;
                                var savingsPercent = avgPrice > 0 ? savings / avgPrice * 100 : 0;

                                if (savingsPercent > 5)
                                {
                                    GenuineDealsFound++;
                                    totalSavingsPercent += savingsPercent;
                                    dealsCount++;
                                }
                            }

                            // Also detect anomalies
                            var anomalies = _forecastingService.DetectAnomalies(item.Id, item.Name, history);
                            foreach (var anomaly in anomalies)
                            {
                                AllAnomalies.Add(new PriceAnomalyInfo
                                {
                                    ItemId = item.Id,
                                    ItemName = item.Name,
                                    Date = anomaly.Date,
                                    Type = anomaly.Type.ToString(),
                                    ActualPrice = (decimal)anomaly.ActualPrice,
                                    ExpectedPrice = (decimal)anomaly.ExpectedPrice,
                                    Description = anomaly.Description,
                                    IsIllusoryDiscount = anomaly.Type == AnomalyType.IllusoryDiscount
                                });
                            }
                        }
                    }

                    TotalItemsScanned++;
                    
                    // Allow UI to update
                    await Task.Delay(1);
                }

                AverageSavingsPercentage = dealsCount > 0 ? totalSavingsPercent / dealsCount : 0;
                ScanStatus = $"Scan complete. Found {IllusoryDiscountsFound} illusory discounts and {GenuineDealsFound} genuine deals from {TotalItemsScanned} items scanned.";
            }
            catch (Exception ex)
            {
                ScanStatus = $"Error during scan: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(HasDetectedDiscounts));
                OnPropertyChanged(nameof(HasAnomalies));
                OnPropertyChanged(nameof(ShowNoResultsMessage));
                ((RelayCommand)ClearResultsCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ExportResultsCommand).RaiseCanExecuteChanged();
            }
        }

        private async Task ScanSelectedItemAsync()
        {
            if (SelectedItem == null) return;

            IsLoading = true;
            LoadingMessage = $"Scanning {SelectedItem.Name}...";
            ScanStatus = "Scanning selected item...";
            DetectedDiscounts.Clear();
            AllAnomalies.Clear();

            try
            {
                var item = SelectedItem;
                var places = _placeRepository.GetAll().ToDictionary(p => p.Id, p => p);
                var priceRecords = _priceRecordRepository.GetByItem(item.Id).ToList();

                TotalItemsScanned = 1;

                if (priceRecords.Count < PriceForecastingService.MinimumDataPoints)
                {
                    ScanStatus = $"Insufficient data for {item.Name}. Need at least {PriceForecastingService.MinimumDataPoints} price records, found {priceRecords.Count}.";
                    return;
                }

                var currentSale = priceRecords
                    .Where(pr => pr.IsOnSale)
                    .OrderByDescending(pr => pr.DateRecorded)
                    .FirstOrDefault();

                if (currentSale == null)
                {
                    ScanStatus = $"{item.Name} is not currently on sale.";
                    return;
                }

                var history = PriceForecastingService.ConvertPriceRecords(
                    item.Id,
                    item.Name,
                    priceRecords,
                    places.TryGetValue(currentSale.PlaceId, out var place) ? place.Name : null,
                    item.Category);

                // Detect illusory discounts
                var illusoryDiscounts = _forecastingService.DetectIllusoryDiscounts(
                    item.Id,
                    item.Name,
                    history,
                    (float)currentSale.Price,
                    true);

                if (illusoryDiscounts.Any())
                {
                    foreach (var discount in illusoryDiscounts)
                    {
                        var info = new IllusoryDiscountInfo
                        {
                            ItemId = item.Id,
                            ItemName = item.Name,
                            Category = item.Category ?? "Uncategorized",
                            Brand = item.Brand ?? "",
                            CurrentPrice = currentSale.Price,
                            AveragePrice = (decimal)discount.ExpectedPrice,
                            SavingsPercentage = (discount.ExpectedPrice - (float)currentSale.Price) / discount.ExpectedPrice * 100,
                            IsIllusory = true,
                            StoreName = places.TryGetValue(currentSale.PlaceId, out var p) ? p.Name : "Unknown",
                            DetectionDate = DateTime.Now,
                            Description = discount.Description,
                            ConfidenceScore = discount.AnomalyScore
                        };
                        
                        DetectedDiscounts.Add(info);
                        IllusoryDiscountsFound = 1;
                    }
                    ScanStatus = $"⚠️ {item.Name}: Illusory discount detected! Current 'sale' price is not significantly better than average.";
                }
                else
                {
                    var avgPrice = priceRecords.Where(pr => !pr.IsOnSale).Select(pr => pr.Price).DefaultIfEmpty(currentSale.Price).Average();
                    var savings = avgPrice - currentSale.Price;
                    var savingsPercent = avgPrice > 0 ? savings / avgPrice * 100 : 0;

                    var info = new IllusoryDiscountInfo
                    {
                        ItemId = item.Id,
                        ItemName = item.Name,
                        Category = item.Category ?? "Uncategorized",
                        Brand = item.Brand ?? "",
                        CurrentPrice = currentSale.Price,
                        AveragePrice = avgPrice,
                        SavingsPercentage = (float)savingsPercent,
                        IsIllusory = false,
                        StoreName = places.TryGetValue(currentSale.PlaceId, out var p) ? p.Name : "Unknown",
                        DetectionDate = DateTime.Now,
                        Description = $"Genuine discount: Save ${savings:F2} ({savingsPercent:F1}% off average price)",
                        ConfidenceScore = 1.0f
                    };
                    
                    DetectedDiscounts.Add(info);
                    GenuineDealsFound = 1;
                    AverageSavingsPercentage = savingsPercent;
                    ScanStatus = $"✓ {item.Name}: Genuine deal detected! Save {savingsPercent:F1}% off average price.";
                }

                // Also detect anomalies
                var anomalies = _forecastingService.DetectAnomalies(item.Id, item.Name, history);
                foreach (var anomaly in anomalies)
                {
                    AllAnomalies.Add(new PriceAnomalyInfo
                    {
                        ItemId = item.Id,
                        ItemName = item.Name,
                        Date = anomaly.Date,
                        Type = anomaly.Type.ToString(),
                        ActualPrice = (decimal)anomaly.ActualPrice,
                        ExpectedPrice = (decimal)anomaly.ExpectedPrice,
                        Description = anomaly.Description,
                        IsIllusoryDiscount = anomaly.Type == AnomalyType.IllusoryDiscount
                    });
                }
            }
            catch (Exception ex)
            {
                ScanStatus = $"Error scanning item: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(HasDetectedDiscounts));
                OnPropertyChanged(nameof(HasAnomalies));
                OnPropertyChanged(nameof(ShowNoResultsMessage));
                ((RelayCommand)ClearResultsCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ExportResultsCommand).RaiseCanExecuteChanged();
            }
        }

        private void ClearResults()
        {
            DetectedDiscounts.Clear();
            AllAnomalies.Clear();
            TotalItemsScanned = 0;
            IllusoryDiscountsFound = 0;
            GenuineDealsFound = 0;
            AverageSavingsPercentage = 0;
            ScanStatus = "Ready to scan";
            OnPropertyChanged(nameof(HasDetectedDiscounts));
            OnPropertyChanged(nameof(HasAnomalies));
            OnPropertyChanged(nameof(ShowNoResultsMessage));
            ((RelayCommand)ClearResultsCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ExportResultsCommand).RaiseCanExecuteChanged();
        }

        private void ExportResults()
        {
            // This will be implemented if needed
            ScanStatus = "Export functionality coming soon...";
        }

        #endregion
    }

    /// <summary>
    /// Information about a detected discount (illusory or genuine)
    /// </summary>
    public class IllusoryDiscountInfo
    {
        public string ItemId { get; set; } = "";
        public string ItemName { get; set; } = "";
        public string Category { get; set; } = "";
        public string Brand { get; set; } = "";
        public decimal CurrentPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public float SavingsPercentage { get; set; }
        public bool IsIllusory { get; set; }
        public string StoreName { get; set; } = "";
        public DateTime DetectionDate { get; set; }
        public string Description { get; set; } = "";
        public float ConfidenceScore { get; set; }

        public string StatusText => IsIllusory ? "⚠️ Illusory Discount" : "✓ Genuine Deal";
        public string StatusColor => IsIllusory ? "#dc2626" : "#16a34a";
        public string SavingsText => $"{SavingsPercentage:F1}%";
        public string PriceComparisonText => $"${CurrentPrice:F2} vs avg ${AveragePrice:F2}";
    }

    /// <summary>
    /// Information about a price anomaly
    /// </summary>
    public class PriceAnomalyInfo
    {
        public string ItemId { get; set; } = "";
        public string ItemName { get; set; } = "";
        public DateTime Date { get; set; }
        public string Type { get; set; } = "";
        public decimal ActualPrice { get; set; }
        public decimal ExpectedPrice { get; set; }
        public string Description { get; set; } = "";
        public bool IsIllusoryDiscount { get; set; }

        public string TypeDisplay => Type switch
        {
            "IllusoryDiscount" => "🎭 Illusory Discount",
            "PriceSpike" => "📈 Price Spike",
            "PriceDrop" => "📉 Price Drop",
            "Seasonal" => "📅 Seasonal",
            _ => Type
        };
    }
}
