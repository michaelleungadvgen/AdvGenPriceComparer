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
- **ML.NET Auto-Categorization** - Automatically categorize products using machine learning
- **ML.NET Price Prediction** - Forecast future prices and identify optimal buying times
- **Ollama Chat Interface** - Natural language price queries with local LLM

---

## 🤖 ML.NET Features Overview

### Auto-Categorization (Phase 9)
**Status:** Planned
**Purpose:** Automatically categorize grocery items using machine learning

### Key Capabilities:
1. **Auto-Categorization During Import**
   - Automatically assigns categories to products without category information
   - Works with JSON imports (Coles, Woolworths) and markdown imports (Drakes)
   - Confidence threshold: 70% (configurable)

2. **Smart Suggestions During Manual Entry**
   - Provides top 3 category suggestions as user types product name
   - Real-time prediction updates based on product name, brand, and description
   - One-click category selection from suggestions

3. **Model Training & Management**
   - Train model from existing categorized items in database
   - Import training data from CSV files
   - Incremental retraining with new user-categorized items
   - Model accuracy tracking and versioning

4. **Supported Categories:**
   - Meat & Seafood, Dairy & Eggs, Fruits & Vegetables, Bakery
   - Pantry Staples, Snacks & Confectionery, Beverages, Frozen Foods
   - Household Products, Personal Care, Baby Products, Pet Care, Health & Wellness

5. **Performance:**
   - Single prediction: <10ms
   - Batch prediction (100 items): <500ms
   - Expected accuracy: 85-95% with sufficient training data
   - Minimum 100 items per category for training

