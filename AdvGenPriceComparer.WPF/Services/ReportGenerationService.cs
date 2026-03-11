using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.WPF.Services
{
    /// <summary>
    /// Service for generating and exporting price reports
    /// </summary>
    public interface IReportGenerationService
    {
        /// <summary>
        /// Generate a best deals report
        /// </summary>
        Task<ReportResult> GenerateBestDealsReportAsync(ReportOptions options);

        /// <summary>
        /// Generate a price trends report
        /// </summary>
        Task<ReportResult> GeneratePriceTrendsReportAsync(ReportOptions options);

        /// <summary>
        /// Generate a comprehensive store comparison report
        /// </summary>
        Task<ReportResult> GenerateStoreComparisonReportAsync(ReportOptions options);

        /// <summary>
        /// Generate a category analysis report
        /// </summary>
        Task<ReportResult> GenerateCategoryAnalysisReportAsync(ReportOptions options);

        /// <summary>
        /// Export report to Markdown format
        /// </summary>
        Task<string> ExportToMarkdownAsync(ReportResult report, string outputPath);

        /// <summary>
        /// Export report to JSON format
        /// </summary>
        Task<string> ExportToJsonAsync(ReportResult report, string outputPath);

        /// <summary>
        /// Export report to CSV format
        /// </summary>
        Task<string> ExportToCsvAsync(ReportResult report, string outputPath);

        /// <summary>
        /// Get preview of report content (for display/copy)
        /// </summary>
        Task<string> GetReportPreviewAsync(ReportResult report, ReportFormat format);
    }

    /// <summary>
    /// Report generation options
    /// </summary>
    public class ReportOptions
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> StoreIds { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public int MaxItems { get; set; } = 50;
        public bool IncludePriceHistory { get; set; } = true;
        public bool IncludeStatistics { get; set; } = true;
        public string ReportTitle { get; set; } = "Price Report";
    }

    /// <summary>
    /// Report result containing data and metadata
    /// </summary>
    public class ReportResult
    {
        public string ReportType { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime GeneratedAt { get; set; }
        public DateTime? ReportStartDate { get; set; }
        public DateTime? ReportEndDate { get; set; }
        public List<ReportSection> Sections { get; set; } = new();
        public ReportStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// Individual section within a report
    /// </summary>
    public class ReportSection
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<ReportItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Individual item in a report
    /// </summary>
    public class ReportItem
    {
        public string ItemName { get; set; } = "";
        public string StoreName { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal CurrentPrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? Savings { get; set; }
        public decimal? DiscountPercent { get; set; }
        public DateTime? DateRecorded { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Report statistics
    /// </summary>
    public class ReportStatistics
    {
        public int TotalItems { get; set; }
        public int TotalStores { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal AverageSavings { get; set; }
        public decimal TotalPotentialSavings { get; set; }
    }

    /// <summary>
    /// Report export formats
    /// </summary>
    public enum ReportFormat
    {
        Markdown,
        Json,
        Csv
    }

    /// <summary>
    /// Implementation of report generation service
    /// </summary>
    public class ReportGenerationService : IReportGenerationService
    {
        private readonly IPriceRecordRepository _priceRecordRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IPlaceRepository _placeRepository;

        public ReportGenerationService(
            IPriceRecordRepository priceRecordRepository,
            IItemRepository itemRepository,
            IPlaceRepository placeRepository)
        {
            _priceRecordRepository = priceRecordRepository;
            _itemRepository = itemRepository;
            _placeRepository = placeRepository;
        }

        /// <inheritdoc />
        public async Task<ReportResult> GenerateBestDealsReportAsync(ReportOptions options)
        {
            var endDate = options.EndDate ?? DateTime.Now;
            var startDate = options.StartDate ?? endDate.AddDays(-7);

            var priceRecords = _priceRecordRepository.GetAll()
                .Where(pr => pr.DateRecorded >= startDate && pr.DateRecorded <= endDate)
                .Where(pr => pr.IsOnSale && pr.OriginalPrice.HasValue && pr.OriginalPrice > pr.Price)
                .ToList();

            // Filter by stores if specified
            if (options.StoreIds?.Any() == true)
            {
                priceRecords = priceRecords.Where(pr => options.StoreIds.Contains(pr.PlaceId)).ToList();
            }

            var items = _itemRepository.GetAll().ToDictionary(i => i.Id, i => i);
            var places = _placeRepository.GetAll().ToDictionary(p => p.Id, p => p);

            // Filter by categories if specified
            if (options.Categories?.Any() == true)
            {
                priceRecords = priceRecords
                    .Where(pr => items.TryGetValue(pr.ItemId, out var item) && 
                                 options.Categories.Contains(item.Category))
                    .ToList();
            }

            var deals = priceRecords
                .Select(pr =>
                {
                    items.TryGetValue(pr.ItemId, out var item);
                    places.TryGetValue(pr.PlaceId, out var place);

                    var savings = pr.OriginalPrice!.Value - pr.Price;
                    var discountPercent = pr.OriginalPrice.Value > 0 
                        ? (savings / pr.OriginalPrice.Value * 100) 
                        : 0;

                    return new ReportItem
                    {
                        ItemName = item?.Name ?? "Unknown Item",
                        StoreName = place?.Name ?? "Unknown Store",
                        Category = item?.Category ?? "Uncategorized",
                        CurrentPrice = pr.Price,
                        OriginalPrice = pr.OriginalPrice,
                        Savings = savings,
                        DiscountPercent = discountPercent,
                        DateRecorded = pr.DateRecorded
                    };
                })
                .OrderByDescending(d => d.DiscountPercent)
                .Take(options.MaxItems)
                .ToList();

            var statistics = new ReportStatistics
            {
                TotalItems = deals.Count,
                TotalStores = deals.Select(d => d.StoreName).Distinct().Count(),
                AveragePrice = deals.Any() ? deals.Average(d => d.CurrentPrice) : 0,
                AverageSavings = deals.Any() && deals.Any(d => d.Savings.HasValue) 
                    ? deals.Where(d => d.Savings.HasValue).Average(d => d.Savings!.Value) 
                    : 0,
                TotalPotentialSavings = deals.Where(d => d.Savings.HasValue).Sum(d => d.Savings!.Value)
            };

            return new ReportResult
            {
                ReportType = "Best Deals",
                Title = options.ReportTitle,
                GeneratedAt = DateTime.Now,
                ReportStartDate = startDate,
                ReportEndDate = endDate,
                Sections = new List<ReportSection>
                {
                    new()
                    {
                        Title = "Top Deals",
                        Description = $"Best deals found between {startDate:d} and {endDate:d}",
                        Items = deals
                    }
                },
                Statistics = statistics
            };
        }

        /// <inheritdoc />
        public async Task<ReportResult> GeneratePriceTrendsReportAsync(ReportOptions options)
        {
            var endDate = options.EndDate ?? DateTime.Now;
            var startDate = options.StartDate ?? endDate.AddDays(-30);

            var priceRecords = _priceRecordRepository.GetAll()
                .Where(pr => pr.DateRecorded >= startDate && pr.DateRecorded <= endDate)
                .ToList();

            // Filter by stores if specified
            if (options.StoreIds?.Any() == true)
            {
                priceRecords = priceRecords.Where(pr => options.StoreIds.Contains(pr.PlaceId)).ToList();
            }

            var items = _itemRepository.GetAll().ToDictionary(i => i.Id, i => i);
            var places = _placeRepository.GetAll().ToDictionary(p => p.Id, p => p);

            // Filter by categories if specified
            if (options.Categories?.Any() == true)
            {
                priceRecords = priceRecords
                    .Where(pr => items.TryGetValue(pr.ItemId, out var item) && 
                                 options.Categories.Contains(item.Category))
                    .ToList();
            }

            // Group by item and calculate trends
            var itemGroups = priceRecords
                .GroupBy(pr => pr.ItemId)
                .Where(g => g.Count() >= 2)
                .Take(options.MaxItems)
                .ToList();

            var trendItems = new List<ReportItem>();
            foreach (var group in itemGroups)
            {
                var ordered = group.OrderBy(pr => pr.DateRecorded).ToList();
                var first = ordered.First();
                var last = ordered.Last();
                
                items.TryGetValue(group.Key, out var item);
                places.TryGetValue(last.PlaceId, out var place);

                var priceChange = last.Price - first.Price;
                var changePercent = first.Price > 0 ? (priceChange / first.Price * 100) : 0;
                var avgPrice = ordered.Average(pr => pr.Price);

                trendItems.Add(new ReportItem
                {
                    ItemName = item?.Name ?? "Unknown Item",
                    StoreName = place?.Name ?? "Unknown Store",
                    Category = item?.Category ?? "Uncategorized",
                    CurrentPrice = last.Price,
                    OriginalPrice = first.Price,
                    DateRecorded = last.DateRecorded,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["PriceChange"] = priceChange,
                        ["ChangePercent"] = changePercent,
                        ["AveragePrice"] = avgPrice,
                        ["DataPoints"] = ordered.Count
                    }
                });
            }

            // Sort by absolute price change
            trendItems = trendItems
                .OrderByDescending(i => Math.Abs(i.AdditionalData.GetValueOrDefault("ChangePercent", 0m) as decimal? ?? 0m))
                .ToList();

            var risingItems = trendItems.Where(i => (i.AdditionalData.GetValueOrDefault("ChangePercent", 0m) as decimal? ?? 0m) > 0).ToList();
            var fallingItems = trendItems.Where(i => (i.AdditionalData.GetValueOrDefault("ChangePercent", 0m) as decimal? ?? 0m) < 0).ToList();

            var statistics = new ReportStatistics
            {
                TotalItems = trendItems.Count,
                TotalStores = trendItems.Select(d => d.StoreName).Distinct().Count(),
                AveragePrice = trendItems.Any() ? trendItems.Average(d => d.CurrentPrice) : 0,
                AverageSavings = fallingItems.Any() && fallingItems.Any(d => d.OriginalPrice.HasValue)
                    ? fallingItems.Where(d => d.OriginalPrice.HasValue).Average(d => d.OriginalPrice!.Value - d.CurrentPrice)
                    : 0,
                TotalPotentialSavings = fallingItems.Where(d => d.OriginalPrice.HasValue).Sum(d => d.OriginalPrice!.Value - d.CurrentPrice)
            };

            return new ReportResult
            {
                ReportType = "Price Trends",
                Title = options.ReportTitle,
                GeneratedAt = DateTime.Now,
                ReportStartDate = startDate,
                ReportEndDate = endDate,
                Sections = new List<ReportSection>
                {
                    new()
                    {
                        Title = "Rising Prices",
                        Description = $"Items with price increases from {startDate:d} to {endDate:d}",
                        Items = risingItems.Take(options.MaxItems / 2).ToList()
                    },
                    new()
                    {
                        Title = "Falling Prices",
                        Description = $"Items with price decreases from {startDate:d} to {endDate:d}",
                        Items = fallingItems.Take(options.MaxItems / 2).ToList()
                    }
                },
                Statistics = statistics
            };
        }

        /// <inheritdoc />
        public async Task<ReportResult> GenerateStoreComparisonReportAsync(ReportOptions options)
        {
            var endDate = options.EndDate ?? DateTime.Now;
            var startDate = options.StartDate ?? endDate.AddDays(-7);

            var priceRecords = _priceRecordRepository.GetAll()
                .Where(pr => pr.DateRecorded >= startDate && pr.DateRecorded <= endDate)
                .ToList();

            var items = _itemRepository.GetAll().ToDictionary(i => i.Id, i => i);
            var places = _placeRepository.GetAll().ToDictionary(p => p.Id, p => p);

            // Group by item and compare across stores
            var itemGroups = priceRecords
                .GroupBy(pr => pr.ItemId)
                .Where(g => g.Select(pr => pr.PlaceId).Distinct().Count() >= 2)
                .Take(options.MaxItems)
                .ToList();

            var comparisonItems = new List<ReportItem>();
            foreach (var group in itemGroups)
            {
                var latestByStore = group
                    .GroupBy(pr => pr.PlaceId)
                    .Select(g => g.OrderByDescending(pr => pr.DateRecorded).First())
                    .ToList();

                if (latestByStore.Count < 2) continue;

                var minPrice = latestByStore.Min(pr => pr.Price);
                var maxPrice = latestByStore.Max(pr => pr.Price);
                var cheapestStore = latestByStore.First(pr => pr.Price == minPrice);
                var mostExpensiveStore = latestByStore.First(pr => pr.Price == maxPrice);

                items.TryGetValue(group.Key, out var item);
                places.TryGetValue(cheapestStore.PlaceId, out var cheapPlace);
                places.TryGetValue(mostExpensiveStore.PlaceId, out var expensivePlace);

                var savings = maxPrice - minPrice;
                var savingsPercent = maxPrice > 0 ? (savings / maxPrice * 100) : 0;

                comparisonItems.Add(new ReportItem
                {
                    ItemName = item?.Name ?? "Unknown Item",
                    StoreName = cheapPlace?.Name ?? "Unknown Store",
                    Category = item?.Category ?? "Uncategorized",
                    CurrentPrice = minPrice,
                    OriginalPrice = maxPrice,
                    Savings = savings,
                    DiscountPercent = savingsPercent,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["CheapestStore"] = cheapPlace?.Name ?? "Unknown",
                        ["MostExpensiveStore"] = expensivePlace?.Name ?? "Unknown",
                        ["StoreCount"] = latestByStore.Count
                    }
                });
            }

            // Sort by potential savings
            comparisonItems = comparisonItems
                .OrderByDescending(i => i.Savings ?? 0)
                .ToList();

            var statistics = new ReportStatistics
            {
                TotalItems = comparisonItems.Count,
                TotalStores = places.Count,
                AveragePrice = comparisonItems.Any() ? comparisonItems.Average(d => d.CurrentPrice) : 0,
                AverageSavings = comparisonItems.Any() && comparisonItems.Any(d => d.Savings.HasValue)
                    ? comparisonItems.Where(d => d.Savings.HasValue).Average(d => d.Savings!.Value)
                    : 0,
                TotalPotentialSavings = comparisonItems.Where(d => d.Savings.HasValue).Sum(d => d.Savings!.Value)
            };

            return new ReportResult
            {
                ReportType = "Store Comparison",
                Title = options.ReportTitle,
                GeneratedAt = DateTime.Now,
                ReportStartDate = startDate,
                ReportEndDate = endDate,
                Sections = new List<ReportSection>
                {
                    new()
                    {
                        Title = "Best Store for Each Item",
                        Description = $"Store comparison for items available at multiple stores ({startDate:d} to {endDate:d})",
                        Items = comparisonItems
                    }
                },
                Statistics = statistics
            };
        }

        /// <inheritdoc />
        public async Task<ReportResult> GenerateCategoryAnalysisReportAsync(ReportOptions options)
        {
            var items = _itemRepository.GetAll().ToList();
            var priceRecords = _priceRecordRepository.GetAll()
                .Where(pr => pr.DateRecorded >= (options.StartDate ?? DateTime.Now.AddDays(-30)))
                .ToList();

            var places = _placeRepository.GetAll().ToDictionary(p => p.Id, p => p);

            // Filter by categories if specified
            if (options.Categories?.Any() == true)
            {
                items = items.Where(i => options.Categories.Contains(i.Category)).ToList();
            }

            var itemIds = items.Select(i => i.Id).ToHashSet();
            priceRecords = priceRecords.Where(pr => itemIds.Contains(pr.ItemId)).ToList();

            // Group by category
            var categoryGroups = items
                .Where(i => !string.IsNullOrEmpty(i.Category))
                .GroupBy(i => i.Category)
                .ToList();

            var categoryItems = new List<ReportItem>();
            foreach (var catGroup in categoryGroups)
            {
                var catItemIds = catGroup.Select(i => i.Id).ToHashSet();
                var catPrices = priceRecords.Where(pr => catItemIds.Contains(pr.ItemId)).ToList();

                if (!catPrices.Any()) continue;

                var avgPrice = catPrices.Average(pr => pr.Price);
                var minPrice = catPrices.Min(pr => pr.Price);
                var maxPrice = catPrices.Max(pr => pr.Price);
                var onSaleCount = catPrices.Count(pr => pr.IsOnSale);
                var salePercent = catPrices.Any() ? (decimal)onSaleCount / catPrices.Count * 100 : 0;

                categoryItems.Add(new ReportItem
                {
                    ItemName = catGroup.Key,
                    Category = catGroup.Key,
                    CurrentPrice = avgPrice,
                    OriginalPrice = maxPrice,
                    Savings = maxPrice - minPrice,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["ItemCount"] = catGroup.Count(),
                        ["PriceRecordCount"] = catPrices.Count,
                        ["MinPrice"] = minPrice,
                        ["OnSalePercent"] = salePercent
                    }
                });
            }

            // Sort by item count
            categoryItems = categoryItems
                .OrderByDescending(i => i.AdditionalData.GetValueOrDefault("ItemCount", 0) as int? ?? 0)
                .Take(options.MaxItems)
                .ToList();

            var statistics = new ReportStatistics
            {
                TotalItems = items.Count,
                TotalStores = places.Count,
                AveragePrice = categoryItems.Any() ? categoryItems.Average(d => d.CurrentPrice) : 0,
                AverageSavings = categoryItems.Any() && categoryItems.Any(d => d.Savings.HasValue)
                    ? categoryItems.Where(d => d.Savings.HasValue).Average(d => d.Savings!.Value)
                    : 0,
                TotalPotentialSavings = 0
            };

            return new ReportResult
            {
                ReportType = "Category Analysis",
                Title = options.ReportTitle,
                GeneratedAt = DateTime.Now,
                ReportStartDate = options.StartDate,
                ReportEndDate = options.EndDate,
                Sections = new List<ReportSection>
                {
                    new()
                    {
                        Title = "Category Summary",
                        Description = "Analysis of items and prices by category",
                        Items = categoryItems
                    }
                },
                Statistics = statistics
            };
        }

        /// <inheritdoc />
        public async Task<string> ExportToMarkdownAsync(ReportResult report, string outputPath)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# {report.Title}");
            sb.AppendLine();
            sb.AppendLine($"**Report Type:** {report.ReportType}");
            sb.AppendLine($"**Generated:** {report.GeneratedAt:yyyy-MM-dd HH:mm}");
            if (report.ReportStartDate.HasValue && report.ReportEndDate.HasValue)
            {
                sb.AppendLine($"**Period:** {report.ReportStartDate.Value:d} to {report.ReportEndDate.Value:d}");
            }
            sb.AppendLine();

            // Statistics
            if (report.Statistics.TotalItems > 0)
            {
                sb.AppendLine("## Summary Statistics");
                sb.AppendLine();
                sb.AppendLine($"- **Total Items:** {report.Statistics.TotalItems}");
                sb.AppendLine($"- **Stores:** {report.Statistics.TotalStores}");
                sb.AppendLine($"- **Average Price:** ${report.Statistics.AveragePrice:F2}");
                if (report.Statistics.AverageSavings > 0)
                {
                    sb.AppendLine($"- **Average Savings:** ${report.Statistics.AverageSavings:F2}");
                }
                if (report.Statistics.TotalPotentialSavings > 0)
                {
                    sb.AppendLine($"- **Total Potential Savings:** ${report.Statistics.TotalPotentialSavings:F2}");
                }
                sb.AppendLine();
            }

            // Sections
            foreach (var section in report.Sections)
            {
                sb.AppendLine($"## {section.Title}");
                sb.AppendLine();
                sb.AppendLine(section.Description);
                sb.AppendLine();

                if (section.Items.Any())
                {
                    sb.AppendLine("| Item | Store | Category | Price | Savings |");
                    sb.AppendLine("|------|-------|----------|-------|---------|");

                    foreach (var item in section.Items)
                    {
                        var savingsText = item.Savings.HasValue && item.Savings.Value > 0
                            ? $"${item.Savings.Value:F2} ({item.DiscountPercent:F0}%)"
                            : "-";

                        sb.AppendLine($"| {item.ItemName} | {item.StoreName} | {item.Category} | ${item.CurrentPrice:F2} | {savingsText} |");
                    }
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("*No items found for this section.*");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("---");
            sb.AppendLine($"*Generated by AdvGen Price Comparer*");

            var content = sb.ToString();
            await File.WriteAllTextAsync(outputPath, content);
            return outputPath;
        }

        /// <inheritdoc />
        public async Task<string> ExportToJsonAsync(ReportResult report, string outputPath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = JsonSerializer.Serialize(report, options);
            await File.WriteAllTextAsync(outputPath, content);
            return outputPath;
        }

        /// <inheritdoc />
        public async Task<string> ExportToCsvAsync(ReportResult report, string outputPath)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("Item Name,Store,Category,Current Price,Original Price,Savings,Discount %,Date");

            // Data
            foreach (var section in report.Sections)
            {
                foreach (var item in section.Items)
                {
                    var originalPrice = item.OriginalPrice.HasValue ? item.OriginalPrice.Value.ToString("F2") : "";
                    var savings = item.Savings.HasValue ? item.Savings.Value.ToString("F2") : "";
                    var discount = item.DiscountPercent.HasValue ? item.DiscountPercent.Value.ToString("F2") : "";
                    var date = item.DateRecorded.HasValue ? item.DateRecorded.Value.ToString("yyyy-MM-dd") : "";

                    sb.AppendLine($"\"{item.ItemName}\",\"{item.StoreName}\",\"{item.Category}\",{item.CurrentPrice:F2},{originalPrice},{savings},{discount},{date}");
                }
            }

            var content = sb.ToString();
            await File.WriteAllTextAsync(outputPath, content);
            return outputPath;
        }

        /// <inheritdoc />
        public async Task<string> GetReportPreviewAsync(ReportResult report, ReportFormat format)
        {
            return format switch
            {
                ReportFormat.Markdown => await GenerateMarkdownPreview(report),
                ReportFormat.Json => await GenerateJsonPreview(report),
                ReportFormat.Csv => await GenerateCsvPreview(report),
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
        }

        private Task<string> GenerateMarkdownPreview(ReportResult report)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# {report.Title}");
            sb.AppendLine();
            sb.AppendLine($"**Report Type:** {report.ReportType}");
            sb.AppendLine($"**Generated:** {report.GeneratedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            foreach (var section in report.Sections)
            {
                sb.AppendLine($"## {section.Title}");
                sb.AppendLine();

                foreach (var item in section.Items.Take(10))
                {
                    var savingsText = item.Savings.HasValue && item.Savings.Value > 0
                        ? $" (Save ${item.Savings.Value:F2})"
                        : "";
                    sb.AppendLine($"- **{item.ItemName}** at {item.StoreName}: ${item.CurrentPrice:F2}{savingsText}");
                }

                if (section.Items.Count > 10)
                {
                    sb.AppendLine($"- ... and {section.Items.Count - 10} more items");
                }

                sb.AppendLine();
            }

            return Task.FromResult(sb.ToString());
        }

        private Task<string> GenerateJsonPreview(ReportResult report)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Create a preview with limited items
            var preview = new
            {
                report.ReportType,
                report.Title,
                report.GeneratedAt,
                Sections = report.Sections.Select(s => new
                {
                    s.Title,
                    Items = s.Items.Take(10).ToList()
                }).ToList(),
                report.Statistics
            };

            return Task.FromResult(JsonSerializer.Serialize(preview, options));
        }

        private Task<string> GenerateCsvPreview(ReportResult report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Item Name,Store,Category,Current Price,Savings");

            var allItems = report.Sections.SelectMany(s => s.Items).Take(20);
            foreach (var item in allItems)
            {
                var savings = item.Savings.HasValue ? item.Savings.Value.ToString("F2") : "";
                sb.AppendLine($"\"{item.ItemName}\",\"{item.StoreName}\",\"{item.Category}\",{item.CurrentPrice:F2},{savings}");
            }

            return Task.FromResult(sb.ToString());
        }
    }
}
