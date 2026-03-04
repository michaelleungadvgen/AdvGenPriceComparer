using System.Collections.Generic;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Chat.Models
{
    public class ChatResponse
    {
        public string TextResponse { get; set; } = string.Empty;
        public List<Item> RelatedItems { get; set; } = new();
        public List<PriceRecord> RelatedPrices { get; set; } = new();
        public List<Place> RelatedStores { get; set; } = new();
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public QueryIntent? DetectedIntent { get; set; }
    }
}
