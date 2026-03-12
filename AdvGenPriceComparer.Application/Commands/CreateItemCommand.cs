using AdvGenPriceComparer.Application.Mediator;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Commands;

/// <summary>
/// Command to create a new grocery item
/// </summary>
public record CreateItemCommand(
    string Name,
    string? Brand = null,
    string? Category = null,
    string? Barcode = null,
    string? PackageSize = null,
    string? Unit = null,
    string? Description = null
) : IRequest<CreateItemResult>;

/// <summary>
/// Result of creating an item
/// </summary>
public record CreateItemResult
{
    public bool Success { get; init; }
    public string ItemId { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public Item? Item { get; init; }

    public static CreateItemResult SuccessResult(string itemId, Item item) =>
        new() { Success = true, ItemId = itemId, Item = item };

    public static CreateItemResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
