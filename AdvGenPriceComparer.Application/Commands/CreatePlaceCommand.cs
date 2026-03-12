using AdvGenPriceComparer.Application.Mediator;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Commands;

/// <summary>
/// Command to create a new supermarket/place
/// </summary>
public record CreatePlaceCommand(
    string Name,
    string Chain,
    string? Address = null,
    string? Suburb = null,
    string? State = null,
    string? Postcode = null,
    string? Phone = null
) : IRequest<CreatePlaceResult>;

/// <summary>
/// Result of creating a place
/// </summary>
public record CreatePlaceResult
{
    public bool Success { get; init; }
    public string PlaceId { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public Place? Place { get; init; }

    public static CreatePlaceResult SuccessResult(string placeId, Place place) =>
        new() { Success = true, PlaceId = placeId, Place = place };

    public static CreatePlaceResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
