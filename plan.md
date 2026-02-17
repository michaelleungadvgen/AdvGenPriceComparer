# AdvGenPriceComparer WPF Application - Complete Development Plan

**Last Updated:** 2026-02-03
**Current Status:** 85% Complete - Missing Critical Services

---

## 📊 Project Overview

Migration of AdvGenPriceComparer from WinUI 3 to WPF with enhanced features for:
- Importing grocery price data from JSON files (Coles/Woolworths format)
- Importing markdown catalogues (drakes.md format)
- Generating JSON exports for price sharing
- Implementing a server component for P2P price data sharing

---

## ✅ Completed Components

### Application Infrastructure
- ✅ **App.xaml/App.xaml.cs** - Full dependency injection configured
- ✅ **Database setup** - LiteDB configured with GroceryPrices.db
- ✅ **Service container** - Microsoft.Extensions.DependencyInjection
- ✅ **MainWindow** - Navigation sidebar, dashboard, menu bar
- ✅ WPF project structure (AdvGenPriceComparer.WPF)
- ✅ Reference Core and Data.LiteDB libraries

### ViewModels (All Implemented)
- ✅ MainWindowViewModel
- ✅ ItemViewModel
- ✅ PlaceViewModel
- ✅ AddStoreViewModel
- ✅ ImportDataViewModel
- ✅ ExportDataViewModel
- ✅ AlertViewModel
- ✅ StoreViewModel
- ✅ ViewModelBase (with INotifyPropertyChanged)

### Services (Partially Complete)
- ✅ IDialogService / SimpleDialogService
- ✅ INotificationService / SimpleNotificationService
- ✅ ILoggerService / FileLoggerService
- ✅ DemoDataService
- ⚠️ **JsonImportService** - MISSING (referenced in App.xaml.cs line 87-91)
- ⚠️ **ServerConfigService** - MISSING (referenced in App.xaml.cs line 81-82)
- ⚠️ **ExportService** - MISSING (export logic not implemented)

### Views (All Implemented)
- ✅ ItemsPage.xaml
- ✅ StoresPage.xaml
- ✅ CategoryPage.xaml
- ✅ AlertsPage.xaml
- ✅ ImportDataWindow.xaml (UI complete, needs service implementation)
- ✅ ExportDataWindow.xaml (UI complete, needs export logic)
- ✅ AddItemWindow.xaml
- ✅ AddStoreWindow.xaml

### Commands
- ✅ RelayCommand (complete implementation)

---

## ⚠️ Critical Missing Components

### 1. JsonImportService (PRIORITY 1) 🔴
**Location:** `AdvGenPriceComparer.WPF/Services/JsonImportService.cs`
**Referenced:** App.xaml.cs lines 87-91
**Status:** Not implemented - app will fail at startup

**Required Class Structure:**
```csharp
public class JsonImportService
{
    public JsonImportService(DatabaseService dbService);

    // Import from Coles/Woolworths JSON format
    public Task<ImportResult> ImportFromJsonAsync(string filePath, string storeId, ImportOptions options);

    // Import from Drakes markdown format
    public Task<ImportResult> ImportFromMarkdownAsync(string filePath, string storeId, ImportOptions options);

    // Bulk import multiple files
    public Task<ImportResult> BulkImportAsync(string[] filePaths, string storeId, ImportOptions options);

    // Parse and preview without saving
    public Task<List<Item>> PreviewImportAsync(string filePath);
}
```

**Features Required:**

#### 1.1 Coles/Woolworths JSON Parser
- Parse JSON fields: `productID`, `productName`, `category`, `brand`, `price`, `originalPrice`, `savings`, `specialType`
- Convert price strings "$2.75" to decimal
- Map to Core.Models.Item
- Example input format:
```json
{
  "productID": "CL001",
  "productName": "Uncle Tobys Milkybar Muesli Bars 145g",
  "category": "General",
  "brand": "Uncle Tobys",
  "price": "$2.75",
  "originalPrice": "$5.50",
  "savings": "$2.75",
  "specialType": "Half Price Special"
}
```

