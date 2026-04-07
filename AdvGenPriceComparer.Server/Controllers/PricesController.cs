using AdvGenPriceComparer.Server.Models;
using AdvGenPriceComparer.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvGenPriceComparer.Server.Controllers;

/// <summary>
/// API controller for price data operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PricesController : ControllerBase
{
    private readonly IPriceDataService _priceDataService;
    private readonly ILogger<PricesController> _logger;

    public PricesController(IPriceDataService priceDataService, ILogger<PricesController> logger)
    {
        _priceDataService = priceDataService;
        _logger = logger;
    }

    /// <summary>
    /// Upload price data from a client
    /// </summary>
    /// <param name="request">Data upload request containing items, places, and price records</param>
    /// <returns>Upload result with counts of uploaded items</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UploadResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResult>> UploadData([FromBody] DataUploadRequest request)
    {
        try
        {
            _logger.LogInformation("Received upload request with {ItemCount} items, {PlaceCount} places, {PriceCount} prices",
                request.Items.Count, request.Places.Count, request.PriceRecords.Count);

            // Get API key ID from the HttpContext (set by ApiKeyMiddleware)
            var apiKeyId = HttpContext.Items["ApiKeyId"] as int? ?? 0;

            var result = await _priceDataService.UploadDataAsync(request, apiKeyId);

            if (result.Success)
            {
                _logger.LogInformation("Upload successful: {Items} items, {Places} places, {Prices} prices",
                    result.ItemsUploaded, result.PlacesUploaded, result.PricesUploaded);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Upload failed: {Error}", result.ErrorMessage);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing upload request");
            return BadRequest(new UploadResult
            {
                Success = false,
                ErrorMessage = "An error occurred while processing the upload request."
            });
        }
    }

    /// <summary>
    /// Download price data with optional filtering
    /// </summary>
    /// <param name="filter">Filter options for price records</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Filtered price records</returns>
    [HttpGet("download")]
    [ProducesResponseType(typeof(IEnumerable<SharedPriceRecord>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedPriceRecord>>> DownloadData(
        [FromQuery] PriceFilter? filter = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 1000) pageSize = 1000; // Max limit

            var records = await _priceDataService.GetPriceRecordsAsync(filter, page, pageSize);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading price data");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Search for products by name
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>Matching items</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<SharedItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedItem>>> SearchProducts(
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
            _logger.LogError(ex, "Error searching products with query: {Query}", query);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Compare prices for a specific item across all stores
    /// </summary>
    /// <param name="itemId">Item ID to compare</param>
    /// <returns>Price records for the item across all stores</returns>
    [HttpGet("compare/{itemId}")]
    [ProducesResponseType(typeof(IEnumerable<SharedPriceRecord>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<SharedPriceRecord>>> ComparePrices(int itemId)
    {
        try
        {
            // Verify item exists
            var item = await _priceDataService.GetItemByIdAsync(itemId);
            if (item == null)
            {
                return NotFound(new { error = $"Item with ID {itemId} not found" });
            }

            var comparisons = await _priceDataService.ComparePricesAsync(itemId);
            return Ok(comparisons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing prices for item {ItemId}", itemId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get the latest deals across all stores
    /// </summary>
    /// <param name="limit">Maximum number of deals to return</param>
    /// <returns>Latest price deals</returns>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(IEnumerable<SharedPriceRecord>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedPriceRecord>>> GetLatestDeals(
        [FromQuery] int limit = 50)
    {
        try
        {
            if (limit < 1) limit = 50;
            if (limit > 200) limit = 200;

            var deals = await _priceDataService.GetLatestDealsAsync(limit);
            return Ok(deals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest deals");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get price history for a specific item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="placeId">Optional place/store ID filter</param>
    /// <param name="from">Optional start date</param>
    /// <param name="to">Optional end date</param>
    /// <returns>Historical price records</returns>
    [HttpGet("history/{itemId}")]
    [ProducesResponseType(typeof(IEnumerable<SharedPriceRecord>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<SharedPriceRecord>>> GetPriceHistory(
        int itemId,
        [FromQuery] int? placeId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        try
        {
            // Verify item exists
            var item = await _priceDataService.GetItemByIdAsync(itemId);
            if (item == null)
            {
                return NotFound(new { error = $"Item with ID {itemId} not found" });
            }

            var history = await _priceDataService.GetPriceHistoryAsync(itemId, placeId, from, to);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history for item {ItemId}", itemId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get current price for an item at a specific place
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="placeId">Place ID</param>
    /// <returns>Current price record or 404 if not found</returns>
    [HttpGet("current/{itemId}/{placeId}")]
    [ProducesResponseType(typeof(SharedPriceRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedPriceRecord>> GetCurrentPrice(int itemId, int placeId)
    {
        try
        {
            var price = await _priceDataService.GetCurrentPriceAsync(itemId, placeId);
            if (price == null)
            {
                return NotFound(new { error = "No current price found for this item at the specified location" });
            }

            return Ok(price);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current price for item {ItemId} at place {PlaceId}", itemId, placeId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get server statistics
    /// </summary>
    /// <returns>Server statistics including item counts and API key info</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ServerStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServerStats>> GetServerStats()
    {
        try
        {
            var stats = await _priceDataService.GetServerStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server statistics");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
