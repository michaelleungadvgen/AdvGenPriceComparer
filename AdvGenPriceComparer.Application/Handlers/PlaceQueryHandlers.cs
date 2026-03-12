using AdvGenPriceComparer.Application.Mediator;
using AdvGenPriceComparer.Application.Queries;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

/// <summary>
/// Handler for GetAllPlacesQuery
/// </summary>
public class GetAllPlacesQueryHandler : IRequestHandler<GetAllPlacesQuery, IEnumerable<Place>>
{
    private readonly IPlaceRepository _placeRepository;
    private readonly ILogger<GetAllPlacesQueryHandler> _logger;

    public GetAllPlacesQueryHandler(IPlaceRepository placeRepository, ILogger<GetAllPlacesQueryHandler> logger)
    {
        _placeRepository = placeRepository;
        _logger = logger;
    }

    public Task<IEnumerable<Place>> Handle(GetAllPlacesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var places = _placeRepository.GetAll();
            return Task.FromResult(places);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all places");
            return Task.FromResult<IEnumerable<Place>>(new List<Place>());
        }
    }
}

/// <summary>
/// Handler for GetPlaceByIdQuery
/// </summary>
public class GetPlaceByIdQueryHandler : IRequestHandler<GetPlaceByIdQuery, Place?>
{
    private readonly IPlaceRepository _placeRepository;
    private readonly ILogger<GetPlaceByIdQueryHandler> _logger;

    public GetPlaceByIdQueryHandler(IPlaceRepository placeRepository, ILogger<GetPlaceByIdQueryHandler> logger)
    {
        _placeRepository = placeRepository;
        _logger = logger;
    }

    public Task<Place?> Handle(GetPlaceByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var place = _placeRepository.GetById(request.PlaceId);
            return Task.FromResult(place);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting place by ID: {PlaceId}", request.PlaceId);
            return Task.FromResult<Place?>(null);
        }
    }
}