### Implementation Details:
See **[Phase 9: ML.NET Auto-Categorization](#-phase-9-mlnet-auto-categorization)** for complete implementation guide including:
- ML.NET project setup and dependencies
- Training and prediction service architecture
- Integration with import workflows
- UI components for model management
- Continuous improvement and feedback loops

---

### Price Prediction & Forecasting (Phase 11)
**Status:** Planned
**Purpose:** Predict future grocery prices and identify optimal buying times

#### Key Capabilities:
1. **Future Price Forecasting**
   - Predict prices up to 30 days in advance
   - 95% confidence intervals for predictions
   - Time series analysis using Singular Spectrum Analysis (SSA)
   - Expected accuracy: 5-15% MAPE

2. **Price Trend Analysis**
   - Identify rising, falling, or stable price trends
   - Calculate trend strength and direction
   - Track moving averages (7-day and 30-day)
   - Seasonal pattern detection

3. **Anomaly Detection**
   - Detect unusual price spikes and drops
   - Identify illusory discounts (fake sales)
   - Compare "sale" prices with historical averages
   - Flag suspicious pricing behavior

4. **Smart Buying Recommendations**
   - **Buy Now**: Price at or near historical low
   - **Wait**: Price expected to drop soon
   - **Avoid**: Price unusually high
   - **Normal**: No significant trend

5. **Optimal Buying Date**
   - Calculate best date to purchase within forecast window
   - Predict lowest price point
   - Set price alerts for optimal timing

#### Use Cases:
- Combat illusory discounts by comparing "sale" prices with predicted normal prices
- Plan shopping around predicted price drops
- Save money by buying at optimal times
- Verify discount claims against historical data
- Budget planning with price forecasts

#### Technical Details:
- **Algorithm**: SSA (Singular Spectrum Analysis) for time series forecasting
- **Data Requirements**: Minimum 30 days history, recommended 90+ days
- **Performance**: <2 seconds for 30-day forecast
- **Anomaly Detection**: Spike detection with 95% confidence
- **UI**: Interactive charts with LiveCharts, price trend visualization

### Implementation Details:
See **[Phase 11: ML.NET Price Prediction & Forecasting](#-phase-11-mlnet-price-prediction--forecasting)** for complete implementation guide including:
- Time series forecasting with ML.NET
- Price anomaly detection algorithms
- Illusory discount identification
- Interactive price forecast UI
- Buying recommendation engine

---

### Ollama Chat Interface (Phase 12)
**Status:** Planned
**Purpose:** Natural language interface for querying prices using local LLM

#### Key Capabilities:
1. **Natural Language Queries**
   - Ask questions in plain English: "What's the price of milk?"
   - No need to learn complex UI or query syntax
   - Conversational interface with context awareness
   - Supports follow-up questions

2. **Intelligent Query Routing**
   - LLM extracts intent from user questions
   - Routes queries to LiteDB or AdvGenNoSqlServer
   - Optimizes database queries based on intent
   - Handles complex multi-condition queries

3. **Supported Query Types**
   - Price queries: "How much is bread at Coles?"
   - Comparisons: "Compare milk prices between stores"
   - Deals: "What's on sale this week?"
   - History: "Show milk price trends"
   - Budget: "What can I buy for $50?"
   - Categories: "Show me all dairy products"

4. **Privacy-First Design**
   - All LLM processing runs locally via Ollama
   - No data sent to external APIs
   - Open-source models (Mistral, Llama 2, Phi)
   - Full user control over data

5. **Smart Features**
   - Context-aware conversations
   - Product recommendations
   - Budget planning assistance
   - Multi-store comparisons
   - Historical insights

#### Example Queries:
- "What's the cheapest bread?"
- "Find the best deals at Woolworths"
- "Show me milk price history over the last month"
- "Compare egg prices between Coles and Woolworths"
- "What groceries can I get for $50?"

#### Technical Details:
- **LLM**: Ollama with Mistral 7B (recommended)
- **Response Time**: 2-4 seconds per query
- **RAM Usage**: ~4GB for Mistral model
- **Privacy**: 100% local processing
- **UI**: Modern chat interface with message history

### Implementation Details:
See **[Phase 12: Ollama Chat Interface for Natural Language Price Queries](#-phase-12-ollama-chat-interface-for-natural-language-price-queries)** for complete implementation guide including:
- Ollama setup and model selection
- Intent extraction from natural language
- Query routing to databases
- Response generation
- Chat UI implementation

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
- ✅ **JsonImportService** - IMPLEMENTED in AdvGenPriceComparer.Data.LiteDB/Services/
- ✅ **ServerConfigService** - IMPLEMENTED in AdvGenPriceComparer.Core/Services/
- ✅ **ExportService** - IMPLEMENTED in AdvGenPriceComparer.WPF/Services/

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
- [x] Connect ImportDataViewModel to JsonImportService
- [x] Test JSON import with existing data files:
  - [x] `data/coles_28012026.json`
  - [x] `data/woolworths_28012026.json`
  - [x] `data/coles_24072025.json` (older format test)
- [x] Test markdown import with `drakes.md` - COMPLETED: Created TestMarkdownImport CLI, all 4 tests passed
- [x] Implement import preview before saving
- [x] Add error handling and validation
- [x] Test duplicate detection strategies
- [x] Add import progress UI updates

### Phase 3: Implement Export (2-3 hours)
- [x] Create ExportService.cs
- [x] Implement JSON export with standardized format
- [x] Add export filters (date range, store, category)
- [x] Add compression support (.json.gz)
- [x] Connect to ExportDataWindow UI
- [x] Test full export workflow
- [x] Add export progress tracking

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
├── servers.json ✅ (already exists)
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
│   ├── JsonImportService.cs ✅ (in Data.LiteDB project)
│   ├── ServerConfigService.cs ✅ (in Core project)
│   └── ExportService.cs ✅ IMPLEMENTED
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
4. ✅ Create `ExportService.cs`
5. Test app runs without errors

**To make it functional:**
6. Implement JSON import logic for Coles/Woolworths
7. Implement markdown import for Drakes
8. ✅ Implement export logic (ExportService created)
9. ✅ Connect ExportDataWindow UI to ExportService
10. Test end-to-end workflows

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

- [x] Set up xUnit test project
- [x] Install testing dependencies (xUnit, Moq, FluentAssertions)
- [x] Create test data samples
- [x] Implement JsonImportService tests (all scenarios) - Agent-012: 24 comprehensive xUnit tests created covering PreviewImportAsync, ImportFromFile, ImportColesProducts, price parsing, progress tracking, and error handling
- [x] Implement ServerConfigService tests
- [x] Implement ExportService tests
- [x] Implement Repository layer tests
- [x] Implement ViewModel tests
- [x] Create integration tests - 7 comprehensive xUnit integration tests covering ImportThenExport, ExportAndReimport, ImportMultipleFormats, DateFiltering, StoreFiltering, Compression, and DuplicateData handling
- [x] Set up CI/CD pipeline for automated testing - Agent-018: Updated GitHub Actions for WPF build with .NET 9
- [x] Generate code coverage reports - Agent-019: Added coverlet.runsettings, generates coverage data in CI (27.67% line coverage)
- [x] Document testing strategy - Agent-020: Created comprehensive TESTING.md in AdvGenPriceComparer.Tests/

### 8.10 Documentation Updates (Completed)

#### README.md Update for WPF Architecture
**Implemented by:** Agent-021 (2026-02-26)

**Changes Made:**
- Updated architecture diagram to show WPF (WPF-UI Fluent) instead of WinUI 3
- Updated project structure section with accurate folder structure
- Updated roadmap to show completed WPF migration and current phases
- Updated Key Features section to reflect implemented import/export functionality
- Fixed build error in ComparePricesWindow.xaml.cs (added missing using statement)

**Files Modified:**
- `README.md` - Updated architecture, features, roadmap, project structure

---

## Phase 9: Enhanced Features (High Priority from PROJECT_STATUS.md)

### 9.1 Price Comparison View
**Implemented by:** Agent-021 (2026-02-26)

**Changes Made:**
- Added `ShowComparePricesDialog` method to IDialogService interface
- Implemented `ShowComparePricesDialog` in SimpleDialogService
- Created PriceComparisonViewModel with store comparison data
- Created ComparePricesWindow with side-by-side store comparison UI
- Added ComparePricesCommand to MainWindowViewModel
- Added "Compare Prices" button to Quick Actions in MainWindow sidebar
- Features: Shows average prices across stores, highlights best value store, displays item count

**Files Modified:**
- `AdvGenPriceComparer.WPF/Services/IDialogService.cs` - Added ShowComparePricesDialog method
- `AdvGenPriceComparer.WPF/Services/SimpleDialogService.cs` - Implemented ShowComparePricesDialog
- `AdvGenPriceComparer.WPF/ViewModels/PriceComparisonViewModel.cs` - Created new ViewModel
- `AdvGenPriceComparer.WPF/ViewModels/MainWindowViewModel.cs` - Added ComparePricesCommand
- `AdvGenPriceComparer.WPF/Views/ComparePricesWindow.xaml` - Created new window
- `AdvGenPriceComparer.WPF/Views/ComparePricesWindow.xaml.cs` - Created code-behind
- `AdvGenPriceComparer.WPF/MainWindow.xaml` - Added Compare Prices button
- `AdvGenPriceComparer.WPF/Views/ComparePricesWindow.xaml.cs` - Added missing using statement

### 8.11 Import Enhancements (Completed)

#### Support for JSON Files Without ProductID
**Implemented by:** Agent-009 (2026-02-25)

**Changes Made:**
- Made `ColesProduct.ProductID` property nullable (`string?`)
- Added XML documentation comments to all `ColesProduct` properties
- Implemented `GetProductId()` method that:
  - Returns existing ProductID if available
  - Generates stable ID based on brand + product name hash when ProductID is null
  - Uses MD5 hashing to create deterministic IDs for the same product across imports

**Benefits:**
- Can now import JSON files that don't have productID fields
- Same product (same name + brand) gets same generated ID across different imports
- Maintains backward compatibility with files that have explicit ProductIDs

**Files Modified:**
- `AdvGenPriceComparer.Data.LiteDB/Services/JsonImportService.cs` - ColesProduct class
- `AdvGenPriceComparer.WPF/ViewModels/ImportDataViewModel.cs` - Uses GetProductId() for mappings

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

## ⚙️ Phase 10: Database Provider Selection & Settings

### 10.1 Overview
Implement a flexible database provider system that allows users to choose between:
- **LiteDB** - Local embedded database (default, no server required)
- **AdvGenNoSQLServer** - Remote NoSQL server for multi-device synchronization

### 10.2 Database Provider Interface

**Create IDatabaseProvider Interface:**
```csharp
public interface IDatabaseProvider : IDisposable
{
    string ProviderName { get; }
    bool IsConnected { get; }
    Task<bool> ConnectAsync(DatabaseConnectionSettings settings);
    Task DisconnectAsync();
    
    // Repository accessors
    IItemRepository Items { get; }
    IPlaceRepository Places { get; }
    IPriceRecordRepository PriceRecords { get; }
    ICategoryRepository Categories { get; }
}
```

**DatabaseProviderType Enum:**
```csharp
public enum DatabaseProviderType
{
    LiteDB,
    AdvGenNoSQLServer
}
```

**DatabaseConnectionSettings:**
```csharp
public class DatabaseConnectionSettings
{
    public DatabaseProviderType ProviderType { get; set; }
    
    // LiteDB specific
    public string LiteDbPath { get; set; } = "GroceryPrices.db";
    
    // AdvGenNoSQLServer specific
    public string ServerHost { get; set; } = "localhost";
    public int ServerPort { get; set; } = 5000;
    public string ApiKey { get; set; }
    public string DatabaseName { get; set; } = "GroceryPrices";
    public bool UseSsl { get; set; } = true;
    
    // Connection pool settings
    public int ConnectionTimeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}
```

### 10.3 Database Provider Factory

**DatabaseProviderFactory:**
```csharp
public class DatabaseProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerService _logger;

    public DatabaseProviderFactory(IServiceProvider serviceProvider, ILoggerService logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IDatabaseProvider CreateProvider(DatabaseProviderType type)
    {
        return type switch
        {
            DatabaseProviderType.LiteDB => _serviceProvider.GetRequiredService<LiteDbProvider>(),
            DatabaseProviderType.AdvGenNoSQLServer => _serviceProvider.GetRequiredService<AdvGenNoSqlProvider>(),
            _ => throw new NotSupportedException($"Database provider '{type}' is not supported")
        };
    }
}
```

### 10.4 LiteDB Provider Implementation

**LiteDbProvider:**
```csharp
public class LiteDbProvider : IDatabaseProvider
{
    private readonly LiteDatabase _database;
    private readonly ILoggerService _logger;

    public string ProviderName => "LiteDB";
    public bool IsConnected => _database != null;

    public IItemRepository Items { get; }
    public IPlaceRepository Places { get; }
    public IPriceRecordRepository PriceRecords { get; }
    public ICategoryRepository Categories { get; }

    public LiteDbProvider(ILoggerService logger)
    {
        _logger = logger;
    }

    public Task<bool> ConnectAsync(DatabaseConnectionSettings settings)
    {
        try
        {
            _logger.LogInfo($"Connecting to LiteDB: {settings.LiteDbPath}");
            
            var connectionString = $"Filename={settings.LiteDbPath};Connection=Shared";
            _database = new LiteDatabase(connectionString);
            
            // Initialize repositories
            Items = new LiteDbItemRepository(_database, _logger);
            Places = new LiteDbPlaceRepository(_database, _logger);
            PriceRecords = new LiteDbPriceRecordRepository(_database, _logger);
            Categories = new LiteDbCategoryRepository(_database, _logger);
            
            _logger.LogInfo("LiteDB connected successfully");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to connect to LiteDB", ex);
            return Task.FromResult(false);
        }
    }

    public Task DisconnectAsync()
    {
        _database?.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _database?.Dispose();
    }
}
```

### 10.5 AdvGenNoSQLServer Provider Implementation

**AdvGenNoSqlProvider:**
```csharp
public class AdvGenNoSqlProvider : IDatabaseProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILoggerService _logger;
    private DatabaseConnectionSettings _settings;

    public string ProviderName => "AdvGenNoSQLServer";
    public bool IsConnected { get; private set; }

    public IItemRepository Items { get; private set; }
    public IPlaceRepository Places { get; private set; }
    public IPriceRecordRepository PriceRecords { get; private set; }
    public ICategoryRepository Categories { get; private set; }

    public AdvGenNoSqlProvider(ILoggerService logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task<bool> ConnectAsync(DatabaseConnectionSettings settings)
    {
        try
        {
            _settings = settings;
            var scheme = settings.UseSsl ? "https" : "http";
            var baseUrl = $"{scheme}://{settings.ServerHost}:{settings.ServerPort}/api";
            
            _logger.LogInfo($"Connecting to AdvGenNoSQLServer: {baseUrl}");
            
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", settings.ApiKey);
            _httpClient.Timeout = TimeSpan.FromSeconds(settings.ConnectionTimeout);
            
            // Test connection
            var response = await _httpClient.GetAsync("/health");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Health check failed: {response.StatusCode}");
                return false;
            }
            
            // Initialize repositories
            Items = new AdvGenNoSqlItemRepository(_httpClient, _logger);
            Places = new AdvGenNoSqlPlaceRepository(_httpClient, _logger);
            PriceRecords = new AdvGenNoSqlPriceRecordRepository(_httpClient, _logger);
            Categories = new AdvGenNoSqlCategoryRepository(_httpClient, _logger);
            
            IsConnected = true;
            _logger.LogInfo("AdvGenNoSQLServer connected successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to connect to AdvGenNoSQLServer", ex);
            IsConnected = false;
            return false;
        }
    }

    public Task DisconnectAsync()
    {
        IsConnected = false;
        _httpClient?.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
```

### 10.6 Settings Service

**ISettingsService Interface:**
```csharp
public interface ISettingsService
{
    DatabaseConnectionSettings DatabaseSettings { get; set; }
    
    // General settings
    string DefaultExportPath { get; set; }
    string DefaultImportPath { get; set; }
    string Culture { get; set; }
    bool AutoCheckForUpdates { get; set; }
    
    // ML Settings
    string MLModelPath { get; set; }
    float AutoCategorizationThreshold { get; set; }
    bool EnableAutoCategorization { get; set; }
    
    // Notification settings
    bool EnablePriceDropAlerts { get; set; }
    bool EnableExpirationAlerts { get; set; }
    int AlertCheckIntervalHours { get; set; }
    
    // Load/Save
    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
}
```

**SettingsService Implementation:**
```csharp
public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private readonly ILoggerService _logger;
    private readonly object _lock = new();

    public DatabaseConnectionSettings DatabaseSettings { get; set; } = new();
    public string DefaultExportPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public string DefaultImportPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public string Culture { get; set; } = "en-AU";
    public bool AutoCheckForUpdates { get; set; } = true;
    public string MLModelPath { get; set; }
    public float AutoCategorizationThreshold { get; set; } = 0.7f;
    public bool EnableAutoCategorization { get; set; } = true;
    public bool EnablePriceDropAlerts { get; set; } = true;
    public bool EnableExpirationAlerts { get; set; } = true;
    public int AlertCheckIntervalHours { get; set; } = 24;

    public SettingsService(ILoggerService logger)
    {
        _logger = logger;
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer");
        _settingsPath = Path.Combine(appDataPath, "settings.json");
        
        MLModelPath = Path.Combine(appDataPath, "MLModels", "category_model.zip");
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                _logger.LogInfo("Settings file not found, using defaults");
                return;
            }

            var json = await File.ReadAllTextAsync(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            
            if (settings != null)
            {
                DatabaseSettings = settings.DatabaseSettings;
                DefaultExportPath = settings.DefaultExportPath;
                DefaultImportPath = settings.DefaultImportPath;
                Culture = settings.Culture;
                AutoCheckForUpdates = settings.AutoCheckForUpdates;
                MLModelPath = settings.MLModelPath ?? MLModelPath;
                AutoCategorizationThreshold = settings.AutoCategorizationThreshold;
                EnableAutoCategorization = settings.EnableAutoCategorization;
                EnablePriceDropAlerts = settings.EnablePriceDropAlerts;
                EnableExpirationAlerts = settings.EnableExpirationAlerts;
                AlertCheckIntervalHours = settings.AlertCheckIntervalHours;
                
                _logger.LogInfo("Settings loaded successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load settings", ex);
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
            }

            var settings = new AppSettings
            {
                DatabaseSettings = DatabaseSettings,
                DefaultExportPath = DefaultExportPath,
                DefaultImportPath = DefaultImportPath,
                Culture = Culture,
                AutoCheckForUpdates = AutoCheckForUpdates,
                MLModelPath = MLModelPath,
                AutoCategorizationThreshold = AutoCategorizationThreshold,
                EnableAutoCategorization = EnableAutoCategorization,
                EnablePriceDropAlerts = EnablePriceDropAlerts,
                EnableExpirationAlerts = EnableExpirationAlerts,
                AlertCheckIntervalHours = AlertCheckIntervalHours
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsPath, json);
            
            _logger.LogInfo("Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save settings", ex);
        }
    }
}

public class AppSettings
{
    public DatabaseConnectionSettings DatabaseSettings { get; set; } = new();
    public string DefaultExportPath { get; set; }
    public string DefaultImportPath { get; set; }
    public string Culture { get; set; } = "en-AU";
    public bool AutoCheckForUpdates { get; set; } = true;
    public string MLModelPath { get; set; }
    public float AutoCategorizationThreshold { get; set; } = 0.7f;
    public bool EnableAutoCategorization { get; set; } = true;
    public bool EnablePriceDropAlerts { get; set; } = true;
    public bool EnableExpirationAlerts { get; set; } = true;
    public int AlertCheckIntervalHours { get; set; } = 24;
}
```

### 10.7 Settings Page UI

**SettingsWindow.xaml:**
```xml
<Window x:Class="AdvGenPriceComparer.WPF.Views.SettingsWindow"
        Title="Settings" Height="600" Width="800"
        WindowStartupLocation="CenterOwner">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <Border Grid.Row="0" Background="{DynamicResource SystemAccentColorBrush}" Padding="20,15">
            <TextBlock Text="Settings" FontSize="24" FontWeight="Bold" Foreground="White"/>
        </Border>

        <!-- Content -->
        <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
            <StackPanel Margin="20">
                
                <!-- Database Settings -->
                <GroupBox Header="Database Configuration" Margin="0,0,0,20">
                    <StackPanel Margin="10">
                        <TextBlock Text="Database Provider:" FontWeight="SemiBold" Margin="0,0,0,5"/>
                        <ComboBox x:Name="DatabaseProviderCombo"
                                  SelectedValuePath="Tag"
                                  SelectedValue="{Binding DatabaseSettings.ProviderType}">
                            <ComboBoxItem Tag="LiteDB" Content="LiteDB (Local)"/>
                            <ComboBoxItem Tag="AdvGenNoSQLServer" Content="AdvGenNoSQLServer (Remote)"/>
                        </ComboBox>

                        <!-- LiteDB Settings -->
                        <StackPanel x:Name="LiteDbSettingsPanel" Margin="0,15,0,0"
                                    Visibility="{Binding IsLiteDbSelected, Converter={StaticResource BooleanToVisibilityConverter}}">
                            <TextBlock Text="Database File Path:" FontWeight="SemiBold" Margin="0,0,0,5"/>
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                <TextBox Grid.Column="0" 
                                         Text="{Binding DatabaseSettings.LiteDbPath}"
                                         IsReadOnly="True"/>
                                <Button Grid.Column="1" Content="Browse..." 
                                        Margin="10,0,0,0" Padding="15,5"
                                        Click="BrowseLiteDbPath_Click"/>
                            </Grid>
                        </StackPanel>

                        <!-- AdvGenNoSQLServer Settings -->
                        <StackPanel x:Name="AdvGenNoSqlSettingsPanel" Margin="0,15,0,0"
                                    Visibility="{Binding IsAdvGenNoSqlSelected, Converter={StaticResource BooleanToVisibilityConverter}}">
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="10"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                <Grid.RowDefinitions>
                                    <RowDefinition Height="Auto"/>
                                    <RowDefinition Height="Auto"/>
                                    <RowDefinition Height="Auto"/>
                                    <RowDefinition Height="Auto"/>
                                </Grid.RowDefinitions>

                                <!-- Server Host -->
                                <TextBlock Grid.Row="0" Grid.Column="0" Text="Server Host:" FontWeight="SemiBold" Margin="0,0,0,5"/>
                                <TextBox Grid.Row="1" Grid.Column="0" 
                                         Text="{Binding DatabaseSettings.ServerHost}"/>

                                <!-- Server Port -->
                                <TextBlock Grid.Row="0" Grid.Column="2" Text="Port:" FontWeight="SemiBold" Margin="0,0,0,5"/>
                                <TextBox Grid.Row="1" Grid.Column="2" Width="80"
                                         Text="{Binding DatabaseSettings.ServerPort}"/>

                                <!-- Database Name -->
                                <TextBlock Grid.Row="2" Grid.Column="0" Text="Database Name:" FontWeight="SemiBold" Margin="0,10,0,5"/>
                                <TextBox Grid.Row="3" Grid.Column="0" Grid.ColumnSpan="3"
                                         Text="{Binding DatabaseSettings.DatabaseName}"/>
                            </Grid>

                            <TextBlock Text="API Key:" FontWeight="SemiBold" Margin="0,10,0,5"/>
                            <PasswordBox x:Name="ApiKeyPasswordBox" PasswordChanged="ApiKeyPasswordBox_PasswordChanged"/>

                            <CheckBox Margin="0,10,0,0" 
                                      Content="Use SSL/TLS"
                                      IsChecked="{Binding DatabaseSettings.UseSsl}"/>

                            <StackPanel Orientation="Horizontal" Margin="0,15,0,0">
                                <Button Content="Test Connection" Padding="15,5"
                                        Click="TestConnection_Click"/>
                                <TextBlock x:Name="ConnectionTestResult" Margin="15,0,0,0" 
                                           VerticalAlignment="Center"/>
                            </StackPanel>
                        </StackPanel>
                    </StackPanel>
                </GroupBox>

                <!-- General Settings -->
                <GroupBox Header="General Settings" Margin="0,0,0,20">
                    <Grid Margin="10">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="10"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>

                        <TextBlock Grid.Row="0" Grid.Column="0" Text="Default Import Path:" FontWeight="SemiBold" Margin="0,0,0,5"/>
                        <Grid Grid.Row="1" Grid.Column="0">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBox Grid.Column="0" Text="{Binding DefaultImportPath}" IsReadOnly="True"/>
                            <Button Grid.Column="1" Content="..." Margin="5,0,0,0" Padding="10,0"
                                    Click="BrowseImportPath_Click"/>
                        </Grid>

                        <TextBlock Grid.Row="0" Grid.Column="2" Text="Default Export Path:" FontWeight="SemiBold" Margin="0,0,0,5"/>
                        <Grid Grid.Row="1" Grid.Column="2">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBox Grid.Column="0" Text="{Binding DefaultExportPath}" IsReadOnly="True"/>
                            <Button Grid.Column="1" Content="..." Margin="5,0,0,0" Padding="10,0"
                                    Click="BrowseExportPath_Click"/>
                        </Grid>

                        <TextBlock Grid.Row="2" Grid.Column="0" Text="Culture/Language:" FontWeight="SemiBold" Margin="0,10,0,5"/>
                        <ComboBox Grid.Row="3" Grid.Column="0" 
                                  SelectedValue="{Binding Culture}"
                                  SelectedValuePath="Tag">
                            <ComboBoxItem Tag="en-AU" Content="English (Australia)"/>
                            <ComboBoxItem Tag="zh-TW" Content="繁體中文 (Traditional Chinese)"/>
                            <ComboBoxItem Tag="zh-CN" Content="简体中文 (Simplified Chinese)"/>
                        </ComboBox>

                        <CheckBox Grid.Row="2" Grid.Column="2" Grid.RowSpan="2"
                                  Margin="0,10,0,0" VerticalAlignment="Center"
                                  Content="Automatically check for updates"
                                  IsChecked="{Binding AutoCheckForUpdates}"/>
                    </Grid>
                </GroupBox>

                <!-- ML Settings -->
                <GroupBox Header="Machine Learning Settings" Margin="0,0,0,20">
                    <StackPanel Margin="10">
                        <CheckBox Content="Enable auto-categorization"
                                  IsChecked="{Binding EnableAutoCategorization}"
                                  Margin="0,0,0,10"/>

                        <TextBlock Text="Confidence Threshold:" FontWeight="SemiBold" Margin="0,0,0,5"/>
                        <Slider Minimum="0" Maximum="1" TickFrequency="0.05" 
                                Value="{Binding AutoCategorizationThreshold}"
                                IsEnabled="{Binding EnableAutoCategorization}"/>
                        <TextBlock Text="{Binding AutoCategorizationThreshold, StringFormat=P0}"
                                   HorizontalAlignment="Center" Margin="0,5,0,0"/>
                    </StackPanel>
                </GroupBox>

                <!-- Notification Settings -->
                <GroupBox Header="Notification Settings" Margin="0,0,0,20">
                    <StackPanel Margin="10">
                        <CheckBox Content="Enable price drop alerts"
                                  IsChecked="{Binding EnablePriceDropAlerts}"
                                  Margin="0,0,0,10"/>
                        <CheckBox Content="Enable deal expiration alerts"
                                  IsChecked="{Binding EnableExpirationAlerts}"
                                  Margin="0,0,0,10"/>
                        
                        <TextBlock Text="Alert Check Interval (hours):" FontWeight="SemiBold" Margin="0,10,0,5"/>
                        <TextBox Text="{Binding AlertCheckIntervalHours}" Width="100" HorizontalAlignment="Left"/>
                    </StackPanel>
                </GroupBox>

            </StackPanel>
        </ScrollViewer>

        <!-- Footer Buttons -->
        <Border Grid.Row="2" Background="{DynamicResource SystemAltMediumColor}" Padding="20,15">
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                <Button Content="Cancel" Padding="20,8" Margin="0,0,10,0"
                        Click="Cancel_Click"/>
                <Button Content="Save" Padding="20,8" 
                        Background="{DynamicResource SystemAccentColorBrush}"
                        Foreground="White"
                        Click="Save_Click"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

**SettingsViewModel:**
```csharp
public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IDatabaseProvider _currentProvider;
    private readonly DatabaseProviderFactory _providerFactory;
    private readonly ILoggerService _logger;

    public DatabaseConnectionSettings DatabaseSettings { get; set; }
    public string DefaultExportPath { get; set; }
    public string DefaultImportPath { get; set; }
    public string Culture { get; set; }
    public bool AutoCheckForUpdates { get; set; }
    public float AutoCategorizationThreshold { get; set; }
    public bool EnableAutoCategorization { get; set; }
    public bool EnablePriceDropAlerts { get; set; }
    public bool EnableExpirationAlerts { get; set; }
    public int AlertCheckIntervalHours { get; set; }

    public bool IsLiteDbSelected => DatabaseSettings?.ProviderType == DatabaseProviderType.LiteDB;
    public bool IsAdvGenNoSqlSelected => DatabaseSettings?.ProviderType == DatabaseProviderType.AdvGenNoSQLServer;

    public ICommand TestConnectionCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public SettingsViewModel(
        ISettingsService settingsService,
        IDatabaseProvider currentProvider,
        DatabaseProviderFactory providerFactory,
        ILoggerService logger)
    {
        _settingsService = settingsService;
        _currentProvider = currentProvider;
        _providerFactory = providerFactory;
        _logger = logger;

        // Load current settings
        DatabaseSettings = settingsService.DatabaseSettings;
        DefaultExportPath = settingsService.DefaultExportPath;
        DefaultImportPath = settingsService.DefaultImportPath;
        Culture = settingsService.Culture;
        AutoCheckForUpdates = settingsService.AutoCheckForUpdates;
        AutoCategorizationThreshold = settingsService.AutoCategorizationThreshold;
        EnableAutoCategorization = settingsService.EnableAutoCategorization;
        EnablePriceDropAlerts = settingsService.EnablePriceDropAlerts;
        EnableExpirationAlerts = settingsService.EnableExpirationAlerts;
        AlertCheckIntervalHours = settingsService.AlertCheckIntervalHours;

        TestConnectionCommand = new RelayCommand(async () => await TestConnectionAsync());
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        CancelCommand = new RelayCommand(Cancel);
    }

    private async Task TestConnectionAsync()
    {
        try
        {
            var provider = _providerFactory.CreateProvider(DatabaseSettings.ProviderType);
            var result = await provider.ConnectAsync(DatabaseSettings);
            
            // Show result to user (via dialog service)
        }
        catch (Exception ex)
        {
            _logger.LogError("Connection test failed", ex);
        }
    }

    private async Task SaveAsync()
    {
        // Update settings service
        _settingsService.DatabaseSettings = DatabaseSettings;
        _settingsService.DefaultExportPath = DefaultExportPath;
        _settingsService.DefaultImportPath = DefaultImportPath;
        _settingsService.Culture = Culture;
        _settingsService.AutoCheckForUpdates = AutoCheckForUpdates;
        _settingsService.AutoCategorizationThreshold = AutoCategorizationThreshold;
        _settingsService.EnableAutoCategorization = EnableAutoCategorization;
        _settingsService.EnablePriceDropAlerts = EnablePriceDropAlerts;
        _settingsService.EnableExpirationAlerts = EnableExpirationAlerts;
        _settingsService.AlertCheckIntervalHours = AlertCheckIntervalHours;

        await _settingsService.SaveSettingsAsync();
        
        // May need to restart app if database provider changed
        var requiresRestart = _currentProvider.ProviderName != DatabaseSettings.ProviderType.ToString();
        
        // Close settings window
    }

    private void Cancel()
    {
        // Close without saving
    }
}
```

### 10.8 App.xaml.cs Updates

**Updated DI Configuration:**
```csharp
private void ConfigureServices(IServiceCollection services)
{
    // ... existing services ...

    // Settings Service
    services.AddSingleton<ISettingsService, SettingsService>();
    services.AddSingleton<SettingsViewModel>();

    // Database Provider Factory
    services.AddSingleton<DatabaseProviderFactory>();

    // Database Providers
    services.AddSingleton<LiteDbProvider>();
    services.AddSingleton<AdvGenNoSqlProvider>();

    // Current Database Provider (resolved based on settings)
    services.AddSingleton<IDatabaseProvider>(provider =>
    {
        var settingsService = provider.GetRequiredService<ISettingsService>();
        settingsService.LoadSettingsAsync().Wait();
        
        var factory = provider.GetRequiredService<DatabaseProviderFactory>();
        var databaseProvider = factory.CreateProvider(settingsService.DatabaseSettings.ProviderType);
        databaseProvider.ConnectAsync(settingsService.DatabaseSettings).Wait();
        
        return databaseProvider;
    });
}
```

### 10.9 AdvGenNoSQLServer Repository

**Question:** Should the AdvGenNoSQLServer repository be included in this project?

**Answer:** The AdvGenNoSQLServer repository can be:

1. **Option A - Separate Repository (Recommended):** 
   - Create a separate `AdvGenNoSQLServer` repository for the server component
   - The WPF app references it via NuGet package or git submodule
   - Server can be deployed independently
   - Better separation of concerns

2. **Option B - Same Repository, Separate Project:**
   - Include server code in this repository as `AdvGenPriceComparer.Server`
   - Easier for development and testing
   - Single codebase for both client and server

3. **Option C - No Server Code (Client Only):**
   - Just implement the client-side provider
   - Assume server is managed separately
   - Document the API protocol for server implementers

**Recommendation:** Start with Option C for now - implement the client-side provider interface with stubs/API contracts. The actual server implementation can be added later as a separate repository (Option A) when needed.

### 10.10 Implementation Checklist

- [ ] Create `IDatabaseProvider` interface
- [ ] Create `DatabaseConnectionSettings` class
- [ ] Create `DatabaseProviderFactory`
- [ ] Implement `LiteDbProvider`
- [ ] Implement `AdvGenNoSqlProvider` (client-side)
- [ ] Create `ISettingsService` interface
- [ ] Implement `SettingsService`
- [ ] Create `SettingsWindow.xaml` UI
- [ ] Create `SettingsViewModel`
- [ ] Update `App.xaml.cs` for provider selection
- [ ] Add menu item to open Settings
- [ ] Implement connection testing for AdvGenNoSQLServer
- [ ] Handle provider switching (with restart notification)
- [ ] Document AdvGenNoSQLServer API protocol
- [ ] Test database switching workflow

### 10.11 File Structure Updates

```
AdvGenPriceComparer.WPF/
├── Services/
│   ├── ISettingsService.cs
│   ├── SettingsService.cs
│   ├── DatabaseProviderFactory.cs
│   └── Providers/
│       ├── IDatabaseProvider.cs
│       ├── LiteDbProvider.cs
│       └── AdvGenNoSqlProvider.cs
├── ViewModels/
│   └── SettingsViewModel.cs
└── Views/
    └── SettingsWindow.xaml
```

---

## 📊 Phase 11: ML.NET Price Prediction & Forecasting

### 11.1 Overview
Use ML.NET to predict future grocery prices, identify price trends, detect anomalies, and provide intelligent buying recommendations based on historical price data.

**Key Objectives:**
- Predict future prices for products based on historical data
- Identify seasonal price patterns and trends
- Detect price anomalies (unusual spikes or drops)
- Forecast optimal buying times
- Alert users to predicted price drops
- Combat illusory discounts by comparing with predicted "normal" prices

### 11.2 ML.NET Setup for Time Series Analysis

**Location:** Extend `AdvGenPriceComparer.ML` project

**Additional Dependencies:**
```xml
<PackageReference Include="Microsoft.ML" Version="3.0.1" />
<PackageReference Include="Microsoft.ML.TimeSeries" Version="3.0.1" />
<PackageReference Include="Microsoft.ML.FastTree" Version="3.0.1" />
```

**Project Structure Enhancement:**
```
AdvGenPriceComparer.ML/
├── Models/
│   ├── PriceHistoryData.cs (input for time series)
│   ├── PriceForecast.cs (output prediction)
│   ├── PriceAnomaly.cs (anomaly detection result)
│   └── BuyingRecommendation.cs
├── Services/
│   ├── PricePredictionService.cs
│   ├── PriceAnomalyDetectionService.cs
│   ├── PriceForecastingService.cs
│   └── BuyingRecommendationService.cs
└── MLModels/
    ├── price_forecast_model.zip
    └── anomaly_detection_model.zip
```

### 11.3 Data Models

**PriceHistoryData.cs (Input):**
```csharp
public class PriceHistoryData
{
    [LoadColumn(0)]
    public string ItemId { get; set; }

    [LoadColumn(1)]
    public string ItemName { get; set; }

    [LoadColumn(2)]
    public DateTime Date { get; set; }

    [LoadColumn(3)]
    public float Price { get; set; }

    [LoadColumn(4)]
    public bool IsOnSale { get; set; }

    [LoadColumn(5)]
    public string Store { get; set; }

    [LoadColumn(6)]
    public string Category { get; set; }

    [LoadColumn(7)]
    public int DayOfWeek { get; set; }

    [LoadColumn(8)]
    public int WeekOfYear { get; set; }

    [LoadColumn(9)]
    public int Month { get; set; }

    // Derived features
    public float MovingAverage7Days { get; set; }
    public float MovingAverage30Days { get; set; }
    public float PriceChange { get; set; }
}
```

**PriceForecast.cs (Output):**
```csharp
public class PriceForecast
{
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public DateTime ForecastDate { get; set; }

    [ColumnName("Score")]
    public float PredictedPrice { get; set; }

    public float ConfidenceInterval { get; set; }
    public float LowerBound { get; set; }
    public float UpperBound { get; set; }

    // Trend analysis
    public PriceTrend Trend { get; set; } // Rising, Falling, Stable
    public float TrendStrength { get; set; } // 0-1

    // Buying recommendation
    public BuyingRecommendation Recommendation { get; set; }
}

public enum PriceTrend
{
    Rising,
    Falling,
    Stable
}

public enum BuyingRecommendation
{
    BuyNow,      // Price is at or near historical low
    Wait,        // Price expected to drop soon
    NormalTime,  // No significant trend
    AvoidHighPrice // Currently unusually high
}
```

**PriceAnomaly.cs:**
```csharp
public class PriceAnomaly
{
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public DateTime Date { get; set; }
    public float ActualPrice { get; set; }
    public float ExpectedPrice { get; set; }
    public float Deviation { get; set; }

    [ColumnName("PredictedLabel")]
    public bool IsAnomaly { get; set; }

    [ColumnName("Score")]
    public float AnomalyScore { get; set; }

    public AnomalyType Type { get; set; } // PriceSpike, PriceDrop, Seasonal
    public string Description { get; set; }
}

public enum AnomalyType
{
    PriceSpike,    // Unusual price increase
    PriceDrop,     // Unusual price decrease
    Seasonal,      // Expected seasonal variation
    IllusoryDiscount // "Sale" price is actually normal or high
}
```

### 11.4 Price Forecasting Service

**PriceForecastingService.cs:**
```csharp
public class PriceForecastingService
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;
    private readonly ILoggerService _logger;
    private readonly IPriceRecordRepository _priceRecords;

    public PriceForecastingService(
        string modelPath,
        IPriceRecordRepository priceRecords,
        ILoggerService logger)
    {
        _mlContext = new MLContext();
        _priceRecords = priceRecords;
        _logger = logger;

        if (File.Exists(modelPath))
        {
            using var stream = File.OpenRead(modelPath);
            _model = _mlContext.Model.Load(stream, out var _);
            _logger.LogInfo($"Price forecast model loaded from {modelPath}");
        }
    }

    /// <summary>
    /// Train forecasting model using historical price data
    /// </summary>
    public async Task<TrainingResult> TrainModelAsync(
        string itemId,
        int forecastHorizon = 30,
        string outputModelPath = null)
    {
        _logger.LogInfo($"Training price forecast model for item {itemId}");

        // 1. Get historical price data (at least 90 days recommended)
        var priceHistory = await GetPriceHistoryAsync(itemId);

        if (priceHistory.Count < 30)
        {
            return new TrainingResult
            {
                Success = false,
                Message = $"Insufficient data. Need at least 30 days, found {priceHistory.Count}"
            };
        }

        // 2. Prepare data with feature engineering
        var trainingData = PrepareTrainingData(priceHistory);
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // 3. Build time series pipeline
        var pipeline = _mlContext.Forecasting.ForecastBySsa(
            outputColumnName: "Score",
            inputColumnName: nameof(PriceHistoryData.Price),
            windowSize: 7,
            seriesLength: priceHistory.Count,
            trainSize: priceHistory.Count,
            horizon: forecastHorizon,
            confidenceLevel: 0.95f,
            confidenceLowerBoundColumn: "LowerBound",
            confidenceUpperBoundColumn: "UpperBound"
        );

        // 4. Train model
        var model = pipeline.Fit(dataView);

        // 5. Save model
        if (!string.IsNullOrEmpty(outputModelPath))
        {
            _mlContext.Model.Save(model, dataView.Schema, outputModelPath);
        }

        _logger.LogInfo($"Price forecast model trained successfully");

        return new TrainingResult
        {
            Success = true,
            Message = $"Model trained with {priceHistory.Count} data points",
            TrainingItemCount = priceHistory.Count
        };
    }

    /// <summary>
    /// Forecast future prices for an item
    /// </summary>
    public async Task<List<PriceForecast>> ForecastPricesAsync(
        string itemId,
        int daysAhead = 30)
    {
        var priceHistory = await GetPriceHistoryAsync(itemId);

        if (priceHistory.Count < 30)
        {
            _logger.LogWarning($"Insufficient data for forecasting item {itemId}");
            return new List<PriceForecast>();
        }

        var trainingData = PrepareTrainingData(priceHistory);
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Build and train SSA model
        var pipeline = _mlContext.Forecasting.ForecastBySsa(
            outputColumnName: "Score",
            inputColumnName: nameof(PriceHistoryData.Price),
            windowSize: 7,
            seriesLength: priceHistory.Count,
            trainSize: priceHistory.Count,
            horizon: daysAhead,
            confidenceLevel: 0.95f,
            confidenceLowerBoundColumn: "LowerBound",
            confidenceUpperBoundColumn: "UpperBound"
        );

        var model = pipeline.Fit(dataView);

        // Create forecast engine
        var forecastEngine = model.CreateTimeSeriesEngine<PriceHistoryData, PriceForecast>(_mlContext);

        // Generate forecasts
        var forecasts = new List<PriceForecast>();
        var currentDate = priceHistory.Max(p => p.Date);

        for (int i = 1; i <= daysAhead; i++)
        {
            var forecast = forecastEngine.Predict();

            forecasts.Add(new PriceForecast
            {
                ItemId = itemId,
                ItemName = priceHistory.First().ItemName,
                ForecastDate = currentDate.AddDays(i),
                PredictedPrice = forecast.PredictedPrice,
                LowerBound = forecast.LowerBound,
                UpperBound = forecast.UpperBound,
                ConfidenceInterval = forecast.UpperBound - forecast.LowerBound,
                Trend = DetermineTrend(forecasts, forecast.PredictedPrice),
                Recommendation = GenerateRecommendation(priceHistory, forecast)
            });
        }

        return forecasts;
    }

    /// <summary>
    /// Forecast prices for multiple items (batch)
    /// </summary>
    public async Task<Dictionary<string, List<PriceForecast>>> ForecastMultipleItemsAsync(
        List<string> itemIds,
        int daysAhead = 30)
    {
        var results = new Dictionary<string, List<PriceForecast>>();

        foreach (var itemId in itemIds)
        {
            var forecasts = await ForecastPricesAsync(itemId, daysAhead);
            if (forecasts.Any())
            {
                results[itemId] = forecasts;
            }
        }

        return results;
    }

    /// <summary>
    /// Get optimal buying date within forecast window
    /// </summary>
    public async Task<(DateTime Date, float Price)> GetOptimalBuyingDateAsync(
        string itemId,
        int daysAhead = 30)
    {
        var forecasts = await ForecastPricesAsync(itemId, daysAhead);

        if (!forecasts.Any())
        {
            return (DateTime.Now, 0);
        }

        // Find date with lowest predicted price
        var optimalForecast = forecasts.OrderBy(f => f.PredictedPrice).First();

        return (optimalForecast.ForecastDate, optimalForecast.PredictedPrice);
    }

    private async Task<List<PriceHistoryData>> GetPriceHistoryAsync(string itemId)
    {
        var records = await _priceRecords.GetPriceHistoryAsync(itemId, DateTime.Now.AddMonths(-6));

        return records.Select(r => new PriceHistoryData
        {
            ItemId = r.ItemId,
            ItemName = r.Item?.Name ?? "",
            Date = r.DateRecorded,
            Price = (float)r.Price,
            IsOnSale = r.IsOnSale,
            Store = r.Place?.Name ?? "",
            Category = r.Item?.Category ?? "",
            DayOfWeek = (int)r.DateRecorded.DayOfWeek,
            WeekOfYear = GetWeekOfYear(r.DateRecorded),
            Month = r.DateRecorded.Month
        }).ToList();
    }

    private List<PriceHistoryData> PrepareTrainingData(List<PriceHistoryData> history)
    {
        // Calculate moving averages
        for (int i = 0; i < history.Count; i++)
        {
            // 7-day moving average
            if (i >= 6)
            {
                history[i].MovingAverage7Days = history
                    .Skip(i - 6)
                    .Take(7)
                    .Average(p => p.Price);
            }

            // 30-day moving average
            if (i >= 29)
            {
                history[i].MovingAverage30Days = history
                    .Skip(i - 29)
                    .Take(30)
                    .Average(p => p.Price);
            }

            // Price change
            if (i > 0)
            {
                history[i].PriceChange = history[i].Price - history[i - 1].Price;
            }
        }

        return history;
    }

    private PriceTrend DetermineTrend(List<PriceForecast> recentForecasts, float currentPrice)
    {
        if (recentForecasts.Count < 3)
            return PriceTrend.Stable;

        var last3 = recentForecasts.TakeLast(3).Select(f => f.PredictedPrice).ToList();
        last3.Add(currentPrice);

        var isRising = last3[1] > last3[0] && last3[2] > last3[1] && last3[3] > last3[2];
        var isFalling = last3[1] < last3[0] && last3[2] < last3[1] && last3[3] < last3[2];

        if (isRising) return PriceTrend.Rising;
        if (isFalling) return PriceTrend.Falling;
        return PriceTrend.Stable;
    }

    private BuyingRecommendation GenerateRecommendation(
        List<PriceHistoryData> history,
        PriceForecast forecast)
    {
        var avgPrice = history.Average(h => h.Price);
        var minPrice = history.Min(h => h.Price);
        var maxPrice = history.Max(h => h.Price);

        var predicted = forecast.PredictedPrice;

        // Buy now if price is near historical minimum
        if (predicted <= minPrice * 1.1f)
            return BuyingRecommendation.BuyNow;

        // Wait if price is falling
        if (forecast.Trend == PriceTrend.Falling)
            return BuyingRecommendation.Wait;

        // Avoid if price is unusually high
        if (predicted >= maxPrice * 0.9f)
            return BuyingRecommendation.AvoidHighPrice;

        return BuyingRecommendation.NormalTime;
    }

    private int GetWeekOfYear(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(
            date,
            System.Globalization.CalendarWeekRule.FirstDay,
            DayOfWeek.Monday);
    }
}
```

### 11.5 Price Anomaly Detection Service

**PriceAnomalyDetectionService.cs:**
```csharp
public class PriceAnomalyDetectionService
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;
    private readonly ILoggerService _logger;

    public PriceAnomalyDetectionService(string modelPath, ILoggerService logger)
    {
        _mlContext = new MLContext();
        _logger = logger;

        if (File.Exists(modelPath))
        {
            using var stream = File.OpenRead(modelPath);
            _model = _mlContext.Model.Load(stream, out var _);
        }
    }

    /// <summary>
    /// Train anomaly detection model
    /// </summary>
    public TrainingResult TrainAnomalyDetectionModel(
        List<PriceHistoryData> priceHistory,
        string outputModelPath)
    {
        _logger.LogInfo("Training price anomaly detection model");

        var dataView = _mlContext.Data.LoadFromEnumerable(priceHistory);

        // Use Spike Detection for sudden price changes
        var pipeline = _mlContext.Transforms.DetectSpikeBySsa(
            outputColumnName: nameof(PriceAnomaly.IsAnomaly),
            inputColumnName: nameof(PriceHistoryData.Price),
            confidence: 95,
            pvalueHistoryLength: priceHistory.Count / 4,
            trainingWindowSize: priceHistory.Count / 2,
            seasonalityWindowSize: 7 // Weekly seasonality
        );

        var model = pipeline.Fit(dataView);

        // Save model
        _mlContext.Model.Save(model, dataView.Schema, outputModelPath);

        _logger.LogInfo("Anomaly detection model trained");

        return new TrainingResult
        {
            Success = true,
            Message = "Anomaly detection model trained successfully"
        };
    }

    /// <summary>
    /// Detect price anomalies in recent data
    /// </summary>
    public List<PriceAnomaly> DetectAnomalies(
        string itemId,
        List<PriceHistoryData> priceHistory)
    {
        if (_model == null || priceHistory.Count < 14)
        {
            return new List<PriceAnomaly>();
        }

        var dataView = _mlContext.Data.LoadFromEnumerable(priceHistory);
        var predictions = _model.Transform(dataView);

        var anomalies = _mlContext.Data.CreateEnumerable<PriceAnomaly>(
            predictions,
            reuseRowObject: false).ToList();

        // Classify anomaly types
        for (int i = 0; i < anomalies.Count; i++)
        {
            if (anomalies[i].IsAnomaly)
            {
                var history = priceHistory[i];
                var avgPrice = priceHistory.Average(p => p.Price);

                if (history.Price > avgPrice * 1.2f)
                {
                    anomalies[i].Type = AnomalyType.PriceSpike;
                    anomalies[i].Description = $"Price spike detected: ${history.Price:F2} vs average ${avgPrice:F2}";
                }
                else if (history.Price < avgPrice * 0.8f)
                {
                    anomalies[i].Type = AnomalyType.PriceDrop;
                    anomalies[i].Description = $"Price drop detected: ${history.Price:F2} vs average ${avgPrice:F2}";
                }

                // Check for illusory discounts
                if (history.IsOnSale && history.Price >= avgPrice * 0.95f)
                {
                    anomalies[i].Type = AnomalyType.IllusoryDiscount;
                    anomalies[i].Description = $"Illusory discount: \"Sale\" price ${history.Price:F2} is near average ${avgPrice:F2}";
                }

                anomalies[i].ItemId = itemId;
                anomalies[i].Date = history.Date;
                anomalies[i].ActualPrice = history.Price;
                anomalies[i].ExpectedPrice = avgPrice;
                anomalies[i].Deviation = Math.Abs(history.Price - avgPrice);
            }
        }

        return anomalies.Where(a => a.IsAnomaly).ToList();
    }

    /// <summary>
    /// Detect illusory discounts across all items
    /// </summary>
    public async Task<List<PriceAnomaly>> DetectIllusoryDiscountsAsync(
        IPriceRecordRepository priceRecords)
    {
        var illusoryDiscounts = new List<PriceAnomaly>();

        // Get all items currently on sale
        var saleItems = await priceRecords.GetItemsOnSaleAsync();

        foreach (var saleRecord in saleItems)
        {
            var history = await priceRecords.GetPriceHistoryAsync(
                saleRecord.ItemId,
                DateTime.Now.AddMonths(-3));

            if (history.Count < 10) continue;

            var avgNonSalePrice = history
                .Where(h => !h.IsOnSale)
                .Average(h => (float)h.Price);

            var currentSalePrice = (float)saleRecord.Price;

            // Flag as illusory if "sale" price is >= 95% of average non-sale price
            if (currentSalePrice >= avgNonSalePrice * 0.95f)
            {
                illusoryDiscounts.Add(new PriceAnomaly
                {
                    ItemId = saleRecord.ItemId,
                    ItemName = saleRecord.Item?.Name ?? "",
                    Date = saleRecord.DateRecorded,
                    ActualPrice = currentSalePrice,
                    ExpectedPrice = avgNonSalePrice,
                    Deviation = currentSalePrice - avgNonSalePrice,
                    IsAnomaly = true,
                    Type = AnomalyType.IllusoryDiscount,
                    Description = $"Illusory discount: Sale price ${currentSalePrice:F2} is {(currentSalePrice / avgNonSalePrice * 100):F1}% of average price ${avgNonSalePrice:F2}"
                });
            }
        }

        return illusoryDiscounts;
    }
}
```

### 11.6 UI Integration - Price Forecast View

**PriceForecastWindow.xaml:**
```xml
<Window x:Class="AdvGenPriceComparer.WPF.Views.PriceForecastWindow"
        Title="Price Forecast & Analysis" Height="700" Width="1000">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <Border Grid.Row="0" Background="{DynamicResource SystemAccentColorBrush}" Padding="20,15">
            <StackPanel>
                <TextBlock Text="Price Forecast & Analysis" FontSize="24" FontWeight="Bold" Foreground="White"/>
                <TextBlock Text="AI-powered price predictions to help you buy at the best time"
                           Foreground="White" Opacity="0.9" Margin="0,5,0,0"/>
            </StackPanel>
        </Border>

        <!-- Content -->
        <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
            <StackPanel Margin="20">

                <!-- Item Selection -->
                <GroupBox Header="Select Item" Margin="0,0,0,20">
                    <Grid Margin="10">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>

                        <ComboBox x:Name="ItemComboBox"
                                  ItemsSource="{Binding Items}"
                                  DisplayMemberPath="Name"
                                  SelectedItem="{Binding SelectedItem}"/>

                        <Button Grid.Column="1" Content="Generate Forecast" Padding="15,5" Margin="10,0,0,0"
                                Command="{Binding GenerateForecastCommand}"/>
                    </Grid>
                </GroupBox>

                <!-- Current Price & Recommendation -->
                <Grid Margin="0,0,0,20">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>

                    <Border Grid.Column="0" Background="LightBlue" Padding="15" Margin="0,0,5,0">
                        <StackPanel>
                            <TextBlock Text="Current Price" FontWeight="SemiBold"/>
                            <TextBlock Text="{Binding CurrentPrice, StringFormat='${0:F2}'}" FontSize="32" FontWeight="Bold"/>
                            <TextBlock Text="{Binding LastUpdated, StringFormat='Updated: {0:d}'}" FontSize="10" Opacity="0.7"/>
                        </StackPanel>
                    </Border>

                    <Border Grid.Column="1" Background="LightGreen" Padding="15" Margin="5,0">
                        <StackPanel>
                            <TextBlock Text="Predicted Price (7 days)" FontWeight="SemiBold"/>
                            <TextBlock Text="{Binding PredictedPrice7Days, StringFormat='${0:F2}'}" FontSize="32" FontWeight="Bold"/>
                            <TextBlock Text="{Binding PriceTrend}" FontSize="12" Opacity="0.8"/>
                        </StackPanel>
                    </Border>

                    <Border Grid.Column="2" Background="LightYellow" Padding="15" Margin="5,0,0,0">
                        <StackPanel>
                            <TextBlock Text="Recommendation" FontWeight="SemiBold"/>
                            <TextBlock Text="{Binding BuyingRecommendation}" FontSize="24" FontWeight="Bold"/>
                            <TextBlock Text="{Binding RecommendationReason}" FontSize="11" TextWrapping="Wrap" Opacity="0.8"/>
                        </StackPanel>
                    </Border>
                </Grid>

                <!-- Price Chart -->
                <GroupBox Header="Price Forecast Chart (30 Days)" Margin="0,0,0,20">
                    <lvc:CartesianChart Series="{Binding ChartSeries}"
                                        XAxes="{Binding XAxes}"
                                        YAxes="{Binding YAxes}"
                                        Height="300" Margin="10"/>
                </GroupBox>

                <!-- Anomaly Detection Results -->
                <GroupBox Header="Price Anomalies Detected" Visibility="{Binding HasAnomalies, Converter={StaticResource BooleanToVisibilityConverter}}">
                    <DataGrid ItemsSource="{Binding Anomalies}" AutoGenerateColumns="False" Margin="10">
                        <DataGrid.Columns>
                            <DataGridTextColumn Header="Date" Binding="{Binding Date, StringFormat='d'}" Width="100"/>
                            <DataGridTextColumn Header="Price" Binding="{Binding ActualPrice, StringFormat='${0:F2}'}" Width="80"/>
                            <DataGridTextColumn Header="Expected" Binding="{Binding ExpectedPrice, StringFormat='${0:F2}'}" Width="80"/>
                            <DataGridTextColumn Header="Type" Binding="{Binding Type}" Width="120"/>
                            <DataGridTextColumn Header="Description" Binding="{Binding Description}" Width="*"/>
                        </DataGrid.Columns>
                    </DataGrid>
                </GroupBox>

                <!-- Optimal Buying Date -->
                <Border Background="#E3F2FD" Padding="20" Margin="0,20,0,0" Visibility="{Binding HasOptimalDate, Converter={StaticResource BooleanToVisibilityConverter}}">
                    <StackPanel>
                        <TextBlock Text="💡 Optimal Buying Date" FontSize="18" FontWeight="Bold" Margin="0,0,0,10"/>
                        <TextBlock FontSize="14">
                            <Run Text="Based on price forecasting, the best time to buy "/>
                            <Run Text="{Binding SelectedItem.Name}" FontWeight="Bold"/>
                            <Run Text=" is on "/>
                            <Run Text="{Binding OptimalBuyingDate, StringFormat='dddd, MMMM d'}" FontWeight="Bold" Foreground="Green"/>
                            <Run Text=" at an estimated price of "/>
                            <Run Text="{Binding OptimalBuyingPrice, StringFormat='${0:F2}'}" FontWeight="Bold" Foreground="Green"/>
                        </TextBlock>

                        <Button Content="Set Price Alert" Margin="0,15,0,0" Padding="15,8"
                                Command="{Binding SetPriceAlertCommand}"/>
                    </StackPanel>
                </Border>

            </StackPanel>
        </ScrollViewer>

        <!-- Footer -->
        <Border Grid.Row="2" Background="{DynamicResource SystemAltMediumColor}" Padding="15">
            <TextBlock Text="Price predictions are estimates based on historical data and ML algorithms. Actual prices may vary."
                       FontSize="11" Opacity="0.7" TextAlignment="Center"/>
        </Border>
    </Grid>
