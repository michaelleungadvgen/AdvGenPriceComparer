using AdvGenPriceComparer.Server.Models;
using AdvGenPriceComparer.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvGenPriceComparer.Server.Controllers;

/// <summary>
/// API controller for store/location management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
    private readonly IPriceDataService _priceDataService;
    private readonly ILogger<PlacesController> _logger;

    public PlacesController(IPriceDataService priceDataService, ILogger<PlacesController> logger)
    {
        _priceDataService = priceDataService;
        _logger = logger;
    }

    /// <summary>
    /// Get all places/stores with optional filtering and pagination
    /// </summary>
    /// <param name="chain">Filter by store chain (e.g., Coles, Woolworths)</param>
    /// <param name="state">Filter by state</param>
    /// <param name="search">Search query for store name or location</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of places per page</param>
    /// <returns>Paginated list of places</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SharedPlace>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedPlace>>> GetPlaces(
        [FromQuery] string? chain = null,
        [FromQuery] string? state = null,
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

            var filter = new PlaceFilter
            {
                Chain = chain,
                State = state,
                SearchQuery = search
            };

            var places = await _priceDataService.GetPlacesAsync(filter, page, pageSize);
            return Ok(places);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving places");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a single place by ID
    /// </summary>
    /// <param name="id">Place ID</param>
    /// <returns>Place details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SharedPlace), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedPlace>> GetPlaceById(int id)
    {
        try
        {
            var place = await _priceDataService.GetPlaceByIdAsync(id);
            if (place == null)
            {
                return NotFound(new { error = $"Place with ID {id} not found" });
            }

            return Ok(place);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving place {PlaceId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create or update a place
    /// </summary>
    /// <param name="place">Place data</param>
    /// <returns>Created or updated place</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SharedPlace), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SharedPlace>> UpsertPlace([FromBody] SharedPlace place)
    {
        try
        {
            if (place == null)
            {
                return BadRequest(new { error = "Place data is required" });
            }

            if (string.IsNullOrWhiteSpace(place.Name))
            {
                return BadRequest(new { error = "Place name is required" });
            }

            // Set timestamps
            if (place.CreatedAt == default)
            {
                place.CreatedAt = DateTime.UtcNow;
            }

            var result = await _priceDataService.UpsertPlaceAsync(place);
            _logger.LogInformation("Place upserted: {PlaceId} - {PlaceName}", result.Id, result.Name);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting place");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create or update multiple places in a batch
    /// </summary>
    /// <param name="places">List of places to create or update</param>
    /// <returns>Created or updated places</returns>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(IEnumerable<SharedPlace>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<SharedPlace>>> UpsertPlacesBatch([FromBody] List<SharedPlace> places)
    {
        try
        {
            if (places == null || places.Count == 0)
            {
                return BadRequest(new { error = "Places list is required and cannot be empty" });
            }

            if (places.Count > 500)
            {
                return BadRequest(new { error = "Batch size cannot exceed 500 places" });
            }

            var results = new List<SharedPlace>();
            foreach (var place in places)
            {
                if (string.IsNullOrWhiteSpace(place.Name))
                {
                    continue; // Skip invalid places
                }

                if (place.CreatedAt == default)
                {
                    place.CreatedAt = DateTime.UtcNow;
                }

                var result = await _priceDataService.UpsertPlaceAsync(place);
                results.Add(result);
            }

            _logger.LogInformation("Batch upsert completed: {Count} places processed", results.Count);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch place upsert");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get places by chain/store brand
    /// </summary>
    /// <param name="chain">Chain name (e.g., Coles, Woolworths, Drakes)</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Places per page</param>
    /// <returns>Places belonging to the specified chain</returns>
    [HttpGet("by-chain/{chain}")]
    [ProducesResponseType(typeof(IEnumerable<SharedPlace>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedPlace>>> GetPlacesByChain(
        string chain,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(chain))
            {
                return BadRequest(new { error = "Chain is required" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 1000) pageSize = 1000;

            var filter = new PlaceFilter { Chain = chain };
            var places = await _priceDataService.GetPlacesAsync(filter, page, pageSize);
            return Ok(places);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving places by chain: {Chain}", chain);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get places by state
    /// </summary>
    /// <param name="state">State abbreviation (e.g., QLD, NSW, VIC)</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Places per page</param>
    /// <returns>Places in the specified state</returns>
    [HttpGet("by-state/{state}")]
    [ProducesResponseType(typeof(IEnumerable<SharedPlace>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedPlace>>> GetPlacesByState(
        string state,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                return BadRequest(new { error = "State is required" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 1000) pageSize = 1000;

            var filter = new PlaceFilter { State = state };
            var places = await _priceDataService.GetPlacesAsync(filter, page, pageSize);
            return Ok(places);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving places by state: {State}", state);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Search places by name or location
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>Matching places</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<SharedPlace>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedPlace>>> SearchPlaces(
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

            var filter = new PlaceFilter { SearchQuery = query };
            var places = await _priceDataService.GetPlacesAsync(filter, 1, limit);
            return Ok(places);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching places with query: {Query}", query);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all available store chains
    /// </summary>
    /// <returns>List of unique chain names</returns>
    [HttpGet("chains")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetChains()
    {
        try
        {
            // Get all places and extract unique chains
            var places = await _priceDataService.GetPlacesAsync(null, 1, 10000);
            var chains = places
                .Where(p => !string.IsNullOrWhiteSpace(p.Chain))
                .Select(p => p.Chain!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();

            return Ok(chains);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chains");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all available states
    /// </summary>
    /// <returns>List of unique state names</returns>
    [HttpGet("states")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetStates()
    {
        try
        {
            // Get all places and extract unique states
            var places = await _priceDataService.GetPlacesAsync(null, 1, 10000);
            var states = places
                .Where(p => !string.IsNullOrWhiteSpace(p.State))
                .Select(p => p.State!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();

            return Ok(states);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving states");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
