using AdvGenFlow;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get a single item by ID
/// </summary>
public record GetItemByIdQuery(string ItemId) : IRequest<Item?>;