</Window>
```

### 11.7 ViewModel for Price Forecasting

**PriceForecastViewModel.cs:**
```csharp
public class PriceForecastViewModel : ViewModelBase
{
    private readonly PriceForecastingService _forecastingService;
    private readonly PriceAnomalyDetectionService _anomalyService;
    private readonly IGroceryDataService _groceryData;
    private readonly ILoggerService _logger;

    public ObservableCollection<Item> Items { get; set; }
    public Item SelectedItem { get; set; }
    public List<PriceForecast> Forecasts { get; set; }
    public List<PriceAnomaly> Anomalies { get; set; }

    public decimal CurrentPrice { get; set; }
    public DateTime LastUpdated { get; set; }
    public decimal PredictedPrice7Days { get; set; }
    public string PriceTrend { get; set; }
    public string BuyingRecommendation { get; set; }
    public string RecommendationReason { get; set; }
    public DateTime OptimalBuyingDate { get; set; }
    public decimal OptimalBuyingPrice { get; set; }

    public bool HasAnomalies => Anomalies?.Any() ?? false;
    public bool HasOptimalDate => OptimalBuyingDate > DateTime.Now;

    // Chart data for LiveCharts
    public ISeries[] ChartSeries { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    public ICommand GenerateForecastCommand { get; }
    public ICommand SetPriceAlertCommand { get; }

    public PriceForecastViewModel(
        PriceForecastingService forecastingService,
        PriceAnomalyDetectionService anomalyService,
        IGroceryDataService groceryData,
        ILoggerService logger)
    {
        _forecastingService = forecastingService;
        _anomalyService = anomalyService;
        _groceryData = groceryData;
        _logger = logger;

        GenerateForecastCommand = new RelayCommand(async () => await GenerateForecastAsync());
        SetPriceAlertCommand = new RelayCommand(SetPriceAlert);

        LoadItems();
    }

    private async void LoadItems()
    {
        var items = await _groceryData.Items.GetAllAsync();
        Items = new ObservableCollection<Item>(items);
    }

    private async Task GenerateForecastAsync()
    {
        if (SelectedItem == null) return;

        _logger.LogInfo($"Generating forecast for {SelectedItem.Name}");

        // Get current price
        var latestPrice = await _groceryData.PriceRecords.GetLatestPriceAsync(SelectedItem.Id);
        CurrentPrice = latestPrice?.Price ?? 0;
        LastUpdated = latestPrice?.DateRecorded ?? DateTime.Now;

        // Generate forecast
        Forecasts = await _forecastingService.ForecastPricesAsync(SelectedItem.Id, 30);

        if (Forecasts.Any())
        {
            var forecast7Days = Forecasts.FirstOrDefault(f => f.ForecastDate == DateTime.Now.AddDays(7));
            if (forecast7Days != null)
            {
                PredictedPrice7Days = (decimal)forecast7Days.PredictedPrice;
                PriceTrend = GetTrendDescription(forecast7Days.Trend);
                BuyingRecommendation = GetRecommendationText(forecast7Days.Recommendation);
                RecommendationReason = GetRecommendationReason(forecast7Days);
            }

            // Get optimal buying date
            var (date, price) = await _forecastingService.GetOptimalBuyingDateAsync(SelectedItem.Id);
            OptimalBuyingDate = date;
            OptimalBuyingPrice = (decimal)price;

            // Update chart
            UpdateChart();
        }

        // Detect anomalies
        var priceHistory = await _groceryData.PriceRecords.GetPriceHistoryAsync(
            SelectedItem.Id,
            DateTime.Now.AddMonths(-3));

        var historyData = priceHistory.Select(p => new PriceHistoryData
        {
            Date = p.DateRecorded,
            Price = (float)p.Price,
            IsOnSale = p.IsOnSale
        }).ToList();

        Anomalies = _anomalyService.DetectAnomalies(SelectedItem.Id, historyData);

        OnPropertyChanged(nameof(HasAnomalies));
        OnPropertyChanged(nameof(HasOptimalDate));
    }

    private void UpdateChart()
    {
        // Historical prices
        var historicalSeries = new LineSeries<decimal>
        {
            Name = "Historical Price",
            Values = Forecasts.Take(7).Select(f => (decimal)f.PredictedPrice).ToArray()
        };

        // Forecasted prices
        var forecastSeries = new LineSeries<decimal>
        {
            Name = "Forecast",
            Values = Forecasts.Select(f => (decimal)f.PredictedPrice).ToArray(),
            Stroke = new SolidColorPaint(SKColors.Orange),
            GeometryStroke = new SolidColorPaint(SKColors.Orange),
            LineSmoothness = 0.5
        };

        ChartSeries = new ISeries[] { historicalSeries, forecastSeries };

        XAxes = new[]
        {
            new Axis
            {
                Name = "Date",
                Labels = Forecasts.Select(f => f.ForecastDate.ToString("MMM dd")).ToArray()
            }
        };

        YAxes = new[]
        {
            new Axis
            {
                Name = "Price ($)",
                MinLimit = (double)Forecasts.Min(f => f.LowerBound) * 0.95
            }
        };
    }

    private string GetTrendDescription(PriceTrend trend)
    {
        return trend switch
        {
            PriceTrend.Rising => "📈 Price Rising",
            PriceTrend.Falling => "📉 Price Falling",
            _ => "➡️ Price Stable"
        };
    }

    private string GetRecommendationText(BuyingRecommendation recommendation)
    {
        return recommendation switch
        {
            BuyingRecommendation.BuyNow => "✅ BUY NOW",
            BuyingRecommendation.Wait => "⏳ WAIT",
            BuyingRecommendation.AvoidHighPrice => "❌ AVOID",
            _ => "ℹ️ NORMAL"
        };
    }

    private string GetRecommendationReason(PriceForecast forecast)
    {
        return forecast.Recommendation switch
        {
            BuyingRecommendation.BuyNow => "Price is at or near historical low",
            BuyingRecommendation.Wait => "Price expected to drop in coming days",
            BuyingRecommendation.AvoidHighPrice => "Price is unusually high right now",
            _ => "No significant price movement expected"
        };
    }

    private void SetPriceAlert()
    {
        // Implementation for setting price alert
        _logger.LogInfo($"Price alert set for {SelectedItem.Name} at ${OptimalBuyingPrice:F2}");
    }
}
```

### 11.8 Implementation Checklist

- [ ] Extend `AdvGenPriceComparer.ML` project with forecasting services
- [ ] Install `Microsoft.ML.TimeSeries` NuGet package
- [ ] Define `PriceHistoryData`, `PriceForecast`, `PriceAnomaly` models
- [ ] Implement `PriceForecastingService`
  - [ ] Train SSA (Singular Spectrum Analysis) model
  - [ ] Generate price forecasts
  - [ ] Determine price trends
  - [ ] Calculate optimal buying dates
- [ ] Implement `PriceAnomalyDetectionService`
  - [ ] Train spike detection model
  - [ ] Detect price anomalies
  - [ ] Identify illusory discounts
- [ ] Create `PriceForecastWindow.xaml` UI
- [ ] Create `PriceForecastViewModel`
- [ ] Integrate LiveCharts for price visualization
- [ ] Add menu item to open Price Forecast window
- [ ] Test forecasting with real historical data
- [ ] Implement price alert system
- [ ] Document forecasting accuracy and limitations

### 11.9 Configuration in App.xaml.cs

```csharp
// ML Services - Forecasting
var forecastModelPath = Path.Combine(appDataPath, "MLModels", "price_forecast_model.zip");
var anomalyModelPath = Path.Combine(appDataPath, "MLModels", "anomaly_detection_model.zip");

services.AddSingleton<PriceForecastingService>(provider =>
    new PriceForecastingService(
        forecastModelPath,
        provider.GetRequiredService<IPriceRecordRepository>(),
        provider.GetRequiredService<ILoggerService>()));

services.AddSingleton<PriceAnomalyDetectionService>(provider =>
    new PriceAnomalyDetectionService(
        anomalyModelPath,
        provider.GetRequiredService<ILoggerService>()));

services.AddTransient<PriceForecastViewModel>();
```

### 11.10 Expected Performance

**Forecasting Requirements:**
- Minimum 30 days of price history per item
- Recommended 90+ days for accurate predictions
- Training time: 5-30 seconds depending on data size
- Forecast generation: <2 seconds for 30-day forecast

**Accuracy Metrics:**
- Expected MAPE (Mean Absolute Percentage Error): 5-15%
- Confidence interval: 95%
- Trend detection accuracy: 75-85%
- Anomaly detection precision: 80-90%

### 11.11 Business Value

**For Consumers:**
- Save money by buying at optimal times
- Identify fake "sale" prices (illusory discounts)
- Plan shopping based on price trends
- Receive alerts for genuine price drops

**For Fighting Illusory Discounts:**
- Detect when "sale" prices are actually normal or high
- Track historical pricing patterns
- Identify suspicious pricing behavior
- Build evidence of misleading discount practices

### 11.12 Use Cases

1. **Smart Shopping Assistant**
   - "Should I buy this now or wait?"
   - "When will the price drop?"
   - "Is this sale genuine?"

2. **Budget Planning**
   - "What will my grocery bill be next month?"
   - "When should I stock up on staples?"

3. **Discount Verification**
   - "Is this 50% off claim real?"
   - "Has this price actually changed?"

4. **Price Tracking**
   - "Show me price history and trends"
   - "Alert me when price drops below $X"

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
- **Phase 10 (Database Provider Selection):** 2-3 days
- **Phase 11 (ML.NET Price Prediction):** 3-4 days
- **Total Full Implementation:** 35-45 days

---

## 💬 Phase 12: Ollama Chat Interface for Natural Language Price Queries

### 12.1 Overview
Integrate Ollama (open-source LLM) to provide a natural language chat interface for querying grocery prices. Users can ask questions in plain English, and the LLM will route queries to LiteDB or AdvGenNoSqlServer to fetch and present price information.

**Key Objectives:**
- Natural language price queries ("What's the price of milk?", "Find cheapest bread")
- Intelligent query routing to appropriate database (LiteDB or AdvGenNoSqlServer)
- Context-aware conversations
- Product recommendations based on budget
- Price comparisons across stores
- Historical price insights

### 12.2 Ollama Setup

**Ollama Installation:**
```bash
# Windows installation
winget install Ollama.Ollama

# Or download from: https://ollama.com/download

# Pull recommended model
ollama pull mistral  # 7B parameter model, good balance of speed/accuracy
# Alternative models:
# ollama pull llama2  # Meta's Llama 2
# ollama pull codellama  # Better for structured queries
# ollama pull phi  # Smaller, faster model
```

**Project Dependencies:**
```xml
<PackageReference Include="OllamaSharp" Version="2.0.0" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />
```

### 12.3 Project Structure

```
AdvGenPriceComparer.Chat/
├── AdvGenPriceComparer.Chat.csproj
├── Models/
│   ├── ChatMessage.cs
│   ├── QueryIntent.cs
│   ├── DatabaseQuery.cs
│   └── ChatResponse.cs
├── Services/
│   ├── IOllamaService.cs
│   ├── OllamaService.cs
│   ├── QueryRouterService.cs
│   ├── IntentRecognitionService.cs
│   └── ResponseFormatterService.cs
└── Prompts/
    └── SystemPrompts.cs
```

### 12.4 Data Models

**ChatMessage.cs:**
```csharp
public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public MessageRole Role { get; set; }
    public string Content { get; set; }
    public List<Item> AttachedItems { get; set; } = new();
    public List<PriceRecord> AttachedPrices { get; set; } = new();
}

