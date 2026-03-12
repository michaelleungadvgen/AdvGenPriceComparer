using AdvGenPriceComparer.Application.Mediator;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Statistics for a single category
/// </summary>
public record CategoryStats(
    string Category,
    decimal AveragePrice,
    int ItemCount,
    decimal MinPrice,
    decimal MaxPrice
);

/// <summary>
/// Query to get statistics grouped by category
/// </summary>
public record GetCategoryStatsQuery : IRequest<IEnumerable<CategoryStats>>;
