using AdvGenFlow;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get all grocery items
/// </summary>
public record GetAllItemsQuery : IRequest<IEnumerable<Item>>;
