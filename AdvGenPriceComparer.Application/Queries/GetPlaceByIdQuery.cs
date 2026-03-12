using AdvGenPriceComparer.Core.Models;
using MediatR;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get a single place by ID
/// </summary>
public record GetPlaceByIdQuery(string PlaceId) : IRequest<Place?>;
