using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Application.Queries;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Tests.Services;

namespace AdvGenPriceComparer.Tests.ViewModels;

/// <summary>
/// Test implementation of IMediator for unit tests
/// </summary>
public class TestMediator : IMediator
{
    private readonly TestGroceryDataService _dataService;

    public TestMediator(TestGroceryDataService dataService)
    {
        _dataService = dataService;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HandleRequest(request));
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Stream requests not supported in tests");
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Notifications are no-ops in tests
        return Task.CompletedTask;
    }

    private TResponse HandleRequest<TResponse>(IRequest<TResponse> request)
    {
        // Handle Queries
        if (request is GetAllItemsQuery)
        {
            var items = _dataService.GetAllItems();
            return (TResponse)(object)items;
        }

        if (request is GetAllPlacesQuery)
        {
            var places = _dataService.GetAllPlaces();
            return (TResponse)(object)places;
        }

        if (request is GetRecentPriceUpdatesQuery recentQuery)
        {
            var priceRecords = _dataService.GetRecentPriceUpdates(recentQuery.Count);
            return (TResponse)(object)priceRecords;
        }

        if (request is GetCategoryStatsQuery)
        {
            var items = _dataService.GetAllItems();
            var stats = items
                .Where(i => !string.IsNullOrEmpty(i.Category))
                .GroupBy(i => i.Category)
                .Select(g => new CategoryStats(Category: g.Key!, AveragePrice: 0m, ItemCount: g.Count(), MinPrice: 0m, MaxPrice: 0m))
                .ToList();
            return (TResponse)(object)stats;
        }

        if (request is GetPriceHistoryQuery priceHistoryQuery)
        {
            var history = _dataService.GetPriceHistory(priceHistoryQuery.ItemId, priceHistoryQuery.PlaceId, priceHistoryQuery.From, priceHistoryQuery.To)
                .Where(pr => priceHistoryQuery.From == null || pr.DateRecorded >= priceHistoryQuery.From)
                .Where(pr => priceHistoryQuery.To == null || pr.DateRecorded <= priceHistoryQuery.To)
                .ToList();
            return (TResponse)(object)history;
        }

        if (request is GetItemByIdQuery itemByIdQuery)
        {
            var item = _dataService.GetItemById(itemByIdQuery.ItemId);
            return (TResponse)(object)item!;
        }

        if (request is GetPlaceByIdQuery placeByIdQuery)
        {
            var place = _dataService.GetPlaceById(placeByIdQuery.PlaceId);
            return (TResponse)(object)place!;
        }

        if (request is GetItemsByCategoryQuery itemsByCategoryQuery)
        {
            var items = _dataService.GetAllItems().Where(i => i.Category == itemsByCategoryQuery.Category).ToList();
            return (TResponse)(object)items;
        }

        if (request is SearchItemsQuery searchQuery)
        {
            var items = _dataService.GetAllItems()
                .Where(i => i.Name.Contains(searchQuery.SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return (TResponse)(object)items;
        }

        if (request is GetDashboardStatsQuery)
        {
            var stats = new DashboardStats
            {
                TotalItems = _dataService.GetAllItems().Count(),
                TotalPlaces = _dataService.GetAllPlaces().Count(),
                TotalPriceRecords = _dataService.GetRecentPriceUpdates(1000).Count()
            };
            return (TResponse)(object)stats;
        }

        if (request is GetStoreComparisonStatsQuery)
        {
            var places = _dataService.GetAllPlaces();
            var priceRecords = _dataService.GetRecentPriceUpdates(1000);
            
            var stats = places.Select(p => new StoreComparisonStat
            {
                PlaceId = p.Id,
                PlaceName = p.Name,
                ItemCount = priceRecords.Count(pr => pr.PlaceId == p.Id)
            }).ToList();
            
            return (TResponse)(object)stats;
        }

        if (request is FindBestDealsQuery bestDealsQuery)
        {
            var items = _dataService.GetAllItems()
                .Take(10)
                .Select(i => new BestDealResult(i, 0m, new Place { Id = "1", Name = "Test Store" }))
                .ToList();
            return (TResponse)(object)items;
        }

        // Handle Commands
        if (request is CreateItemCommand createItemCmd)
        {
            var item = new Item
            {
                Id = Guid.NewGuid().ToString(),
                Name = createItemCmd.Name,
                Brand = createItemCmd.Brand,
                Category = createItemCmd.Category,
                Description = createItemCmd.Description,
                Barcode = createItemCmd.Barcode
            };
            _dataService.AddTestItem(item.Name, item.Brand ?? "", item.Category ?? "");
            return (TResponse)(object)true;
        }

        if (request is UpdateItemCommand updateItemCmd)
        {
            return (TResponse)(object)true;
        }

        if (request is DeleteItemCommand deleteItemCmd)
        {
            return (TResponse)(object)true;
        }

        if (request is CreatePlaceCommand createPlaceCmd)
        {
            var place = new Place
            {
                Id = Guid.NewGuid().ToString(),
                Name = createPlaceCmd.Name,
                Chain = createPlaceCmd.Chain,
                Address = createPlaceCmd.Address,
                Suburb = createPlaceCmd.Suburb,
                State = createPlaceCmd.State,
                Postcode = createPlaceCmd.Postcode,
                Phone = createPlaceCmd.Phone
            };
            _dataService.AddTestPlace(place.Name, place.Chain ?? "");
            return (TResponse)(object)true;
        }

        if (request is RecordPriceCommand recordPriceCmd)
        {
            _dataService.AddTestPriceRecord(recordPriceCmd.ItemId, recordPriceCmd.PlaceId, recordPriceCmd.Price);
            return (TResponse)(object)true;
        }

        // Default fallback
        return default!;
    }
}

// Response models for queries
public class DashboardStats
{
    public int TotalItems { get; set; }
    public int TotalPlaces { get; set; }
    public int TotalPriceRecords { get; set; }
}

public class StoreComparisonStat
{
    public string PlaceId { get; set; } = string.Empty;
    public string PlaceName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}
