using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for tracking deal expirations and providing reminders
/// </summary>
public class DealExpirationService : IDealExpirationService
{
    private readonly IGroceryDataService _dataService;
    private readonly string _dismissedDealsFilePath;
    private HashSet<string> _dismissedDeals;

    public DealExpirationService(IGroceryDataService dataService, string? appDataPath = null)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        
        var dataPath = appDataPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer");
        
        _dismissedDealsFilePath = Path.Combine(dataPath, "dismissed_deals.json");
        _dismissedDeals = LoadDismissedDeals();
    }

    /// <inheritdoc />
    public List<ExpiringDeal> GetExpiringDeals(int daysThreshold = 7)
    {
        var today = DateTime.Today;
        var cutoffDate = today.AddDays(daysThreshold);
        
        var expiringDeals = new List<ExpiringDeal>();
        var dismissed = GetDismissedDeals();

        // Get all price records that have ValidTo dates
        var allItems = _dataService.GetAllItems().ToList();
        var allPlaces = _dataService.GetAllPlaces().ToList();
        
        foreach (var item in allItems)
        {
            // Get the most recent price record for this item
            var priceHistory = _dataService.GetPriceHistory(item.Id!);
            var latestPrice = priceHistory.OrderByDescending(p => p.DateRecorded).FirstOrDefault();
            
            if (latestPrice?.ValidTo == null) continue;
            
            // Check if the deal is expiring within the threshold
            if (latestPrice.ValidTo.Value.Date >= today && 
                latestPrice.ValidTo.Value.Date <= cutoffDate)
            {
                var dealKey = $"{item.Id}_{latestPrice.ValidTo.Value:yyyyMMdd}";
                
                // Skip if dismissed
                if (dismissed.Contains(dealKey)) continue;
                
                var store = allPlaces.FirstOrDefault(p => p.Id == latestPrice.PlaceId);
                
                expiringDeals.Add(new ExpiringDeal
                {
                    ItemId = item.Id!,
                    ItemName = item.Name,
                    StoreName = store?.Name ?? "Unknown Store",
                    Price = latestPrice.Price,
                    OriginalPrice = latestPrice.OriginalPrice,
                    ExpiryDate = latestPrice.ValidTo.Value,
                    DateRecorded = latestPrice.DateRecorded,
                    Savings = CalculateSavings(latestPrice.Price, latestPrice.OriginalPrice)
                });
            }
        }

        return expiringDeals.OrderBy(d => d.DaysUntilExpiry).ToList();
    }

    /// <inheritdoc />
    public List<ExpiringDeal> GetExpiredDeals()
    {
        var today = DateTime.Today;
        var expiredDeals = new List<ExpiringDeal>();

        var allItems = _dataService.GetAllItems().ToList();
        var allPlaces = _dataService.GetAllPlaces().ToList();

        foreach (var item in allItems)
        {
            var priceHistory = _dataService.GetPriceHistory(item.Id!);
            var latestPrice = priceHistory.OrderByDescending(p => p.DateRecorded).FirstOrDefault();
            
            if (latestPrice?.ValidTo == null) continue;
            
            if (latestPrice.ValidTo.Value.Date < today)
            {
                var store = allPlaces.FirstOrDefault(p => p.Id == latestPrice.PlaceId);
                
                expiredDeals.Add(new ExpiringDeal
                {
                    ItemId = item.Id!,
                    ItemName = item.Name,
                    StoreName = store?.Name ?? "Unknown Store",
                    Price = latestPrice.Price,
                    OriginalPrice = latestPrice.OriginalPrice,
                    ExpiryDate = latestPrice.ValidTo.Value,
                    DateRecorded = latestPrice.DateRecorded,
                    Savings = CalculateSavings(latestPrice.Price, latestPrice.OriginalPrice)
                });
            }
        }

        return expiredDeals.OrderByDescending(d => d.ExpiryDate).ToList();
    }

    /// <inheritdoc />
    public bool HasExpiringDeals(int daysThreshold = 3)
    {
        return GetExpiringDealsCount(daysThreshold) > 0;
    }

    /// <inheritdoc />
    public int GetExpiringDealsCount(int daysThreshold = 7)
    {
        return GetExpiringDeals(daysThreshold).Count;
    }

    /// <inheritdoc />
    public void DismissDeal(string itemId, DateTime expiryDate)
    {
        var dealKey = $"{itemId}_{expiryDate:yyyyMMdd}";
        _dismissedDeals.Add(dealKey);
        SaveDismissedDeals();
    }

    /// <inheritdoc />
    public HashSet<string> GetDismissedDeals()
    {
        return new HashSet<string>(_dismissedDeals);
    }

    /// <inheritdoc />
    public void ClearDismissedDeals()
    {
        _dismissedDeals.Clear();
        SaveDismissedDeals();
    }

    private string? CalculateSavings(decimal price, decimal? originalPrice)
    {
        if (!originalPrice.HasValue || originalPrice.Value <= 0) return null;
        
        var savings = originalPrice.Value - price;
        if (savings <= 0) return null;
        
        var percent = (savings / originalPrice.Value) * 100;
        return $"Save ${savings:F2} ({percent:F0}%)";
    }

    private HashSet<string> LoadDismissedDeals()
    {
        try
        {
            if (File.Exists(_dismissedDealsFilePath))
            {
                var json = File.ReadAllText(_dismissedDealsFilePath);
                var deals = JsonSerializer.Deserialize<HashSet<string>>(json);
                return deals ?? new HashSet<string>();
            }
        }
        catch (Exception)
        {
            // If loading fails, return empty set
        }
        
        return new HashSet<string>();
    }

    private void SaveDismissedDeals()
    {
        try
        {
            var directory = Path.GetDirectoryName(_dismissedDealsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_dismissedDeals, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_dismissedDealsFilePath, json);
        }
        catch (Exception)
        {
            // If saving fails, just continue - dismissed deals won't persist
        }
    }
}
