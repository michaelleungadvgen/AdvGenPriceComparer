using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Represents a deal that is about to expire
/// </summary>
public class ExpiringDeal
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime DateRecorded { get; set; }
    public int DaysUntilExpiry => (ExpiryDate - DateTime.Today).Days;
    public bool IsExpired => DaysUntilExpiry < 0;
    public string? Savings { get; set; }
}

/// <summary>
/// Service for tracking and managing deal expiration reminders
/// </summary>
public interface IDealExpirationService
{
    /// <summary>
    /// Gets all deals that are expiring within the specified number of days
    /// </summary>
    /// <param name="daysThreshold">Number of days to look ahead (default: 7)</param>
    /// <returns>List of expiring deals</returns>
    List<ExpiringDeal> GetExpiringDeals(int daysThreshold = 7);

    /// <summary>
    /// Gets deals that have already expired
    /// </summary>
    /// <returns>List of expired deals</returns>
    List<ExpiringDeal> GetExpiredDeals();

    /// <summary>
    /// Checks if there are any deals expiring soon
    /// </summary>
    /// <param name="daysThreshold">Number of days to look ahead</param>
    /// <returns>True if there are expiring deals</returns>
    bool HasExpiringDeals(int daysThreshold = 3);

    /// <summary>
    /// Gets the count of deals expiring within the specified days
    /// </summary>
    int GetExpiringDealsCount(int daysThreshold = 7);

    /// <summary>
    /// Marks a deal as dismissed (won't show in reminders again)
    /// </summary>
    void DismissDeal(string itemId, DateTime expiryDate);

    /// <summary>
    /// Gets all dismissed deal keys
    /// </summary>
    HashSet<string> GetDismissedDeals();

    /// <summary>
    /// Clears all dismissed deal records
    /// </summary>
    void ClearDismissedDeals();
}
