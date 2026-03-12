using AdvGenPriceComparer.Core.Models;
using MediatR;

namespace AdvGenPriceComparer.Application.Queries;

/// <summary>
/// Query to search items by name, brand, or barcode
/// </summary>
public record SearchItemsQuery(
    string SearchTerm,
    bool IncludeBrand = true,
    bool IncludeBarcode = true
) : IRequest<IEnumerable<Item>>;
