using AdvGenPriceComparer.Application.Queries;
using AdvGenPriceComparer.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

/// <summary>
/// Handler for FindBestDealsQuery
/// </summary>
public class FindBestDealsQueryHandler : IRequestHandler<FindBestDealsQuery, IEnumerable<BestDealResult>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<FindBestDealsQueryHandler> _logger;

    public FindBestDealsQueryHandler(
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        ILogger<FindBestDealsQueryHandler> logger)
    {
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<IEnumerable<BestDealResult>> Handle(FindBestDealsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = new List<BestDealResult>();
            var items = string.IsNullOrEmpty(request.Category)
                ? _itemRepository.GetAll()
                : _itemRepository.GetByCategory(request.Category);

            foreach (var item in items)
            {
                var prices = _priceRecordRepository.GetByItem(item.Id);
                if (!prices.Any()) continue;

                var lowestPrice = prices.Min(p => p.Price);
                var lowestPriceRecord = prices.First(p => p.Price == lowestPrice);
                var place = _placeRepository.GetById(lowestPriceRecord.PlaceId);

                if (place != null)
                {
                    decimal? savingsPercent = null;
                    if (lowestPriceRecord.OriginalPrice.HasValue && lowestPriceRecord.OriginalPrice > 0)
                    {
                        savingsPercent = (1 - lowestPrice / lowestPriceRecord.OriginalPrice.Value) * 100;
                    }

                    results.Add(new BestDealResult(item, lowestPrice, place, savingsPercent));
                }
            }

            // Sort by savings percentage (highest first), then by lowest price
            var sortedResults = results
                .OrderByDescending(r => r.SavingsPercent ?? 0)
                .ThenBy(r => r.LowestPrice)
                .ToList();

            return Task.FromResult<IEnumerable<BestDealResult>>(sortedResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding best deals");
            return Task.FromResult<IEnumerable<BestDealResult>>(new List<BestDealResult>());
        }
    }
}

/// <summary>
/// Handler for GetDashboardStatsQuery
/// </summary>
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsResult>
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<GetDashboardStatsQueryHandler> _logger;

    public GetDashboardStatsQueryHandler(
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        ILogger<GetDashboardStatsQueryHandler> logger)
    {
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<DashboardStatsResult> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = _itemRepository.GetAll();
            var places = _placeRepository.GetAll();
            var priceRecords = _priceRecordRepository.GetAll();

            var activeDeals = priceRecords.Count(pr => pr.IsOnSale && (pr.ValidTo == null || pr.ValidTo > DateTime.UtcNow));

            // Calculate average savings for active deals
            var savingsList = priceRecords
                .Where(pr => pr.IsOnSale && pr.OriginalPrice.HasValue && pr.OriginalPrice > pr.Price)
                .Select(pr => (pr.OriginalPrice!.Value - pr.Price) / pr.OriginalPrice.Value * 100)
                .ToList();

            var averageSavings = savingsList.Any() ? savingsList.Average() : 0;

            // Items by category
            var itemsByCategory = items
                .Where(i => !string.IsNullOrEmpty(i.Category))
                .GroupBy(i => i.Category!)
                .ToDictionary(g => g.Key, g => g.Count());

            // Average price by category
            var avgPriceByCategory = new Dictionary<string, decimal>();
            foreach (var category in itemsByCategory.Keys)
            {
                var categoryItemIds = items.Where(i => i.Category == category).Select(i => i.Id);
                var categoryPrices = priceRecords.Where(pr => categoryItemIds.Contains(pr.ItemId)).Select(pr => pr.Price);
                if (categoryPrices.Any())
                {
                    avgPriceByCategory[category] = categoryPrices.Average();
                }
            }

            var result = new DashboardStatsResult
            {
                TotalItems = items.Count(),
                TotalPlaces = places.Count(),
                TotalPriceRecords = priceRecords.Count(),
                ActiveDeals = activeDeals,
                AverageSavings = averageSavings,
                ItemsByCategory = itemsByCategory,
                AveragePriceByCategory = avgPriceByCategory
            };

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return Task.FromResult(new DashboardStatsResult());
        }
    }
}

/// <summary>
/// Handler for GetCategoryStatsQuery
/// </summary>
public class GetCategoryStatsQueryHandler : IRequestHandler<GetCategoryStatsQuery, IEnumerable<CategoryStats>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<GetCategoryStatsQueryHandler> _logger;

    public GetCategoryStatsQueryHandler(
        IItemRepository itemRepository,
        IPriceRecordRepository priceRecordRepository,
        ILogger<GetCategoryStatsQueryHandler> logger)
    {
        _itemRepository = itemRepository;
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<IEnumerable<CategoryStats>> Handle(GetCategoryStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = new List<CategoryStats>();
            var items = _itemRepository.GetAll();
            var priceRecords = _priceRecordRepository.GetAll();

            var categories = items
                .Where(i => !string.IsNullOrEmpty(i.Category))
                .Select(i => i.Category!)
                .Distinct();

            foreach (var category in categories)
            {
                var categoryItemIds = items.Where(i => i.Category == category).Select(i => i.Id).ToList();
                var categoryPrices = priceRecords.Where(pr => categoryItemIds.Contains(pr.ItemId)).Select(pr => pr.Price).ToList();

                if (categoryPrices.Any())
                {
                    results.Add(new CategoryStats(
                        category,
                        categoryPrices.Average(),
                        categoryItemIds.Count,
                        categoryPrices.Min(),
                        categoryPrices.Max()
                    ));
                }
            }

            return Task.FromResult<IEnumerable<CategoryStats>>(results.OrderBy(c => c.Category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category stats");
            return Task.FromResult<IEnumerable<CategoryStats>>(new List<CategoryStats>());
        }
    }
}

/// <summary>
/// Handler for GetStoreComparisonStatsQuery
/// </summary>
public class GetStoreComparisonStatsQueryHandler : IRequestHandler<GetStoreComparisonStatsQuery, IEnumerable<StoreComparisonStats>>
{
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<GetStoreComparisonStatsQueryHandler> _logger;

    public GetStoreComparisonStatsQueryHandler(
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        ILogger<GetStoreComparisonStatsQueryHandler> logger)
    {
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<IEnumerable<StoreComparisonStats>> Handle(GetStoreComparisonStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = new List<StoreComparisonStats>();
            var places = _placeRepository.GetAll();
            var priceRecords = _priceRecordRepository.GetAll();

            foreach (var place in places)
            {
                var storePrices = priceRecords.Where(pr => pr.PlaceId == place.Id).ToList();
                var uniqueProducts = storePrices.Select(pr => pr.ItemId).Distinct().Count();
                var deals = storePrices.Count(pr => pr.IsOnSale);

                if (storePrices.Any())
                {
                    results.Add(new StoreComparisonStats(
                        place.Name,
                        storePrices.Average(pr => pr.Price),
                        uniqueProducts,
                        deals
                    ));
                }
            }

            return Task.FromResult<IEnumerable<StoreComparisonStats>>(results.OrderBy(s => s.StoreName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store comparison stats");
            return Task.FromResult<IEnumerable<StoreComparisonStats>>(new List<StoreComparisonStats>());
        }
    }
}