public enum MessageRole
{
    User,
    Assistant,
    System
}
```

**QueryIntent.cs:**
```csharp
public class QueryIntent
{
    public QueryType Type { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public string Store { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinPrice { get; set; }
    public bool OnSaleOnly { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public ComparisonType? Comparison { get; set; }
    public int? Limit { get; set; } = 10;
}

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
    Budget Query,          // "What can I buy for $50?"
    Unknown
}

public enum ComparisonType
{
    Cheaper,
    MoreExpensive,
    SimilarPrice
}
```

**DatabaseQuery.cs:**
```csharp
public class DatabaseQuery
{
    public DatabaseTarget Target { get; set; }  // LiteDB or AdvGenNoSqlServer
    public string Query { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public QueryIntent Intent { get; set; }
}

public enum DatabaseTarget
{
    LiteDB,
    AdvGenNoSqlServer,
    Both  // Query both and merge results
}
```

### 12.5 Ollama Service

**OllamaService.cs:**
```csharp
public class OllamaService : IOllamaService
{
    private readonly OllamaApiClient _ollama;
    private readonly ILoggerService _logger;
    private readonly List<ChatMessage> _conversationHistory = new();
    private const string MODEL = "mistral";

    public OllamaService(ILoggerService logger)
    {
        _logger = logger;
        _ollama = new OllamaApiClient("http://localhost:11434");
    }

    /// <summary>
    /// Send a chat message and get response
    /// </summary>
    public async Task<string> ChatAsync(string userMessage, string systemPrompt = null)
    {
        try
        {
            // Add user message to history
            _conversationHistory.Add(new ChatMessage
            {
                Role = MessageRole.User,
                Content = userMessage
            });

            // Build chat request
            var messages = new List<Message>();

            // Add system prompt if provided
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(new Message
                {
                    Role = "system",
                    Content = systemPrompt
                });
            }

            // Add conversation history (last 10 messages for context)
            var recentHistory = _conversationHistory.TakeLast(10);
            foreach (var msg in recentHistory)
            {
                messages.Add(new Message
                {
                    Role = msg.Role == MessageRole.User ? "user" : "assistant",
                    Content = msg.Content
                });
            }

            // Send to Ollama
            var chatRequest = new ChatRequest
            {
                Model = MODEL,
                Messages = messages,
                Stream = false
            };

            var response = await _ollama.SendChatAsync(chatRequest);

            // Add assistant response to history
            var assistantMessage = response.Message.Content;
            _conversationHistory.Add(new ChatMessage
            {
                Role = MessageRole.Assistant,
                Content = assistantMessage
            });

            _logger.LogInfo($"Chat response received: {assistantMessage.Substring(0, Math.Min(50, assistantMessage.Length))}...");

            return assistantMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError("Ollama chat error", ex);
            return "I'm sorry, I encountered an error processing your request. Please try again.";
        }
    }

    /// <summary>
    /// Extract structured intent from natural language query
    /// </summary>
    public async Task<QueryIntent> ExtractIntentAsync(string userQuery)
    {
        var systemPrompt = SystemPrompts.IntentExtractionPrompt;

        var prompt = $@"Extract intent from this grocery price query:
User Query: ""{userQuery}""

Respond ONLY with valid JSON matching this schema:
{{
    ""queryType"": ""PriceQuery|PriceComparison|CheapestItem|ItemsInCategory|ItemsOnSale|PriceHistory|BestDeal|StoreInventory|BudgetQuery"",
    ""productName"": ""product name or null"",
    ""category"": ""category or null"",
    ""store"": ""store name or null"",
    ""maxPrice"": number or null,
    ""minPrice"": number or null,
    ""onSaleOnly"": boolean,
    ""dateFrom"": ""ISO date or null"",
    ""dateTo"": ""ISO date or null"",
    ""comparison"": ""Cheaper|MoreExpensive|SimilarPrice or null"",
    ""limit"": number (default 10)
}}";

        var response = await ChatAsync(prompt, systemPrompt);

        try
        {
            // Extract JSON from response (handle if LLM adds extra text)
            var jsonStart = response.IndexOf("{");
            var jsonEnd = response.LastIndexOf("}") + 1;
            var json = response.Substring(jsonStart, jsonEnd - jsonStart);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var intent = JsonSerializer.Deserialize<QueryIntent>(json, options);
            return intent ?? new QueryIntent { Type = QueryType.Unknown };
        }
        catch (Exception ex)
        {
            _logger.LogError("Intent extraction failed", ex);
            return new QueryIntent { Type = QueryType.Unknown };
        }
    }

    /// <summary>
    /// Generate natural language response from query results
    /// </summary>
    public async Task<string> GenerateResponseAsync(
        QueryIntent intent,
        List<Item> items,
        List<PriceRecord> priceRecords)
    {
        var dataContext = FormatDataContext(items, priceRecords);

        var prompt = $@"Generate a helpful, conversational response for this grocery price query.

Query Intent: {intent.Type}
Product: {intent.ProductName ?? "N/A"}
Store: {intent.Store ?? "N/A"}

Data Retrieved:
{dataContext}

Generate a natural, friendly response that:
1. Directly answers the user's question
2. Highlights the most relevant information
3. Includes specific prices and product names
4. Suggests alternatives if appropriate
5. Keeps response concise (2-3 sentences)";

        return await ChatAsync(prompt, SystemPrompts.ResponseGenerationPrompt);
    }

    public void ClearHistory()
    {
        _conversationHistory.Clear();
    }

    private string FormatDataContext(List<Item> items, List<PriceRecord> priceRecords)
    {
        var sb = new StringBuilder();

        if (items?.Any() == true)
        {
            sb.AppendLine("Items Found:");
            foreach (var item in items.Take(10))
            {
                var priceRecord = priceRecords?.FirstOrDefault(p => p.ItemId == item.Id);
                var price = priceRecord?.Price ?? item.Price;
                var store = priceRecord?.Place?.Name ?? item.StoreName ?? "Unknown";

                sb.AppendLine($"- {item.Name} ({item.Brand ?? "Generic"}) - ${price:F2} at {store}");
            }
        }

        if (priceRecords?.Any() == true && items?.Count == 1)
        {
            sb.AppendLine("\nPrice History:");
            foreach (var record in priceRecords.OrderByDescending(p => p.DateRecorded).Take(5))
            {
                sb.AppendLine($"- ${record.Price:F2} on {record.DateRecorded:d} at {record.Place?.Name}");
            }
        }

        return sb.ToString();
    }
}
```

### 12.6 Query Router Service

**QueryRouterService.cs:**
```csharp
public class QueryRouterService
{
    private readonly IDatabaseProvider _databaseProvider;
    private readonly IGroceryDataService _groceryData;
    private readonly ILoggerService _logger;

    public QueryRouterService(
        IDatabaseProvider databaseProvider,
        IGroceryDataService groceryData,
        ILoggerService logger)
    {
        _databaseProvider = databaseProvider;
        _groceryData = groceryData;
        _logger = logger;
    }

    /// <summary>
    /// Route query to appropriate database and execute
    /// </summary>
    public async Task<(List<Item> Items, List<PriceRecord> Prices)> ExecuteQueryAsync(QueryIntent intent)
    {
        _logger.LogInfo($"Executing query: {intent.Type}");

        return intent.Type switch
        {
            QueryType.PriceQuery => await HandlePriceQuery(intent),
            QueryType.PriceComparison => await HandlePriceComparison(intent),
            QueryType.CheapestItem => await HandleCheapestItem(intent),
            QueryType.ItemsInCategory => await HandleItemsInCategory(intent),
            QueryType.ItemsOnSale => await HandleItemsOnSale(intent),
            QueryType.PriceHistory => await HandlePriceHistory(intent),
            QueryType.BestDeal => await HandleBestDeal(intent),
            QueryType.StoreInventory => await HandleStoreInventory(intent),
            QueryType.BudgetQuery => await HandleBudgetQuery(intent),
            _ => (new List<Item>(), new List<PriceRecord>())
        };
    }

    private async Task<(List<Item>, List<PriceRecord>)> HandlePriceQuery(QueryIntent intent)
    {
        // Find item by name
        var items = await _groceryData.Items.SearchByNameAsync(intent.ProductName);

        // Filter by store if specified
        if (!string.IsNullOrEmpty(intent.Store))
        {
            items = items.Where(i =>
                i.StoreName?.Contains(intent.Store, StringComparison.OrdinalIgnoreCase) == true).ToList();
        }

        // Get latest prices
        var priceRecords = new List<PriceRecord>();
        foreach (var item in items.Take(intent.Limit ?? 10))
        {
            var price = await _groceryData.PriceRecords.GetLatestPriceAsync(item.Id);
            if (price != null)
            {
                priceRecords.Add(price);
            }
        }

        return (items, priceRecords);
    }

    private async Task<(List<Item>, List<PriceRecord>)> HandlePriceComparison(QueryIntent intent)
    {
        // Find product
        var items = await _groceryData.Items.SearchByNameAsync(intent.ProductName);

        // Get prices from different stores
        var priceRecords = new List<PriceRecord>();
        foreach (var item in items)
        {
            var prices = await _groceryData.PriceRecords.GetPricesByItemAsync(item.Id);
            priceRecords.AddRange(prices.OrderByDescending(p => p.DateRecorded).Take(3));
        }

        return (items, priceRecords);
    }

    private async Task<(List<Item>, List<PriceRecord>)> HandleCheapestItem(QueryIntent intent)
    {
        List<Item> items;

        if (!string.IsNullOrEmpty(intent.ProductName))
        {
            items = await _groceryData.Items.SearchByNameAsync(intent.ProductName);
        }
        else if (!string.IsNullOrEmpty(intent.Category))
        {
            items = await _groceryData.Items.GetByCategoryAsync(intent.Category);
        }
        else
        {
            items = (await _groceryData.Items.GetAllAsync()).ToList();
        }

        // Sort by price
        items = items.OrderBy(i => i.Price).Take(intent.Limit ?? 10).ToList();

        // Get price records
        var priceRecords = new List<PriceRecord>();
        foreach (var item in items)
        {
            var price = await _groceryData.PriceRecords.GetLatestPriceAsync(item.Id);
            if (price != null)
            {
                priceRecords.Add(price);
            }
        }

        return (items, priceRecords);
    }

    private async Task<(List<Item>, List<PriceRecord>)> HandleItemsInCategory(QueryIntent intent)
    {
        var items = await _groceryData.Items.GetByCategoryAsync(intent.Category);
        items = items.Take(intent.Limit ?? 20).ToList();

        var priceRecords = new List<PriceRecord>();
        foreach (var item in items)
        {
            var price = await _groceryData.PriceRecords.GetLatestPriceAsync(item.Id);
            if (price != null)
            {
                priceRecords.Add(price);
            }
        }

        return (items, priceRecords);
    }

    private async Task<(List<Item>, List<PriceRecord>)> HandleItemsOnSale(QueryIntent intent)
    {
        var priceRecords = await _groceryData.PriceRecords.GetItemsOnSaleAsync();

        // Filter by store if specified
        if (!string.IsNullOrEmpty(intent.Store))
        {
            priceRecords = priceRecords.Where(p =>
                p.Place?.Name?.Contains(intent.Store, StringComparison.OrdinalIgnoreCase) == true).ToList();
        }

        priceRecords = priceRecords.Take(intent.Limit ?? 20).ToList();

        var items = new List<Item>();
        foreach (var record in priceRecords)
        {
            var item = await _groceryData.Items.GetByIdAsync(record.ItemId);
            if (item != null)
            {
                items.Add(item);
            }
        }

        return (items, priceRecords);
    }

    private async Task<(List<Item>, List<PriceRecord>)> HandlePriceHistory(QueryIntent intent)
    {
        var items = await _groceryData.Items.SearchByNameAsync(intent.ProductName);
        var item = items.FirstOrDefault();

        if (item == null)
        {
            return (new List<Item>(), new List<PriceRecord>());
        }

        var fromDate = intent.DateFrom ?? DateTime.Now.AddMonths(-3);
        var priceRecords = await _groceryData.PriceRecords.GetPriceHistoryAsync(item.Id, fromDate);

        return (new List<Item> { item }, priceRecords.ToList());
    }

    private async Task<(List<Item>, List<PriceRecord>)> HandleBestDeal(QueryIntent intent)
    {
        var priceRecords = await _groceryData.PriceRecords.GetItemsOnSaleAsync();

        // Calculate best deals (highest discount percentage)
        var bestDeals = priceRecords
            .Where(p => p.IsOnSale && p.OriginalPrice.HasValue && p.OriginalPrice > p.Price)
            .Select(p => new
            {
                Record = p,
                DiscountPercent = ((p.OriginalPrice.Value - p.Price) / p.OriginalPrice.Value) * 100
            })
            .OrderByDescending(x => x.DiscountPercent)
            .Take(intent.Limit ?? 10)
            .Select(x => x.Record)
            .ToList();

        var items = new List<Item>();
        foreach (var record in bestDeals)
        {
            var item = await _groceryData.Items.GetByIdAsync(record.ItemId);
            if (item != null)
            {
                items.Add(item);
            }
        }

        return (items, bestDeals);
    }

    private async Task<(List<Item>, List<PriceRecord>)> HandleStoreInventory(QueryIntent intent)
    {
        var place = (await _groceryData.Places.GetAllAsync())
            .FirstOrDefault(p => p.Name?.Contains(intent.Store, StringComparison.OrdinalIgnoreCase) == true);

        if (place == null)
        {
            return (new List<Item>(), new List<PriceRecord>());
        }

        var priceRecords = await _groceryData.PriceRecords.GetPricesByStoreAsync(place.Id);
        priceRecords = priceRecords.Take(intent.Limit ?? 50).ToList();

        var items = new List<Item>();
        foreach (var record in priceRecords)
        {
            var item = await _groceryData.Items.GetByIdAsync(record.ItemId);
            if (item != null)
            {
                items.Add(item);
            }
        }

        return (items, priceRecords.ToList());
    }

    private async Task<(List<Item>, List<PriceRecord>)> HandleBudgetQuery(QueryIntent intent)
    {
        var allItems = (await _groceryData.Items.GetAllAsync()).ToList();

        // Filter by max price
        if (intent.MaxPrice.HasValue)
        {
            allItems = allItems.Where(i => i.Price <= intent.MaxPrice.Value).ToList();
        }

        // Sort by price to maximize items within budget
        allItems = allItems.OrderBy(i => i.Price).Take(intent.Limit ?? 20).ToList();

        var priceRecords = new List<PriceRecord>();
        foreach (var item in allItems)
        {
            var price = await _groceryData.PriceRecords.GetLatestPriceAsync(item.Id);
            if (price != null)
            {
                priceRecords.Add(price);
            }
        }

        return (allItems, priceRecords);
    }
}
```

### 12.7 System Prompts

**SystemPrompts.cs:**
```csharp
public static class SystemPrompts
{
    public const string IntentExtractionPrompt = @"You are an intent extraction specialist for a grocery price comparison application.
Your task is to analyze user queries about grocery prices and extract structured intent information.
Be precise and extract all relevant filters (product name, store, price range, dates, etc.).
Always respond with valid JSON only, no additional text.";

    public const string ResponseGenerationPrompt = @"You are a helpful grocery shopping assistant.
Your responses should be:
- Friendly and conversational
- Focused on helping users save money
- Specific with prices and product names
- Concise (2-3 sentences typically)
- Actionable (suggest next steps when appropriate)

When presenting prices:
- Always include the store name
- Mention if items are on sale
- Highlight the best deals
- Compare prices when multiple options exist";

    public const string GeneralChatPrompt = @"You are a knowledgeable grocery shopping assistant integrated with a price comparison database.
You help users find the best grocery prices, compare products across stores, and make smart shopping decisions.

Available capabilities:
- Query current prices
- Compare prices across stores
- Find items on sale
- View price history
- Identify best deals
- Budget planning

Be helpful, friendly, and always reference specific data from the database when available.";
}
```

### 12.8 Chat UI - PriceChatWindow.xaml

**PriceChatWindow.xaml:**
```csharp
<Window x:Class="AdvGenPriceComparer.WPF.Views.PriceChatWindow"
        Title="Price Chat Assistant" Height="600" Width="800">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <Border Grid.Row="0" Background="{DynamicResource SystemAccentColorBrush}" Padding="20,15">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="💬 " FontSize="24"/>
                <StackPanel>
                    <TextBlock Text="Price Chat Assistant" FontSize="20" FontWeight="Bold" Foreground="White"/>
                    <TextBlock Text="Ask me about grocery prices in natural language" FontSize="11" Foreground="White" Opacity="0.9"/>
                </StackPanel>
                <Button Content="Clear Chat" Margin="Auto,0,0,0" Padding="10,5"
                        Command="{Binding ClearChatCommand}"/>
            </StackPanel>
        </Border>

        <!-- Chat Messages -->
        <ScrollViewer Grid.Row="1" x:Name="ChatScrollViewer" VerticalScrollBarVisibility="Auto">
            <ItemsControl ItemsSource="{Binding ChatMessages}" Margin="20">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Border Margin="0,5" Padding="15,10" CornerRadius="10"
                                Background="{Binding IsUserMessage, Converter={StaticResource MessageBackgroundConverter}}"
                                HorizontalAlignment="{Binding IsUserMessage, Converter={StaticResource MessageAlignmentConverter}}">
                            <StackPanel MaxWidth="500">
                                <TextBlock Text="{Binding Role}" FontWeight="Bold" FontSize="11" Opacity="0.7"/>
                                <TextBlock Text="{Binding Content}" TextWrapping="Wrap" Margin="0,5,0,0"/>
                                <TextBlock Text="{Binding Timestamp, StringFormat='HH:mm'}" FontSize="10" Opacity="0.5" Margin="0,5,0,0"/>

                                <!-- Show attached items if any -->
                                <ItemsControl ItemsSource="{Binding AttachedItems}" Margin="0,10,0,0"
                                              Visibility="{Binding HasAttachedItems, Converter={StaticResource BooleanToVisibilityConverter}}">
                                    <ItemsControl.ItemTemplate>
                                        <DataTemplate>
                                            <Border Background="#F5F5F5" Padding="10" Margin="0,2" CornerRadius="5">
                                                <StackPanel>
                                                    <TextBlock Text="{Binding Name}" FontWeight="SemiBold"/>
                                                    <TextBlock>
                                                        <Run Text="$"/><Run Text="{Binding Price, StringFormat=F2}"/>
                                                        <Run Text=" at "/>
                                                        <Run Text="{Binding StoreName}"/>
                                                    </TextBlock>
                                                </StackPanel>
                                            </Border>
                                        </DataTemplate>
                                    </ItemsControl.ItemTemplate>
                                </ItemsControl>
                            </StackPanel>
                        </Border>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </ScrollViewer>

        <!-- Input Area -->
        <Border Grid.Row="2" Background="#F8F8F8" Padding="20,15" BorderBrush="#DDD" BorderThickness="0,1,0,0">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <TextBox x:Name="MessageTextBox"
                         Grid.Column="0"
                         Text="{Binding UserMessage, UpdateSourceTrigger=PropertyChanged}"
                         AcceptsReturn="False"
                         VerticalContentAlignment="Center"
                         Padding="10"
                         FontSize="14"
                         BorderThickness="1"
                         BorderBrush="#CCC"
                         KeyDown="MessageTextBox_KeyDown"/>

                <Button Grid.Column="1" Content="Send" Padding="20,10" Margin="10,0,0,0"
                        Command="{Binding SendMessageCommand}"
                        IsEnabled="{Binding IsNotBusy}"
                        Background="{DynamicResource SystemAccentColorBrush}"
                        Foreground="White"/>
            </Grid>

            <!-- Suggested Queries -->
            <StackPanel Margin="0,10,0,0" Visibility="{Binding ShowSuggestions, Converter={StaticResource BooleanToVisibilityConverter}}">
                <TextBlock Text="Try asking:" FontSize="11" Opacity="0.7" Margin="0,0,0,5"/>
                <WrapPanel>
                    <Button Content="What's the price of milk?" Margin="0,0,5,5" Padding="8,4" FontSize="11"
                            Command="{Binding UseSuggestionCommand}" CommandParameter="What's the price of milk?"/>
                    <Button Content="Find cheapest bread" Margin="0,0,5,5" Padding="8,4" FontSize="11"
                            Command="{Binding UseSuggestionCommand}" CommandParameter="Find cheapest bread"/>
                    <Button Content="What's on sale at Coles?" Margin="0,0,5,5" Padding="8,4" FontSize="11"
                            Command="{Binding UseSuggestionCommand}" CommandParameter="What's on sale at Coles?"/>
                    <Button Content="Show milk price history" Margin="0,0,5,5" Padding="8,4" FontSize="11"
                            Command="{Binding UseSuggestionCommand}" CommandParameter="Show milk price history"/>
                </WrapPanel>
            </StackPanel>
        </Border>

        <!-- Loading Indicator -->
        <Border Grid.RowSpan="3" Background="#80000000"
                Visibility="{Binding IsBusy, Converter={StaticResource BooleanToVisibilityConverter}}">
            <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
                <TextBlock Text="🤔 Thinking..." FontSize="18" Foreground="White" FontWeight="Bold"/>
                <ProgressBar IsIndeterminate="True" Width="200" Height="4" Margin="0,10,0,0"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

### 12.9 Chat ViewModel

**PriceChatViewModel.cs:**
```csharp
public class PriceChatViewModel : ViewModelBase
{
    private readonly IOllamaService _ollamaService;
    private readonly QueryRouterService _queryRouter;
    private readonly ILoggerService _logger;

    public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new();
    public string UserMessage { get; set; }
    public bool IsBusy { get; set; }
    public bool IsNotBusy => !IsBusy;
    public bool ShowSuggestions => !ChatMessages.Any();

    public ICommand SendMessageCommand { get; }
    public ICommand ClearChatCommand { get; }
    public ICommand UseSuggestionCommand { get; }

    public PriceChatViewModel(
        IOllamaService ollamaService,
        QueryRouterService queryRouter,
        ILoggerService logger)
    {
        _ollamaService = ollamaService;
        _queryRouter = queryRouter;
        _logger = logger;

        SendMessageCommand = new RelayCommand(async () => await SendMessageAsync(), () => !string.IsNullOrWhiteSpace(UserMessage) && !IsBusy);
        ClearChatCommand = new RelayCommand(ClearChat);
        UseSuggestionCommand = new RelayCommand<string>(async (suggestion) => await UseSuggestionAsync(suggestion));

        // Welcome message
        AddSystemMessage("Hello! I'm your grocery price assistant. Ask me about prices, compare stores, or find the best deals!");
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserMessage)) return;

        var userMsg = UserMessage;
        UserMessage = string.Empty;
        OnPropertyChanged(nameof(UserMessage));

        // Add user message
        AddUserMessage(userMsg);

        IsBusy = true;
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(IsNotBusy));

        try
        {
            // Extract intent
            var intent = await _ollamaService.ExtractIntentAsync(userMsg);
            _logger.LogInfo($"Extracted intent: {intent.Type}");

            // Execute database query
            var (items, prices) = await _queryRouter.ExecuteQueryAsync(intent);

            // Generate response
            var response = await _ollamaService.GenerateResponseAsync(intent, items, prices);

            // Add assistant message with attached data
            var assistantMsg = new ChatMessage
            {
                Role = MessageRole.Assistant,
                Content = response,
                AttachedItems = items,
                AttachedPrices = prices
            };

            ChatMessages.Add(assistantMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError("Chat error", ex);
            AddSystemMessage("Sorry, I encountered an error processing your request. Please try again.");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    private async Task UseSuggestionAsync(string suggestion)
    {
        UserMessage = suggestion;
        OnPropertyChanged(nameof(UserMessage));
        await SendMessageAsync();
    }

    private void ClearChat()
    {
        ChatMessages.Clear();
        _ollamaService.ClearHistory();
        AddSystemMessage("Chat cleared. How can I help you?");
        OnPropertyChanged(nameof(ShowSuggestions));
    }

    private void AddUserMessage(string content)
    {
        ChatMessages.Add(new ChatMessage
        {
            Role = MessageRole.User,
            Content = content
        });
    }

    private void AddSystemMessage(string content)
    {
        ChatMessages.Add(new ChatMessage
        {
            Role = MessageRole.System,
            Content = content
        });
    }
}
```

### 12.10 Implementation Checklist

- [ ] Install Ollama on development machine
- [ ] Pull recommended model (Mistral 7B)
- [ ] Create `AdvGenPriceComparer.Chat` project
- [ ] Install `OllamaSharp` NuGet package
- [ ] Define chat models (`ChatMessage`, `QueryIntent`, `DatabaseQuery`)
- [ ] Implement `OllamaService` for LLM communication
- [ ] Implement `QueryRouterService` for database queries
- [ ] Create system prompts for intent extraction and response generation
- [ ] Build `PriceChatWindow.xaml` UI
- [ ] Implement `PriceChatViewModel`
- [ ] Add menu item to open Chat Assistant
- [ ] Test with various natural language queries
- [ ] Optimize prompts for better intent recognition
- [ ] Document supported query types

### 12.11 Configuration in App.xaml.cs

```csharp
// Chat Services
services.AddSingleton<IOllamaService, OllamaService>();
services.AddSingleton<QueryRouterService>();
services.AddTransient<PriceChatViewModel>();
```

### 12.12 Example Queries

**Price Queries:**
- "What's the price of milk at Coles?"
- "How much does bread cost?"
- "Show me milk prices"

**Comparisons:**
- "Compare milk prices between Coles and Woolworths"
- "Which store has cheaper bread?"
- "Price difference for eggs between stores"

**Finding Deals:**
- "What's the cheapest bread?"
- "Find the best deals this week"
- "Show me all items on sale"
- "What's on sale at Woolworths?"

**Category Queries:**
- "Show me all dairy products"
- "What vegetables are available?"
- "List all beverages under $5"

**Price History:**
- "Show milk price history"
- "How has the price of bread changed?"
- "Price trends for eggs over the last month"

**Budget Planning:**
- "What can I buy for $50?"
- "Show me items under $10"
- "Budget-friendly groceries"

### 12.13 Ollama Model Recommendations

**Mistral 7B (Recommended):**
- Best balance of speed and accuracy
- Good at structured output (JSON)
- ~4GB RAM usage
- Response time: 1-3 seconds

**Llama 2 7B:**
- Slightly slower than Mistral
- Very good accuracy
- ~4GB RAM usage

**Phi 2:**
- Faster responses
- Smaller model (2.7B parameters)
- Good for simple queries
- ~2GB RAM usage

**CodeLlama:**
- Excellent for structured queries
- Good JSON generation
- ~4GB RAM usage

### 12.14 Performance Considerations

**Expected Performance:**
- Intent extraction: 1-2 seconds
- Database query: <500ms
- Response generation: 1-2 seconds
- Total response time: 2-4 seconds

**Optimization Tips:**
- Cache frequently asked queries
- Use smaller models for simple queries
- Implement query result caching
- Batch database queries when possible

### 12.15 Privacy & Security

**Data Privacy:**
- All LLM processing happens locally (Ollama runs on localhost)
- No data sent to external APIs
- Chat history stored locally
- User control over data retention

**Security:**
- Input sanitization for database queries
- Parameterized queries to prevent injection
- Rate limiting for API calls
- Error handling to prevent information leakage

---

### 12.16 Standard Price Query Language (SPQL) - JSON Specification

#### Overview
The Standard Price Query Language (SPQL) provides a unified JSON-based interface for querying grocery price data across LiteDB, AdvGenNoSqlServer, and future data sources. This specification ensures consistency between the Ollama chat interface, direct API calls, and programmatic access.

#### Design Principles
1. **Simplicity**: Easy to read, write, and understand
2. **Consistency**: Same structure across all query types
3. **Extensibility**: Easy to add new query types and filters
4. **Type Safety**: Clear data types for all fields
5. **Versioning**: Support for future spec versions

---

#### Core Query Structure

**Base Query Format:**
```json
{
  "version": "1.0",
  "queryType": "string",
  "target": "LiteDB|AdvGenNoSqlServer|Both",
  "filters": {
    "product": { },
    "price": { },
    "store": { },
    "time": { },
    "category": { }
  },
  "options": {
    "limit": 10,
    "offset": 0,
    "sortBy": "price",
    "sortOrder": "asc|desc",
    "includeHistory": false
  }
}
```

---

#### Query Types

##### 1. **Price Query** (`priceQuery`)
Get current price(s) for specific product(s).

```json
{
  "version": "1.0",
  "queryType": "priceQuery",
  "target": "Both",
  "filters": {
    "product": {
      "name": "Milk",
      "nameMatch": "contains|exact|startsWith",
      "brand": "Dairy Farmers",
      "barcode": "9300632123456"
    },
    "store": {
      "name": "Coles",
      "chain": "Coles",
      "suburb": "Chermside",
      "state": "QLD"
    }
  },
  "options": {
    "limit": 10,
    "includeHistory": false
  }
}
```

**Response:**
```json
{
  "version": "1.0",
  "queryType": "priceQuery",
  "timestamp": "2026-02-25T10:30:00Z",
  "itemCount": 3,
  "items": [
    {
      "itemId": "item_001",
      "name": "Milk Full Cream 2L",
      "brand": "Dairy Farmers",
      "category": "Dairy & Eggs",
      "barcode": "9300632123456",
      "currentPrice": {
        "price": 3.99,
        "currency": "AUD",
        "store": "Coles Chermside",
        "storeId": "place_001",
        "dateRecorded": "2026-02-25T08:00:00Z",
        "isOnSale": false,
        "originalPrice": null
      }
    }
  ]
}
```

---

##### 2. **Price Comparison** (`priceComparison`)
Compare prices across multiple stores.

```json
{
  "version": "1.0",
  "queryType": "priceComparison",
  "target": "Both",
  "filters": {
    "product": {
      "name": "Milk Full Cream 2L",
      "nameMatch": "exact"
    },
    "store": {
      "chains": ["Coles", "Woolworths", "IGA"],
      "state": "QLD"
    }
  },
  "options": {
    "sortBy": "price",
    "sortOrder": "asc"
  }
}
```

**Response:**
```json
{
  "version": "1.0",
  "queryType": "priceComparison",
  "timestamp": "2026-02-25T10:30:00Z",
  "product": "Milk Full Cream 2L",
  "comparison": [
    {
      "store": "Woolworths Chermside",
      "chain": "Woolworths",
      "price": 3.79,
      "isOnSale": true,
      "originalPrice": 4.50,
      "savings": 0.71,
      "savingsPercent": 15.8
    },
    {
      "store": "Coles Chermside",
      "chain": "Coles",
      "price": 3.99,
      "isOnSale": false
    },
    {
      "store": "IGA Chermside",
      "chain": "IGA",
      "price": 4.20,
      "isOnSale": false
    }
  ],
  "bestDeal": {
    "store": "Woolworths Chermside",
    "price": 3.79,
    "savings": 0.71
  }
}
```

---

##### 3. **Category Query** (`categoryQuery`)
Get all items in a category.

```json
{
  "version": "1.0",
  "queryType": "categoryQuery",
  "target": "LiteDB",
  "filters": {
    "category": {
      "name": "Dairy & Eggs",
      "subcategory": "Milk"
    },
    "price": {
      "min": 0,
      "max": 10.00
    },
    "store": {
      "chain": "Coles"
    }
  },
  "options": {
    "limit": 20,
    "sortBy": "price",
    "sortOrder": "asc"
  }
}
```

---

##### 4. **Sale Items Query** (`saleItemsQuery`)
Find items currently on sale.

```json
{
  "version": "1.0",
  "queryType": "saleItemsQuery",
  "target": "Both",
  "filters": {
    "store": {
      "chain": "Woolworths",
      "state": "QLD"
    },
    "price": {
      "minDiscount": 20,
      "minDiscountType": "percent"
    },
    "category": {
      "names": ["Dairy & Eggs", "Bakery", "Meat & Seafood"]
    }
  },
  "options": {
    "limit": 50,
    "sortBy": "discountPercent",
    "sortOrder": "desc"
  }
}
```

**Response:**
```json
{
  "version": "1.0",
  "queryType": "saleItemsQuery",
  "timestamp": "2026-02-25T10:30:00Z",
  "itemCount": 23,
  "items": [
    {
      "itemId": "item_042",
      "name": "Butter Unsalted 500g",
      "brand": "Western Star",
      "category": "Dairy & Eggs",
      "store": "Woolworths Chermside",
      "salePrice": 4.50,
      "originalPrice": 9.00,
      "savings": 4.50,
      "discountPercent": 50.0,
      "saleType": "Half Price",
      "validFrom": "2026-02-25",
      "validTo": "2026-03-03"
    }
  ]
}
```

---

##### 5. **Price History Query** (`priceHistoryQuery`)
Get historical price data for a product.

```json
{
  "version": "1.0",
  "queryType": "priceHistoryQuery",
  "target": "Both",
  "filters": {
    "product": {
      "name": "Milk Full Cream 2L",
      "brand": "Dairy Farmers"
    },
    "time": {
      "from": "2025-11-25T00:00:00Z",
      "to": "2026-02-25T23:59:59Z"
    },
    "store": {
      "chain": "Coles"
    }
  },
  "options": {
    "sortBy": "date",
    "sortOrder": "desc",
    "aggregation": "daily|weekly|monthly"
  }
}
```

**Response:**
```json
{
  "version": "1.0",
  "queryType": "priceHistoryQuery",
  "timestamp": "2026-02-25T10:30:00Z",
  "product": {
    "itemId": "item_001",
    "name": "Milk Full Cream 2L",
    "brand": "Dairy Farmers"
  },
  "history": [
    {
      "date": "2026-02-25",
      "price": 3.99,
      "store": "Coles Chermside",
      "isOnSale": false
    },
    {
      "date": "2026-02-18",
      "price": 3.99,
      "store": "Coles Chermside",
      "isOnSale": false
    },
    {
      "date": "2026-02-11",
      "price": 3.50,
      "store": "Coles Chermside",
      "isOnSale": true,
      "originalPrice": 4.50
    }
  ],
  "statistics": {
    "averagePrice": 3.82,
    "minPrice": 3.50,
    "maxPrice": 4.50,
    "currentPrice": 3.99,
    "trend": "stable",
    "volatility": 0.15
  }
}
```

---

##### 6. **Best Deals Query** (`bestDealsQuery`)
Find the best current deals.

```json
{
  "version": "1.0",
  "queryType": "bestDealsQuery",
  "target": "Both",
  "filters": {
    "price": {
      "minDiscount": 30,
      "minDiscountType": "percent"
    },
    "store": {
      "state": "QLD"
    },
    "category": {
      "exclude": ["Alcohol", "Tobacco"]
    }
  },
  "options": {
    "limit": 20,
    "sortBy": "discountPercent",
    "sortOrder": "desc",
    "verifyDiscount": true
  }
}
```

---

##### 7. **Budget Query** (`budgetQuery`)
Find items within a budget.

```json
{
  "version": "1.0",
  "queryType": "budgetQuery",
  "target": "LiteDB",
  "filters": {
    "price": {
      "totalBudget": 50.00,
      "maxItemPrice": 10.00
    },
    "category": {
      "names": ["Meat & Seafood", "Fruits & Vegetables", "Pantry Staples"]
    },
    "store": {
      "chain": "Coles"
    }
  },
  "options": {
    "limit": 100,
    "sortBy": "price",
    "sortOrder": "asc",
    "optimizeFor": "quantity|variety|nutrition"
  }
}
```

**Response:**
```json
{
  "version": "1.0",
  "queryType": "budgetQuery",
  "timestamp": "2026-02-25T10:30:00Z",
  "budget": 50.00,
  "totalCost": 48.75,
  "remaining": 1.25,
  "itemCount": 12,
  "items": [
    {
      "name": "Chicken Breast 1kg",
      "category": "Meat & Seafood",
      "price": 12.00,
      "store": "Coles"
    },
    {
      "name": "Tomatoes 1kg",
      "category": "Fruits & Vegetables",
      "price": 4.50,
      "store": "Coles"
    }
  ]
}
```

---

##### 8. **Store Inventory Query** (`storeInventoryQuery`)
Get available items at a specific store.

```json
{
  "version": "1.0",
  "queryType": "storeInventoryQuery",
  "target": "Both",
  "filters": {
    "store": {
      "name": "Coles Chermside",
      "chain": "Coles"
    },
    "category": {
      "names": ["Dairy & Eggs"]
    }
  },
  "options": {
    "limit": 100,
    "includeOutOfStock": false
  }
}
```

---

##### 9. **Cheapest Item Query** (`cheapestItemQuery`)
Find cheapest options for a product type.

```json
{
  "version": "1.0",
  "queryType": "cheapestItemQuery",
  "target": "Both",
  "filters": {
    "product": {
      "keywords": ["bread", "white"],
      "category": "Bakery"
    },
    "store": {
      "state": "QLD",
      "maxDistance": 10,
      "distanceUnit": "km",
      "fromLocation": {
        "suburb": "Chermside",
        "postcode": "4032"
      }
    }
  },
  "options": {
    "limit": 5,
    "includeUnitPrice": true
  }
}
```

**Response:**
```json
{
  "version": "1.0",
  "queryType": "cheapestItemQuery",
  "timestamp": "2026-02-25T10:30:00Z",
  "searchCriteria": "bread, white",
  "items": [
    {
      "itemId": "item_123",
      "name": "White Bread 650g",
      "brand": "Helga's",
      "price": 2.50,
      "unitPrice": 0.38,
      "unitPriceLabel": "per 100g",
      "store": "Woolworths Chermside",
      "distance": 2.3,
      "distanceUnit": "km"
    },
    {
      "itemId": "item_124",
      "name": "White Sliced Bread 700g",
      "brand": "TipTop",
      "price": 2.80,
      "unitPrice": 0.40,
      "unitPriceLabel": "per 100g",
      "store": "Coles Chermside",
      "distance": 1.8,
      "distanceUnit": "km"
    }
  ]
}
```

---

#### Filter Specifications

##### Product Filters
```json
{
  "product": {
    "name": "string",                    // Product name
    "nameMatch": "contains|exact|startsWith|regex",
    "brand": "string",                   // Brand name
    "barcode": "string",                 // EAN/UPC barcode
    "keywords": ["string"],              // Search keywords (OR logic)
    "excludeKeywords": ["string"],       // Exclude items containing these
    "packageSize": "string",             // e.g., "2L", "1kg", "500g"
    "packageSizeRange": {
      "min": "number",
      "max": "number",
      "unit": "L|kg|g|ml"
    }
  }
}
```

##### Price Filters
```json
{
  "price": {
    "min": 0.00,                         // Minimum price
    "max": 999.99,                       // Maximum price
    "currency": "AUD",                   // Currency code
    "minDiscount": 20,                   // Minimum discount amount/percent
    "minDiscountType": "amount|percent", // Discount type
    "maxUnitPrice": 5.00,                // Max price per unit (kg, L, etc.)
    "totalBudget": 100.00,               // Total budget constraint
    "maxItemPrice": 20.00                // Max price per item (for budget queries)
  }
}
```

##### Store Filters
```json
{
  "store": {
    "name": "string",                    // Exact store name
    "chain": "string",                   // Chain name: Coles, Woolworths, etc.
    "chains": ["string"],                // Multiple chains (OR logic)
    "suburb": "string",                  // Suburb name
    "state": "QLD|NSW|VIC|SA|WA|TAS|NT|ACT",
    "postcode": "string",                // Postcode
    "maxDistance": 10,                   // Maximum distance
    "distanceUnit": "km|miles",
    "fromLocation": {
      "suburb": "string",
      "postcode": "string",
      "latitude": -27.3853,
      "longitude": 153.0356
    }
  }
}
```

##### Time Filters
```json
{
  "time": {
    "from": "2026-01-01T00:00:00Z",      // ISO 8601 datetime
    "to": "2026-02-25T23:59:59Z",        // ISO 8601 datetime
    "validOn": "2026-02-25",             // Items valid on specific date
    "daysBack": 30,                      // Last N days (alternative to from/to)
    "includeExpired": false              // Include expired sales
  }
}
```

##### Category Filters
```json
{
  "category": {
    "name": "string",                    // Single category
    "names": ["string"],                 // Multiple categories (OR logic)
    "subcategory": "string",             // Subcategory within category
    "exclude": ["string"]                // Exclude these categories
  }
}
```

---

#### Options Specification

```json
{
  "options": {
    "limit": 10,                         // Max results (default: 10)
    "offset": 0,                         // Pagination offset (default: 0)
    "sortBy": "price|name|date|discount|distance",
    "sortOrder": "asc|desc",             // Sort direction (default: asc)
    "includeHistory": false,             // Include price history (default: false)
    "includeUnitPrice": true,            // Calculate unit prices (default: true)
    "includeOutOfStock": false,          // Include out of stock items
    "aggregation": "daily|weekly|monthly", // Time aggregation for history
    "optimizeFor": "price|quantity|variety|nutrition", // Budget optimization
    "verifyDiscount": true               // Verify discount against history
  }
}
```

---

#### Error Response Format

```json
{
  "version": "1.0",
  "error": true,
  "errorCode": "INVALID_QUERY|DATABASE_ERROR|NOT_FOUND|PERMISSION_DENIED",
  "message": "Human-readable error message",
  "details": {
    "field": "filters.product.name",
    "reason": "Field cannot be empty"
  },
  "timestamp": "2026-02-25T10:30:00Z"
}
```

---

#### Implementation Notes

1. **Backward Compatibility**: Always include `version` field to support future schema changes
2. **Validation**: Validate all queries against JSON schema before execution
3. **Security**: Sanitize all string inputs to prevent injection attacks
4. **Performance**: Index frequently queried fields (name, category, price, date)
5. **Caching**: Cache query results for common queries (configurable TTL)
6. **Logging**: Log all queries for analytics and debugging
7. **Rate Limiting**: Implement rate limiting to prevent abuse

---

#### JSON Schema Validation

**Query Schema:**
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["version", "queryType"],
  "properties": {
    "version": {
      "type": "string",
      "pattern": "^\\d+\\.\\d+$"
    },
    "queryType": {
      "type": "string",
      "enum": [
        "priceQuery",
        "priceComparison",
        "categoryQuery",
        "saleItemsQuery",
        "priceHistoryQuery",
        "bestDealsQuery",
        "budgetQuery",
        "storeInventoryQuery",
        "cheapestItemQuery"
      ]
    },
    "target": {
      "type": "string",
      "enum": ["LiteDB", "AdvGenNoSqlServer", "Both"],
      "default": "Both"
    },
    "filters": {
      "type": "object"
    },
    "options": {
      "type": "object"
    }
  }
}
```

---

#### Usage Example: Ollama Integration

```csharp
// Natural language query
var userQuery = "What's the cheapest milk at Coles?";

// LLM extracts intent and generates SPQL
var spqlQuery = await ollamaService.ExtractIntentToSPQLAsync(userQuery);
// Result:
// {
//   "version": "1.0",
//   "queryType": "cheapestItemQuery",
//   "target": "Both",
//   "filters": {
//     "product": { "keywords": ["milk"] },
//     "store": { "chain": "Coles" }
//   },
//   "options": { "limit": 5 }
// }

// Execute query
var results = await spqlExecutor.ExecuteAsync(spqlQuery);

// Generate natural language response
var response = await ollamaService.GenerateResponseAsync(results);
```

---

#### Benefits of SPQL

1. **Standardization**: Consistent query format across all components
2. **Interoperability**: Easy integration with external systems via JSON
3. **Documentation**: Self-documenting with clear field names
4. **Validation**: JSON schema validation ensures correctness
5. **Versioning**: Built-in version support for future compatibility
6. **Flexibility**: Extensible for new query types and filters
7. **Testing**: Easy to write test queries in JSON format
8. **API-Ready**: Can be used directly as REST API request body

---

### 12.17 P2P Data Exchange Format Specification

#### Overview
The AdvGenPriceComparer P2P network uses five interconnected JSON file formats for decentralized peer-to-peer data exchange. This format enables both active P2P nodes and static HTTP-hosted data sharing, supporting the distributed grocery price tracking network.

**Official Specification**: [JSON File Format Wiki](https://github.com/michaelleungadvgen/AdvGenPriceComparer/wiki/Json-File-Format)

#### Design Principles
1. **Decentralization**: No central authority required for data sharing
2. **Simplicity**: Easy to generate, parse, and validate
3. **Interoperability**: Works with both active P2P nodes and static file hosting
4. **Temporal Tracking**: All pricing data includes timestamps for historical analysis
5. **Extensibility**: Schema supports future additions without breaking changes

---

#### 1. Discovery.json - Peer Network Registry

Lists available servers sharing price information, enabling decentralized peer discovery.

**Purpose:**
- Enable peers to discover other price-sharing nodes
- Support both full P2P nodes and static file servers
- Track server availability and geographic coverage

**File Location:**
- Full peers: `http(s)://{server}/discovery.json`
- Static peers: `http(s)://{server}/data/discovery.json`

**Schema:**
```json
[
  {
    "id": "string (required)",
    "type": "full_peer|static_peer (required)",
    "address": "string (required)",
    "location": "string (optional)",
    "last_seen": "ISO 8601 timestamp (optional)",
    "last_updated": "ISO 8601 timestamp (optional)",
    "description": "string (optional)"
  }
]
```

**Example:**
```json
[
  {
    "id": "aus-qld-brisbane-01",
    "type": "full_peer",
    "address": "https://price.brisbane.example.com:8080",
    "location": "Brisbane, QLD, Australia",
    "last_seen": "2026-02-25T10:30:00Z",
    "description": "Brisbane grocery price sharing node"
  },
  {
    "id": "static-sydney-01",
    "type": "static_peer",
    "address": "https://pricedata.sydney.example.com/data",
    "location": "Sydney, NSW, Australia",
    "last_updated": "2026-02-25T08:00:00Z",
    "description": "Sydney static price data archive"
  }
]
```

**Field Descriptions:**
- `id`: Unique identifier for the server (convention: region-city-number)
- `type`:
  - `full_peer`: Active P2P node with real-time sync capabilities
  - `static_peer`: HTTP-hosted static JSON files, updated periodically
- `address`: Base URL for accessing peer data
- `location`: Human-readable geographic location
- `last_seen`: Last successful connection (for full_peer)
- `last_updated`: Last data update (for static_peer)
- `description`: Human-readable purpose/description

---

#### 2. Shop.json - Retail Location Registry

Stores information about physical and online retail locations (supermarkets).

**Purpose:**
- Maintain consistent store identifiers across the network
- Enable geographic filtering of price data
- Support multi-chain price comparison

**File Location:** `{address}/shop.json` or `{address}/data/shop.json`

**Schema:**
```json
[
  {
    "shopID": "string (required)",
    "shopName": "string (required)",
    "chain": "string (optional)",
    "location": "string (required)",
    "suburb": "string (optional)",
    "state": "string (optional)",
    "postcode": "string (optional)",
    "latitude": "number (optional)",
    "longitude": "number (optional)",
    "contact": "string (required)"
  }
]
```

**Example:**
```json
[
  {
    "shopID": "coles-chermside-qld",
    "shopName": "Coles Chermside",
    "chain": "Coles",
    "location": "Westfield Chermside, Gympie Rd, Chermside QLD 4032",
    "suburb": "Chermside",
    "state": "QLD",
    "postcode": "4032",
    "latitude": -27.3853,
    "longitude": 153.0356,
    "contact": "(07) 3123 4567"
  },
  {
    "shopID": "woolworths-toowong-qld",
    "shopName": "Woolworths Toowong",
    "chain": "Woolworths",
    "location": "52 Sherwood Rd, Toowong QLD 4066",
    "suburb": "Toowong",
    "state": "QLD",
    "postcode": "4066",
    "latitude": -27.4856,
    "longitude": 152.9897,
    "contact": "(07) 3371 2345"
  }
]
```

**Field Descriptions:**
- `shopID`: Unique identifier (convention: chain-suburb-state)
- `shopName`: Display name of the store
- `chain`: Supermarket chain (Coles, Woolworths, IGA, Aldi, Drakes, etc.)
- `location`: Full street address
- `suburb`, `state`, `postcode`: Geographic identifiers for filtering
- `latitude`, `longitude`: GPS coordinates for proximity searches
- `contact`: Phone number or contact information

---

#### 3. Goods.json - Product Catalog

Product catalog with detailed specifications and identifiers.

**Purpose:**
- Maintain consistent product identifiers across the network
- Enable accurate price comparisons across different stores
- Support product search and filtering

**File Location:** `{address}/goods.json` or `{address}/data/goods.json`

**Schema:**
```json
[
  {
    "productID": "string (required)",
    "productName": "string (required)",
    "brand": "string (optional)",
    "category": "string (required)",
    "description": "string (required)",
    "packageSize": "string (optional)",
    "barcode": "string (optional)",
    "unit": "string (optional)"
  }
]
```

**Example:**
```json
[
  {
    "productID": "milk-dairy-farmers-2l",
    "productName": "Milk Full Cream 2L",
    "brand": "Dairy Farmers",
    "category": "Dairy & Eggs",
    "description": "Fresh full cream milk",
    "packageSize": "2L",
    "barcode": "9300632123456",
    "unit": "each"
  },
  {
    "productID": "bread-wonder-white-700g",
    "productName": "Wonder White Sandwich Bread",
    "brand": "Wonder White",
    "category": "Bakery",
    "description": "Soft white sandwich bread",
    "packageSize": "700g",
    "barcode": "9310072012345",
    "unit": "each"
  }
]
```

**Field Descriptions:**
- `productID`: Unique identifier (convention: category-brand-size)
- `productName`: Display name of the product
- `brand`: Manufacturer or brand name
- `category`: Product category (must match standard categories)
- `description`: Brief product overview
- `packageSize`: Size specification (e.g., "2L", "500g", "6 pack")
- `barcode`: EAN/UPC barcode for precise matching
- `unit`: Pricing unit (each, kg, L, etc.)

---

#### 4. records.json - Price History Index

Index/manifest of available historical price files from static peers.

**Purpose:**
- Provide directory of available historical price data
- Enable efficient data synchronization
- Support incremental updates

**File Location:** `{address}/records.json` or `{address}/data/records.json`

**Schema:**
```json
{
  "generated_at": "ISO 8601 timestamp (required)",
  "price_records": [
    "string (filename)"
  ]
}
```

**Example:**
```json
{
  "generated_at": "2026-02-25T10:30:00Z",
  "price_records": [
    "price-2026-02-25.json",
    "price-2026-02-24.json",
    "price-2026-02-23.json",
    "price-2026-02-22.json",
    "price-2026-02-21.json"
  ]
}
```

**Field Descriptions:**
- `generated_at`: Timestamp when the index was last updated
- `price_records`: Array of available price file names (format: `price-{timestamp}.json`)

**Usage Pattern:**
```csharp
// Fetch records index
var recordsUrl = $"{peerAddress}/data/records.json";
var recordsIndex = await httpClient.GetFromJsonAsync<RecordsIndex>(recordsUrl);

// Determine which files we need
var lastSync = await GetLastSyncTimestampAsync();
var newFiles = recordsIndex.PriceRecords
    .Where(filename => ExtractDate(filename) > lastSync)
    .ToList();

// Download only new files
foreach (var filename in newFiles)
{
    var priceUrl = $"{peerAddress}/data/{filename}";
    var priceData = await httpClient.GetFromJsonAsync<PriceData>(priceUrl);
    await ImportPriceDataAsync(priceData);
}
```

---

#### 5. price-{timestamp}.json - Price Data

Actual pricing data with temporal markers for historical tracking.

**Purpose:**
- Store actual grocery prices with timestamps
- Support historical price analysis
- Enable illusory discount detection

**File Location:** `{address}/price-{timestamp}.json` or `{address}/data/price-{timestamp}.json`

**Filename Convention:** `price-YYYY-MM-DD.json` (one file per day)

**Schema:**
```json
{
  "timestamp": "ISO 8601 timestamp (required)",
  "source": "string (optional)",
  "region": "string (optional)",
  "prices": [
    {
      "shopID": "string (required)",
      "productID": "string (required)",
      "price": "number (required)",
      "currency": "string (required)",
      "isOnSale": "boolean (optional)",
      "originalPrice": "number (optional)",
      "saleDescription": "string (optional)",
      "validFrom": "ISO 8601 timestamp (optional)",
      "validTo": "ISO 8601 timestamp (optional)",
      "recordedAt": "ISO 8601 timestamp (optional)"
    }
  ]
}
```

**Example:**
```json
{
  "timestamp": "2026-02-25T10:30:00Z",
  "source": "brisbane-p2p-node-01",
  "region": "QLD",
  "prices": [
    {
      "shopID": "coles-chermside-qld",
      "productID": "milk-dairy-farmers-2l",
      "price": 3.99,
      "currency": "AUD",
      "isOnSale": false,
      "recordedAt": "2026-02-25T10:00:00Z"
    },
    {
      "shopID": "woolworths-toowong-qld",
      "productID": "milk-dairy-farmers-2l",
      "price": 3.79,
      "currency": "AUD",
      "isOnSale": true,
      "originalPrice": 4.50,
      "saleDescription": "50% Off - Special",
      "validFrom": "2026-02-25T00:00:00Z",
      "validTo": "2026-02-28T23:59:59Z",
      "recordedAt": "2026-02-25T10:15:00Z"
    },
    {
      "shopID": "aldi-kedron-qld",
      "productID": "bread-wonder-white-700g",
      "price": 2.49,
      "currency": "AUD",
      "isOnSale": false,
      "recordedAt": "2026-02-25T09:30:00Z"
    }
  ]
}
```

**Field Descriptions:**
- `timestamp`: When this price data file was generated
- `source`: Identifier of the node/system that recorded these prices
- `region`: Geographic region for this data (NSW, VIC, QLD, etc.)
- `prices`: Array of individual price records
  - `shopID`: References Shop.json entry
  - `productID`: References Goods.json entry
  - `price`: Current price (decimal number)
  - `currency`: Currency code (ISO 4217, typically "AUD")
  - `isOnSale`: Whether this is a sale/special price
  - `originalPrice`: Regular price before discount
  - `saleDescription`: Marketing description of the sale
  - `validFrom`, `validTo`: Sale validity period
  - `recordedAt`: When this specific price was observed

---

#### Data Exchange Workflow

**For Full Peers (Active P2P Nodes):**
```csharp
// 1. Server starts and advertises itself
var serverInfo = new ServerInfo
{
    Id = "aus-qld-brisbane-01",
    Type = "full_peer",
    Address = "https://localhost:8080",
    Location = "Brisbane, QLD",
    LastSeen = DateTime.UtcNow
};

// 2. Discover other peers
var discoveryUrl = "https://known-peer.example.com/discovery.json";
var peers = await httpClient.GetFromJsonAsync<List<ServerInfo>>(discoveryUrl);

// 3. Connect to peers and sync data
foreach (var peer in peers.Where(p => p.Type == "full_peer"))
{
    await networkManager.ConnectToServer(peer.Address, peer.Port);
    await networkManager.RequestPriceSync(region: "QLD");
}

// 4. Share local prices with network
await networkManager.SharePrice(
    itemId: "milk-dairy-farmers-2l",
    placeId: "coles-chermside-qld",
    price: 3.99m,
    isOnSale: false
);

// 5. Generate static files for backup/archive
await dataExporter.ExportToStaticFiles(
    outputPath: "./export/data",
    includeDays: 30
);
```

**For Static Peers (HTTP File Hosting):**
```csharp
// 1. Generate JSON files on schedule (e.g., daily)
var exporter = new StaticDataExporter(groceryDataService);

// Export current catalog
await exporter.ExportShops("./wwwroot/data/shop.json");
await exporter.ExportGoods("./wwwroot/data/goods.json");

// Export today's prices
var today = DateTime.UtcNow.Date;
var filename = $"price-{today:yyyy-MM-dd}.json";
await exporter.ExportPrices($"./wwwroot/data/{filename}", today);

// Update records index
await exporter.UpdateRecordsIndex("./wwwroot/data/records.json");

// Update discovery (advertise this static peer)
var discovery = new List<ServerInfo>
{
    new()
    {
        Id = "static-brisbane-archive",
        Type = "static_peer",
        Address = "https://archive.example.com/data",
        Location = "Brisbane, QLD",
        LastUpdated = DateTime.UtcNow,
        Description = "Brisbane price data archive (updated daily)"
    }
};
await File.WriteAllTextAsync(
    "./wwwroot/data/discovery.json",
    JsonSerializer.Serialize(discovery, new JsonSerializerOptions { WriteIndented = true })
);

// 2. Files are now accessible via HTTP
// https://archive.example.com/data/discovery.json
// https://archive.example.com/data/shop.json
// https://archive.example.com/data/goods.json
// https://archive.example.com/data/records.json
// https://archive.example.com/data/price-2026-02-25.json
```

**Consuming Data from Static Peer:**
```csharp
// 1. Discover static peer
var discovery = await httpClient.GetFromJsonAsync<List<ServerInfo>>(
    "https://known-peer.example.com/discovery.json"
);

var staticPeer = discovery.FirstOrDefault(p => p.Type == "static_peer");

// 2. Load catalog data
var shops = await httpClient.GetFromJsonAsync<List<Shop>>(
    $"{staticPeer.Address}/shop.json"
);

var goods = await httpClient.GetFromJsonAsync<List<Product>>(
    $"{staticPeer.Address}/goods.json"
);

// 3. Check available price history
var records = await httpClient.GetFromJsonAsync<RecordsIndex>(
    $"{staticPeer.Address}/records.json"
);

// 4. Load recent price data
foreach (var recordFile in records.PriceRecords.Take(7)) // Last 7 days
{
    var priceData = await httpClient.GetFromJsonAsync<PriceData>(
        $"{staticPeer.Address}/{recordFile}"
    );

    await ImportPriceDataAsync(priceData);
}
```

---

#### Validation Rules

**All Files:**
- Must be valid UTF-8 encoded JSON
- Must use ISO 8601 format for all timestamps (`YYYY-MM-DDTHH:MM:SSZ`)
- Must not exceed 50MB per file (for static hosting compatibility)

**Discovery.json:**
- Must contain at least one server entry
- Each `id` must be unique within the file
- `address` must be valid HTTP(S) URL
- `type` must be exactly "full_peer" or "static_peer"

**Shop.json:**
- Each `shopID` must be unique
- `shopID` should follow convention: `{chain}-{suburb}-{state}` (lowercase, hyphens)
- `location` must be a valid street address

**Goods.json:**
- Each `productID` must be unique
- `productID` should follow convention: `{category}-{brand}-{size}` (lowercase, hyphens)
- `category` should match standard category names

**Price Files:**
- `shopID` must reference valid entry in Shop.json
- `productID` must reference valid entry in Goods.json
- `price` must be positive number with 2 decimal places
- `currency` must be valid ISO 4217 code
- If `isOnSale` is true, `originalPrice` should be provided
- `validFrom` must be before `validTo`

---

#### Implementation Checklist

**Export Functionality:**
- [ ] Create `StaticDataExporter` service
- [ ] Implement `ExportShops()` method
- [ ] Implement `ExportGoods()` method
- [ ] Implement `ExportPrices()` method with date parameter
- [ ] Implement `UpdateRecordsIndex()` method
- [ ] Implement `ExportDiscovery()` method
- [ ] Add scheduled export job (daily/hourly)
- [ ] Add validation before export
- [ ] Add file size monitoring
- [ ] Add export history tracking

**Import Functionality:**
- [ ] Create `StaticDataImporter` service
- [ ] Implement `ImportShops()` method with deduplication
- [ ] Implement `ImportGoods()` method with deduplication
- [ ] Implement `ImportPrices()` method with conflict resolution
- [ ] Implement `SyncFromStaticPeer()` method
- [ ] Add incremental sync (only new files)
- [ ] Add validation on import
- [ ] Add error handling for malformed data
- [ ] Add import progress tracking
- [ ] Add import history tracking

**P2P Integration:**
- [ ] Update `NetworkManager` to generate these formats
- [ ] Add discovery file update on server start
- [ ] Add automatic export of price updates
- [ ] Add periodic refresh of records.json
- [ ] Add peer discovery from multiple sources
- [ ] Add fallback to static peers if full peers unavailable
- [ ] Add data consistency verification

**UI Features:**
- [ ] Add "Export Data" button in settings
- [ ] Add "Import from URL" dialog
- [ ] Add static peer configuration UI
- [ ] Add export/import progress indicators
- [ ] Add data validation reports
- [ ] Add export file browser

---

#### Security Considerations

1. **Data Validation:**
   - Validate all JSON against schemas before import
   - Sanitize product names and descriptions to prevent XSS
   - Verify numeric values are within reasonable ranges
   - Check timestamps are not in the future

2. **Trust Model:**
   - Data from unknown peers should be flagged
   - Allow users to configure trusted peer lists
   - Implement reputation system for peers (future)
   - Detect and reject obviously fraudulent prices

3. **Rate Limiting:**
   - Limit import frequency per peer (e.g., once per hour)
   - Limit total file size downloaded per day
   - Implement exponential backoff on errors

4. **HTTPS Requirement:**
   - Prefer HTTPS URLs for static peers
   - Warn users when importing from HTTP sources
   - Support certificate validation

---

#### Testing Data

Create test files for development:

**test-discovery.json:**
```json
[
  {
    "id": "test-static-peer",
    "type": "static_peer",
    "address": "http://localhost:8080/data",
    "location": "Local Test",
    "last_updated": "2026-02-25T10:30:00Z",
    "description": "Local testing peer"
  }
]
```

**Test Import Command:**
```csharp
var importer = new StaticDataImporter(groceryDataService);
await importer.ImportFromStaticPeer("http://localhost:8080/data");
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
- **Phase 10 (Database Provider Selection):** 2-3 days
- **Phase 11 (ML.NET Price Prediction):** 3-4 days
- **Phase 12 (Ollama Chat Interface):** 2-3 days
- **Total Full Implementation:** 40-50 days

---

**END OF PLAN**
