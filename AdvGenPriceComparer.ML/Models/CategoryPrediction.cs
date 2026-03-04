using Microsoft.ML.Data;

namespace AdvGenPriceComparer.ML.Models;

/// <summary>
/// Output model for category prediction results
/// </summary>
public class CategoryPrediction
{
    /// <summary>
    /// The predicted category label
    /// </summary>
    [ColumnName("PredictedLabel")]
    public string PredictedCategory { get; set; } = string.Empty;

    /// <summary>
    /// Confidence scores for all categories
    /// </summary>
    [ColumnName("Score")]
    public float[] Score { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Maximum confidence score for the prediction
    /// </summary>
    public float Confidence => Score?.Max() ?? 0f;

    /// <summary>
    /// Dictionary mapping category names to their confidence scores
    /// </summary>
    public Dictionary<string, float> CategoryScores { get; set; } = new();
}
