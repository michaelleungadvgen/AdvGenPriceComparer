using AdvGenFlow;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get a single place by ID
/// </summary>
public record GetPlaceByIdQuery(string PlaceId) : IRequest<Place?>;
