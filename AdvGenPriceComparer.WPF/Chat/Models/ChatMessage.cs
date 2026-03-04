using System;
using System.Collections.Generic;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Chat.Models
{
    public enum MessageRole
    {
        User,
        Assistant,
        System
    }

    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public MessageRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<Item> AttachedItems { get; set; } = new();
        public List<PriceRecord> AttachedPrices { get; set; } = new();
        public bool IsError { get; set; }
        public bool IsThinking { get; set; }
    }
}
