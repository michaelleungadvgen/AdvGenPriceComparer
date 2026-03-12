using AdvGenPriceComparer.Application.Mediator;
using AdvGenPriceComparer.Application.Queries;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

/// <summary>
/// Handler for GetAllItemsQuery
/// </summary>
public class GetAllItemsQueryHandler : IRequestHandler<GetAllItemsQuery, IEnumerable<Item>>
{
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<GetAllItemsQueryHandler> _logger;

    public GetAllItemsQueryHandler(IItemRepository itemRepository, ILogger<GetAllItemsQueryHandler> logger)
    {
        _itemRepository = itemRepository;
        _logger = logger;
    }

    public Task<IEnumerable<Item>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = _itemRepository.GetAll();
            return Task.FromResult(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all items");
            return Task.FromResult<IEnumerable<Item>>(new List<Item>());
        }
    }
}

/// <summary>
/// Handler for GetItemByIdQuery
/// </summary>
public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, Item?>
{
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<GetItemByIdQueryHandler> _logger;

    public GetItemByIdQueryHandler(IItemRepository itemRepository, ILogger<GetItemByIdQueryHandler> logger)
    {
        _itemRepository = itemRepository;
        _logger = logger;
    }

    public Task<Item?> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = _itemRepository.GetById(request.ItemId);
            return Task.FromResult(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item by ID: {ItemId}", request.ItemId);
            return Task.FromResult<Item?>(null);
        }
    }
}

/// <summary>
/// Handler for GetItemsByCategoryQuery
/// </summary>
public class GetItemsByCategoryQueryHandler : IRequestHandler<GetItemsByCategoryQuery, IEnumerable<Item>>
{
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<GetItemsByCategoryQueryHandler> _logger;

    public GetItemsByCategoryQueryHandler(IItemRepository itemRepository, ILogger<GetItemsByCategoryQueryHandler> logger)
    {
        _itemRepository = itemRepository;
        _logger = logger;
    }

    public Task<IEnumerable<Item>> Handle(GetItemsByCategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = _itemRepository.GetByCategory(request.Category);
            return Task.FromResult(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting items by category: {Category}", request.Category);
            return Task.FromResult<IEnumerable<Item>>(new List<Item>());
        }
    }
}

/// <summary>
/// Handler for SearchItemsQuery
/// </summary>
public class SearchItemsQueryHandler : IRequestHandler<SearchItemsQuery, IEnumerable<Item>>
{
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<SearchItemsQueryHandler> _logger;

    public SearchItemsQueryHandler(IItemRepository itemRepository, ILogger<SearchItemsQueryHandler> logger)
    {
        _itemRepository = itemRepository;
        _logger = logger;
    }

    public Task<IEnumerable<Item>> Handle(SearchItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = new List<Item>();
            var searchTerm = request.SearchTerm.ToLowerInvariant();

            // Search by name
            var nameResults = _itemRepository.SearchByName(request.SearchTerm);
            results.AddRange(nameResults);

            // Search by brand if enabled
            if (request.IncludeBrand)
            {
                var allItems = _itemRepository.GetAll();
                var brandResults = allItems.Where(i => 
                    !string.IsNullOrEmpty(i.Brand) && 
                    i.Brand.ToLowerInvariant().Contains(searchTerm));
                results.AddRange(brandResults);
            }

            // Search by barcode if enabled
            if (request.IncludeBarcode)
            {
                var allItems = _itemRepository.GetAll();
                var barcodeResults = allItems.Where(i => 
                    !string.IsNullOrEmpty(i.Barcode) && 
                    i.Barcode.ToLowerInvariant().Contains(searchTerm));
                results.AddRange(barcodeResults);
            }

            // Remove duplicates and return
            var distinctResults = results.GroupBy(i => i.Id).Select(g => g.First());
            return Task.FromResult(distinctResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching items: {SearchTerm}", request.SearchTerm);
            return Task.FromResult<IEnumerable<Item>>(new List<Item>());
        }
    }
}
