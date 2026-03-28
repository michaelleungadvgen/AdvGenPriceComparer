using AdvGenFlow;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Commands;

/// <summary>
/// Command to record a price observation
/// </summary>
public record RecordPriceCommand(
    string ItemId,
    string PlaceId,
    decimal Price,
    bool IsOnSale = false,
    decimal? OriginalPrice = null,
    string? SaleDescription = null,
    DateTime? ValidFrom = null,
    DateTime? ValidTo = null,
    string Source = "manual"
) : IRequest<RecordPriceResult>;

/// <summary>
/// Result of recording a price
/// </summary>
public record RecordPriceResult
{
    public bool Success { get; init; }
    public string PriceRecordId { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public PriceRecord? PriceRecord { get; init; }

    public static RecordPriceResult SuccessResult(string priceRecordId, PriceRecord priceRecord) =>
        new() { Success = true, PriceRecordId = priceRecordId, PriceRecord = priceRecord };

    public static RecordPriceResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };

    public static RecordPriceResult ItemNotFound(string itemId) =>
        new() { Success = false, ErrorMessage = $"Item with ID '{itemId}' not found." };

    public static RecordPriceResult PlaceNotFound(string placeId) =>
        new() { Success = false, ErrorMessage = $"Place with ID '{placeId}' not found." };
}
