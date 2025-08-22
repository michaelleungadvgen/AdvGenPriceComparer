using System;
using System.Collections.Generic;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Desktop.WinUI.Services;

public class DemoDataService
{
    private readonly IGroceryDataService _groceryDataService;
    private readonly Random _random = new();

    public DemoDataService(IGroceryDataService groceryDataService)
    {
        _groceryDataService = groceryDataService;
    }

    public void GenerateDemoData()
    {
        try
        {
            GenerateStores();
            GenerateProducts();
            GeneratePriceRecords();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating demo data: {ex.Message}");
        }
    }

    private void GenerateStores()
    {
        var stores = new[]
        {
            ("Coles Chatswood", "Coles", "123 Victoria Ave", "Chatswood", "NSW", "2067"),
            ("Woolworths Town Hall", "Woolworths", "456 George St", "Sydney", "NSW", "2000"),
            ("IGA Express Bondi", "IGA", "789 Campbell Parade", "Bondi Beach", "NSW", "2026"),
            ("ALDI Parramatta", "ALDI", "321 Church St", "Parramatta", "NSW", "2150"),
            ("Coles Bondi Junction", "Coles", "654 Oxford St", "Bondi Junction", "NSW", "2022"),
            ("Woolworths Double Bay", "Woolworths", "987 New South Head Rd", "Double Bay", "NSW", "2028"),
            ("Foodworks Glebe", "Foodworks", "147 Glebe Point Rd", "Glebe", "NSW", "2037"),
            ("IGA Newtown", "IGA", "258 King St", "Newtown", "NSW", "2042")
        };

        foreach (var (name, chain, address, suburb, state, postcode) in stores)
        {
            _groceryDataService.AddSupermarket(name, chain, address, suburb, state, postcode);
        }
    }

    private void GenerateProducts()
    {
        var products = new[]
        {
            // Fruits & Vegetables
            ("Bananas", "Fresh", "Fruits & Vegetables", null, "1kg"),
            ("Apples - Royal Gala", "Fresh", "Fruits & Vegetables", null, "1kg"),
            ("Potatoes - Brushed", "Fresh", "Fruits & Vegetables", null, "2kg"),
            ("Tomatoes - Truss", "Fresh", "Fruits & Vegetables", null, "1kg"),
            ("Carrots", "Fresh", "Fruits & Vegetables", null, "1kg"),
            ("Onions - Brown", "Fresh", "Fruits & Vegetables", null, "2kg"),
            
            // Dairy & Eggs
            ("Milk - Full Cream", "Dairy Farmers", "Dairy & Eggs", "9310015000123", "2L"),
            ("Bread - White", "Tip Top", "Bakery", "9300675000456", "700g"),
            ("Eggs - Free Range", "Sunny Queen", "Dairy & Eggs", "9310067000789", "12 pack"),
            ("Cheese - Tasty", "Bega", "Dairy & Eggs", "9310015001012", "500g"),
            ("Yogurt - Greek", "Chobani", "Dairy & Eggs", "9310067001345", "1kg"),
            ("Butter", "Western Star", "Dairy & Eggs", "9310015001678", "500g"),
            
            // Meat & Seafood
            ("Chicken Breast", "Fresh", "Meat & Seafood", null, "1kg"),
            ("Beef Mince", "Fresh", "Meat & Seafood", null, "500g"),
            ("Salmon Fillet", "Fresh", "Meat & Seafood", null, "300g"),
            
            // Pantry
            ("Rice - Jasmine", "SunRice", "Pantry", "9310015002901", "2kg"),
            ("Pasta - Spaghetti", "San Remo", "Pantry", "9310067002234", "500g"),
            ("Olive Oil", "Bertolli", "Pantry", "9310015003567", "500ml"),
            ("Cereal - Weet-Bix", "Sanitarium", "Pantry", "9310067003890", "1.2kg"),
            
            // Beverages
            ("Orange Juice", "Just Juice", "Beverages", "9310015004123", "2L"),
            ("Coffee - Instant", "Nescafe", "Beverages", "9310067004456", "150g"),
            
            // Frozen
            ("Ice Cream - Vanilla", "Streets", "Frozen", "9310015005789", "2L"),
            ("Frozen Peas", "Birds Eye", "Frozen", "9310067005012", "1kg"),
            
            // Snacks
            ("Chips - Original", "Smiths", "Snacks", "9310015006345", "200g"),
            ("Chocolate - Dairy Milk", "Cadbury", "Snacks", "9310067006678", "350g")
        };

        foreach (var (name, brand, category, barcode, packageSize) in products)
        {
            _groceryDataService.AddGroceryItem(name, brand, category, barcode, packageSize);
        }
    }

