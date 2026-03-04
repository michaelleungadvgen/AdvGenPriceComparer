namespace AdvGenPriceComparer.ML.Models;

/// <summary>
/// Result of a model training operation
/// </summary>
public class TrainingResult
{
    /// <summary>
    /// Whether training was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Model macro accuracy (average across all categories)
    /// </summary>
    public double Accuracy { get; set; }

    /// <summary>
    /// Micro accuracy (overall prediction accuracy)
    /// </summary>
    public double MicroAccuracy { get; set; }

    /// <summary>
    /// Number of items used for training
    /// </summary>
    public int TrainingItemCount { get; set; }

    /// <summary>
    /// Path where model was saved
    /// </summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>
    /// Training duration
    /// </summary>
    public TimeSpan Duration { get; set; }
}
