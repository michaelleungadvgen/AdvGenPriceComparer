using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvGenPriceComparer.Core.Interfaces;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for generating and managing weekly specials digests
/// </summary>
public class WeeklySpecialsService : IWeeklySpecialsService
{
    private readonly IGroceryDataService _dataService;
    private readonly ILoggerService _logger;

    public WeeklySpecialsService(IGroceryDataService dataService, ILoggerService logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public WeeklyDigestReport GenerateWeeklyDigest()
    {
        // Calculate current week (Thursday to Wednesday cycle typical for supermarkets)
        var today = DateTime.Today;
        var daysSinceThursday = (today.DayOfWeek - DayOfWeek.Thursday + 7) % 7;
        var weekStart = today.AddDays(-daysSinceThursday);
        var weekEnd = weekStart.AddDays(6);

        return GenerateDigestForDateRange(weekStart, weekEnd);
    }

    /// <inheritdoc />
    public WeeklyDigestReport GenerateDigestForDateRange(DateTime startDate, DateTime endDate)
    {
        _logger.LogInfo($"Generating weekly digest from {startDate:d} to {endDate:d}");

        var report = new WeeklyDigestReport
        {
            ReportDate = DateTime.Today,
            WeekStart = startDate,
            WeekEnd = endDate
        };

        var allItems = _dataService.GetAllItems().ToList();
        var allPlaces = _dataService.GetAllPlaces().ToList();
        var allDeals = new List<WeeklySpecialItem>();

        foreach (var item in allItems)
        {
            var priceHistory = _dataService.GetPriceHistory(item.Id!);
            var latestPrice = priceHistory
                .Where(p => p.DateRecorded.Date >= startDate.Date && p.DateRecorded.Date <= endDate.Date)
                .OrderByDescending(p => p.DateRecorded)
                .FirstOrDefault();

            if (latestPrice == null) continue;
            if (!latestPrice.OriginalPrice.HasValue || latestPrice.OriginalPrice <= latestPrice.Price) continue;

            var store = allPlaces.FirstOrDefault(p => p.Id == latestPrice.PlaceId);

            var specialItem = new WeeklySpecialItem
            {
                ItemId = item.Id!,
                ItemName = item.Name,
                Brand = item.Brand ?? "",
                Category = item.Category ?? "Uncategorized",
                StoreName = store?.Name ?? "Unknown Store",
                Price = latestPrice.Price,
                OriginalPrice = latestPrice.OriginalPrice,
                ValidFrom = latestPrice.DateRecorded,
                ValidTo = latestPrice.ValidTo ?? latestPrice.DateRecorded.AddDays(7)
            };

            allDeals.Add(specialItem);
        }

        report.AllDeals = allDeals.OrderByDescending(d => d.SavingsPercentage).ToList();
        report.TotalDeals = allDeals.Count;
        report.HalfPriceDeals = allDeals.Count(d => d.IsHalfPrice);

        // Organize by category
        report.ByCategory = allDeals
            .GroupBy(d => d.Category)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(d => d.SavingsPercentage).ToList()
            );

        // Organize by store
        report.ByStore = allDeals
            .GroupBy(d => d.StoreName)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(d => d.SavingsPercentage).ToList()
            );

        _logger.LogInfo($"Weekly digest generated: {report.TotalDeals} deals, {report.HalfPriceDeals} half-price");

