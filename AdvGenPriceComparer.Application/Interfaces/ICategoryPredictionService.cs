using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Interfaces
{
    /// <summary>
    /// Interface for category prediction service.
    /// Implemented by ML layer to provide auto-categorization capabilities.
    /// This keeps Application layer independent of ML implementation details.
    /// </summary>
    public interface ICategoryPredictionService
    {
        /// <summary>
        /// Indicates whether the prediction model is loaded and ready
        /// </summary>
        bool IsModelLoaded { get; }

        /// <summary>
        /// Try to auto-categorize an item based on its properties
        /// </summary>
        /// <param name="item">The item to categorize</param>
        /// <param name="category">The predicted category (output)</param>
        /// <returns>True if categorization succeeded with sufficient confidence</returns>
        bool TryAutoCategorize(Item item, out string category);

        /// <summary>
        /// Try to auto-categorize an item with a specific confidence threshold
        /// </summary>
        /// <param name="item">The item to categorize</param>
        /// <param name="threshold">Minimum confidence threshold (0.0 to 1.0)</param>
        /// <param name="category">The predicted category (output)</param>
        /// <returns>True if categorization succeeded with sufficient confidence</returns>
        bool TryAutoCategorize(Item item, float threshold, out string category);

        /// <summary>
        /// Predict category for an item and return confidence score
        /// </summary>
        /// <param name="item">The item to categorize</param>
        /// <returns>Prediction result with category and confidence</returns>
        CategoryPredictionResult PredictCategory(Item item);
    }

    /// <summary>
    /// Result of category prediction
    /// </summary>
    public class CategoryPredictionResult
    {
        /// <summary>
        /// The predicted category name
        /// </summary>
        public string PredictedCategory { get; set; } = string.Empty;

        /// <summary>
        /// Confidence score (0.0 to 1.0)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// All category scores for this prediction
        /// </summary>
        public Dictionary<string, float> CategoryScores { get; set; } = new();
    }
}
