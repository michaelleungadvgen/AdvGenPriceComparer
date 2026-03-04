using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Chat.Models;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.Chat.Services
{
    public class QueryRouterService : IQueryRouterService
    {
        private readonly IGroceryDataService _groceryData;
        private readonly IItemRepository _itemRepo;
        private readonly IPlaceRepository _placeRepo;
        private readonly IPriceRecordRepository _priceRepo;
        private readonly ILoggerService _logger;

        public QueryRouterService(
            IGroceryDataService groceryData,
            IItemRepository itemRepo,
            IPlaceRepository placeRepo,
            IPriceRecordRepository priceRepo,
            ILoggerService logger)
        {
            _groceryData = groceryData;
            _itemRepo = itemRepo;
            _placeRepo = placeRepo;
            _priceRepo = priceRepo;
            _logger = logger;
        }

        public async Task<ChatResponse> ExecuteQueryAsync(QueryIntent intent)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInfo($"Executing query of type: {intent.Type}");

                    return intent.Type switch
                    {
                        QueryType.PriceQuery => ExecutePriceQuery(intent),
                        QueryType.PriceComparison => ExecutePriceComparison(intent),
                        QueryType.CheapestItem => ExecuteCheapestItemQuery(intent),
                        QueryType.ItemsInCategory => ExecuteItemsInCategoryQuery(intent),
                        QueryType.ItemsOnSale => ExecuteItemsOnSaleQuery(intent),
                        QueryType.BestDeal => ExecuteBestDealsQuery(intent),
                        QueryType.StoreInventory => ExecuteStoreInventoryQuery(intent),
                        QueryType.PriceHistory => ExecutePriceHistoryQuery(intent),
                        QueryType.BudgetQuery => ExecuteBudgetQuery(intent),
                        QueryType.GeneralChat => ExecuteGeneralChat(intent),
                        _ => new ChatResponse
                        {
                            TextResponse = "I'm not sure how to answer that. Try asking about prices, products, stores, or deals.",
                            Success = true,
                            DetectedIntent = intent
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Query execution error: {ex.Message}");
                    return new ChatResponse
                    {
                        TextResponse = "I encountered an error while searching. Please try again or rephrase your question.",
                        Success = false,
                        ErrorMessage = ex.Message,
                        DetectedIntent = intent
                    };
                }
            });
        }

        private ChatResponse ExecutePriceQuery(QueryIntent intent)
        {
            var allItems = _itemRepo.GetAll().ToList();
            var filteredItems = FilterItems(allItems, intent).Take(5).ToList();

            if (!filteredItems.Any())
            {
                return new ChatResponse
                {
                    TextResponse = $"I couldn't find any products matching '{intent.ProductName}'. Try a different name or check your spelling.",
                    Success = true,
                    RelatedItems = new List<Item>(),
                    DetectedIntent = intent
                };
            }

            // Get prices for the items
            var priceRecords = new List<PriceRecord>();
            foreach (var item in filteredItems)
            {
                var prices = _priceRepo.GetByItem(item.Id).Take(1);
                priceRecords.AddRange(prices);
            }

            return new ChatResponse
            {
                Success = true,
                RelatedItems = filteredItems,
                RelatedPrices = priceRecords,
                DetectedIntent = intent
            };
        }

        private ChatResponse ExecutePriceComparison(QueryIntent intent)
        {
            var allItems = _itemRepo.GetAll().ToList();
            var filteredItems = FilterItems(allItems, intent).Take(5).ToList();

            var priceRecords = new List<PriceRecord>();
            foreach (var item in filteredItems)
            {
                var prices = _priceRepo.GetByItem(item.Id);
                priceRecords.AddRange(prices);
            }

            var places = _placeRepo.GetAll().ToList();

            return new ChatResponse
            {
                Success = true,
                RelatedItems = filteredItems,
                RelatedPrices = priceRecords,
                RelatedStores = places,
                DetectedIntent = intent
            };
        }

        private ChatResponse ExecuteCheapestItemQuery(QueryIntent intent)
        {
            var allItems = _itemRepo.GetAll().ToList();
            
            // Get prices and sort by cheapest
            var itemPrices = new List<(Item Item, decimal? Price)>();
            foreach (var item in allItems)
            {
                var latestPrice = _priceRepo.GetPriceHistory(item.Id).FirstOrDefault();
                itemPrices.Add((item, latestPrice?.Price));
            }

            var cheapestItems = itemPrices
                .Where(x => x.Price.HasValue && x.Price.Value > 0)
                .OrderBy(x => x.Price)
                .Take(intent.Limit ?? 5)
                .Select(x => x.Item)
                .ToList();

            return new ChatResponse
            {
                Success = true,
                RelatedItems = cheapestItems,
                DetectedIntent = intent
            };
        }

        private ChatResponse ExecuteItemsInCategoryQuery(QueryIntent intent)
        {
            var categoryItems = _itemRepo.GetByCategory(intent.Category ?? "")
                .Take(intent.Limit ?? 10)
                .ToList();

            return new ChatResponse
            {
                Success = true,
                RelatedItems = categoryItems,
                DetectedIntent = intent
            };
        }

        private ChatResponse ExecuteItemsOnSaleQuery(QueryIntent intent)
        {
            var saleRecords = _priceRepo.GetCurrentSales()
                .Take(intent.Limit ?? 10)
                .ToList();

            var items = new List<Item>();
            foreach (var record in saleRecords)
            {
                var item = _itemRepo.GetById(record.ItemId);
                if (item != null)
                    items.Add(item);
            }

            return new ChatResponse
            {
                Success = true,
                RelatedItems = items,
                RelatedPrices = saleRecords,
                DetectedIntent = intent
            };
        }

        private ChatResponse ExecuteBestDealsQuery(QueryIntent intent)
        {
            var bestDeals = _priceRepo.GetBestDeals(intent.Limit ?? 10).ToList();
            
            var items = new List<Item>();
            foreach (var record in bestDeals)
            {
                var item = _itemRepo.GetById(record.ItemId);
                if (item != null)
                    items.Add(item);
            }

            return new ChatResponse
            {
                Success = true,
                RelatedItems = items,
                RelatedPrices = bestDeals,
                DetectedIntent = intent
            };
        }

        private ChatResponse ExecuteStoreInventoryQuery(QueryIntent intent)
        {
            var allPlaces = _placeRepo.GetAll().ToList();
            var store = allPlaces
                .FirstOrDefault(p => p.Name.Contains(intent.Store ?? "", StringComparison.OrdinalIgnoreCase));

            if (store == null)
            {
                return new ChatResponse
                {
                    TextResponse = $"I couldn't find a store named '{intent.Store}'.",
                    Success = true,
                    RelatedStores = allPlaces.Take(5).ToList(),
                    DetectedIntent = intent
                };
            }

            var priceRecords = _priceRepo.GetByPlace(store.Id).Take(10).ToList();
            var itemIds = priceRecords.Select(p => p.ItemId).Distinct().Take(10).ToList();
            var items = new List<Item>();

            foreach (var itemId in itemIds)
            {
                var item = _itemRepo.GetById(itemId);
                if (item != null)
                    items.Add(item);
            }

            return new ChatResponse
            {
                Success = true,
                RelatedItems = items,
                RelatedStores = new List<Place> { store },
                RelatedPrices = priceRecords,
                DetectedIntent = intent
            };
        }

        private ChatResponse ExecutePriceHistoryQuery(QueryIntent intent)
        {
            var allItems = _itemRepo.GetAll().ToList();
            var filteredItems = FilterItems(allItems, intent).Take(1).ToList();

            if (!filteredItems.Any())
            {
                return new ChatResponse
                {
                    TextResponse = $"I couldn't find a product named '{intent.ProductName}'.",
                    Success = true,
                    DetectedIntent = intent
                };
            }

            var item = filteredItems.First();
            var history = _priceRepo.GetPriceHistory(item.Id, DateTime.Now.AddMonths(-3)).ToList();

            return new ChatResponse
            {
                Success = true,
                RelatedItems = filteredItems,
                RelatedPrices = history,
                DetectedIntent = intent
            };
        }

        private ChatResponse ExecuteBudgetQuery(QueryIntent intent)
        {
            var budget = intent.MaxPrice ?? 50;
            var allItems = _itemRepo.GetAll().ToList();
            
            var affordableItems = new List<Item>();
            foreach (var item in allItems)
            {
                var latestPrice = _priceRepo.GetPriceHistory(item.Id).FirstOrDefault();
                if (latestPrice != null && latestPrice.Price <= budget)
                {
                    affordableItems.Add(item);
                }
            }

            affordableItems = affordableItems
                .OrderBy(i => _priceRepo.GetPriceHistory(i.Id).FirstOrDefault()?.Price ?? decimal.MaxValue)
                .Take(intent.Limit ?? 10)
                .ToList();

            return new ChatResponse
            {
                Success = true,
                RelatedItems = affordableItems,
                DetectedIntent = intent
            };
        }

        private ChatResponse ExecuteGeneralChat(QueryIntent intent)
        {
            return new ChatResponse
            {
                TextResponse = "", // Will be filled by Ollama
                Success = true,
                DetectedIntent = intent
            };
        }

        private IEnumerable<Item> FilterItems(IEnumerable<Item> items, QueryIntent intent)
        {
            var result = items;

            if (!string.IsNullOrEmpty(intent.ProductName))
            {
                result = result.Where(i =>
                    i.Name.Contains(intent.ProductName, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(i.Brand) && i.Brand.Contains(intent.ProductName, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(intent.Category))
            {
                result = result.Where(i =>
                    !string.IsNullOrEmpty(i.Category) &&
                    i.Category.Contains(intent.Category, StringComparison.OrdinalIgnoreCase));
            }

            return result;
        }
    }
}
