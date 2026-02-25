using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AdvGenPriceComparer.Core.Models;

public class Item
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Brand { get; set; }
    
    public string? Category { get; set; }
    
    public string? SubCategory { get; set; }
    
    public string? Barcode { get; set; }
    
    public string? PackageSize { get; set; }
    
    public string? Unit { get; set; }
    
    public decimal? Weight { get; set; }
    
    public decimal? Volume { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public Dictionary<string, decimal> NutritionalInfo { get; set; } = new();
    
    public List<string> Allergens { get; set; } = new();
    
    public List<string> DietaryFlags { get; set; } = new(); // "Vegan", "Gluten-Free", "Organic", etc.
    
    public List<string> Tags { get; set; } = new();
    
    public bool IsActive { get; set; } = true;
    
    public bool IsFavorite { get; set; } = false;
    
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public Dictionary<string, string> ExtraInformation { get; set; } = new();

    // Computed Properties (not stored in database)
    [JsonIgnore]
    public string DisplayName => !string.IsNullOrEmpty(Brand) ? $"{Brand} {Name}" : Name;
    
    [JsonIgnore]
    public string FullDescription => !string.IsNullOrEmpty(Description) ? 
        $"{DisplayName} - {Description}" : DisplayName;
    
    [JsonIgnore]
    public string CategoryPath => !string.IsNullOrEmpty(SubCategory) ? 
        $"{Category} > {SubCategory}" : Category ?? "Uncategorized";
    
    [JsonIgnore]
    public bool HasBarcode => !string.IsNullOrEmpty(Barcode);
    
    [JsonIgnore]
    public bool HasNutritionalInfo => NutritionalInfo.Count > 0;
    
    [JsonIgnore]
    public bool HasAllergens => Allergens.Count > 0;
    
    [JsonIgnore]
    public bool HasDietaryFlags => DietaryFlags.Count > 0;
    
    [JsonIgnore]
    public bool IsRecent => DateTime.UtcNow - DateAdded <= TimeSpan.FromDays(7);
    
    [JsonIgnore]
    public bool WasRecentlyUpdated => DateTime.UtcNow - LastUpdated <= TimeSpan.FromDays(1);

    // Business Logic Methods
    
    /// <summary>
    /// Validates if the item has all required fields for grocery tracking
    /// </summary>
    public ValidationResult ValidateItem()
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError("Name is required");
            
        if (Name?.Length > 200)
            result.AddError("Name cannot exceed 200 characters");
            
        if (!string.IsNullOrEmpty(Barcode) && !IsValidBarcode(Barcode))
            result.AddError("Invalid barcode format");
            
        if (!string.IsNullOrEmpty(Category) && Category.Length > 50)
            result.AddError("Category cannot exceed 50 characters");
            
        if (!string.IsNullOrEmpty(Brand) && Brand.Length > 100)
            result.AddError("Brand cannot exceed 100 characters");
            
        return result;
    }
    
    /// <summary>
    /// Extracts numeric weight/volume from package size string (e.g., "500g", "2L", "12 pack")
    /// </summary>
    public (decimal? numericValue, string? unit) ParsePackageSize()
    {
        if (string.IsNullOrEmpty(PackageSize))
            return (null, null);
            
        // Regex to match patterns like: 500g, 2.5L, 12 pack, 1.2kg, etc.
        var regex = new Regex(@"(\d+\.?\d*)\s*([a-zA-Z]+)", RegexOptions.IgnoreCase);
        var match = regex.Match(PackageSize);
        
        if (match.Success)
        {
            if (decimal.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal value))
            {
                return (value, match.Groups[2].Value.ToLowerInvariant());
            }
        }
        
        return (null, null);
    }
    
    /// <summary>
    /// Calculates price per unit for comparison (e.g., price per 100g, per L)
    /// </summary>
    public decimal? CalculatePricePerUnit(decimal price, string targetUnit = "100g")
    {
        var (packageValue, packageUnit) = ParsePackageSize();
        
        if (!packageValue.HasValue || string.IsNullOrEmpty(packageUnit))
            return null;
            
        // Convert to common units for comparison
        var normalizedWeight = NormalizeWeight(packageValue.Value, packageUnit);
        if (!normalizedWeight.HasValue)
            return null;
            
        var targetWeight = targetUnit.ToLowerInvariant() switch
        {
            "100g" => 100m,
            "1kg" => 1000m,
            "1l" => 1000m, // ml
            _ => 100m // default to 100g
        };
        
        return price * targetWeight / normalizedWeight.Value;
    }
    
    /// <summary>
    /// Checks if this item matches search criteria
    /// </summary>
    public bool MatchesSearch(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return true;
            
        var term = searchTerm.ToLowerInvariant();
        
        return Name.ToLowerInvariant().Contains(term) ||
               (Brand?.ToLowerInvariant().Contains(term) ?? false) ||
               (Category?.ToLowerInvariant().Contains(term) ?? false) ||
               (SubCategory?.ToLowerInvariant().Contains(term) ?? false) ||
               (Description?.ToLowerInvariant().Contains(term) ?? false) ||
               Tags.Any(tag => tag.ToLowerInvariant().Contains(term)) ||
               (Barcode?.Contains(searchTerm) ?? false);
    }
    
    /// <summary>
    /// Checks if item matches dietary requirements
    /// </summary>
    public bool MatchesDietaryRequirements(List<string> requirements)
    {
        if (requirements.Count == 0)
            return true;
            
        return requirements.All(req => 
            DietaryFlags.Any(flag => flag.Equals(req, StringComparison.OrdinalIgnoreCase)));
    }
    
    /// <summary>
    /// Checks if item contains any specified allergens
    /// </summary>
    public bool ContainsAllergens(List<string> avoidAllergens)
    {
        if (avoidAllergens.Count == 0)
            return false;
            
        return Allergens.Any(allergen =>
            avoidAllergens.Any(avoid => allergen.Equals(avoid, StringComparison.OrdinalIgnoreCase)));
    }
    
    /// <summary>
    /// Gets similarity score with another item (for duplicate detection)
    /// </summary>
    public double CalculateSimilarity(Item other)
    {
        if (other == null) return 0;
        
        double score = 0;
        int factors = 0;
        
        // Name similarity (most important)
        score += CalculateStringSimilarity(Name, other.Name) * 0.4;
        factors++;
        
        // Brand similarity
        if (!string.IsNullOrEmpty(Brand) && !string.IsNullOrEmpty(other.Brand))
        {
            score += CalculateStringSimilarity(Brand, other.Brand) * 0.3;
            factors++;
        }
        
        // Package size similarity
        if (!string.IsNullOrEmpty(PackageSize) && !string.IsNullOrEmpty(other.PackageSize))
        {
            score += CalculateStringSimilarity(PackageSize, other.PackageSize) * 0.2;
            factors++;
        }
        
        // Barcode exact match (definitive)
        if (!string.IsNullOrEmpty(Barcode) && !string.IsNullOrEmpty(other.Barcode))
        {
            if (Barcode.Equals(other.Barcode, StringComparison.OrdinalIgnoreCase))
                return 1.0; // Perfect match
            factors++;
        }
        
        // Category similarity
        if (!string.IsNullOrEmpty(Category) && !string.IsNullOrEmpty(other.Category))
        {
            score += CalculateStringSimilarity(Category, other.Category) * 0.1;
            factors++;
        }
        
        return factors > 0 ? score / factors : 0;
    }
    
    /// <summary>
    /// Updates the last modified timestamp
    /// </summary>
    public void MarkAsUpdated()
    {
        LastUpdated = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Adds or updates nutritional information
    /// </summary>
    public void SetNutritionalInfo(string nutrient, decimal value)
    {
        NutritionalInfo[nutrient.ToLowerInvariant()] = value;
        MarkAsUpdated();
    }
    
    /// <summary>
    /// Adds dietary flag if not already present
    /// </summary>
    public void AddDietaryFlag(string flag)
    {
        if (!DietaryFlags.Any(f => f.Equals(flag, StringComparison.OrdinalIgnoreCase)))
        {
            DietaryFlags.Add(flag);
            MarkAsUpdated();
        }
    }
    
    /// <summary>
    /// Adds allergen if not already present
    /// </summary>
    public void AddAllergen(string allergen)
    {
        if (!Allergens.Any(a => a.Equals(allergen, StringComparison.OrdinalIgnoreCase)))
        {
            Allergens.Add(allergen);
            MarkAsUpdated();
        }
    }
    
    /// <summary>
    /// Adds tag if not already present
    /// </summary>
    public void AddTag(string tag)
    {
        if (!Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
        {
            Tags.Add(tag);
            MarkAsUpdated();
        }
    }

    // Private Helper Methods
    
    private static bool IsValidBarcode(string barcode)
    {
        // Basic validation for common barcode formats
        if (string.IsNullOrWhiteSpace(barcode))
            return false;
            
        // Remove any spaces or dashes
        var cleanBarcode = barcode.Replace(" ", "").Replace("-", "");
        
        // Check common barcode lengths (UPC-A: 12, EAN-13: 13, EAN-8: 8)
        return cleanBarcode.Length >= 8 && cleanBarcode.Length <= 14 && 
               cleanBarcode.All(char.IsDigit);
    }
    
    private static decimal? NormalizeWeight(decimal value, string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "g" or "gram" or "grams" => value,
            "kg" or "kilogram" or "kilograms" => value * 1000,
            "oz" or "ounce" or "ounces" => value * 28.35m, // Convert to grams
            "lb" or "pound" or "pounds" => value * 453.59m, // Convert to grams
            "ml" or "milliliter" or "milliliters" => value,
            "l" or "liter" or "liters" or "litre" or "litres" => value * 1000, // Convert to ml
            "fl oz" => value * 29.57m, // Convert to ml
            _ => null
        };
    }
    
    private static double CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0;
            
        if (str1.Equals(str2, StringComparison.OrdinalIgnoreCase))
            return 1.0;
            
        // Simple Levenshtein distance-based similarity
        var distance = CalculateLevenshteinDistance(str1.ToLowerInvariant(), str2.ToLowerInvariant());
        var maxLength = Math.Max(str1.Length, str2.Length);
        
        return 1.0 - (double)distance / maxLength;
    }
    
    private static int CalculateLevenshteinDistance(string str1, string str2)
    {
        int[,] matrix = new int[str1.Length + 1, str2.Length + 1];
        
        for (int i = 0; i <= str1.Length; i++)
            matrix[i, 0] = i;
            
        for (int j = 0; j <= str2.Length; j++)
            matrix[0, j] = j;
            
        for (int i = 1; i <= str1.Length; i++)
        {
            for (int j = 1; j <= str2.Length; j++)
            {
                int cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }
        
        return matrix[str1.Length, str2.Length];
    }
    
    public override string ToString()
    {
        return DisplayName;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is Item other)
        {
            // Use barcode for equality if available
            if (HasBarcode && other.HasBarcode)
                return Barcode!.Equals(other.Barcode, StringComparison.OrdinalIgnoreCase);
                
            // Otherwise use ID
            return Id.Equals(other.Id);
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return HasBarcode ? Barcode!.GetHashCode() : Id.GetHashCode();
    }
}

/// <summary>
/// Validation result for item validation
/// </summary>
public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = new();
    
    public void AddError(string error)
    {
        Errors.Add(error);
    }
    
    public string GetErrorsString()
    {
        return string.Join(", ", Errors);
    }
}

