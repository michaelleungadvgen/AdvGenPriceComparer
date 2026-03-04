using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using AdvGenPriceComparer.ML.Services;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for price forecasting with ML.NET Time Series analysis
/// </summary>
public class PriceForecastViewModel : ViewModelBase
{
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IDialogService _dialogService;
    private readonly ILoggerService _logger;
    private readonly PriceForecastingService _forecastingService;
    
    private ObservableCollection<Item> _items = new();
    private ObservableCollection<Place> _places = new();
    private Item? _selectedItem;
    private Place? _selectedPlace;
    private int _forecastDays = 30;
    private bool _isLoading;
    private string _loadingMessage = string.Empty;
    
    private PriceForecastResult? _forecastResult;
    private ObservableCollection<PriceForecast> _forecasts = new();
    private ObservableCollection<PriceAnomaly> _anomalies = new();
    
    private ISeries[] _forecastSeries = Array.Empty<ISeries>();
    private Axis[] _chartXAxes = Array.Empty<Axis>();
    private Axis[] _chartYAxes = Array.Empty<Axis>();
    
    private string _optimalBuyingDateText = "N/A";
    private string _optimalBuyingPriceText = "N/A";
    private string _currentPriceText = "N/A";
    private string _predictedLowText = "N/A";
    private string _predictedHighText = "N/A";
    private string _predictedAvgText = "N/A";
    private string _overallRecommendationText = "Select an item to generate forecast";
    private string _recommendationColor = "#666";

    public PriceForecastViewModel(
        IPriceRecordRepository priceRecordRepository,
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IDialogService dialogService,
        ILoggerService logger)
    {
        _priceRecordRepository = priceRecordRepository;
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _dialogService = dialogService;
        _logger = logger;
        
        var modelPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer",
            "MLModels");
        _forecastingService = new PriceForecastingService(
            modelPath,
            msg => _logger.LogInfo(msg),
            (msg, ex) => _logger.LogError(msg, ex),
            msg => _logger.LogWarning(msg));

        GenerateForecastCommand = new RelayCommand(async () => await GenerateForecastAsync(), CanGenerateForecast);
        RefreshDataCommand = new RelayCommand(LoadData);
        ClearResultsCommand = new RelayCommand(ClearResults);
        
