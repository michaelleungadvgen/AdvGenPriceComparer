using AdvGenFlow;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Commands;

public record UpdatePlaceCommand(
    string PlaceId,
    string? Name = null,
    string? Chain = null,
    string? Address = null,
    string? Suburb = null,
    string? State = null,
    string? Postcode = null,
    string? Phone = null
) : IRequest<UpdatePlaceResult>;

public record UpdatePlaceResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Place? Place { get; init; }

    public static UpdatePlaceResult SuccessResult(Place place) => new() { Success = true, Place = place };
    public static UpdatePlaceResult NotFound(string placeId) =>
        new() { Success = false, ErrorMessage = $"Place not found: {placeId}" };
    public static UpdatePlaceResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