#### 1.2 Drakes Markdown Parser
- Extract from format: `**Product Name** size - $price ea - SAVE $amount`
- Parse category sections (## Groceries, ## Fresh Produce, ## Meat & Seafood, etc.)
- Extract date range: `**Valid: 28 January 2026 - 3 February 2026**`
- Handle various price formats:
  - "$5 ea" (each)
  - "$15.90 per kg" (kilogram)
  - "$1.50 per litre" (litre)
  - "$2.40 per 100g" (unit pricing)
- Example input format:
```markdown
# Drakes Supermarket Specials
**Valid: 28 January 2026 - 3 February 2026**

## Groceries
- **Sunrice Medium Grain White Rice** 5kg - $5 ea - SAVE $7.50 ($0.10 per 100g)
- **Arnott's Tim Tams** 165g-200g - $3 ea - SAVE $3 ($1.50 per 100g)
```

#### 1.3 Duplicate Detection
- Check by product name + brand + size combination
- Merge strategies:
  - **Skip**: Don't import duplicates
  - **Overwrite**: Replace existing with new data
  - **Update Price Only**: Keep metadata, update price and date

#### 1.4 Progress Tracking
- Report progress for large imports (>100 items)
- Use `IProgress<ImportProgress>` for UI updates
- Track: Total items, Imported, Skipped, Failed

### 2. ServerConfigService (PRIORITY 2) 🔴
**Location:** `AdvGenPriceComparer.WPF/Services/ServerConfigService.cs`
**Referenced:** App.xaml.cs lines 81-82
**Status:** Not implemented - app will fail at startup

**Required Class Structure:**
```csharp
public class ServerConfigService
{
    public ServerConfigService(string configPath);

    // Configuration management
    public List<ServerConfig> GetServers();
    public void AddServer(ServerConfig server);
    public void RemoveServer(string serverId);
    public void UpdateServer(ServerConfig server);
    public ServerConfig GetDefaultServer();
    public void SetDefaultServer(string serverId);

    // Connection management
    public Task<bool> TestConnectionAsync(ServerConfig server);
    public Task<ServerStatus> GetServerStatusAsync(string serverId);
}
```

**servers.json Format:**
```json
{
  "servers": [
    {
      "id": "server-1",
      "name": "Local Server",
      "host": "localhost",
      "port": 5000,
      "apiKey": "your-api-key",
      "isDefault": true,
      "enabled": true
    }
  ]
}
```

**File Location:**
- Development: Project root `servers.json`
- Runtime: `%AppData%\AdvGenPriceComparer\servers.json`

### 3. ExportService (PRIORITY 3) 🟡
**Location:** Create `AdvGenPriceComparer.WPF/Services/ExportService.cs` or enhance ExportDataViewModel
**Status:** ExportDataViewModel exists but export logic not implemented

**Required Class Structure:**
```csharp
public class ExportService
{
    // Export to standardized JSON format
    public Task<ExportResult> ExportToJsonAsync(ExportOptions options, string outputPath);

    // Export with compression
    public Task<ExportResult> ExportToJsonGzAsync(ExportOptions options, string outputPath);

    // Incremental export (only changed since last export)
    public Task<ExportResult> IncrementalExportAsync(DateTime lastExportDate, string outputPath);
}
```

**Export Format Specification:**
```json
{
  "exportVersion": "1.0",
  "exportDate": "2026-02-03T10:30:00Z",
  "source": "AdvGenPriceComparer",
  "location": {
    "suburb": "Brisbane",
    "state": "QLD",
    "country": "Australia"
  },
  "items": [
    {
      "id": "guid",
      "name": "Product Name",
      "brand": "Brand",
      "category": "Category",
      "price": 5.00,
      "priceUnit": "ea",
      "originalPrice": 7.50,
      "validFrom": "2026-01-28",
      "validTo": "2026-02-03",
      "store": "Drakes",
      "storeId": "store-guid"
    }
  ]
}
```

**Export Filters:**
- Date range (validFrom/validTo)
- Store selection (single or multiple stores)
- Category filter
- Price range filter
- Only items with discounts/savings

---

## 📋 Implementation Checklist

### Phase 1: Fix Startup Errors (CRITICAL - 4-5 hours)
- [ ] **Create JsonImportService.cs**
  - [ ] Coles/Woolworths JSON parser (1-2 hours)
  - [ ] Drakes markdown parser (2-3 hours)
  - [ ] Duplicate detection (30 mins)
  - [ ] Progress tracking (30 mins)
- [ ] **Create ServerConfigService.cs** (1 hour)
  - [ ] Load/save servers.json
  - [ ] Connection management
  - [ ] Health check methods
- [ ] **Create sample servers.json** in project root (5 mins)
- [ ] **Test app startup** - should run without errors

### Phase 2: Complete Import Functionality (3-4 hours)
- [ ] Connect ImportDataViewModel to JsonImportService
- [ ] Test JSON import with existing data files:
  - [ ] `data/coles_28012026.json`
  - [ ] `data/woolworths_28012026.json`
  - [ ] `data/coles_24072025.json` (older format test)
- [ ] Test markdown import with `drakes.md`
- [ ] Implement import preview before saving
- [ ] Add error handling and validation
- [ ] Test duplicate detection strategies
- [ ] Add import progress UI updates

### Phase 3: Implement Export (2-3 hours)
- [ ] Create ExportService.cs
- [ ] Implement JSON export with standardized format
- [ ] Add export filters (date range, store, category)
- [ ] Add compression support (.json.gz)
- [ ] Connect to ExportDataWindow UI
- [ ] Test full export workflow
- [ ] Add export progress tracking

### Phase 4: Server Integration (Future - 5-7 days)
- [ ] Create ASP.NET Core Web API project (AdvGenPriceComparer.Server)
- [ ] Implement database schema for shared prices
- [ ] Create API endpoints:
  - [ ] POST /api/prices/upload - Upload price data
  - [ ] GET /api/prices/download - Download with filters
  - [ ] GET /api/prices/search - Search products
  - [ ] GET /api/prices/compare - Compare across stores
  - [ ] GET /api/prices/latest - Get latest deals
- [ ] Add SignalR for real-time updates
- [ ] Implement authentication (API key based)
- [ ] Add rate limiting
- [ ] Create upload/download UI in WPF app
- [ ] Test price sharing workflow

### Phase 5: Price Comparison & Analysis (3-4 days)
- [ ] Track historical prices in database
- [ ] Detect genuine vs. illusory discounts
- [ ] Calculate average prices over time
- [ ] Create PriceComparisonView.xaml
- [ ] Implement side-by-side store comparison
- [ ] Create price history charts (LiveCharts)
- [ ] Add "best price" highlighting
- [ ] Generate reports (weekly specials, best deals, trends)

### Phase 6: Enhanced Features (5-6 days)
- [ ] Product Management (CRUD operations)
- [ ] Store Management (CRUD, location mapping)
- [ ] Shopping list integration
- [ ] Price drop alerts
- [ ] Deal expiration reminders
- [ ] Weekly specials digest

### Phase 7: Testing & Deployment (3-4 days)
- [ ] Unit tests for import/export services
- [ ] Integration tests for database operations
- [ ] UI automation tests
- [ ] Create installer (WiX Toolset or ClickOnce)
- [ ] Configure auto-update mechanism
- [ ] User documentation

---

## 🚀 Quick Start Implementation Order

### Step 1: Create JsonImportService (2-3 hours) 🔴
**Focus on JSON parsing first (simpler than markdown):**

1. Create `AdvGenPriceComparer.WPF/Services/JsonImportService.cs`
2. Implement Coles/Woolworths JSON parser
3. Add basic duplicate detection
4. Test with existing JSON files (`data/coles_28012026.json`)

**Implementation hints:**
- Use `System.Text.Json` or `Newtonsoft.Json` for parsing
- Create DTOs matching JSON structure
- Map DTOs to `Core.Models.Item`
- Handle price string parsing: `"$2.75"` → `2.75m`

### Step 2: Create ServerConfigService (1 hour) 🔴
**Simple configuration file management:**

1. Create `AdvGenPriceComparer.WPF/Services/ServerConfigService.cs`
2. Create `ServerConfig` model class
3. Implement JSON file read/write
4. Create default `servers.json` in project root
5. Test load/save operations

**Implementation hints:**
- Use `System.Text.Json.JsonSerializer`
- Handle file not found gracefully (create default)
- Support `%AppData%\AdvGenPriceComparer\servers.json`

### Step 3: Add Drakes Markdown Parser (2-3 hours) 🟡
**More complex due to regex parsing:**

1. Add markdown parsing to JsonImportService
2. Implement regex patterns for:
   - Product lines: `**Product Name** size - $price ea - SAVE $amount`
   - Category headers: `## Category Name`
   - Date ranges: `**Valid: DD Month YYYY - DD Month YYYY**`
3. Handle different price units (ea, kg, litre, per 100g)
4. Test with `drakes.md`

**Implementation hints:**
- Use `System.Text.RegularExpressions`
- Split file by `##` for category sections
- Use named capture groups for clarity
- Handle optional fields (savings may not always be present)

### Step 4: Implement ExportService (2 hours) 🟡
1. Create `AdvGenPriceComparer.WPF/Services/ExportService.cs`
2. Query items from database with filters
3. Map to standardized export format
4. Serialize to JSON
5. Connect to ExportDataWindow UI
6. Test export workflow

**Implementation hints:**
- Use LINQ for filtering database queries
- Create export DTOs matching spec
- Add `System.IO.Compression` for .gz support
- Show progress for large exports

### Step 5: Testing & Refinement (2-3 hours)
1. End-to-end import testing (all formats)
2. End-to-end export testing
3. UI polish and error handling
4. Add logging for debugging
5. Update documentation

**Total Estimated Time for MVP:** 10-14 hours

---

## 📂 File Structure

```
AdvGenPriceComparer.WPF/
├── App.xaml ✅
├── App.xaml.cs ✅
├── MainWindow.xaml ✅
├── MainWindow.xaml.cs ✅
├── servers.json ⚠️ MISSING (create this)
├── Commands/
│   └── RelayCommand.cs ✅
├── Services/
│   ├── IDialogService.cs ✅
│   ├── SimpleDialogService.cs ✅
│   ├── INotificationService.cs ✅
│   ├── SimpleNotificationService.cs ✅
│   ├── ILoggerService.cs ✅
│   ├── FileLoggerService.cs ✅
│   ├── DemoDataService.cs ✅
│   ├── JsonImportService.cs ⚠️ MISSING - CREATE THIS
│   ├── ServerConfigService.cs ⚠️ MISSING - CREATE THIS
│   └── ExportService.cs ⚠️ MISSING - CREATE THIS
├── ViewModels/
│   ├── ViewModelBase.cs ✅
│   ├── MainWindowViewModel.cs ✅
│   ├── ItemViewModel.cs ✅
│   ├── PlaceViewModel.cs ✅
│   ├── AddStoreViewModel.cs ✅
│   ├── ImportDataViewModel.cs ✅
│   ├── ExportDataViewModel.cs ✅
│   ├── AlertViewModel.cs ✅
│   └── StoreViewModel.cs ✅
└── Views/
    ├── ItemsPage.xaml ✅
    ├── StoresPage.xaml ✅
    ├── CategoryPage.xaml ✅
    ├── AlertsPage.xaml ✅
    ├── ImportDataWindow.xaml ✅
    ├── ExportDataWindow.xaml ✅
    ├── AddItemWindow.xaml ✅
    └── AddStoreWindow.xaml ✅
```

---

## 🎯 Next Immediate Actions

**To make the app run:**
1. ✅ Create `JsonImportService.cs` (blocks app startup)
2. ✅ Create `ServerConfigService.cs` (blocks app startup)
3. ✅ Create `servers.json` in project root
4. Test app runs without errors

**To make it functional:**
5. Implement JSON import logic for Coles/Woolworths
6. Implement markdown import for Drakes
7. Implement export logic
8. Test end-to-end workflows

---

## 📝 Technical Requirements

### Dependencies (Already Installed)
- ✅ .NET 8.0 (using net8.0-windows)
- ✅ Microsoft.Extensions.DependencyInjection
- ✅ Microsoft.Extensions.Hosting
- ✅ WPF-UI (modern UI library)
- ✅ LiveChartsCore (for future price charts)
- ✅ LiteDB (via AdvGenPriceComparer.Data.LiteDB project)

### Additional Dependencies Needed
- Consider adding `Markdig` for markdown parsing (optional, can use regex)
- `System.IO.Compression` for export compression (built-in)

### Database Schema (Already Implemented)
- Items collection
- Places collection
- PriceRecords collection

---

## 📚 Existing Data Files for Testing

The repository has extensive test data:
- `data/coles_28012026.json` - Latest Coles prices (Jan 28, 2026)
- `data/woolworths_28012026.json` - Latest Woolworths prices
- `drakes.md` - Current Drakes specials (28 Jan - 3 Feb 2026)
- 50+ historical JSON files from Jul 2025 - Jan 2026:
  - `data/coles_24072025.json`
  - `data/woolworths_24072025.json`
  - (and many more...)

**Test Data Format Examples:**

**Coles JSON:**
```json
[
  {
    "productID": "CL001",
    "productName": "Uncle Tobys Milkybar Muesli Bars 145g",
    "category": "General",
    "brand": "Uncle Tobys",
    "description": "Breakfast muesli bars",
    "price": "$2.75",
    "originalPrice": "$5.50",
    "savings": "$2.75",
    "specialType": "Half Price Special"
  }
]
```

**Drakes Markdown:**
```markdown
# Drakes Supermarket Specials
**Valid: 28 January 2026 - 3 February 2026**

## Groceries
- **Sunrice Medium Grain White Rice** 5kg - $5 ea - SAVE $7.50 ($0.10 per 100g)
- **Selected Kellogg's Cereal** 460g-740g - $5 ea - SAVE $5

## Meat & Seafood
- **Australian Beef Lean Mince** Min 1.2kg - $15.90 per kg
```

---

## 🏗️ Known Working Features

- ✅ Database connection and initialization
- ✅ Logging to `%AppData%\AdvGenPriceComparer\Logs`
- ✅ Dialog and notification services
- ✅ MVVM architecture with full DI
- ✅ Demo data generation
- ✅ Navigation between views
- ✅ Modern UI with WPF-UI library

---

## 🔧 Build & Run Commands

```bash
# Navigate to WPF project
cd AdvGenPriceComparer.WPF

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

**Expected Startup Behavior:**
- Currently: Will fail due to missing JsonImportService and ServerConfigService
- After fixes: Should open MainWindow with dashboard visible

---

## 📅 Development Timeline Estimate

### MVP (Phases 1-3)
- **Phase 1 (Fix Startup):** 4-5 hours
- **Phase 2 (Import):** 3-4 hours
- **Phase 3 (Export):** 2-3 hours
- **Testing:** 2-3 hours
- **Total MVP:** 10-14 hours

### Full Implementation
- **Phase 4 (Server):** 5-7 days
- **Phase 5 (Analysis):** 3-4 days
- **Phase 6 (Enhanced Features):** 5-6 days
- **Phase 7 (Testing & Deployment):** 3-4 days
- **Total Full:** 24-32 days

---

## 🎓 Implementation Notes for AI Agent

### When implementing JsonImportService:
1. Start with JSON parsing (simpler) before markdown
2. Create DTOs that match JSON structure exactly
3. Use nullable types for optional fields
4. Handle price parsing carefully: `"$2.75"` → `2.75m`
5. Extract size from product name when present
6. Map specialType to notes or description field

### When implementing ServerConfigService:
1. Keep it simple - just JSON file read/write
2. Handle missing file gracefully (create default)
3. Validate server config before saving
4. Don't implement network calls yet (placeholder is fine)

### When implementing markdown parser:
1. Use regex with named groups for clarity
2. Split by `##` to get category sections
3. Extract date range from header first
4. Apply date range to all items in that import
5. Handle various price formats with unit tests
6. Skip promotion/competition sections

### Error Handling:
- Log all errors to ILoggerService
- Show user-friendly messages via IDialogService
- Don't crash on malformed data (skip and continue)
- Track failed imports in ImportResult

---

## 🎯 Success Criteria

**Phase 1 Complete When:**
- App starts without exceptions
- MainWindow displays correctly
- Can navigate to all views

**Phase 2 Complete When:**
- Can import Coles JSON successfully
- Can import Woolworths JSON successfully
- Can import Drakes markdown successfully
- Items appear in ItemsPage after import
- Duplicate detection works correctly

**Phase 3 Complete When:**
- Can export to standardized JSON format
- Export filters work correctly
- Exported file matches specification
- Can re-import exported file successfully

---

## 🧪 Phase 8: Unit Testing & Quality Assurance

### 8.1 Test Project Setup
**Location:** Create `AdvGenPriceComparer.Tests` project

**Project Structure:**
```
AdvGenPriceComparer.Tests/
├── AdvGenPriceComparer.Tests.csproj
├── Services/
│   ├── JsonImportServiceTests.cs
│   ├── ServerConfigServiceTests.cs
│   ├── ExportServiceTests.cs
│   └── CategoryPredictionServiceTests.cs
├── Repositories/
│   ├── ItemRepositoryTests.cs
│   ├── PlaceRepositoryTests.cs
│   └── PriceRecordRepositoryTests.cs
├── ViewModels/
│   ├── MainWindowViewModelTests.cs
│   ├── ItemViewModelTests.cs
│   └── ImportDataViewModelTests.cs
├── TestData/
│   ├── sample_coles.json
│   ├── sample_woolworths.json
│   ├── sample_drakes.md
│   └── invalid_data_samples/
└── Helpers/
    ├── TestDatabaseHelper.cs
    └── MockDataGenerator.cs
```

**Dependencies:**
```xml
<PackageReference Include="xUnit" Version="2.6.6" />
<PackageReference Include="xUnit.runner.visualstudio" Version="2.5.6" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
```

### 8.2 Service Layer Tests

#### JsonImportService Tests
```csharp
public class JsonImportServiceTests
{
    [Fact]
    public async Task ImportFromJsonAsync_ValidColesJson_ReturnsSuccessResult()

    [Fact]
    public async Task ImportFromJsonAsync_ValidWoolworthsJson_ReturnsSuccessResult()

    [Fact]
    public async Task ImportFromMarkdownAsync_ValidDrakesMarkdown_ParsesCorrectly()

    [Theory]
    [InlineData("$5 ea", 5.0, "ea")]
    [InlineData("$15.90 per kg", 15.90, "kg")]
    [InlineData("$1.50 per litre", 1.50, "litre")]
    public void ParsePrice_VariousFormats_ParsesCorrectly(string input, decimal expected, string unit)

    [Fact]
    public async Task ImportFromJsonAsync_DuplicateProduct_AppliesSkipStrategy()

    [Fact]
    public async Task ImportFromJsonAsync_DuplicateProduct_AppliesOverwriteStrategy()

    [Fact]
    public async Task ImportFromJsonAsync_DuplicateProduct_AppliesUpdatePriceStrategy()

    [Fact]
    public async Task ImportFromJsonAsync_InvalidJson_ReturnsFailureResult()

    [Fact]
    public async Task ImportFromMarkdownAsync_MissingDateRange_UsesCurrentDate()

    [Fact]
    public async Task BulkImportAsync_MultipleFiles_ReportsProgressCorrectly()

    [Fact]
    public async Task PreviewImportAsync_ValidFile_DoesNotSaveToDatabase()
}
```

#### ServerConfigService Tests
```csharp
public class ServerConfigServiceTests
{
    [Fact]
    public void GetServers_ConfigFileExists_ReturnsServerList()

    [Fact]
    public void GetServers_ConfigFileNotFound_ReturnsEmptyList()

    [Fact]
    public void AddServer_ValidServer_SavesSuccessfully()

    [Fact]
    public void AddServer_DuplicateId_ThrowsException()

    [Fact]
    public void GetDefaultServer_HasDefault_ReturnsCorrectServer()

    [Fact]
    public void SetDefaultServer_ValidServerId_UpdatesDefault()

    [Fact]
    public async Task TestConnectionAsync_ValidServer_ReturnsTrue()

    [Fact]
    public async Task TestConnectionAsync_InvalidServer_ReturnsFalse()
}
```

#### ExportService Tests
```csharp
public class ExportServiceTests
{
    [Fact]
    public async Task ExportToJsonAsync_WithItems_CreatesValidJson()

    [Fact]
    public async Task ExportToJsonAsync_WithDateFilter_FiltersCorrectly()

    [Fact]
    public async Task ExportToJsonAsync_WithStoreFilter_FiltersCorrectly()

    [Fact]
    public async Task ExportToJsonAsync_WithCategoryFilter_FiltersCorrectly()

    [Fact]
    public async Task ExportToJsonGzAsync_LargeDataset_CreatesCompressedFile()

    [Fact]
    public async Task IncrementalExportAsync_OnlyChangedItems_ExportsCorrectly()

    [Fact]
    public void ExportToJsonAsync_ValidExport_MatchesSpecification()
}
```

### 8.3 Repository Layer Tests

#### ItemRepository Tests
```csharp
public class ItemRepositoryTests : IDisposable
{
    private readonly DatabaseService _testDb;

    [Fact]
    public async Task AddAsync_ValidItem_AddsToDatabase()

    [Fact]
    public async Task GetByIdAsync_ExistingItem_ReturnsItem()

    [Fact]
    public async Task GetAllAsync_MultipleItems_ReturnsAll()

    [Fact]
    public async Task UpdateAsync_ExistingItem_UpdatesSuccessfully()

    [Fact]
    public async Task DeleteAsync_ExistingItem_DeletesSuccessfully()

    [Fact]
    public async Task SearchByNameAsync_PartialMatch_ReturnsMatches()

    [Fact]
    public async Task GetByCategoryAsync_ValidCategory_ReturnsFiltered()

    [Fact]
    public async Task GetByPriceRangeAsync_ValidRange_ReturnsFiltered()
}
```

### 8.4 ViewModel Tests

#### MainWindowViewModel Tests
```csharp
public class MainWindowViewModelTests
{
    [Fact]
    public async Task LoadDashboardStats_WithData_UpdatesCorrectly()

    [Fact]
    public void NavigateToItems_UpdatesCurrentView()

    [Fact]
    public void NavigateToStores_UpdatesCurrentView()

    [Fact]
    public async Task RefreshData_UpdatesAllStats()
}
```

#### ImportDataViewModel Tests
```csharp
public class ImportDataViewModelTests
{
    [Fact]
    public async Task ImportData_ValidJson_CallsImportService()

    [Fact]
    public async Task ImportData_InvalidFile_ShowsErrorDialog()

    [Fact]
    public void SetSelectedFiles_ValidFiles_UpdatesFileList()

    [Fact]
    public async Task PreviewImport_ValidFile_LoadsPreview()
}
```

### 8.5 Integration Tests

```csharp
public class ImportExportIntegrationTests
{
    [Fact]
    public async Task ImportThenExport_DataIntegrity_Maintained()

    [Fact]
    public async Task ImportMultipleFormats_AllStoresInDatabase()

    [Fact]
    public async Task ExportAndReimport_NoDataLoss()
}
```

### 8.6 Test Data Management

**TestDatabaseHelper.cs:**
```csharp
public class TestDatabaseHelper
{
    public static DatabaseService CreateInMemoryDatabase()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        return new DatabaseService(tempPath);
    }

    public static void CleanupDatabase(DatabaseService db)
    {
        // Cleanup logic
    }

    public static async Task SeedTestData(DatabaseService db, int itemCount = 10)
    {
        // Seed data logic
    }
}
```

**MockDataGenerator.cs:**
```csharp
public class MockDataGenerator
{
    public static Item CreateMockItem(string name = "Test Product")
    {
        return new Item
        {
            Name = name,
            Price = 10.99m,
            Brand = "Test Brand",
            Category = "Test Category",
            CreatedAt = DateTime.Now
        };
    }

    public static Place CreateMockPlace(string name = "Test Store")
    {
        // Mock place logic
    }

    public static string CreateMockColesJson(int productCount = 5)
    {
        // Generate mock Coles JSON
    }

    public static string CreateMockDrakesMarkdown(int productCount = 5)
    {
        // Generate mock Drakes markdown
    }
}
```

### 8.7 Test Coverage Goals

- **Services:** 90%+ code coverage
- **Repositories:** 95%+ code coverage
- **ViewModels:** 80%+ code coverage
- **Overall:** 85%+ code coverage

### 8.8 Testing Checklist

- [ ] Set up xUnit test project
- [ ] Install testing dependencies (xUnit, Moq, FluentAssertions)
- [ ] Create test data samples
- [ ] Implement JsonImportService tests (all scenarios)
- [ ] Implement ServerConfigService tests
- [ ] Implement ExportService tests
- [ ] Implement Repository layer tests
- [ ] Implement ViewModel tests
- [ ] Create integration tests
- [ ] Set up CI/CD pipeline for automated testing
- [ ] Generate code coverage reports
- [ ] Document testing strategy

### 8.9 Continuous Integration

**Azure DevOps / GitHub Actions Pipeline:**
```yaml
name: Build and Test

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    - name: Upload coverage
      uses: codecov/codecov-action@v3
```

---

## 🤖 Phase 9: ML.NET Auto-Categorization

### 9.1 Overview
Use ML.NET to automatically categorize products based on product name, brand, and description when manually entered or imported from sources without category information.

### 9.2 ML.NET Project Setup

**Location:** Create `AdvGenPriceComparer.ML` project

**Project Structure:**
```
AdvGenPriceComparer.ML/
├── AdvGenPriceComparer.ML.csproj
├── Models/
│   ├── ProductData.cs (input model)
│   ├── CategoryPrediction.cs (output model)
│   └── TrainingData.cs
├── Services/
│   ├── CategoryPredictionService.cs
│   ├── ModelTrainingService.cs
│   └── DataPreparationService.cs
├── Data/
│   ├── training_data.csv
│   └── category_mappings.json
└── MLModels/
    └── category_model.zip (trained model)
```

**Dependencies:**
```xml
<PackageReference Include="Microsoft.ML" Version="3.0.1" />
<PackageReference Include="Microsoft.ML.AutoML" Version="0.21.1" />
<PackageReference Include="Microsoft.ML.FastTree" Version="3.0.1" />
```

### 9.3 Data Models

**ProductData.cs (Input):**
```csharp
public class ProductData
{
    [LoadColumn(0)]
    public string ProductName { get; set; }

    [LoadColumn(1)]
    public string Brand { get; set; }

    [LoadColumn(2)]
    public string Description { get; set; }

    [LoadColumn(3)]
    public string Store { get; set; }

    [LoadColumn(4)]
    [ColumnName("Label")]
    public string Category { get; set; }
}
```

**CategoryPrediction.cs (Output):**
```csharp
public class CategoryPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedCategory { get; set; }

    [ColumnName("Score")]
    public float[] Score { get; set; }

    public float Confidence => Score?.Max() ?? 0f;

    public Dictionary<string, float> CategoryScores { get; set; }
}
```

### 9.4 Category Definitions

**Standard Categories:**
```csharp
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

    public static readonly string[] AllCategories = new[]
    {
        Meat, Dairy, FruitsVegetables, Bakery, Pantry,
        Snacks, Beverages, Frozen, Household, PersonalCare,
        BabyProducts, PetCare, Health
    };
}
```

### 9.5 Model Training Service

**ModelTrainingService.cs:**
```csharp
public class ModelTrainingService
{
    private readonly MLContext _mlContext;
    private readonly ILoggerService _logger;

    public ModelTrainingService(ILoggerService logger)
    {
        _mlContext = new MLContext(seed: 0);
        _logger = logger;
    }

    /// <summary>
    /// Train model using existing categorized products from database
    /// </summary>
    public async Task<TrainingResult> TrainModelFromDatabaseAsync(
        IGroceryDataService dataService,
        string outputModelPath)
    {
        _logger.LogInfo("Starting model training from database");

        // 1. Extract training data from database
        var items = await dataService.Items.GetAllAsync();
        var categorizedItems = items
            .Where(i => !string.IsNullOrEmpty(i.Category))
            .ToList();

        if (categorizedItems.Count < 100)
        {
            return new TrainingResult
            {
                Success = false,
                Message = $"Insufficient training data. Need at least 100 categorized items, found {categorizedItems.Count}"
            };
        }

        // 2. Convert to ProductData
        var trainingData = categorizedItems.Select(item => new ProductData
        {
            ProductName = item.Name ?? "",
            Brand = item.Brand ?? "",
            Description = item.Description ?? "",
            Store = item.StoreName ?? "",
            Category = item.Category
        }).ToList();

        // 3. Create IDataView
        IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // 4. Split into train/test sets (80/20)
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

        // 5. Build training pipeline
        var pipeline = BuildTrainingPipeline();

        // 6. Train model
        _logger.LogInfo("Training model...");
        var model = pipeline.Fit(split.TrainSet);

        // 7. Evaluate model
        var predictions = model.Transform(split.TestSet);
        var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

        _logger.LogInfo($"Model trained - Accuracy: {metrics.MacroAccuracy:P2}, MicroAccuracy: {metrics.MicroAccuracy:P2}");

        // 8. Save model
        _mlContext.Model.Save(model, dataView.Schema, outputModelPath);

        return new TrainingResult
        {
            Success = true,
            Message = "Model trained successfully",
            Accuracy = metrics.MacroAccuracy,
            MicroAccuracy = metrics.MicroAccuracy,
            TrainingItemCount = categorizedItems.Count,
            ModelPath = outputModelPath
        };
    }

    /// <summary>
    /// Train model from CSV file
    /// </summary>
    public TrainingResult TrainModelFromCsv(string csvPath, string outputModelPath)
    {
        _logger.LogInfo($"Training model from CSV: {csvPath}");

        // Load data
        IDataView dataView = _mlContext.Data.LoadFromTextFile<ProductData>(
            csvPath,
            hasHeader: true,
            separatorChar: ',');

        // Split and train
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
        var pipeline = BuildTrainingPipeline();
        var model = pipeline.Fit(split.TrainSet);

        // Evaluate
        var predictions = model.Transform(split.TestSet);
        var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

        // Save
        _mlContext.Model.Save(model, dataView.Schema, outputModelPath);

        _logger.LogInfo($"Model trained - Accuracy: {metrics.MacroAccuracy:P2}");

        return new TrainingResult
        {
            Success = true,
            Accuracy = metrics.MacroAccuracy,
            ModelPath = outputModelPath
        };
    }

    private IEstimator<ITransformer> BuildTrainingPipeline()
    {
        return _mlContext.Transforms.Conversion
            .MapValueToKey("Label", "Label")
            .Append(_mlContext.Transforms.Text.FeaturizeText("ProductNameFeatures", "ProductName"))
            .Append(_mlContext.Transforms.Text.FeaturizeText("BrandFeatures", "Brand"))
            .Append(_mlContext.Transforms.Text.FeaturizeText("DescriptionFeatures", "Description"))
            .Append(_mlContext.Transforms.Text.FeaturizeText("StoreFeatures", "Store"))
            .Append(_mlContext.Transforms.Concatenate("Features",
                "ProductNameFeatures", "BrandFeatures", "DescriptionFeatures", "StoreFeatures"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
    }

    /// <summary>
    /// Retrain model with new data (incremental learning)
    /// </summary>
    public async Task<TrainingResult> RetrainModelAsync(
        string existingModelPath,
        List<ProductData> newTrainingData,
        string outputModelPath)
    {
        _logger.LogInfo($"Retraining model with {newTrainingData.Count} new samples");

        // Load existing model
        ITransformer existingModel;
        using (var stream = File.OpenRead(existingModelPath))
        {
            existingModel = _mlContext.Model.Load(stream, out var _);
        }

        // Combine with new data and retrain
        var newDataView = _mlContext.Data.LoadFromEnumerable(newTrainingData);
        var pipeline = BuildTrainingPipeline();
        var retrainedModel = pipeline.Fit(newDataView);

        // Save
        _mlContext.Model.Save(retrainedModel, newDataView.Schema, outputModelPath);

        return new TrainingResult
        {
            Success = true,
            Message = $"Model retrained with {newTrainingData.Count} new samples"
        };
    }
}
```

### 9.6 Category Prediction Service

**CategoryPredictionService.cs:**
```csharp
public class CategoryPredictionService
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;
    private readonly PredictionEngine<ProductData, CategoryPrediction> _predictionEngine;
    private readonly ILoggerService _logger;
    private const float CONFIDENCE_THRESHOLD = 0.7f;

    public CategoryPredictionService(string modelPath, ILoggerService logger)
    {
        _mlContext = new MLContext();
        _logger = logger;

        if (File.Exists(modelPath))
        {
            using var stream = File.OpenRead(modelPath);
            _model = _mlContext.Model.Load(stream, out var _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductData, CategoryPrediction>(_model);
            _logger.LogInfo($"ML model loaded from {modelPath}");
        }
        else
        {
            _logger.LogWarning($"ML model not found at {modelPath}");
        }
    }

    /// <summary>
    /// Predict category for a single product
    /// </summary>
    public CategoryPrediction PredictCategory(Item item)
    {
        if (_predictionEngine == null)
        {
            return new CategoryPrediction
            {
                PredictedCategory = "Uncategorized",
                Confidence = 0f
            };
        }

        var productData = new ProductData
        {
            ProductName = item.Name ?? "",
            Brand = item.Brand ?? "",
            Description = item.Description ?? "",
            Store = item.StoreName ?? ""
        };

        var prediction = _predictionEngine.Predict(productData);

        // Extract category scores
        prediction.CategoryScores = new Dictionary<string, float>();
        for (int i = 0; i < prediction.Score.Length && i < ProductCategories.AllCategories.Length; i++)
        {
            prediction.CategoryScores[ProductCategories.AllCategories[i]] = prediction.Score[i];
        }

        _logger.LogInfo($"Predicted category for '{item.Name}': {prediction.PredictedCategory} (confidence: {prediction.Confidence:P2})");

        return prediction;
    }

    /// <summary>
    /// Predict category for multiple products (batch prediction)
    /// </summary>
    public List<(Item Item, CategoryPrediction Prediction)> PredictCategories(List<Item> items)
    {
        if (_predictionEngine == null)
        {
            return items.Select(item => (item, new CategoryPrediction
            {
                PredictedCategory = "Uncategorized",
                Confidence = 0f
            })).ToList();
        }

        var results = new List<(Item, CategoryPrediction)>();

        foreach (var item in items)
        {
            var prediction = PredictCategory(item);
            results.Add((item, prediction));
        }

        return results;
    }

    /// <summary>
    /// Auto-categorize item if confidence is high enough
    /// </summary>
    public bool TryAutoCategorize(Item item, out string category)
    {
        var prediction = PredictCategory(item);

        if (prediction.Confidence >= CONFIDENCE_THRESHOLD)
        {
            category = prediction.PredictedCategory;
            return true;
        }

        category = null;
        return false;
    }

    /// <summary>
    /// Get top N category suggestions
    /// </summary>
    public List<(string Category, float Confidence)> GetTopSuggestions(Item item, int topN = 3)
    {
        var prediction = PredictCategory(item);

        return prediction.CategoryScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(topN)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }
}
```

### 9.7 Integration with Import Process

**Enhanced JsonImportService:**
```csharp
public class JsonImportService
{
    private readonly CategoryPredictionService _categoryPredictor;
    private readonly ILoggerService _logger;

    public async Task<ImportResult> ImportFromJsonAsync(
        string filePath,
        string storeId,
        ImportOptions options)
    {
        // ... existing import logic ...

        foreach (var item in items)
        {
            // If no category provided, use ML prediction
            if (string.IsNullOrEmpty(item.Category) && options.EnableAutoCategorization)
            {
                if (_categoryPredictor.TryAutoCategorize(item, out string predictedCategory))
                {
                    item.Category = predictedCategory;
                    item.Notes = $"Auto-categorized with {_categoryPredictor.PredictCategory(item).Confidence:P0} confidence";
                    _logger.LogInfo($"Auto-categorized '{item.Name}' as '{predictedCategory}'");
                }
            }

            // ... save to database ...
        }
    }
}
```

### 9.8 Manual Entry with Auto-Suggestions

**AddItemWindow Enhancement:**
```csharp
private void ProductName_TextChanged(object sender, TextChangedEventArgs e)
{
    if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
        return;

    // Create temporary item for prediction
    var tempItem = new Item
    {
        Name = ProductNameTextBox.Text,
        Brand = BrandTextBox.Text,
        Description = DescriptionTextBox.Text
    };

    // Get category suggestions
    var suggestions = _categoryPredictor.GetTopSuggestions(tempItem, topN: 3);

    // Display suggestions
    CategorySuggestionsPanel.Children.Clear();
    foreach (var (category, confidence) in suggestions)
    {
        var button = new Button
        {
            Content = $"{category} ({confidence:P0})",
            Tag = category
        };
        button.Click += (s, args) =>
        {
            CategoryComboBox.SelectedValue = button.Tag;
        };
        CategorySuggestionsPanel.Children.Add(button);
    }
}
```

### 9.9 Model Management UI

**Create MLModelManagementWindow.xaml:**
```xml
<Window>
    <StackPanel Margin="20">
        <TextBlock Text="ML Model Management" FontSize="20" FontWeight="Bold"/>

        <GroupBox Header="Current Model" Margin="0,20,0,0">
            <StackPanel>
                <TextBlock Text="Model Path:"/>
                <TextBlock x:Name="ModelPathText" FontWeight="Bold"/>

                <TextBlock Text="Last Trained:" Margin="0,10,0,0"/>
                <TextBlock x:Name="LastTrainedText" FontWeight="Bold"/>

                <TextBlock Text="Accuracy:" Margin="0,10,0,0"/>
                <TextBlock x:Name="AccuracyText" FontWeight="Bold"/>

                <TextBlock Text="Training Items:" Margin="0,10,0,0"/>
                <TextBlock x:Name="TrainingItemsText" FontWeight="Bold"/>
            </StackPanel>
        </GroupBox>

        <GroupBox Header="Training" Margin="0,20,0,0">
            <StackPanel>
                <Button Content="Train from Database" Click="TrainFromDatabase_Click" Margin="0,10,0,5"/>
                <TextBlock Text="Uses all categorized items in database" FontStyle="Italic" Foreground="Gray"/>

                <Button Content="Train from CSV" Click="TrainFromCsv_Click" Margin="0,10,0,5"/>
                <TextBlock Text="Import training data from CSV file" FontStyle="Italic" Foreground="Gray"/>

                <Button Content="Retrain with New Data" Click="Retrain_Click" Margin="0,10,0,5"/>
                <TextBlock Text="Improve model with recently added items" FontStyle="Italic" Foreground="Gray"/>
            </StackPanel>
        </GroupBox>

        <GroupBox Header="Testing" Margin="0,20,0,0">
            <StackPanel>
                <TextBlock Text="Test Product Name:"/>
                <TextBox x:Name="TestProductName" Margin="0,5,0,10"/>

                <Button Content="Test Prediction" Click="TestPrediction_Click"/>

                <TextBlock Text="Prediction:" Margin="0,10,0,5" FontWeight="Bold"/>
                <TextBlock x:Name="PredictionResult"/>

                <TextBlock Text="Confidence:" Margin="0,5,0,5" FontWeight="Bold"/>
                <TextBlock x:Name="ConfidenceResult"/>
            </StackPanel>
        </GroupBox>

        <ProgressBar x:Name="TrainingProgress" Height="20" Margin="0,20,0,0" Visibility="Collapsed"/>
        <TextBlock x:Name="StatusText" Margin="0,10,0,0" TextAlignment="Center"/>
    </StackPanel>
</Window>
```

### 9.10 Training Data Preparation

**Generate training_data.csv from existing imports:**
```csharp
public class DataPreparationService
{
    public async Task ExportTrainingDataAsync(
        IGroceryDataService dataService,
        string outputCsvPath)
    {
        var items = await dataService.Items.GetAllAsync();
        var categorizedItems = items
            .Where(i => !string.IsNullOrEmpty(i.Category))
            .ToList();

        using var writer = new StreamWriter(outputCsvPath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write header
        csv.WriteField("ProductName");
        csv.WriteField("Brand");
        csv.WriteField("Description");
        csv.WriteField("Store");
        csv.WriteField("Category");
        csv.NextRecord();

        // Write data
        foreach (var item in categorizedItems)
        {
            csv.WriteField(item.Name ?? "");
            csv.WriteField(item.Brand ?? "");
            csv.WriteField(item.Description ?? "");
            csv.WriteField(item.StoreName ?? "");
            csv.WriteField(item.Category);
            csv.NextRecord();
        }
    }
}
```

### 9.11 Implementation Checklist

- [ ] Create AdvGenPriceComparer.ML project
- [ ] Install ML.NET NuGet packages
- [ ] Define ProductData and CategoryPrediction models
- [ ] Implement ModelTrainingService
- [ ] Implement CategoryPredictionService
- [ ] Create DataPreparationService
- [ ] Export training data from existing categorized items
- [ ] Train initial model with existing data
- [ ] Integrate prediction into JsonImportService
- [ ] Add auto-suggestion to AddItemWindow UI
- [ ] Create MLModelManagementWindow for training/testing
- [ ] Add configuration for confidence threshold
- [ ] Implement model versioning
- [ ] Test prediction accuracy
- [ ] Document ML workflow

### 9.12 Configuration

**Add to App.xaml.cs:**
```csharp
// ML Services
var mlModelPath = Path.Combine(appDataPath, "MLModels", "category_model.zip");
Directory.CreateDirectory(Path.GetDirectoryName(mlModelPath));

services.AddSingleton<ModelTrainingService>();
services.AddSingleton<CategoryPredictionService>(provider =>
    new CategoryPredictionService(mlModelPath, provider.GetRequiredService<ILoggerService>()));
services.AddSingleton<DataPreparationService>();
```

### 9.13 Expected Performance

**Training Requirements:**
- Minimum 100 categorized items per category
- Recommended 500+ items for good accuracy
- Training time: 10-60 seconds depending on dataset size

**Prediction Performance:**
- Single prediction: <10ms
- Batch prediction (100 items): <500ms
- Expected accuracy: 85-95% with sufficient training data

### 9.14 Continuous Improvement

**Auto-Retraining Strategy:**
1. User manually categorizes imported items
2. Every 100 new categorizations, trigger automatic retraining
3. Evaluate new model accuracy
4. If improved, replace production model
5. Log model performance metrics

**User Feedback Loop:**
```csharp
public class CategoryFeedbackService
{
    /// <summary>
    /// User corrects a prediction - use for retraining
    /// </summary>
    public async Task RecordFeedbackAsync(
        Item item,
        string predictedCategory,
        string actualCategory)
    {
        // Store feedback for future retraining
        var feedback = new CategoryFeedback
        {
            ProductName = item.Name,
            Brand = item.Brand,
            PredictedCategory = predictedCategory,
            ActualCategory = actualCategory,
            Timestamp = DateTime.Now
        };

        // Save to feedback collection
        await _feedbackRepository.AddAsync(feedback);

        // Check if we have enough feedback to retrain
        var feedbackCount = await _feedbackRepository.CountAsync();
        if (feedbackCount >= 100)
        {
            await TriggerRetrainingAsync();
        }
    }
}
```

---

## 📅 Updated Development Timeline

### MVP (Phases 1-3)
- **Phase 1 (Fix Startup):** 4-5 hours
- **Phase 2 (Import):** 3-4 hours
- **Phase 3 (Export):** 2-3 hours
- **Total MVP:** 10-14 hours

### Extended Features
- **Phase 4 (Server):** 5-7 days
- **Phase 5 (Analysis):** 3-4 days
- **Phase 6 (Enhanced Features):** 5-6 days
- **Phase 7 (Testing & Deployment):** 3-4 days
- **Phase 8 (Unit Testing):** 3-5 days
- **Phase 9 (ML.NET Auto-Categorization):** 2-3 days
- **Total Full Implementation:** 30-40 days

---

**END OF PLAN**