        LoadData();
    }

    public ObservableCollection<Item> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public ObservableCollection<Place> Places
    {
        get => _places;
        set => SetProperty(ref _places, value);
    }

    public Item? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                ClearResults();
                ((RelayCommand)GenerateForecastCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public Place? SelectedPlace
    {
        get => _selectedPlace;
        set
        {
            if (SetProperty(ref _selectedPlace, value))
            {
                ClearResults();
            }
        }
    }

    public int ForecastDays
    {
        get => _forecastDays;
        set
        {
            if (SetProperty(ref _forecastDays, Math.Clamp(value, 7, 90)))
            {
                ((RelayCommand)GenerateForecastCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        set => SetProperty(ref _loadingMessage, value);
    }

    public PriceForecastResult? ForecastResult
    {
        get => _forecastResult;
        set => SetProperty(ref _forecastResult, value);
    }

    public ObservableCollection<PriceForecast> Forecasts
    {
        get => _forecasts;
        set => SetProperty(ref _forecasts, value);
    }

    public ObservableCollection<PriceAnomaly> Anomalies
    {
        get => _anomalies;
        set => SetProperty(ref _anomalies, value);
    }

    public bool HasForecasts => Forecasts.Count > 0;
    public bool HasAnomalies => Anomalies.Count > 0;
    public bool ShowNoForecastsMessage => !IsLoading && !HasForecasts && SelectedItem != null;
    public bool ShowNoAnomaliesMessage => !IsLoading && !HasAnomalies && HasForecasts;

    public ISeries[] ForecastSeries
    {
        get => _forecastSeries;
        set => SetProperty(ref _forecastSeries, value);
    }

    public Axis[] ChartXAxes
    {
        get => _chartXAxes;
        set => SetProperty(ref _chartXAxes, value);
    }

    public Axis[] ChartYAxes
    {
        get => _chartYAxes;
        set => SetProperty(ref _chartYAxes, value);
    }

    public string OptimalBuyingDateText
    {
        get => _optimalBuyingDateText;
        set => SetProperty(ref _optimalBuyingDateText, value);
    }

    public string OptimalBuyingPriceText
    {
        get => _optimalBuyingPriceText;
        set => SetProperty(ref _optimalBuyingPriceText, value);
    }

    public string CurrentPriceText
    {
        get => _currentPriceText;
        set => SetProperty(ref _currentPriceText, value);
    }

    public string PredictedLowText
    {
        get => _predictedLowText;
        set => SetProperty(ref _predictedLowText, value);
    }

    public string PredictedHighText
    {
        get => _predictedHighText;
        set => SetProperty(ref _predictedHighText, value);
    }

    public string PredictedAvgText
    {
        get => _predictedAvgText;
        set => SetProperty(ref _predictedAvgText, value);
    }

    public string OverallRecommendationText
    {
        get => _overallRecommendationText;
        set => SetProperty(ref _overallRecommendationText, value);
    }

    public string RecommendationColor
    {
        get => _recommendationColor;
        set => SetProperty(ref _recommendationColor, value);
    }

    public string Title => "Price Forecast";
    public string Subtitle => SelectedItem != null 
        ? $"Forecasting prices for {SelectedItem.Name}" 
        : "Select an item to generate price predictions";

    public ICommand GenerateForecastCommand { get; }
    public ICommand RefreshDataCommand { get; }
    public ICommand ClearResultsCommand { get; }

    private void LoadData()
    {
        try
        {
            LoadItems();
            LoadPlaces();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load data for price forecast", ex);
            _dialogService.ShowError($"Failed to load data: {ex.Message}");
        }
    }

    private void LoadItems()
    {
        var items = _itemRepository.GetAll().OrderBy(i => i.Name);
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    private void LoadPlaces()
    {
        var places = _placeRepository.GetAll().OrderBy(p => p.Name);
        Places.Clear();
        foreach (var place in places)
        {
            Places.Add(place);
        }
    }

    private bool CanGenerateForecast()
    {
        return SelectedItem != null && ForecastDays >= 7;
    }

    private async Task GenerateForecastAsync()
    {
        if (SelectedItem == null) return;

        IsLoading = true;
        LoadingMessage = $"Generating {ForecastDays}-day forecast for {SelectedItem.Name}...";
        ClearResults();

        try
        {
            var priceHistory = await Task.Run(() => 
                _priceRecordRepository.GetPriceHistory(SelectedItem.Id, null, null).ToList());
            
            if (priceHistory.Count < PriceForecastingService.MinimumDataPoints)
            {
                _dialogService.ShowWarning(
                    $"Insufficient data for forecasting. Need at least {PriceForecastingService.MinimumDataPoints} price records, " +
                    $"but only found {priceHistory.Count}. Please add more price history for this item.");
                IsLoading = false;
                return;
            }

            var historyData = PriceForecastingService.ConvertPriceRecords(
                SelectedItem.Id,
                SelectedItem.Name,
                priceHistory,
                SelectedPlace?.Name,
                SelectedItem.Category);

            var result = await _forecastingService.ForecastPricesAsync(
                SelectedItem.Id,
                SelectedItem.Name,
                historyData,
                ForecastDays);

            if (!result.Success)
            {
                _dialogService.ShowError($"Forecast generation failed: {result.ErrorMessage}");
                IsLoading = false;
                return;
            }

            ForecastResult = result;
            
            Forecasts.Clear();
            foreach (var forecast in result.Forecasts)
            {
                Forecasts.Add(forecast);
            }

            Anomalies.Clear();
            foreach (var anomaly in result.Anomalies)
            {
                Anomalies.Add(anomaly);
            }

            UpdateStatistics(result, priceHistory);
            UpdateChart(result, priceHistory);

            OnPropertyChanged(nameof(HasForecasts));
            OnPropertyChanged(nameof(HasAnomalies));
            OnPropertyChanged(nameof(ShowNoForecastsMessage));
            OnPropertyChanged(nameof(ShowNoAnomaliesMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to generate forecast", ex);
            _dialogService.ShowError($"Failed to generate forecast: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateStatistics(PriceForecastResult result, List<PriceRecord> priceHistory)
    {
        var latestPrice = priceHistory.OrderByDescending(p => p.DateRecorded).FirstOrDefault();
        CurrentPriceText = latestPrice != null ? $"${latestPrice.Price:F2}" : "N/A";

        if (result.OptimalBuyingDate.HasValue && result.OptimalBuyingPrice.HasValue)
        {
            OptimalBuyingDateText = result.OptimalBuyingDate.Value.ToString("dd MMM yyyy");
            OptimalBuyingPriceText = $"${result.OptimalBuyingPrice.Value:F2}";
        }
        else
        {
            OptimalBuyingDateText = "N/A";
            OptimalBuyingPriceText = "N/A";
        }

        if (result.Forecasts.Any())
        {
            var low = result.Forecasts.Min(f => f.PredictedPrice);
            var high = result.Forecasts.Max(f => f.PredictedPrice);
            var avg = result.Forecasts.Average(f => f.PredictedPrice);
            
            PredictedLowText = $"${low:F2}";
            PredictedHighText = $"${high:F2}";
            PredictedAvgText = $"${avg:F2}";
        }
        else
        {
            PredictedLowText = "N/A";
            PredictedHighText = "N/A";
            PredictedAvgText = "N/A";
        }

        OverallRecommendationText = GetRecommendationDisplayText(result.OverallRecommendation);
        RecommendationColor = GetRecommendationColor(result.OverallRecommendation);
    }

    private void UpdateChart(PriceForecastResult result, List<PriceRecord> priceHistory)
    {
        if (!result.Forecasts.Any()) return;

        var series = new List<ISeries>();

        var historicalPrices = priceHistory
            .OrderBy(p => p.DateRecorded)
            .Select(p => (float)p.Price)
            .ToArray();
        
        var historicalDates = priceHistory
            .OrderBy(p => p.DateRecorded)
            .Select(p => p.DateRecorded.ToString("dd/MM"))
            .ToArray();

        if (historicalPrices.Length > 0)
        {
            var historicalSeries = new LineSeries<float>
            {
                Name = "Historical",
                Values = historicalPrices,
                Stroke = new SolidColorPaint(new SKColor(100, 100, 100)) { StrokeThickness = 2 },
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0.3
            };
            series.Add(historicalSeries);
        }

        var forecastValues = result.Forecasts.Select(f => f.PredictedPrice).ToArray();
        var forecastDates = result.Forecasts.Select(f => f.ForecastDate.ToString("dd/MM")).ToArray();
        
        var forecastSeries = new LineSeries<float>
        {
            Name = "Forecast",
            Values = forecastValues,
            Stroke = new SolidColorPaint(new SKColor(37, 99, 235)) { StrokeThickness = 3 },
            Fill = new SolidColorPaint(new SKColor(37, 99, 235, 30)),
            GeometrySize = 6,
            GeometryStroke = new SolidColorPaint(new SKColor(37, 99, 235)) { StrokeThickness = 2 },
            LineSmoothness = 0.5
        };
        series.Add(forecastSeries);

        var upperBounds = result.Forecasts.Select(f => f.UpperBound).ToArray();
        var upperSeries = new LineSeries<float>
        {
            Name = "Upper Bound (95%)",
            Values = upperBounds,
            Stroke = new SolidColorPaint(new SKColor(37, 99, 235, 100)) { StrokeThickness = 1 },
            Fill = null,
            GeometrySize = 0,
            LineSmoothness = 0.5
        };
        series.Add(upperSeries);

        var lowerBounds = result.Forecasts.Select(f => f.LowerBound).ToArray();
        var lowerSeries = new LineSeries<float>
        {
            Name = "Lower Bound (95%)",
            Values = lowerBounds,
            Stroke = new SolidColorPaint(new SKColor(37, 99, 235, 100)) { StrokeThickness = 1 },
            Fill = null,
            GeometrySize = 0,
            LineSmoothness = 0.5
        };
        series.Add(lowerSeries);

        ForecastSeries = series.ToArray();

        var allDates = historicalDates.Concat(forecastDates).ToArray();
        
        ChartXAxes = new[]
        {
            new Axis
            {
                Name = "Date",
                Labels = allDates,
                LabelsRotation = 45,
                TextSize = 11,
                ShowSeparatorLines = false
            }
        };

        ChartYAxes = new[]
        {
            new Axis
            {
                Name = "Price ($)",
                TextSize = 12,
                Labeler = value => $"${value:F2}",
                ShowSeparatorLines = true,
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 100))
            }
        };
    }

    private void ClearResults()
    {
        ForecastResult = null;
        Forecasts.Clear();
        Anomalies.Clear();
        ForecastSeries = Array.Empty<ISeries>();
        ChartXAxes = Array.Empty<Axis>();
        ChartYAxes = Array.Empty<Axis>();
        
        OptimalBuyingDateText = "N/A";
        OptimalBuyingPriceText = "N/A";
        CurrentPriceText = "N/A";
        PredictedLowText = "N/A";
        PredictedHighText = "N/A";
        PredictedAvgText = "N/A";
        OverallRecommendationText = "Select an item to generate forecast";
        RecommendationColor = "#666";
        
        OnPropertyChanged(nameof(HasForecasts));
        OnPropertyChanged(nameof(HasAnomalies));
        OnPropertyChanged(nameof(ShowNoForecastsMessage));
        OnPropertyChanged(nameof(ShowNoAnomaliesMessage));
    }

    private static string GetRecommendationDisplayText(BuyingRecommendation recommendation)
    {
        return recommendation switch
        {
            BuyingRecommendation.BuyNow => "BUY NOW - Price at or near historical low",
            BuyingRecommendation.Wait => "WAIT - Price expected to drop",
            BuyingRecommendation.AvoidHighPrice => "AVOID - Price unusually high",
            _ => "NORMAL - No significant trend"
        };
    }

    private static string GetRecommendationColor(BuyingRecommendation recommendation)
    {
        return recommendation switch
        {
            BuyingRecommendation.BuyNow => "#16a34a",
            BuyingRecommendation.Wait => "#ca8a04",
            BuyingRecommendation.AvoidHighPrice => "#dc2626",
            _ => "#666"
        };
    }
}
