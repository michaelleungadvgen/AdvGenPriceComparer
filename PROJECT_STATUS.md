# AdvGen Price Comparer - WPF Application Project Status

**Last Updated:** March 2026

---

## Project Overview

**AdvGen Price Comparer** is a Windows desktop application built with WPF (.NET 9.0) for tracking and comparing grocery prices across Australian supermarkets. The application helps consumers identify genuine deals versus "illusory discounts" by maintaining historical price data.

---

## Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | .NET | 9.0 |
| UI Framework | WPF with WPF-UI (Fluent Design) | 3.0.5 |
| Database | LiteDB (embedded NoSQL) | - |
| Charts | LiveChartsCore.SkiaSharpView | 2.0.0-rc6.1 |
| DI Container | Microsoft.Extensions.DependencyInjection | 9.0.0 |
| Custom Packages | AdvGenNoSqlServer (Client, Core, Network) | 1.0.0 |

---

## Solution Structure

```
AdvGenPriceComparer/
├── AdvGenPriceComparer.Core/           # Core models and interfaces
│   ├── Models/
│   │   ├── Item.cs                     # Grocery product model
│   │   ├── Place.cs                    # Supermarket location model
│   │   ├── PriceRecord.cs              # Price tracking entity
│   │   ├── AlertLogicEntity.cs         # Price alert model
│   │   └── NetworkModels.cs            # P2P network models
│   ├── Interfaces/
│   │   ├── IGroceryDataService.cs      # Main data service interface
│   │   ├── IItemRepository.cs          # Item CRUD operations
│   │   ├── IPlaceRepository.cs         # Place CRUD operations
│   │   ├── IPriceRecordRepository.cs   # Price record operations
│   │   └── IAlertRepository.cs         # Alert management
│   ├── Helpers/
│   │   └── NetworkManager.cs           # P2P networking helper
│   └── Services/
│       └── ServerConfigService.cs      # Server configuration
│
├── AdvGenPriceComparer.Data.LiteDB/    # Data access layer
│   ├── Entities/
│   │   ├── ItemEntity.cs
│   │   ├── PlaceEntity.cs
│   │   ├── PriceRecordEntity.cs
│   │   ├── CategoryEntity.cs
│   │   └── AlertEntity.cs
│   ├── Repositories/
│   │   ├── ItemRepository.cs
│   │   ├── PlaceRepository.cs
│   │   ├── PriceRecordRepository.cs
│   │   └── AlertRepository.cs
│   ├── Services/
│   │   ├── DatabaseService.cs          # LiteDB connection management
│   │   ├── GroceryDataService.cs       # Main data service implementation
│   │   ├── JsonImportService.cs        # Import from JSON files
│   │   └── JsonExportService.cs        # Export to JSON files
│   ├── Mappings/
│   │   └── ItemLiteDbMapper.cs
│   └── Utilities/
│       └── ObjectIdHelper.cs
│
└── AdvGenPriceComparer.WPF/            # WPF desktop application
    ├── Views/
    │   ├── ItemsPage.xaml              # Items management page
    │   ├── StoresPage.xaml             # Stores management page
    │   ├── CategoryPage.xaml           # Category management page
    │   ├── AlertsPage.xaml             # Price alerts page
    │   ├── AddItemWindow.xaml          # Add new item dialog
    │   ├── AddStoreWindow.xaml         # Add new store dialog
    │   ├── ImportDataWindow.xaml       # JSON import dialog
    │   └── ExportDataWindow.xaml       # JSON export dialog
    ├── ViewModels/
    │   ├── ViewModelBase.cs            # MVVM base class
    │   ├── MainWindowViewModel.cs      # Main window logic
    │   ├── ItemViewModel.cs            # Item management VM
    │   ├── StoreViewModel.cs           # Store management VM
    │   ├── PlaceViewModel.cs           # Place details VM
    │   ├── AddStoreViewModel.cs        # Add store dialog VM
    │   ├── AlertViewModel.cs           # Alerts management VM
    │   └── ExportDataViewModel.cs      # Export functionality VM
    ├── Commands/
    │   └── RelayCommand.cs             # ICommand implementation
    ├── Services/
    │   ├── IDialogService.cs           # Dialog service interface
    │   ├── SimpleDialogService.cs      # Dialog implementation
    │   ├── INotificationService.cs     # Notification interface
    │   ├── SimpleNotificationService.cs # Notification implementation
    │   └── DemoDataService.cs          # Demo data generation
    ├── MainWindow.xaml                 # Main application window
    └── App.xaml                        # Application resources
```

---

## Implemented Features

### Dashboard (MainWindow)
- [x] Custom Fluent Design title bar with window controls
- [x] Navigation sidebar with menu items
- [x] Statistics cards (Total Items, Tracked Stores, Price Updates)
- [x] Category distribution pie chart (LiveCharts)
- [x] Price trends line chart (LiveCharts)
- [x] Quick actions panel

### Items Management
- [x] Items list view with DataGrid
- [x] Add new item dialog
- [x] Item search functionality
- [x] Category filtering

