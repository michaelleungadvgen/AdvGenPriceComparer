using AdvGenPriceComparer.Core.Models;
using MediatR;

namespace AdvGenPriceComparer.Application.Commands;

/// <summary>
/// Command to update an existing grocery item
/// </summary>
public record UpdateItemCommand(
    string ItemId,
    string? Name = null,
    string? Brand = null,
    string? Category = null,
    string? Barcode = null,
    string? PackageSize = null,
    string? Unit = null,
    string? Description = null
) : IRequest<UpdateItemResult>;

/// <summary>
/// Result of updating an item
/// </summary>
public record UpdateItemResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Item? Item { get; init; }

    public static UpdateItemResult SuccessResult(Item item) =>
        new() { Success = true, Item = item };

    public static UpdateItemResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };

    public static UpdateItemResult NotFound(string itemId) =>
        new() { Success = false, ErrorMessage = $"Item with ID '{itemId}' not found." };
}
