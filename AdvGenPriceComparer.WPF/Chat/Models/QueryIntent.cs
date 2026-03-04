using System;

namespace AdvGenPriceComparer.WPF.Chat.Models
{
    public enum QueryType
    {
        PriceQuery,           // "What's the price of milk?"
        PriceComparison,      // "Compare milk prices between Coles and Woolworths"
        CheapestItem,         // "Find the cheapest bread"
        ItemsInCategory,      // "Show me all dairy products"
        ItemsOnSale,          // "What's on sale this week?"
        PriceHistory,         // "Show me milk price history"
        BestDeal,             // "What are the best deals?"
        StoreInventory,       // "What products are available at Coles?"
        BudgetQuery,          // "What can I buy for $50?"
        GeneralChat,          // "Hello", "How are you?"
        Unknown
    }

    public enum ComparisonType
    {
        Cheaper,
        MoreExpensive,
        SimilarPrice
    }

    public class QueryIntent
    {
        public QueryType Type { get; set; }
        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public string? Store { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public bool OnSaleOnly { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public ComparisonType? Comparison { get; set; }
        public int? Limit { get; set; } = 10;
        public string? OriginalQuery { get; set; }
    }
}
