using AdvGenFlow;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get all places/stores
/// </summary>
public record GetAllPlacesQuery : IRequest<IEnumerable<Place>>;
