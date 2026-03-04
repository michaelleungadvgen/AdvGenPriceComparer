using System.Collections.Generic;
using System.Threading.Tasks;
using AdvGenPriceComparer.WPF.Chat.Models;

namespace AdvGenPriceComparer.WPF.Chat.Services
{
    public interface IOllamaService
    {
        /// <summary>
        /// Send a chat message and get response
        /// </summary>
        Task<string> ChatAsync(string userMessage, string? systemPrompt = null);

        /// <summary>
        /// Extract structured intent from natural language query
        /// </summary>
        Task<QueryIntent> ExtractIntentAsync(string userQuery);

        /// <summary>
        /// Generate a formatted response based on query results
        /// </summary>
        Task<string> GenerateResponseAsync(QueryIntent intent, ChatResponse data);

        /// <summary>
        /// Check if Ollama server is available
        /// </summary>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Get list of available models
        /// </summary>
        Task<List<string>> GetAvailableModelsAsync();

        /// <summary>
        /// Clear conversation history
        /// </summary>
        void ClearHistory();
    }
}
