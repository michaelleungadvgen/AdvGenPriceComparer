using AdvGenPriceComparer.Application.Queries;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

/// <summary>
/// Handler for GetRecentPriceUpdatesQuery
/// </summary>
public class GetRecentPriceUpdatesQueryHandler : IRequestHandler<GetRecentPriceUpdatesQuery, IEnumerable<PriceRecord>>
{
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<GetRecentPriceUpdatesQueryHandler> _logger;

    public GetRecentPriceUpdatesQueryHandler(
        IPriceRecordRepository priceRecordRepository,
        ILogger<GetRecentPriceUpdatesQueryHandler> logger)
    {
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<IEnumerable<PriceRecord>> Handle(GetRecentPriceUpdatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var records = _priceRecordRepository.GetRecentPriceUpdates(request.Count);
            return Task.FromResult(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent price updates");
            return Task.FromResult<IEnumerable<PriceRecord>>(new List<PriceRecord>());
        }
    }
}

/// <summary>
/// Handler for GetPriceHistoryQuery
/// </summary>
public class GetPriceHistoryQueryHandler : IRequestHandler<GetPriceHistoryQuery, IEnumerable<PriceRecord>>
{
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<GetPriceHistoryQueryHandler> _logger;

    public GetPriceHistoryQueryHandler(
        IPriceRecordRepository priceRecordRepository,
        ILogger<GetPriceHistoryQueryHandler> logger)
    {
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<IEnumerable<PriceRecord>> Handle(GetPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<PriceRecord> records;

            if (!string.IsNullOrEmpty(request.ItemId) && !string.IsNullOrEmpty(request.PlaceId))
            {
                // Get history for specific item at specific place
                records = _priceRecordRepository.GetByItemAndPlace(request.ItemId, request.PlaceId);
            }
            else if (!string.IsNullOrEmpty(request.ItemId))
            {
                // Get history for item across all places
                records = _priceRecordRepository.GetByItem(request.ItemId);
            }
            else if (!string.IsNullOrEmpty(request.PlaceId))
            {
                // Get all prices at a specific place
                records = _priceRecordRepository.GetByPlace(request.PlaceId);
            }
            else
            {
                // Get all price records
                records = _priceRecordRepository.GetAll();
            }

            // Apply date filters if provided
            if (request.From.HasValue)
            {
                records = records.Where(r => r.DateRecorded >= request.From.Value);
            }
            if (request.To.HasValue)
            {
                records = records.Where(r => r.DateRecorded <= request.To.Value);
            }

            return Task.FromResult(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history");
            return Task.FromResult<IEnumerable<PriceRecord>>(new List<PriceRecord>());
        }
    }
}
