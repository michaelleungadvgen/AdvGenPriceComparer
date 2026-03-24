using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

public class DeletePlaceCommandHandler : IRequestHandler<DeletePlaceCommand, DeletePlaceResult>
{
    private readonly IPlaceRepository _placeRepository;
    private readonly ILogger<DeletePlaceCommandHandler> _logger;

    public DeletePlaceCommandHandler(IPlaceRepository placeRepository, ILogger<DeletePlaceCommandHandler> logger)
    {
        _placeRepository = placeRepository;
        _logger = logger;
    }

    public Task<DeletePlaceResult> Handle(DeletePlaceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var place = _placeRepository.GetById(request.PlaceId);
            if (place == null)
                return Task.FromResult(DeletePlaceResult.NotFound(request.PlaceId));

            _placeRepository.Delete(request.PlaceId);
            _logger.LogInformation("Deleted place with ID: {PlaceId}", request.PlaceId);
            return Task.FromResult(DeletePlaceResult.SuccessResult());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting place: {PlaceId}", request.PlaceId);
            return Task.FromResult(DeletePlaceResult.Failure($"Failed to delete place: {ex.Message}"));
        }
    }
}

public class UpdatePlaceCommandHandler : IRequestHandler<UpdatePlaceCommand, UpdatePlaceResult>
{
    private readonly IPlaceRepository _placeRepository;
    private readonly ILogger<UpdatePlaceCommandHandler> _logger;

    public UpdatePlaceCommandHandler(IPlaceRepository placeRepository, ILogger<UpdatePlaceCommandHandler> logger)
    {
        _placeRepository = placeRepository;
        _logger = logger;
    }

    public Task<UpdatePlaceResult> Handle(UpdatePlaceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var place = _placeRepository.GetById(request.PlaceId);
            if (place == null)
                return Task.FromResult(UpdatePlaceResult.NotFound(request.PlaceId));

            if (request.Name != null) place.Name = request.Name.Trim();
            if (request.Chain != null) place.Chain = request.Chain.Trim();
            if (request.Address != null) place.Address = request.Address.Trim();
            if (request.Suburb != null) place.Suburb = request.Suburb.Trim();
            if (request.State != null) place.State = request.State.Trim();
            if (request.Postcode != null) place.Postcode = request.Postcode.Trim();
            if (request.Phone != null) place.Phone = request.Phone.Trim();

            _placeRepository.Update(place);
            _logger.LogInformation("Updated place: {PlaceName} with ID: {PlaceId}", place.Name, place.Id);
            return Task.FromResult(UpdatePlaceResult.SuccessResult(place));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating place: {PlaceId}", request.PlaceId);
            return Task.FromResult(UpdatePlaceResult.Failure($"Failed to update place: {ex.Message}"));
        }
    }
}
