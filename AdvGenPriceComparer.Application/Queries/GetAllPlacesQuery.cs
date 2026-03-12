using AdvGenPriceComparer.Core.Models;
using MediatR;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get all places/stores
/// </summary>
public record GetAllPlacesQuery : IRequest<IEnumerable<Place>>;
