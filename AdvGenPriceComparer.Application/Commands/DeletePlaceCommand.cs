using AdvGenFlow;

namespace AdvGenPriceComparer.Application.Commands;

public record DeletePlaceCommand(string PlaceId) : IRequest<DeletePlaceResult>;

public record DeletePlaceResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static DeletePlaceResult SuccessResult() => new() { Success = true };
    public static DeletePlaceResult NotFound(string placeId) =>
        new() { Success = false, ErrorMessage = $"Place not found: {placeId}" };
    public static DeletePlaceResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
