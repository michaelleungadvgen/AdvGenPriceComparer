using System;
using System.Collections.Generic;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

public class DemoDataService
{
    private readonly IGroceryDataService _dataService;
    private readonly Random _random = new();

    public DemoDataService(IGroceryDataService dataService)
    {
        _dataService = dataService;
    }

    public void GenerateDemoData()
    {
        // Create demo stores
        var storeNames = new (string name, string chain)[]
        {
            ("Coles Brisbane CBD", "Coles"),
            ("Woolworths Fortitude Valley", "Woolworths"),
            ("IGA New Farm", "IGA"),
            ("Drakes Stones Corner", "Drakes")
        };

        var storeIds = new List<string>();
        foreach (var (name, chain) in storeNames)
        {
            var id = _dataService.AddSupermarket(name, chain, "123 Main St", "Brisbane", "QLD", "4000");
            storeIds.Add(id);
        }

        // Create demo items
        var items = new (string name, string brand, string category, string unit)[]
        {
            ("Milk", "Dairy Farmers", "Dairy", "2L"),
            ("Bread White", "Tip Top", "Bakery", "700g"),
            ("Eggs Free Range", "Sunny Queen", "Dairy", "12 pack"),
            ("Bananas", "", "Fruit", "per kg"),
            ("Chicken Breast", "Steggles", "Meat", "per kg"),
            ("Rice", "SunRice", "Pantry", "5kg"),
            ("Pasta Penne", "San Remo", "Pantry", "500g"),
            ("Tomato Sauce", "Leggo's", "Pantry", "500g"),
            ("Coffee Beans", "Lavazza", "Beverages", "1kg"),
            ("Orange Juice", "Nudie", "Beverages", "1L")
        };

        var itemIds = new List<string>();
        foreach (var (name, brand, category, unit) in items)
        {
            var id = _dataService.AddGroceryItem(name, brand, category, packageSize: unit, unit: unit);
            itemIds.Add(id);
        }

        // Create price records
        var basePrices = new[] { 3.50m, 3.20m, 6.50m, 3.99m, 12.99m, 15.00m, 2.50m, 3.00m, 18.00m, 5.50m };

        for (int i = 0; i < itemIds.Count && i < basePrices.Length; i++)
        {
            foreach (var storeId in storeIds)
            {
                var price = GenerateRandomPrice(basePrices[i]);
                var isOnSale = _random.NextDouble() > 0.7;
                var originalPrice = isOnSale ? price * 1.2m : (decimal?)null;

                _dataService.RecordPrice(
                    itemIds[i],
                    storeId,
                    price,
                    isOnSale,
                    originalPrice,
                    isOnSale ? "Special" : null,
                    validFrom: DateTime.Now,
                    validTo: isOnSale ? DateTime.Now.AddDays(7) : null,
                    source: "demo");
            }
        }
    }

    private decimal GenerateRandomPrice(decimal basePrice)
    {
        // Generate price within ±20% of base price
        var variation = (decimal)(_random.NextDouble() * 0.4 - 0.2);
        var price = basePrice * (1 + variation);
        return Math.Round(price, 2);
    }
}
