using AdvGenPriceComparer.Server.Models;
using AdvGenPriceComparer.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvGenPriceComparer.Server.Controllers;

/// <summary>
/// API controller for item/product management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IPriceDataService _priceDataService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IPriceDataService priceDataService, ILogger<ItemsController> logger)
    {
        _priceDataService = priceDataService;
        _logger = logger;
    }

    /// <summary>
    /// Get all items with optional filtering and pagination
    /// </summary>
    /// <param name="category">Filter by category</param>
    /// <param name="brand">Filter by brand</param>
    /// <param name="search">Search query for item name</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of items</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SharedItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedItem>>> GetItems(
        [FromQuery] string? category = null,
        [FromQuery] string? brand = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 1000) pageSize = 1000;

            var filter = new ItemFilter
            {
                Category = category,
                Brand = brand,
                SearchQuery = search
            };

            var items = await _priceDataService.GetItemsAsync(filter, page, pageSize);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a single item by ID
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Item details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SharedItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedItem>> GetItemById(int id)
    {
        try
        {
            var item = await _priceDataService.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { error = $"Item with ID {id} not found" });
            }

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item {ItemId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get an item by its external product ID
    /// </summary>
    /// <param name="productId">External product ID</param>
    /// <returns>Item details</returns>
    [HttpGet("by-product-id/{productId}")]
    [ProducesResponseType(typeof(SharedItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedItem>> GetItemByProductId(string productId)
    {
        try
        {
            var item = await _priceDataService.GetItemByProductIdAsync(productId);
            if (item == null)
            {
                return NotFound(new { error = $"Item with product ID '{productId}' not found" });
            }

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item by product ID {ProductId}", productId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create or update an item
    /// </summary>
    /// <param name="item">Item data</param>
    /// <returns>Created or updated item</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SharedItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SharedItem>> UpsertItem([FromBody] SharedItem item)
    {
        try
        {
            if (item == null)
            {
                return BadRequest(new { error = "Item data is required" });
            }

            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return BadRequest(new { error = "Item name is required" });
            }

            // Set timestamps
            item.UpdatedAt = DateTime.UtcNow;
            if (item.CreatedAt == default)
            {
                item.CreatedAt = DateTime.UtcNow;
            }

            var result = await _priceDataService.UpsertItemAsync(item);
            _logger.LogInformation("Item upserted: {ItemId} - {ItemName}", result.Id, result.Name);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting item");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create or update multiple items in a batch
    /// </summary>
    /// <param name="items">List of items to create or update</param>
    /// <returns>Created or updated items</returns>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(IEnumerable<SharedItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<SharedItem>>> UpsertItemsBatch([FromBody] List<SharedItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return BadRequest(new { error = "Items list is required and cannot be empty" });
            }

            if (items.Count > 1000)
            {
                return BadRequest(new { error = "Batch size cannot exceed 1000 items" });
            }

            var results = new List<SharedItem>();
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    continue; // Skip invalid items
                }

                item.UpdatedAt = DateTime.UtcNow;
                if (item.CreatedAt == default)
                {
                    item.CreatedAt = DateTime.UtcNow;
                }

                var result = await _priceDataService.UpsertItemAsync(item);
                results.Add(result);
            }

            _logger.LogInformation("Batch upsert completed: {Count} items processed", results.Count);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch item upsert");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Search items by name with relevance scoring
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>Matching items ordered by relevance</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<SharedItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedItem>>> SearchItems(
        [FromQuery] string query,
        [FromQuery] int limit = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Query parameter is required" });
            }

            if (limit < 1) limit = 20;
            if (limit > 100) limit = 100;

            var results = await _priceDataService.SearchItemsAsync(query, limit);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching items with query: {Query}", query);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get items by category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Items in the specified category</returns>
    [HttpGet("by-category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<SharedItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedItem>>> GetItemsByCategory(
        string category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest(new { error = "Category is required" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 1000) pageSize = 1000;

            var filter = new ItemFilter { Category = category };
            var items = await _priceDataService.GetItemsAsync(filter, page, pageSize);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items by category: {Category}", category);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get items by brand
    /// </summary>
    /// <param name="brand">Brand name</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Items from the specified brand</returns>
    [HttpGet("by-brand/{brand}")]
    [ProducesResponseType(typeof(IEnumerable<SharedItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedItem>>> GetItemsByBrand(
        string brand,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(brand))
            {
                return BadRequest(new { error = "Brand is required" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 1000) pageSize = 1000;

            var filter = new ItemFilter { Brand = brand };
            var items = await _priceDataService.GetItemsAsync(filter, page, pageSize);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items by brand: {Brand}", brand);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
