using MediatR;

namespace AdvGenPriceComparer.Application.Commands;

/// <summary>
/// Command to delete a grocery item
/// </summary>
public record DeleteItemCommand(string ItemId) : IRequest<DeleteItemResult>;

/// <summary>
/// Result of deleting an item
/// </summary>
public record DeleteItemResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static DeleteItemResult SuccessResult() =>
        new() { Success = true };

    public static DeleteItemResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };

    public static DeleteItemResult NotFound(string itemId) =>
        new() { Success = false, ErrorMessage = $"Item with ID '{itemId}' not found." };
}
