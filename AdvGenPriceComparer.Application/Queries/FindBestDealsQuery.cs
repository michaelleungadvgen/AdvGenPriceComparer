using AdvGenPriceComparer.Application.Mediator;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Represents a single best deal result
/// </summary>
public record BestDealResult(
    Item Item,
    decimal LowestPrice,
    Place Place,
    decimal? SavingsPercent = null
);

/// <summary>
/// Query to find the best deals across all stores
/// </summary>
public record FindBestDealsQuery(string? Category = null) : IRequest<IEnumerable<BestDealResult>>;