        return report;
    }

    /// <inheritdoc />
    public List<WeeklySpecialItem> GetDealsByCategory(string category)
    {
        var digest = GenerateWeeklyDigest();
        return digest.ByCategory.TryGetValue(category, out var deals) ? deals : new List<WeeklySpecialItem>();
    }

    /// <inheritdoc />
    public List<WeeklySpecialItem> GetDealsByStore(string storeName)
    {
        var digest = GenerateWeeklyDigest();
        return digest.ByStore.TryGetValue(storeName, out var deals) ? deals : new List<WeeklySpecialItem>();
    }

    /// <inheritdoc />
    public string ExportToMarkdown(WeeklyDigestReport report)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"# 🛒 Weekly Specials Digest");
        sb.AppendLine($"**Week:** {report.WeekStart:dddd, dd MMMM} - {report.WeekEnd:dddd, dd MMMM yyyy}");
        sb.AppendLine($"**Generated:** {report.ReportDate:dd MMMM yyyy}");
        sb.AppendLine();
        
        sb.AppendLine($"## 📊 Summary");
        sb.AppendLine($"- **Total Deals:** {report.TotalDeals}");
        sb.AppendLine($"- **Half-Price Deals:** {report.HalfPriceDeals} 🔥");
        sb.AppendLine();

        // Best deals section
        if (report.BestDeals.Any())
        {
            sb.AppendLine($"## 🔥 Best Deals");
            sb.AppendLine();
            foreach (var deal in report.BestDeals.Take(5))
            {
                var savingsBadge = deal.IsHalfPrice ? " **[HALF PRICE]**" : "";
                sb.AppendLine($"- **{deal.ItemName}**{savingsBadge}");
                sb.AppendLine($"  - Store: {deal.StoreName}");
                sb.AppendLine($"  - Price: ${deal.Price:F2} (was ${deal.OriginalPrice:F2})");
                sb.AppendLine($"  - Savings: {deal.SavingsPercentage:F0}%");
                sb.AppendLine();
            }
        }

        // By Category
        if (report.ByCategory.Any())
        {
            sb.AppendLine($"## 📁 By Category");
            sb.AppendLine();
            
            foreach (var categoryGroup in report.ByCategory)
            {
                sb.AppendLine($"### {categoryGroup.Key}");
                sb.AppendLine();
                
                foreach (var deal in categoryGroup.Value.Take(10))
                {
                    var halfPriceEmoji = deal.IsHalfPrice ? "🔥 " : "";
                    sb.AppendLine($"- {halfPriceEmoji}**{deal.ItemName}** - ${deal.Price:F2} @ {deal.StoreName} ({deal.SavingsPercentage:F0}% off)");
                }
                
                if (categoryGroup.Value.Count > 10)
                {
                    sb.AppendLine($"- *... and {categoryGroup.Value.Count - 10} more*");
                }
                
                sb.AppendLine();
            }
        }

        // By Store
        if (report.ByStore.Any())
        {
            sb.AppendLine($"## 🏪 By Store");
            sb.AppendLine();
            
            foreach (var storeGroup in report.ByStore)
            {
                sb.AppendLine($"### {storeGroup.Key}");
                sb.AppendLine();
                
                foreach (var deal in storeGroup.Value.Take(10))
                {
                    var halfPriceEmoji = deal.IsHalfPrice ? "🔥 " : "";
                    sb.AppendLine($"- {halfPriceEmoji}**{deal.ItemName}** - ${deal.Price:F2} ({deal.SavingsPercentage:F0}% off)");
                }
                
                if (storeGroup.Value.Count > 10)
                {
                    sb.AppendLine($"- *... and {storeGroup.Value.Count - 10} more*");
                }
                
                sb.AppendLine();
            }
        }

        sb.AppendLine($"---");
        sb.AppendLine($"*Generated by AdvGen Price Comparer*");

        return sb.ToString();
    }

    /// <inheritdoc />
    public string ExportToPlainText(WeeklyDigestReport report)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"WEEKLY SPECIALS DIGEST");
        sb.AppendLine($"Week: {report.WeekStart:dd MMM} - {report.WeekEnd:dd MMM yyyy}");
        sb.AppendLine($"Generated: {report.ReportDate:dd MMM yyyy}");
        sb.AppendLine();
        
        sb.AppendLine($"SUMMARY");
        sb.AppendLine($"Total Deals: {report.TotalDeals}");
        sb.AppendLine($"Half-Price: {report.HalfPriceDeals}");
        sb.AppendLine();

        if (report.BestDeals.Any())
        {
            sb.AppendLine($"BEST DEALS");
            sb.AppendLine();
            foreach (var deal in report.BestDeals.Take(10))
            {
                var halfPrice = deal.IsHalfPrice ? " [HALF PRICE]" : "";
                sb.AppendLine($"{deal.ItemName}{halfPrice}");
                sb.AppendLine($"  {deal.StoreName} - ${deal.Price:F2} (save {deal.SavingsPercentage:F0}%)");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public List<string> GetAvailableCategories()
    {
        var digest = GenerateWeeklyDigest();
        return digest.ByCategory.Keys.OrderBy(c => c).ToList();
    }

    /// <inheritdoc />
    public List<string> GetAvailableStores()
    {
        var digest = GenerateWeeklyDigest();
        return digest.ByStore.Keys.OrderBy(s => s).ToList();
    }
}
