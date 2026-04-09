using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdvGenPriceComparer.WPF.Chat.Models;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.Chat.Services
{
    /// <summary>
    /// Service for interacting with Ollama LLM for natural language price queries.
    /// Uses optimized prompts from SystemPrompts for better intent recognition.
    /// </summary>
    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggerService _logger;
        private readonly List<ChatMessage> _conversationHistory = new();
        private string _model = "llama3.2";
        private const string OllamaBaseUrl = "http://localhost:11434";

        public OllamaService(ILoggerService logger)
        {
            _logger = logger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(OllamaBaseUrl),
                Timeout = TimeSpan.FromSeconds(60)
            };
        }

        /// <summary>
        /// Constructor for testing - allows injecting HttpClient
        /// </summary>
        public OllamaService(ILoggerService logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Ollama server not available: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/tags");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var models = new List<string>();
                if (doc.RootElement.TryGetProperty("models", out var modelsElement))
                {
                    foreach (var model in modelsElement.EnumerateArray())
                    {
                        if (model.TryGetProperty("name", out var nameElement))
                        {
                            models.Add(nameElement.GetString() ?? "");
                        }
                    }
                }
                return models;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get Ollama models: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<string> ChatAsync(string userMessage, string? systemPrompt = null)
        {
            try
            {
                _conversationHistory.Add(new ChatMessage
                {
                    Role = MessageRole.User,
                    Content = userMessage
                });
                var messages = new List<object>();
                if (!string.IsNullOrEmpty(systemPrompt))
                {
                    messages.Add(new { role = "system", content = systemPrompt });
                }
                var recentHistory = _conversationHistory.TakeLast(10);
                foreach (var msg in recentHistory)
                {
                    messages.Add(new
                    {
                        role = msg.Role == MessageRole.User ? "user" : "assistant",
                        content = msg.Content
                    });
                }
                var requestBody = new
                {
                    model = _model,
                    messages = messages.ToArray(),
                    stream = false
                };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogInfo($"Sending chat request to Ollama with model: {_model}");
                var response = await _httpClient.PostAsync("/api/chat", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                string assistantMessage = "I am sorry, I could not process that request.";
                if (doc.RootElement.TryGetProperty("message", out var messageElement))
                {
                    if (messageElement.TryGetProperty("content", out var contentElement))
                    {
                        assistantMessage = contentElement.GetString() ?? assistantMessage;
                    }
                }
                _conversationHistory.Add(new ChatMessage
                {
                    Role = MessageRole.Assistant,
                    Content = assistantMessage
                });
                var preview = assistantMessage.Length > 50 ? assistantMessage.Substring(0, 50) : assistantMessage;
                _logger.LogInfo($"Chat response received: {preview}...");
                return assistantMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ollama chat error: {ex.Message}");
                return "I am sorry, I encountered an error processing your request. Please ensure Ollama is running and try again.";
            }
        }

        public async Task<QueryIntent> ExtractIntentAsync(string userQuery)
        {
            try
            {
                var systemPrompt = SystemPrompts.IntentExtractionPrompt;
                var prompt = BuildIntentExtractionPrompt(userQuery);
                var response = await ChatAsync(prompt, systemPrompt);
                var intent = ParseIntentFromResponse(response, userQuery);
                if (intent != null)
                {
                    await EnrichTemporalInformation(intent);
                    return intent;
                }
                _logger.LogWarning($"Could not parse intent from response: {response}");
                return new QueryIntent { Type = QueryType.Unknown, OriginalQuery = userQuery };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Intent extraction error: {ex.Message}");
                return new QueryIntent { Type = QueryType.Unknown, OriginalQuery = userQuery };
            }
        }

        public async Task<string> GenerateResponseAsync(QueryIntent intent, ChatResponse data)
        {
            try
            {
                var systemPrompt = SystemPrompts.GetResponsePromptForQueryType(intent.Type);
                var contextBuilder = new StringBuilder();
                contextBuilder.AppendLine($"Query Type: {intent.Type}");
                contextBuilder.AppendLine($"Product: {intent.ProductName ?? "N/A"}");
                contextBuilder.AppendLine($"Category: {intent.Category ?? "N/A"}");
                contextBuilder.AppendLine($"Store: {intent.Store ?? "N/A"}");
                contextBuilder.AppendLine();
                if (data.RelatedItems.Any())
                {
                    contextBuilder.AppendLine("Found Items:");
                    foreach (var item in data.RelatedItems.Take(5))
                    {
                        contextBuilder.AppendLine($"- {item.Name} ({item.Brand ?? "No brand"})");
                    }
                }
                if (data.RelatedPrices.Any())
                {
                    contextBuilder.AppendLine("Price Records:");
                    foreach (var price in data.RelatedPrices.Take(5))
                    {
                        contextBuilder.AppendLine($"- Item ID: {price.ItemId} at Store ID: {price.PlaceId}: ${price.Price:F2}");
                    }
                }
                if (data.RelatedStores.Any())
                {
                    contextBuilder.AppendLine("Stores:");
                    foreach (var store in data.RelatedStores.Take(5))
                    {
                        contextBuilder.AppendLine($"- {store.Name} ({store.Chain ?? "Independent"})");
                    }
                }
                var prompt = $@"Based on the following query results, provide a helpful, conversational response to the user.

{contextBuilder}

Original Query: {intent.OriginalQuery}

Provide a natural, friendly response that answers the user's question based on this data. If no data was found, suggest alternatives or ask for clarification.

Response:";
                return await ChatAsync(prompt, systemPrompt);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Response generation error: {ex.Message}");
                return "I found some results for you, but I am having trouble formatting the response. Please check the details below.";
            }
        }

        public void ClearHistory()
        {
            _conversationHistory.Clear();
            _logger.LogInfo("Chat history cleared");
        }

        public void SetModel(string model)
        {
            _model = model;
            _logger.LogInfo($"Ollama model set to: {model}");
        }

        private static string BuildIntentExtractionPrompt(string userQuery)
        {
            var escapedQuery = userQuery.Replace("\"", "\\\"");
            return $@"Extract intent from this grocery price query:

User Query: ""{escapedQuery}""

Respond ONLY with valid JSON matching this exact schema:
{{
    ""queryType"": ""PriceQuery|PriceComparison|CheapestItem|ItemsInCategory|ItemsOnSale|PriceHistory|BestDeal|StoreInventory|BudgetQuery|GeneralChat"",
    ""productName"": ""product name or null"",
    ""category"": ""category or null (Dairy, Produce, Meat, Beverages, Pantry, Frozen, Bakery, etc.)"",
    ""store"": ""store name or null (Coles, Woolworths, Aldi, Drakes, IGA, etc.)"",
    ""maxPrice"": number or null,
    ""minPrice"": number or null,
    ""onSaleOnly"": boolean,
    ""dateFrom"": ""ISO date (yyyy-MM-dd) or null"",
    ""dateTo"": ""ISO date (yyyy-MM-dd) or null"",
    ""comparison"": ""Cheaper|MoreExpensive|SimilarPrice or null"",
    ""limit"": number (default 10, max 50)
}}

JSON Response:";
        }

        private static QueryIntent? ParseIntentFromResponse(string response, string originalQuery)
        {
            var jsonMatch = Regex.Match(response, @"\{[\s\S]*\}");
            if (!jsonMatch.Success)
            {
                return null;
            }
            var json = jsonMatch.Value;
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var intent = JsonSerializer.Deserialize<QueryIntent>(json, options);
            if (intent != null)
            {
                intent.OriginalQuery = originalQuery;
                if (!intent.Limit.HasValue || intent.Limit.Value < 1)
                {
                    intent.Limit = 10;
                }
                else if (intent.Limit.Value > 50)
                {
                    intent.Limit = 50;
                }
            }
            return intent;
        }

        private async Task EnrichTemporalInformation(QueryIntent intent)
        {
            if (intent.DateFrom.HasValue || intent.DateTo.HasValue)
            {
                return;
            }
            var temporalKeywords = new[] { "today", "this week", "last week", "this month", "last month", "yesterday", "last 7 days", "last 30 days", "since", "between", "trend", "history" };
            if (!temporalKeywords.Any(kw => intent.OriginalQuery?.Contains(kw, StringComparison.OrdinalIgnoreCase) == true))
            {
                return;
            }
            try
            {
                var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                var temporalPrompt = $@"Extract temporal information from this query: ""{intent.OriginalQuery}""

Current date: {currentDate}

Respond ONLY with JSON: {{ ""dateFrom"": ""yyyy-MM-dd or null"", ""dateTo"": ""yyyy-MM-dd or null"" }}";
                var response = await ChatAsync(temporalPrompt, SystemPrompts.GetTemporalExtractionPrompt());
                var jsonMatch = Regex.Match(response, @"\{[\s\S]*\}");
                if (jsonMatch.Success)
                {
                    var json = jsonMatch.Value;
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("dateFrom", out var dateFromElement) && dateFromElement.ValueKind != JsonValueKind.Null)
                    {
                        var dateFromStr = dateFromElement.GetString();
                        if (DateTime.TryParse(dateFromStr, out var dateFrom))
                        {
                            intent.DateFrom = dateFrom;
                        }
                    }
                    if (root.TryGetProperty("dateTo", out var dateToElement) && dateToElement.ValueKind != JsonValueKind.Null)
                    {
                        var dateToStr = dateToElement.GetString();
                        if (DateTime.TryParse(dateToStr, out var dateTo))
                        {
                            intent.DateTo = dateTo;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to enrich temporal information: {ex.Message}");
            }
        }
    }
}
