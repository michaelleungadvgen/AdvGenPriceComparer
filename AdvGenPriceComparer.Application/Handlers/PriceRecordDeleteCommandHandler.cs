using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

public class DeletePriceRecordCommandHandler : IRequestHandler<DeletePriceRecordCommand, DeletePriceRecordResult>
{
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<DeletePriceRecordCommandHandler> _logger;

    public DeletePriceRecordCommandHandler(
        IPriceRecordRepository priceRecordRepository,
        ILogger<DeletePriceRecordCommandHandler> logger)
    {
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<DeletePriceRecordResult> Handle(DeletePriceRecordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var record = _priceRecordRepository.GetById(request.PriceRecordId);
            if (record == null)
                return Task.FromResult(DeletePriceRecordResult.NotFound(request.PriceRecordId));

            _priceRecordRepository.Delete(request.PriceRecordId);
            _logger.LogInformation("Deleted price record with ID: {PriceRecordId}", request.PriceRecordId);
            return Task.FromResult(DeletePriceRecordResult.SuccessResult());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting price record: {PriceRecordId}", request.PriceRecordId);
            return Task.FromResult(DeletePriceRecordResult.Failure($"Failed to delete price record: {ex.Message}"));
        }
    }
}