    private void GeneratePriceRecords()
    {
        var items = _groceryDataService.GetAllItems().ToList();
        var places = _groceryDataService.GetAllPlaces().ToList();

        if (!items.Any() || !places.Any()) return;

        // Generate price records for the last 90 days
        for (int dayOffset = 90; dayOffset >= 0; dayOffset--)
        {
            var date = DateTime.Now.AddDays(-dayOffset);
            
            // Skip some days to create realistic gaps
            if (_random.NextDouble() < 0.3) continue;

            // Record prices for random subset of items each day
            var itemsToPrice = items.OrderBy(x => _random.Next()).Take(_random.Next(5, 15));
            
            foreach (var item in itemsToPrice)
            {
                // Each item might be available at 1-3 stores
                var storesToPrice = places.OrderBy(x => _random.Next()).Take(_random.Next(1, 4));
                
                foreach (var store in storesToPrice)
                {
                    var basePrice = GetBasePriceForItem(item.Name);
                    var storeMultiplier = GetStoreMultiplier(store.Chain ?? "Unknown");
                    var timeVariation = 1.0m + ((decimal)_random.NextDouble() - 0.5m) * 0.2m; // ±10% variation
                    
                    var price = basePrice * storeMultiplier * timeVariation;
                    price = Math.Round(price, 2);
                    
                    // Occasionally create sales
                    var isOnSale = _random.NextDouble() < 0.15; // 15% chance of sale
                    decimal? originalPrice = null;
                    string? saleDescription = null;
                    
                    if (isOnSale)
                    {
                        originalPrice = price;
                        price = price * (0.7m + (decimal)_random.NextDouble() * 0.2m); // 10-30% off
                        price = Math.Round(price, 2);
                        saleDescription = "Special Offer";
                    }
                    
                    _groceryDataService.RecordPrice(
                        item.Id,
                        store.Id,
                        price,
                        isOnSale,
                        originalPrice,
                        saleDescription,
                        date,
                        null,
                        "demo_data"
                    );
                }
            }
        }
    }

    private decimal GetBasePriceForItem(string itemName)
    {
        return itemName.ToLower() switch
        {
            var name when name.Contains("banana") => 2.50m,
            var name when name.Contains("apple") => 4.00m,
            var name when name.Contains("potato") => 3.50m,
            var name when name.Contains("tomato") => 6.00m,
            var name when name.Contains("carrot") => 2.00m,
            var name when name.Contains("onion") => 2.50m,
            var name when name.Contains("milk") => 3.20m,
            var name when name.Contains("bread") => 2.80m,
            var name when name.Contains("eggs") => 5.50m,
            var name when name.Contains("cheese") => 8.00m,
            var name when name.Contains("yogurt") => 7.50m,
            var name when name.Contains("butter") => 6.00m,
            var name when name.Contains("chicken") => 12.00m,
            var name when name.Contains("beef") => 8.50m,
            var name when name.Contains("salmon") => 15.00m,
            var name when name.Contains("rice") => 4.50m,
            var name when name.Contains("pasta") => 2.20m,
            var name when name.Contains("oil") => 7.00m,
            var name when name.Contains("cereal") => 5.50m,
            var name when name.Contains("juice") => 4.20m,
            var name when name.Contains("coffee") => 8.50m,
            var name when name.Contains("ice cream") => 6.50m,
            var name when name.Contains("peas") => 3.50m,
            var name when name.Contains("chips") => 3.00m,
            var name when name.Contains("chocolate") => 5.50m,
            _ => 5.00m
        };
    }

    private decimal GetStoreMultiplier(string chain)
    {
        return chain.ToLower() switch
        {
            "aldi" => 0.85m,        // ALDI is typically cheapest
            "coles" => 1.00m,       // Baseline
            "woolworths" => 1.02m,  // Slightly more expensive
            "iga" => 1.15m,         // More expensive, convenience
            "foodworks" => 1.20m,   // Most expensive, convenience
            _ => 1.00m
        };
    }
}