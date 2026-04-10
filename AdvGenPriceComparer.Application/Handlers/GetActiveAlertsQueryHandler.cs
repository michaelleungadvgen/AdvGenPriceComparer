using AdvGenFlow;
using AdvGenPriceComparer.Application.Queries;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

/// <summary>
/// Handler for GetActiveAlertsQuery
/// </summary>
public class GetActiveAlertsQueryHandler : IRequestHandler<GetActiveAlertsQuery, IEnumerable<AlertLogicEntity>>
{
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<GetActiveAlertsQueryHandler> _logger;

    public GetActiveAlertsQueryHandler(IAlertRepository alertRepository, ILogger<GetActiveAlertsQueryHandler> logger)
    {
        _alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IEnumerable<AlertLogicEntity>> Handle(GetActiveAlertsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting active alerts");
            var alerts = _alertRepository.GetActiveAlerts();
            _logger.LogDebug("Retrieved {Count} active alerts", alerts?.Count() ?? 0);
            return Task.FromResult(alerts ?? Enumerable.Empty<AlertLogicEntity>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active alerts");
            throw;
        }
    }
}
