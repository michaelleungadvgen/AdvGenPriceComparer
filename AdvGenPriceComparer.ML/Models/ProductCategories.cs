namespace AdvGenPriceComparer.ML.Models;

/// <summary>
/// Standard product categories for grocery items
/// </summary>
public static class ProductCategories
{
    public const string Meat = "Meat & Seafood";
    public const string Dairy = "Dairy & Eggs";
    public const string FruitsVegetables = "Fruits & Vegetables";
    public const string Bakery = "Bakery";
    public const string Pantry = "Pantry Staples";
    public const string Snacks = "Snacks & Confectionery";
    public const string Beverages = "Beverages";
    public const string Frozen = "Frozen Foods";
    public const string Household = "Household Products";
    public const string PersonalCare = "Personal Care";
    public const string BabyProducts = "Baby Products";
    public const string PetCare = "Pet Care";
    public const string Health = "Health & Wellness";

    /// <summary>
    /// All available categories
    /// </summary>
    public static readonly string[] AllCategories = new[]
    {
        Meat, Dairy, FruitsVegetables, Bakery, Pantry,
        Snacks, Beverages, Frozen, Household, PersonalCare,
        BabyProducts, PetCare, Health
    };

    /// <summary>
    /// Gets the index of a category for model scoring
    /// </summary>
    public static int GetCategoryIndex(string category)
    {
        for (int i = 0; i < AllCategories.Length; i++)
        {
            if (AllCategories[i].Equals(category, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }
}
