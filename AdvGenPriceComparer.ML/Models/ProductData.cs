using Microsoft.ML.Data;

namespace AdvGenPriceComparer.ML.Models;

/// <summary>
/// Input data model for product category prediction
/// </summary>
public class ProductData
{
    /// <summary>
    /// Product name
    /// </summary>
    [LoadColumn(0)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product brand
    /// </summary>
    [LoadColumn(1)]
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Product description
    /// </summary>
    [LoadColumn(2)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Store name where product is sold
    /// </summary>
    [LoadColumn(3)]
    public string Store { get; set; } = string.Empty;

    /// <summary>
    /// Category label for training (mapped from Label column)
    /// </summary>
    [LoadColumn(4)]
    [ColumnName("Label")]
    public string Category { get; set; } = string.Empty;
}
