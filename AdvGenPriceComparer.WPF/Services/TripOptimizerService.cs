using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for optimizing multi-store shopping trips to minimize travel distance and cost
/// </summary>
public interface ITripOptimizerService
{
    /// <summary>
    /// Generate an optimized shopping trip plan based on a shopping list
    /// </summary>
    Task<TripOptimizationResult> OptimizeTripAsync(
        ShoppingList shoppingList,
        TripOptimizationOptions options);

    /// <summary>
    /// Calculate estimated travel time between two stores (in minutes)
    /// </summary>
    double CalculateTravelTime(Place from, Place to);

    /// <summary>
    /// Calculate estimated travel distance between two stores (in kilometers)
    /// </summary>
    double CalculateDistance(Place from, Place to);
}

/// <summary>
/// Result of trip optimization containing the optimized route and store assignments
/// </summary>
public class TripOptimizationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<StoreStop> StoreStops { get; set; } = new();
    public List<ShoppingListItem> UnavailableItems { get; set; } = new();
    public double TotalDistanceKm { get; set; }
    public double TotalTravelTimeMinutes { get; set; }
    public decimal TotalCost { get; set; }
    public decimal PotentialSavings { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Statistics
    public int TotalItems => StoreStops.Sum(s => s.Items.Count);
    public int NumberOfStores => StoreStops.Count;
    public double AverageTimePerStore => NumberOfStores > 0 ? TotalTravelTimeMinutes / NumberOfStores : 0;
}

/// <summary>
/// Represents a single store stop in the optimized trip
/// </summary>
public class StoreStop
{
    public Place Store { get; set; } = null!;
    public List<TripItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public double TravelTimeFromPrevious { get; set; }
    public double DistanceFromPreviousKm { get; set; }
    public int StopNumber { get; set; }
    public string? SpecialInstructions { get; set; }
}

