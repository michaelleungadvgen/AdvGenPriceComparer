# JSON Import Guide for LiteDB

This guide explains how to import grocery data from JSON files into the LiteDB database.

## Overview

The `JsonImportService` class provides functionality to import Coles and Woolworths catalogue data from JSON files into the LiteDB database. It handles:
- Creating or updating items (products)
- Creating store locations
- Recording price history with discount information
- Automatic store chain detection from filename or content
- File picker dialog for easy file selection

## Quick Start

### 1. Using the WinUI Application

The easiest way to import JSON data is through the WinUI application:

1. **Launch the application**
2. **Click "File" menu** → **"Import JSON Data..."**
3. **Select your JSON file** (e.g., `coles_29102025.json` or `woolworths_28102025.json`)
4. **Wait for import** - The app will automatically detect the store chain and import the data
5. **View results** - A notification will show the number of items and price records imported

The application provides:
- File picker dialog for easy file selection
- Automatic format detection (Coles/Woolworths)
- Progress notifications during import
- Automatic dashboard refresh after import
- Error reporting with detailed logs

### 2. Programmatic Usage

```csharp
using AdvGenPriceComparer.Data.LiteDB.Services;

// Create database service
using var dbService = new DatabaseService("GroceryPrices.db");

// Create import service
var importService = new JsonImportService(dbService);

// Import Coles data
var result = importService.ImportColesJson(
    jsonFilePath: @"C:\path\to\coles_17092025.json",
    storeName: "Coles",
    validDate: new DateTime(2025, 9, 17));

// Check results
if (result.Success)
{
    Console.WriteLine($"Imported {result.ItemsProcessed} items");
    Console.WriteLine($"Created {result.PriceRecordsCreated} price records");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

### 2. Running the Example

1. Build the solution:
   ```
   dotnet build
   ```

2. Run the ImportExample:
   ```
   dotnet run --project AdvGenPriceComparer
   ```

   Or compile and run ImportExample.cs directly.

## JSON Format

The import service expects JSON files in the following format:

```json
{
  "products": [
    {
      "productID": "CL001",
      "productName": "Product Name",
      "category": "Category",
      "brand": "Brand Name",
      "description": "Product description or size",
      "price": "$10.00",
      "originalPrice": "$20.00",
      "savings": "$10.00",
      "unitPrice": "$5.00 per kg",
      "specialType": "1/2 Price"
    }
  ]
}
```

### Supported JSON Files

The following JSON files are available in the `data` folder:
- `coles_17092025.json` - Coles catalogue from 17/09/2025
- `coles_24092025.json` - Coles catalogue from 24/09/2025
- `coles_01102025.json` - Coles catalogue from 01/10/2025
- `coles_08102025.json` - Coles catalogue from 08/10/2025
- `coles_15102025.json` - Coles catalogue from 15/10/2025

## Import Process

The import service performs the following steps:

1. **Read JSON File**: Deserializes the JSON into product objects
2. **Create/Get Store**: Finds or creates the store location (e.g., "Coles")
3. **Process Each Product**:
   - Checks if the item already exists (by name and brand)
   - Creates a new item or updates the existing one
   - Creates a price record with discount information
4. **Return Results**: Provides statistics and error information

## Data Mapping

### Item (Product) Fields
- `Name` ← `productName`
- `Description` ← `description`
- `Brand` ← `brand`
- `Category` ← `category`
- `PackageSize` ← `description`
- `ExtraInformation["ProductID"]` ← `productID`
- `ExtraInformation["Store"]` ← "Coles"
- `ExtraInformation["UnitPrice"]` ← `unitPrice`

### PriceRecord Fields
- `Price` ← `price` (parsed)
- `OriginalPrice` ← `originalPrice` (parsed)
- `Discount` ← `savings` (parsed)
- `DiscountPercentage` ← calculated from savings/original
- `IsOnSale` ← true if savings > 0
- `ValidFrom` ← `validDate` parameter
- `ValidTo` ← `validDate` + 7 days
- `Source` ← "Coles Catalogue"
- `ExtraInformation["SpecialType"]` ← `specialType`

## ImportResult Object

The `ImportResult` object contains:

```csharp
public class ImportResult
{
    public bool Success { get; set; }           // True if import succeeded
    public string? Message { get; set; }        // Success message
    public string? ErrorMessage { get; set; }   // Error message if failed
    public int ItemsProcessed { get; set; }     // Number of items processed
    public int PriceRecordsCreated { get; set; } // Number of price records created
    public List<string> Errors { get; set; }    // Individual product errors
}
```

## Database Location

By default, the database is created as `GroceryPrices.db` in the current directory. You can specify a different location:

```csharp
// Absolute path
var dbService = new DatabaseService(@"C:\Data\MyGroceryPrices.db");

// Relative path
var dbService = new DatabaseService("../data/GroceryPrices.db");

// User's AppData folder
var appDataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "AdvGenPriceComparer",
    "GroceryPrices.db");
var dbService = new DatabaseService(appDataPath);
```

## Advanced Usage

### Import Multiple Files

```csharp
var jsonFiles = new[]
{
    @"data\coles_17092025.json",
    @"data\coles_24092025.json",
    @"data\coles_01102025.json"
};

var validDates = new[]
{
    new DateTime(2025, 9, 17),
    new DateTime(2025, 9, 24),
    new DateTime(2025, 10, 1)
};

using var dbService = new DatabaseService("GroceryPrices.db");
var importService = new JsonImportService(dbService);

for (int i = 0; i < jsonFiles.Length; i++)
{
    Console.WriteLine($"Importing {jsonFiles[i]}...");
    var result = importService.ImportColesJson(jsonFiles[i], "Coles", validDates[i]);

    if (result.Success)
        Console.WriteLine($"✓ {result.ItemsProcessed} items imported");
    else
        Console.WriteLine($"✗ {result.ErrorMessage}");
}
```

### Querying Imported Data

After import, you can query the data:

```csharp
using var dbService = new DatabaseService("GroceryPrices.db");
var itemRepo = new ItemRepository(dbService);
var priceRepo = new PriceRecordRepository(dbService);

// Get all items in a category
var dairyItems = itemRepo.GetByCategory("Dairy");

// Get items by brand
var colesItems = itemRepo.GetByBrand("Coles");

// Get price history for an item
var priceHistory = priceRepo.GetByItemId(itemId);

// Get current specials
var currentSpecials = priceRepo.GetCurrentSpecials();
```

## Troubleshooting

### File Not Found
Ensure the JSON file path is correct and the file exists.

### Permission Denied
Make sure you have write permissions to the database directory.

### Import Shows 0 Items
Check that the JSON file contains a `products` array with items.

### Duplicate Items
The service checks for existing items by name and brand. If you want to update existing items, ensure the name and brand match exactly.

## Database Schema

### Items Collection
- Stores unique products (grocery items)
- Indexed by: Name, Brand, Category, Barcode
- Contains nutritional info, allergens, dietary flags

### Places Collection
- Stores store locations
- Indexed by: Name, Chain, Suburb, State
- Contains address, contact, coordinates

### PriceRecords Collection
- Stores historical price data
- Indexed by: ItemId, PlaceId, DateRecorded, Price
- Links items to places with price and discount info

## Support

For issues or questions, refer to the main CLAUDE.md file or check the project documentation.
