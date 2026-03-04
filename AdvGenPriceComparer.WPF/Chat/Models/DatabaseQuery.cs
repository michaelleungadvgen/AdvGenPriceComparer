using System.Collections.Generic;

namespace AdvGenPriceComparer.WPF.Chat.Models
{
    public enum DatabaseTarget
    {
        LiteDB,
        AdvGenNoSqlServer,
        Both
    }

    public class DatabaseQuery
    {
        public DatabaseTarget Target { get; set; }
        public string Query { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public QueryIntent Intent { get; set; } = new();
    }
}
