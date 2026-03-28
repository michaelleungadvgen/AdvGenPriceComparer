using AdvGenPriceComparer.Application.Commands;
using AdvGenFlow;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

/// <summary>
/// Handler for RecordPriceCommand
/// </summary>
public class RecordPriceCommandHandler : IRequestHandler<RecordPriceCommand, RecordPriceResult>
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<RecordPriceCommandHandler> _logger;

    public RecordPriceCommandHandler(
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        ILogger<RecordPriceCommandHandler> logger)
    {
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<RecordPriceResult> Handle(RecordPriceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate item exists
            var item = _itemRepository.GetById(request.ItemId);
            if (item == null)
            {
                return Task.FromResult(RecordPriceResult.ItemNotFound(request.ItemId));
            }

            // Validate place exists
            var place = _placeRepository.GetById(request.PlaceId);
            if (place == null)
            {
                return Task.FromResult(RecordPriceResult.PlaceNotFound(request.PlaceId));
            }

            // Validate price
            if (request.Price <= 0)
            {
                return Task.FromResult(RecordPriceResult.Failure("Price must be greater than zero."));
            }

            var priceRecord = new PriceRecord
            {
                ItemId = request.ItemId,
                PlaceId = request.PlaceId,
                Price = request.Price,
                IsOnSale = request.IsOnSale,
                OriginalPrice = request.OriginalPrice,
                SaleDescription = request.SaleDescription?.Trim(),
                ValidFrom = request.ValidFrom ?? DateTime.UtcNow,
                ValidTo = request.ValidTo,
                Source = request.Source,
                DateRecorded = DateTime.UtcNow
            };

            _priceRecordRepository.Add(priceRecord);

            _logger.LogInformation(
                "Recorded price: {Price} for item: {ItemId} at place: {PlaceId}",
                request.Price, request.ItemId, request.PlaceId);

            return Task.FromResult(RecordPriceResult.SuccessResult(priceRecord.Id, priceRecord));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording price for item: {ItemId}", request.ItemId);
            return Task.FromResult(RecordPriceResult.Failure($"Failed to record price: {ex.Message}"));
        }
    }
}
