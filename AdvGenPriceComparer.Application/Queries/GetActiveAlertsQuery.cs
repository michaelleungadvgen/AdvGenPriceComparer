using AdvGenFlow;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get all active alerts from the system
/// </summary>
public record GetActiveAlertsQuery : IRequest<IEnumerable<AlertLogicEntity>>;
