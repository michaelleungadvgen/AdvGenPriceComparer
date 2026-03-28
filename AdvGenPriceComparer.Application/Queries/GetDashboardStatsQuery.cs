using AdvGenFlow;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Dashboard statistics result
/// </summary>
public record DashboardStatsResult
{
    public int TotalItems { get; init; }
    public int TotalPlaces { get; init; }
    public int TotalPriceRecords { get; init; }
    public int ActiveDeals { get; init; }
    public decimal AverageSavings { get; init; }
    public Dictionary<string, int> ItemsByCategory { get; init; } = new();
    public Dictionary<string, decimal> AveragePriceByCategory { get; init; } = new();
}

/// <summary>
/// Query to get dashboard statistics
/// </summary>
public record GetDashboardStatsQuery : IRequest<DashboardStatsResult>;
