using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for deal expiration reminders window
/// </summary>
public class DealExpirationReminderViewModel : ViewModelBase
{
    private readonly IDealExpirationService _dealExpirationService;
    private ExpiringDealViewModel? _selectedDeal;
    private int _selectedDaysFilter = 7;
    private bool _showExpiredDeals;

    public DealExpirationReminderViewModel(IDealExpirationService dealExpirationService)
    {
        _dealExpirationService = dealExpirationService ?? throw new ArgumentNullException(nameof(dealExpirationService));
        
        ExpiringDeals = new ObservableCollection<ExpiringDealViewModel>();
        
        RefreshCommand = new RelayCommand(() => LoadDeals());
        DismissDealCommand = new RelayCommand(() => DismissSelectedDeal(), () => SelectedDeal != null);
        DismissAllCommand = new RelayCommand(() => DismissAllDeals(), () => ExpiringDeals.Count > 0);
        ClearDismissedCommand = new RelayCommand(() => ClearDismissedDeals());
        
        LoadDeals();
    }

    public ObservableCollection<ExpiringDealViewModel> ExpiringDeals { get; }

    public ExpiringDealViewModel? SelectedDeal
    {
        get => _selectedDeal;
        set
        {
            if (SetProperty(ref _selectedDeal, value))
            {
                ((RelayCommand)DismissDealCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public int SelectedDaysFilter
    {
        get => _selectedDaysFilter;
        set
        {
            if (SetProperty(ref _selectedDaysFilter, value))
            {
                LoadDeals();
            }
        }
    }

    public bool ShowExpiredDeals
    {
        get => _showExpiredDeals;
        set
        {
            if (SetProperty(ref _showExpiredDeals, value))
            {
                LoadDeals();
            }
        }
    }

    public int TotalExpiringCount => ExpiringDeals.Count;

    public string ExpiringSummary => $"{TotalExpiringCount} deals expiring" + 
        (TotalExpiringCount > 0 ? $" (next {SelectedDaysFilter} days)" : "");

    public ICommand RefreshCommand { get; }
    public ICommand DismissDealCommand { get; }
    public ICommand DismissAllCommand { get; }
    public ICommand ClearDismissedCommand { get; }

    public int[] DaysFilterOptions { get; } = { 1, 3, 7, 14, 30 };

    private void LoadDeals()
    {
        ExpiringDeals.Clear();

        var deals = ShowExpiredDeals 
            ? _dealExpirationService.GetExpiredDeals()
            : _dealExpirationService.GetExpiringDeals(SelectedDaysFilter);

        foreach (var deal in deals)
        {
            ExpiringDeals.Add(new ExpiringDealViewModel(deal));
        }

        OnPropertyChanged(nameof(TotalExpiringCount));
        OnPropertyChanged(nameof(ExpiringSummary));
        ((RelayCommand)DismissAllCommand).RaiseCanExecuteChanged();
    }

    private void DismissSelectedDeal()
    {
        if (SelectedDeal == null) return;

        _dealExpirationService.DismissDeal(SelectedDeal.ItemId, SelectedDeal.ExpiryDate);
        ExpiringDeals.Remove(SelectedDeal);
        SelectedDeal = null;
        
        OnPropertyChanged(nameof(TotalExpiringCount));
        OnPropertyChanged(nameof(ExpiringSummary));
        ((RelayCommand)DismissAllCommand).RaiseCanExecuteChanged();
    }

    private void DismissAllDeals()
    {
        foreach (var deal in ExpiringDeals.ToList())
        {
            _dealExpirationService.DismissDeal(deal.ItemId, deal.ExpiryDate);
        }
        
        ExpiringDeals.Clear();
        SelectedDeal = null;
        
        OnPropertyChanged(nameof(TotalExpiringCount));
        OnPropertyChanged(nameof(ExpiringSummary));
        ((RelayCommand)DismissAllCommand).RaiseCanExecuteChanged();
    }

    private void ClearDismissedDeals()
    {
        _dealExpirationService.ClearDismissedDeals();
        LoadDeals();
    }
}

/// <summary>
/// ViewModel wrapper for an expiring deal
/// </summary>
public class ExpiringDealViewModel : ViewModelBase
{
    private readonly ExpiringDeal _deal;

    public ExpiringDealViewModel(ExpiringDeal deal)
    {
        _deal = deal ?? throw new ArgumentNullException(nameof(deal));
    }

    public string ItemId => _deal.ItemId;
    public string ItemName => _deal.ItemName;
    public string StoreName => _deal.StoreName;
    public decimal Price => _deal.Price;
    public decimal? OriginalPrice => _deal.OriginalPrice;
    public DateTime ExpiryDate => _deal.ExpiryDate;
    public DateTime DateRecorded => _deal.DateRecorded;
    public int DaysUntilExpiry => _deal.DaysUntilExpiry;
    public bool IsExpired => _deal.IsExpired;
    public string? Savings => _deal.Savings;

    public string PriceDisplay => $"${Price:F2}";
    public string OriginalPriceDisplay => OriginalPrice.HasValue ? $"${OriginalPrice.Value:F2}" : "";
    
    public string ExpiryDisplay
    {
        get
        {
            if (IsExpired) return "Expired";
            if (DaysUntilExpiry == 0) return "Expires today!";
            if (DaysUntilExpiry == 1) return "Expires tomorrow";
            return $"Expires in {DaysUntilExpiry} days";
        }
    }

    public string ExpiryDateDisplay => ExpiryDate.ToString("ddd, dd MMM yyyy");

    public string UrgencyColor
    {
        get
        {
            if (IsExpired) return "#9E9E9E"; // Gray
            if (DaysUntilExpiry <= 1) return "#F44336"; // Red - urgent
            if (DaysUntilExpiry <= 3) return "#FF9800"; // Orange - warning
            return "#4CAF50"; // Green - ok
        }
    }
}
