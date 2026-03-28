using AdvGenFlow;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get price history for an item and/or place
/// </summary>
public record GetPriceHistoryQuery(
    string? ItemId = null,
    string? PlaceId = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<IEnumerable<PriceRecord>>;
