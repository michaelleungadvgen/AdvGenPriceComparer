using MediatR;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Statistics for a single store comparison
/// </summary>
public record StoreComparisonStats(
    string StoreName,
    decimal AveragePrice,
    int ProductCount,
    int DealCount
);

/// <summary>
/// Query to get store comparison statistics
/// </summary>
public record GetStoreComparisonStatsQuery : IRequest<IEnumerable<StoreComparisonStats>>;
