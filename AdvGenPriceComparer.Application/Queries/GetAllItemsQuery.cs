using AdvGenPriceComparer.Core.Models;
using MediatR;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to get all grocery items
/// </summary>
public record GetAllItemsQuery : IRequest<IEnumerable<Item>>;
