using AdvGenPriceComparer.Application.Mediator;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get recent price updates
/// </summary>
public record GetRecentPriceUpdatesQuery(int Count = 10) : IRequest<IEnumerable<PriceRecord>>;
