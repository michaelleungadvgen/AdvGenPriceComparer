using AdvGenPriceComparer.Application.Commands;
using AdvGenFlow;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

/// <summary>
/// Handler for CreatePlaceCommand
/// </summary>
public class CreatePlaceCommandHandler : IRequestHandler<CreatePlaceCommand, CreatePlaceResult>
{
    private readonly IPlaceRepository _placeRepository;
    private readonly ILogger<CreatePlaceCommandHandler> _logger;

    public CreatePlaceCommandHandler(IPlaceRepository placeRepository, ILogger<CreatePlaceCommandHandler> logger)
    {
        _placeRepository = placeRepository;
        _logger = logger;
    }

    public Task<CreatePlaceResult> Handle(CreatePlaceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Task.FromResult(CreatePlaceResult.Failure("Place name is required."));
            }

            if (string.IsNullOrWhiteSpace(request.Chain))
            {
                return Task.FromResult(CreatePlaceResult.Failure("Chain is required."));
            }

            var place = new Place
            {
                Name = request.Name.Trim(),
                Chain = request.Chain.Trim(),
                Address = request.Address?.Trim(),
                Suburb = request.Suburb?.Trim(),
                State = request.State?.Trim(),
                Postcode = request.Postcode?.Trim(),
                Phone = request.Phone?.Trim(),
                DateAdded = DateTime.UtcNow
            };

            _placeRepository.Add(place);

            _logger.LogInformation("Created place: {PlaceName} with ID: {PlaceId}", place.Name, place.Id);

            return Task.FromResult(CreatePlaceResult.SuccessResult(place.Id, place));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating place: {PlaceName}", request.Name);
            return Task.FromResult(CreatePlaceResult.Failure($"Failed to create place: {ex.Message}"));
        }
    }
}