/// <summary>
/// An item to be purchased at a specific store with pricing information
/// </summary>
public class TripItem
{
    public ShoppingListItem ShoppingListItem { get; set; } = null!;
    public Item? Product { get; set; }
    public PriceRecord? Price { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal? Savings { get; set; }
    public bool IsOnSale { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Options for trip optimization
/// </summary>
public class TripOptimizationOptions
{
    /// <summary>
    /// Starting location (if null, assumes user starts from home/any location)
    /// </summary>
    public Place? StartingLocation { get; set; }

    /// <summary>
    /// Maximum number of stores to visit (default: 5)
    /// </summary>
    public int MaxStores { get; set; } = 5;

    /// <summary>
    /// Maximum acceptable travel distance in km (default: 50)
    /// </summary>
    public double MaxTravelDistanceKm { get; set; } = 50;

    /// <summary>
    /// Optimization strategy: Cost, Distance, or Balanced
    /// </summary>
    public OptimizationStrategy Strategy { get; set; } = OptimizationStrategy.Balanced;

    /// <summary>
    /// Stores to exclude from consideration
    /// </summary>
    public List<string> ExcludedStoreIds { get; set; } = new();

    /// <summary>
    /// Preferred stores (will be prioritized if items available)
    /// </summary>
    public List<string> PreferredStoreIds { get; set; } = new();

    /// <summary>
    /// Minimum savings threshold to justify visiting an additional store (default: $5)
    /// </summary>
    public decimal MinSavingsThreshold { get; set; } = 5.00m;

    /// <summary>
    /// Whether to prioritize stores with the most items on the list
    /// </summary>
    public bool PrioritizeOneStopShopping { get; set; } = true;

    /// <summary>
    /// Average shopping time per item in minutes (default: 2)
    /// </summary>
    public double ShoppingTimePerItemMinutes { get; set; } = 2.0;
}

/// <summary>
/// Optimization strategy for trip planning
/// </summary>
public enum OptimizationStrategy
{
    /// <summary>
    /// Minimize total cost regardless of travel distance
    /// </summary>
    Cost,

    /// <summary>
    /// Minimize travel distance/time regardless of cost
    /// </summary>
    Distance,

    /// <summary>
    /// Balance cost savings against travel time
    /// </summary>
    Balanced
}

/// <summary>
/// Service implementation for multi-store trip optimization
/// </summary>
public class TripOptimizerService : ITripOptimizerService
{
    private readonly IGroceryDataService _groceryDataService;
    private readonly ILoggerService _logger;

    // Average driving speed in km/h for suburban areas
    private const double AVERAGE_SPEED_KMH = 35.0;

    public TripOptimizerService(
        IGroceryDataService groceryDataService,
        ILoggerService logger)
    {
        _groceryDataService = groceryDataService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TripOptimizationResult> OptimizeTripAsync(
        ShoppingList shoppingList,
        TripOptimizationOptions options)
    {
        try
        {
            _logger.LogInfo($"Starting trip optimization for shopping list '{shoppingList.Name}' with {shoppingList.Items.Count} items");

            if (shoppingList.Items.Count == 0)
            {
                return new TripOptimizationResult
                {
                    Success = false,
                    Message = "Shopping list is empty"
                };
            }

            // Get all stores
            var allStores = _groceryDataService.Places.GetAll();
            var availableStores = allStores
                .Where(s => s.IsActive && !options.ExcludedStoreIds.Contains(s.Id))
                .ToList();

            if (!availableStores.Any())
            {
                return new TripOptimizationResult
                {
                    Success = false,
                    Message = "No available stores found"
                };
            }

            // Get current prices for all items across all stores
            var itemPrices = await GetItemPricesAcrossStoresAsync(shoppingList.Items, availableStores);

            // Generate optimized route based on strategy
            var result = options.Strategy switch
            {
                OptimizationStrategy.Cost => await OptimizeForCostAsync(shoppingList, itemPrices, availableStores, options),
                OptimizationStrategy.Distance => await OptimizeForDistanceAsync(shoppingList, itemPrices, availableStores, options),
                _ => await OptimizeBalancedAsync(shoppingList, itemPrices, availableStores, options)
            };

            // Calculate totals
            CalculateTripTotals(result, options);

            _logger.LogInfo($"Trip optimization completed: {result.NumberOfStores} stores, ${result.TotalCost:F2} total, {result.TotalDistanceKm:F1}km");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during trip optimization", ex);
            return new TripOptimizationResult
            {
                Success = false,
                Message = $"Optimization failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Get current prices for shopping list items across all stores
    /// </summary>
    private async Task<Dictionary<string, List<StorePrice>>> GetItemPricesAcrossStoresAsync(
        List<ShoppingListItem> items,
        List<Place> stores)
    {
        var result = new Dictionary<string, List<StorePrice>>();

        foreach (var item in items)
        {
            var storePrices = new List<StorePrice>();

            foreach (var store in stores)
            {
                // Try to find matching item by ItemId first, then by name
                var matchingItems = _groceryDataService.Items.SearchByName(item.Name);
                var bestMatch = matchingItems.FirstOrDefault();

                if (bestMatch != null)
                {
                    var latestPrice = _groceryDataService.PriceRecords.GetLatestPrice(bestMatch.Id, store.Id);
                    if (latestPrice != null)
                    {
                        storePrices.Add(new StorePrice
                        {
                            Store = store,
                            Item = bestMatch,
                            PriceRecord = latestPrice,
                            Price = latestPrice.Price,
                            IsOnSale = latestPrice.IsOnSale
                        });
                    }
                }
            }

            // Sort by price
            result[item.Id] = storePrices.OrderBy(sp => sp.Price).ToList();
        }

        return result;
    }

    /// <summary>
    /// Optimize purely for lowest cost
    /// </summary>
    private async Task<TripOptimizationResult> OptimizeForCostAsync(
        ShoppingList shoppingList,
        Dictionary<string, List<StorePrice>> itemPrices,
        List<Place> stores,
        TripOptimizationOptions options)
    {
        var result = new TripOptimizationResult { Success = true };
        var assignedItems = new HashSet<string>();
        var selectedStores = new List<StoreAssignment>();

        // Group items by cheapest store
        var storeItemGroups = new Dictionary<string, List<(ShoppingListItem Item, StorePrice Price)>>();

        foreach (var shoppingItem in shoppingList.Items.Where(i => !i.IsChecked))
        {
            if (itemPrices.TryGetValue(shoppingItem.Id, out var prices) && prices.Any())
            {
                var cheapest = prices.First();
                var storeId = cheapest.Store.Id;

                if (!storeItemGroups.ContainsKey(storeId))
                    storeItemGroups[storeId] = new List<(ShoppingListItem, StorePrice)>();

                storeItemGroups[storeId].Add((shoppingItem, cheapest));
                assignedItems.Add(shoppingItem.Id);
            }
        }

        // Sort stores by number of items (descending) to minimize number of stores
        var sortedStores = storeItemGroups
            .OrderByDescending(g => g.Value.Count)
            .ThenBy(g => g.Value.Sum(i => i.Price.Price))
            .Take(options.MaxStores)
            .ToList();

        // Build store stops
        int stopNumber = 1;
        foreach (var storeGroup in sortedStores)
        {
            var store = stores.First(s => s.Id == storeGroup.Key);
            var items = storeGroup.Value;

            var storeStop = new StoreStop
            {
                Store = store,
                StopNumber = stopNumber++,
                Items = items.Select(i => new TripItem
                {
                    ShoppingListItem = i.Item,
                    Product = i.Price.Item,
                    Price = i.Price.PriceRecord,
                    FinalPrice = i.Price.Price,
                    IsOnSale = i.Price.IsOnSale
                }).ToList()
            };

            result.StoreStops.Add(storeStop);
        }

        // Add unavailable items
        result.UnavailableItems = shoppingList.Items
            .Where(i => !i.IsChecked && !assignedItems.Contains(i.Id))
            .ToList();

        return result;
    }

    /// <summary>
    /// Optimize purely for shortest distance
    /// </summary>
    private async Task<TripOptimizationResult> OptimizeForDistanceAsync(
        ShoppingList shoppingList,
        Dictionary<string, List<StorePrice>> itemPrices,
        List<Place> stores,
        TripOptimizationOptions options)
    {
        var result = new TripOptimizationResult { Success = true };

        // Find the single store that has the most items
        var storeCoverage = new Dictionary<string, List<(ShoppingListItem Item, StorePrice Price)>>();

        foreach (var item in shoppingList.Items.Where(i => !i.IsChecked))
        {
            if (itemPrices.TryGetValue(item.Id, out var prices))
            {
                foreach (var price in prices)
                {
                    var storeId = price.Store.Id;
                    if (!storeCoverage.ContainsKey(storeId))
                        storeCoverage[storeId] = new List<(ShoppingListItem, StorePrice)>();

                    storeCoverage[storeId].Add((item, price));
                }
            }
        }

        // Select the store with most items (prioritize preferred stores if tie)
        var bestStore = storeCoverage
            .OrderByDescending(s => s.Value.Count)
            .ThenBy(s => options.PreferredStoreIds.Contains(s.Key) ? 0 : 1)
            .FirstOrDefault();

        if (bestStore.Key != null)
        {
            var store = stores.First(s => s.Id == bestStore.Key);
            var items = bestStore.Value
                .GroupBy(v => v.Item.Id)
                .Select(g => g.First()) // Take first (cheapest) for each item
                .ToList();

            result.StoreStops.Add(new StoreStop
            {
                Store = store,
                StopNumber = 1,
                Items = items.Select(i => new TripItem
                {
                    ShoppingListItem = i.Item,
                    Product = i.Price.Item,
                    Price = i.Price.PriceRecord,
                    FinalPrice = i.Price.Price,
                    IsOnSale = i.Price.IsOnSale
                }).ToList()
            });

            // Add unavailable items
            var coveredItemIds = new HashSet<string>(items.Select(i => i.Item.Id));
            result.UnavailableItems = shoppingList.Items
                .Where(i => !i.IsChecked && !coveredItemIds.Contains(i.Id))
                .ToList();
        }
        else
        {
            result.UnavailableItems = shoppingList.Items.Where(i => !i.IsChecked).ToList();
        }

        return result;
    }

    /// <summary>
    /// Balanced optimization considering both cost and distance
    /// </summary>
    private async Task<TripOptimizationResult> OptimizeForDistanceAsync(
        ShoppingList shoppingList,
        Dictionary<string, List<StorePrice>> itemPrices,
        List<Place> stores,
        TripOptimizationOptions options,
        bool balanced = true) // overload marker
    {
        return await OptimizeBalancedAsync(shoppingList, itemPrices, stores, options);
    }

    /// <summary>
    /// Balanced optimization considering both cost and distance
    /// </summary>
    private async Task<TripOptimizationResult> OptimizeBalancedAsync(
        ShoppingList shoppingList,
        Dictionary<string, List<StorePrice>> itemPrices,
        List<Place> stores,
        TripOptimizationOptions options)
    {
        var result = new TripOptimizationResult { Success = true };
        var assignedItems = new HashSet<string>();

        // Calculate store scores based on items available and savings potential
        var storeScores = CalculateStoreScores(shoppingList.Items.Where(i => !i.IsChecked), itemPrices, stores, options);

        // Greedy selection: pick best stores one by one
        var selectedStores = new List<StoreAssignment>();
        var remainingItems = shoppingList.Items.Where(i => !i.IsChecked).ToList();

        while (selectedStores.Count < options.MaxStores && remainingItems.Any())
        {
            // Score each store for remaining items
            var bestStore = FindBestNextStore(remainingItems, itemPrices, stores, selectedStores, options);

            if (bestStore == null || bestStore.Items.Count == 0)
                break;

            // Check if savings justify the extra stop
            if (selectedStores.Count > 0)
            {
                var additionalSavings = bestStore.Items.Sum(i => i.PotentialSavings);
                if (additionalSavings < options.MinSavingsThreshold)
                {
                    _logger.LogInfo($"Skipping store {bestStore.Store.Name} - additional savings ${additionalSavings:F2} below threshold");
                    break;
                }
            }

            selectedStores.Add(bestStore);

            // Mark items as assigned
            foreach (var item in bestStore.Items)
            {
                assignedItems.Add(item.ShoppingListItem.Id);
            }

            // Update remaining items
            remainingItems = remainingItems.Where(i => !assignedItems.Contains(i.Id)).ToList();
        }

        // Build store stops with routing
        int stopNumber = 1;
        Place? previousLocation = options.StartingLocation;

        foreach (var storeAssignment in selectedStores)
        {
            var travelTime = previousLocation != null
                ? CalculateTravelTime(previousLocation, storeAssignment.Store)
                : 0;
            var distance = previousLocation != null
                ? CalculateDistance(previousLocation, storeAssignment.Store)
                : 0;

            var storeStop = new StoreStop
            {
                Store = storeAssignment.Store,
                StopNumber = stopNumber++,
                TravelTimeFromPrevious = travelTime,
                DistanceFromPreviousKm = distance,
                Items = storeAssignment.Items.Select(i => new TripItem
                {
                    ShoppingListItem = i.ShoppingListItem,
                    Product = i.StorePrice?.Item,
                    Price = i.StorePrice?.PriceRecord,
                    FinalPrice = i.StorePrice?.Price ?? i.ShoppingListItem.EstimatedPrice ?? 0,
                    IsOnSale = i.StorePrice?.IsOnSale ?? false,
                    Savings = i.PotentialSavings > 0 ? i.PotentialSavings : null
                }).ToList()
            };

            result.StoreStops.Add(storeStop);
            previousLocation = storeAssignment.Store;
        }

        // Add unavailable items
        result.UnavailableItems = remainingItems.Where(i => !assignedItems.Contains(i.Id)).ToList();

        return result;
    }

    /// <summary>
    /// Calculate scores for each store based on available items and savings
    /// </summary>
    private Dictionary<string, double> CalculateStoreScores(
        IEnumerable<ShoppingListItem> items,
        Dictionary<string, List<StorePrice>> itemPrices,
        List<Place> stores,
        TripOptimizationOptions options)
    {
        var scores = new Dictionary<string, double>();

        foreach (var store in stores)
        {
            double score = 0;
            int availableItems = 0;

            foreach (var item in items)
            {
                if (itemPrices.TryGetValue(item.Id, out var prices))
                {
                    var storePrice = prices.FirstOrDefault(p => p.Store.Id == store.Id);
                    if (storePrice != null)
                    {
                        availableItems++;

                        // Add to score based on how competitive the price is
                        var cheapestPrice = prices.First().Price;
                        var priceRatio = cheapestPrice > 0 ? (double)(cheapestPrice / storePrice.Price) : 1.0;
                        score += priceRatio;

                        // Bonus for preferred stores
                        if (options.PreferredStoreIds.Contains(store.Id))
                        {
                            score += 0.5;
                        }
                    }
                }
            }

            // Normalize by number of items
            scores[store.Id] = availableItems > 0 ? score / availableItems : 0;
        }

        return scores;
    }

    /// <summary>
    /// Find the best next store to add to the route
    /// </summary>
    private StoreAssignment? FindBestNextStore(
        List<ShoppingListItem> remainingItems,
        Dictionary<string, List<StorePrice>> itemPrices,
        List<Place> stores,
        List<StoreAssignment> selectedStores,
        TripOptimizationOptions options)
    {
        StoreAssignment? bestAssignment = null;
        double bestScore = double.MinValue;

        var lastStore = selectedStores.LastOrDefault()?.Store;

        foreach (var store in stores)
        {
            if (selectedStores.Any(s => s.Store.Id == store.Id))
                continue;

            var assignment = new StoreAssignment { Store = store };
            decimal totalSavings = 0;

            foreach (var item in remainingItems)
            {
                if (itemPrices.TryGetValue(item.Id, out var prices))
                {
                    var storePrice = prices.FirstOrDefault(p => p.Store.Id == store.Id);
                    if (storePrice != null)
                    {
                        // Calculate savings compared to next best alternative
                        var alternativePrice = prices.FirstOrDefault(p => 
                            p.Store.Id != store.Id && 
                            !selectedStores.Any(s => s.Store.Id == p.Store.Id));

                        var savings = alternativePrice != null 
                            ? alternativePrice.Price - storePrice.Price 
                            : 0;

                        assignment.Items.Add(new ItemAssignment
                        {
                            ShoppingListItem = item,
                            StorePrice = storePrice,
                            PotentialSavings = savings > 0 ? savings : 0
                        });

                        if (savings > 0)
                            totalSavings += savings;
                    }
                }
            }

            if (assignment.Items.Count == 0)
                continue;

            // Calculate score based on strategy
            double score;
            if (lastStore != null)
            {
                var distance = CalculateDistance(lastStore, store);
                var travelTime = CalculateTravelTime(lastStore, store);

                // Score: items count + savings - distance penalty
                score = assignment.Items.Count * 10 +
                        (double)totalSavings * 2 -
                        distance * 0.5 -
                        travelTime * 0.1;
            }
            else
            {
                score = assignment.Items.Count * 10 + (double)totalSavings * 2;
            }

            // Boost for preferred stores
            if (options.PreferredStoreIds.Contains(store.Id))
            {
                score += 20;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestAssignment = assignment;
            }
        }

        return bestAssignment;
    }

    /// <summary>
    /// Calculate total distance, time, and cost for the trip
    /// </summary>
    private void CalculateTripTotals(TripOptimizationResult result, TripOptimizationOptions options)
    {
        double totalDistance = 0;
        double totalTime = 0;
        decimal totalCost = 0;
        decimal potentialSavings = 0;

        Place? previousLocation = options.StartingLocation;

        foreach (var stop in result.StoreStops)
        {
            // Travel time and distance
            if (previousLocation != null)
            {
                stop.TravelTimeFromPrevious = CalculateTravelTime(previousLocation, stop.Store);
                stop.DistanceFromPreviousKm = CalculateDistance(previousLocation, stop.Store);
            }

            totalDistance += stop.DistanceFromPreviousKm;
            totalTime += stop.TravelTimeFromPrevious;

            // Shopping time
            totalTime += stop.Items.Count * options.ShoppingTimePerItemMinutes;

            // Cost and savings
            foreach (var item in stop.Items)
            {
                totalCost += item.FinalPrice * (item.ShoppingListItem.Quantity ?? 1);
                if (item.Savings.HasValue)
                {
                    potentialSavings += item.Savings.Value * (item.ShoppingListItem.Quantity ?? 1);
                }
            }

            stop.Subtotal = stop.Items.Sum(i => i.FinalPrice * (i.ShoppingListItem.Quantity ?? 1));
            previousLocation = stop.Store;
        }

        result.TotalDistanceKm = totalDistance;
        result.TotalTravelTimeMinutes = totalTime;
        result.TotalCost = totalCost;
        result.PotentialSavings = potentialSavings;
    }

    /// <inheritdoc/>
    public double CalculateTravelTime(Place from, Place to)
    {
        var distance = CalculateDistance(from, to);
        // Time = Distance / Speed + fixed overhead per stop (parking, etc.)
        return (distance / AVERAGE_SPEED_KMH * 60) + 5; // 5 minutes overhead
    }

    /// <inheritdoc/>
    public double CalculateDistance(Place from, Place to)
    {
        // If we have coordinates, use Haversine formula
        if (from.Latitude.HasValue && from.Longitude.HasValue &&
            to.Latitude.HasValue && to.Longitude.HasValue)
        {
            return CalculateHaversineDistance(
                from.Latitude.Value, from.Longitude.Value,
                to.Latitude.Value, to.Longitude.Value);
        }

        // Otherwise, estimate based on suburb/postcode similarity
        if (from.Postcode == to.Postcode)
            return 2.0; // Same area, ~2km

        if (from.Suburb?.Equals(to.Suburb, StringComparison.OrdinalIgnoreCase) == true)
            return 3.0; // Same suburb, ~3km

        if (from.State == to.State)
            return 10.0; // Same state, different area, ~10km

        return 50.0; // Different states, far
    }

    /// <summary>
    /// Calculate distance between two coordinates using Haversine formula
    /// </summary>
    private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}

/// <summary>
/// Internal class for store price information
/// </summary>
internal class StorePrice
{
    public Place Store { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public PriceRecord PriceRecord { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsOnSale { get; set; }
}

/// <summary>
/// Internal class for store assignment
/// </summary>
internal class StoreAssignment
{
    public Place Store { get; set; } = null!;
    public List<ItemAssignment> Items { get; set; } = new();
}

/// <summary>
/// Internal class for item assignment
/// </summary>
internal class ItemAssignment
{
    public ShoppingListItem ShoppingListItem { get; set; } = null!;
    public StorePrice? StorePrice { get; set; }
    public decimal PotentialSavings { get; set; }
}
