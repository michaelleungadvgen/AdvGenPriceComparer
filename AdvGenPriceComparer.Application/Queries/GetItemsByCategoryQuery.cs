using AdvGenPriceComparer.Application.Mediator;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get items filtered by category
/// </summary>
public record GetItemsByCategoryQuery(string Category) : IRequest<IEnumerable<Item>>;