### Stores Management
- [x] Stores list view
- [x] Add new store dialog
- [x] Store details (name, chain, address, suburb, state, postcode)

### Categories
- [x] Category listing page
- [x] Category-based item grouping

### Alerts
- [x] Price alert management page
- [x] Alert creation and tracking

### Data Import/Export
- [x] JSON import functionality
- [x] JSON export functionality
- [x] Demo data generation

---

## Pending Features / To-Do

### High Priority
- [x] Price comparison view (side-by-side store comparison) - Implemented with ComparePricesWindow
- [x] Historical price charts for individual items - Implemented with PriceHistoryPage and LiveCharts
- [x] Barcode scanner integration - Implemented using ZXing library
- [x] Search across all entities - Implemented with GlobalSearchWindow and GlobalSearchService

### Medium Priority
- [x] Price drop notifications - Implemented with PriceDropNotificationService and UI
- [x] Favourite items list
- [x] Shopping list integration - Implemented with ShoppingListService and ShoppingListWindow
- [x] Multi-store trip optimization - Implemented with TripOptimizerService and TripOptimizerWindow
- [x] Weekly specials import (Coles, Woolworths, ALDI, Drakes) - IMPLEMENTED: IWeeklySpecialsImportService with support for JSON (Coles/Woolworths) and Markdown (ALDI/Drakes) formats

### Low Priority
- [x] P2P price data sharing (NetworkManager) - Implemented with IP2PNetworkService interface and NetworkManager
- [x] Cloud sync functionality
- [x] Dark mode theme - Implemented with IThemeService/ThemeService
- [x] Localization (multiple languages) - Implemented with ILocalizationService, RESX resource files (English, Simplified Chinese, Traditional Chinese), language switching in Settings
- [x] Mobile companion app API - Implemented MobileApiController with 15+ endpoints including dashboard, price-check, nearby-stores, barcode lookup, shopping list sync, price alerts, push notifications. See MOBILE_API.md for documentation.

---

## Database Schema

### Collections

| Collection | Description |
|------------|-------------|
| `items` | Grocery products with metadata |
| `places` | Supermarket locations |
| `price_records` | Historical price data |
| `alerts` | Price alert configurations |
| `categories` | Product categories |

### Key Indexes
- Items: `Name`, `Brand`, `Category`, `Barcode`
- Places: `Name`, `Chain`, `Suburb`, `State`
- PriceRecords: `ItemId`, `PlaceId`, `DateRecorded`

---

## Build & Run Instructions

### Prerequisites
- Visual Studio 2022 or later
- .NET 9.0 SDK
- Windows 10/11

### Build Commands
```bash
# Build solution
cd AdvGenPriceComparer
dotnet build -p:Platform=x64

# Run application
dotnet run --project AdvGenPriceComparer.WPF
```

### Configuration
- Database file: `%AppData%\AdvGenPriceComparer\GroceryPrices.db`

---

## Known Issues

1. **COM Registration Error (0x80040154)**
   - Occurs with certain Windows App SDK configurations
   - Solution: Run via Visual Studio or use packaged deployment

2. **Chart Performance**
   - Large datasets may cause UI lag
   - Pagination recommended for 1000+ records

---

## Dependencies

### NuGet Packages
```xml
<PackageReference Include="AdvGenNoSqlServer.Client" Version="1.0.0" />
<PackageReference Include="AdvGenNoSqlServer.Core" Version="1.0.0" />
<PackageReference Include="AdvGenNoSqlServer.Network" Version="1.0.0" />
<PackageReference Include="LiveChartsCore" Version="2.0.0-rc6.1" />
<PackageReference Include="LiveChartsCore.SkiaSharpView" Version="2.0.0-rc6.1" />
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc6.1" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
<PackageReference Include="WPF-UI" Version="3.0.5" />
```

### Project References
- `AdvGenPriceComparer.Core`
- `AdvGenPriceComparer.Data.LiteDB`

---

## API Surface (IGroceryDataService)

### Item Operations
- `AddGroceryItem()` - Create new grocery item
- `GetItemById()` - Retrieve item by ID
- `GetAllItems()` - List all items

### Place Operations
- `AddSupermarket()` - Create new store location
- `GetPlaceById()` - Retrieve place by ID
- `GetAllPlaces()` - List all places

### Price Operations
- `RecordPrice()` - Record a price observation
- `GetRecentPriceUpdates()` - Get recent price changes

### Analytics
- `FindBestDeals()` - Find lowest prices by category
- `GetDashboardStats()` - Dashboard statistics
- `GetPriceHistory()` - Historical price data
- `GetCategoryStats()` - Category analytics
- `GetStoreComparisonStats()` - Store comparison

---

## Contact & Repository

- **Repository:** Local development
- **Primary Developer:** AdvGen
- **Last Commit:** See git log for details

---

## Changelog

### v1.0.0 (Current)
- Initial WPF application with Fluent Design
- LiteDB database integration
- Dashboard with charts
- CRUD operations for Items, Stores, Categories
- JSON import/export functionality
- Price alert system
- Demo data generation
