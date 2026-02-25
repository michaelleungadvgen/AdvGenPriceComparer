# AdvGenPriceComparer

> **Combat Illusory Discounts with P2P Price Intelligence & AI**

A Windows desktop application for tracking and comparing grocery prices across Australian supermarkets, powered by peer-to-peer data sharing and artificial intelligence.

## 🎯 Mission

Fight misleading "sale" prices and illusory discounts by building a transparent, community-driven grocery price tracking network. Share real-time price data across a P2P network and leverage AI to automatically extract pricing from supermarket catalogues.

## ✨ Key Features

### 🌐 Peer-to-Peer Price Sharing
- **Decentralized Network**: Share grocery prices directly with other users without central servers
- **Server Discovery**: Use `servers.json` configuration to find and connect to P2P nodes
- **Regional Filtering**: Connect to price-sharing nodes in your region (NSW, VIC, QLD, etc.)
- **Real-time Sync**: Automatic synchronization of price updates across the network
- **Privacy-Focused**: Direct peer connections, no central data collection
- **Scalable Storage**: For large-scale data, use [AdvGenNoSqlServer](https://github.com/michaelleungadvgen/AdvGenNoSQLServer) (sister project)

### 🤖 AI-Powered Catalogue Processing
- **LLM Integration**: Automatically extract pricing from PDF catalogues using Large Language Models
- **ML.Net Support**: Machine learning capabilities for price prediction and analysis (planned)
- **Smart Matching**: AI-assisted product matching across different supermarket chains
- **OCR Processing**: Extract text from image-based catalogue PDFs

### 📊 Price Intelligence
- **Historical Tracking**: Track price changes over time to identify genuine vs. fake discounts
- **Multi-Store Comparison**: Compare prices across Coles, Woolworths, IGA, Aldi, and other chains
- **Price Alerts**: Get notified when prices drop
- **Discount Analysis**: Identify illusory discounts by comparing current "sale" prices with historical data
- **Import/Export**: JSON import from Coles/Woolworths/Drakes, export with filters and compression

### 🏪 Comprehensive Coverage
- Coles
- Woolworths
- IGA
- Aldi
- Drakes
- Other Australian supermarkets

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│           WPF Desktop App (WPF-UI Fluent Design)                │
│                     (UI Layer - .NET 9)                          │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────┐   │
│  │   Views      │  │  ViewModels  │  │   Converters        │   │
│  │  • ItemsPage │  │  • MainWindow│  │  • BoolToVisibility │   │
│  │  • StoresPage│  │  • ItemVM    │  │  • InverseBool      │   │
│  │  • Dashboard │  │  • ImportVM  │  │                     │   │
│  └──────────────┘  └──────────────┘  └─────────────────────┘   │
└─────────────────────┬───────────────────────────────────────────┘
                      │
┌─────────────────────┴───────────────────────────────────────────┐
│              AdvGenPriceComparer.Data.LiteDB                     │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────┐   │
│  │ Repositories │  │   Services   │  │     Entities        │   │
│  │  • Items     │  │  • JsonImport│  │  • ItemEntity       │   │
│  │  • Places    │  │  • Database  │  │  • PlaceEntity      │   │
│  │  • Prices    │  │  • Export    │  │  • PriceRecordEntity│   │
│  └──────────────┘  └──────────────┘  └─────────────────────┘   │
└─────────────────────┬───────────────────────────────────────────┘
                      │
┌─────────────────────┴───────────────────────────────────────────┐
│                   AdvGenPriceComparer.Core                       │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────┐   │
│  │   Models     │  │  Interfaces  │  │  NetworkManager     │   │
│  │  • Item      │  │  • IGrocery  │  │  • P2P Server       │   │
│  │  • Place     │  │  • IRepos    │  │  • P2P Client       │   │
│  │  • PriceRec  │  │  • Services  │  │  • Discovery        │   │
│  └──────────────┘  └──────────────┘  └─────────────────────┘   │
└─────────────────────┬───────────────────────────────────────────┘
                      │
┌─────────────────────┴───────────────────────────────────────────┐
│                LiteDB Embedded Database                          │
│    • Items Collection  • Places Collection  • PriceRecords      │
│    • Categories        • Alerts                                  │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                  Python AI Processing                         │
│  • PDF Catalogue Extraction (LLM-powered)                    │
│  • OCR Processing (pdfplumber, PyPDF2)                       │
│  • Product Categorization                                    │
└──────────────────────────────────────────────────────────────┘
```

## 📈 Scalability & Sister Project

### AdvGenNoSqlServer - Enterprise-Scale Price Storage

As your pricing data grows beyond the capacity of the embedded LiteDB database, you can seamlessly migrate to **[AdvGenNoSqlServer](https://github.com/michaelleungadvgen/AdvGenNoSQLServer)**, our sister project designed for large-scale data management.

**When to Use AdvGenNoSqlServer:**
- **Millions of price records**: LiteDB is great for personal use, but enterprise deployments need more
- **Multi-region deployments**: Centralized NoSQL database for regional P2P hub servers
- **Advanced analytics**: Run complex queries across massive historical datasets
- **Community hubs**: Power community-run price-sharing nodes serving hundreds of users
- **API services**: Build public APIs for price data access

**Features:**
- High-performance NoSQL database (MongoDB, Cassandra, or CosmosDB)
- RESTful API for data access
- Horizontal scaling for millions of records
- Advanced indexing and query optimization
- Backup and replication support
- Docker deployment ready

**Migration Path:**
```csharp
// Export from LiteDB
var exporter = new PriceDataExporter(groceryDataService);
await exporter.ExportToJson("price_data_export.json");

// Import to AdvGenNoSqlServer
var importer = new NoSqlImporter("https://your-nosql-server.com");
await importer.ImportFromJson("price_data_export.json");
```

**Architecture with AdvGenNoSqlServer:**
```
Desktop App (LiteDB) ──P2P──> Regional Hub (AdvGenNoSqlServer) ──P2P──> Other Hubs
      ↓                              ↓                                     ↓
 Personal Data              Regional Aggregation                   National Network
```

This hybrid approach lets individuals use lightweight P2P sharing while community leaders can run powerful regional hubs with AdvGenNoSqlServer for aggregated price intelligence.

## 🚀 Getting Started

### Prerequisites

**For C# Application:**
- Windows 10/11
- .NET 9.0 SDK
- Visual Studio 2022 (recommended)

**For Python Scripts:**
- Python 3.8+
- pip (Python package manager)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/AdvGenPriceComparer.git
   cd AdvGenPriceComparer
   ```

2. **Build the C# application**
   ```bash
   cd AdvGenPriceComparer
   dotnet restore
   dotnet build -p:Platform=x64
   ```

3. **Setup Python environment**
   ```bash
   pip install -r requirements.txt
   ```

### Running the Application

**Option 1: Visual Studio (Recommended)**
1. Open `AdvGenPriceComparer.sln` in Visual Studio 2022
2. Set platform to `x64`
3. Press `F5` to run with debugging or `Ctrl+F5` to run without debugging

**Option 2: Packaged Deployment**
```bash
cd AdvGenPriceComparer
dotnet publish -c Release -p:Platform=x64 --self-contained true
# Run from: bin\Release\net9.0-windows10.0.19041.0\win-x64\publish\
```

## 🌐 P2P Configuration

### Server Discovery with `servers.json`

The application uses a `servers.json` configuration file to discover and connect to P2P price-sharing nodes. The file is stored at:
```
%AppData%\AdvGenPriceComparer\servers.json
```

**Example Configuration:**
```json
[
  {
    "name": "AusPriceShare-Sydney",
    "host": "price.aus.example.com",
    "port": 8080,
    "isSecure": false,
    "region": "NSW",
    "description": "Australian grocery price sharing - Sydney region",
    "isActive": true
  },
  {
    "name": "LocalTestServer",
    "host": "localhost",
    "port": 8081,
    "isSecure": false,
    "region": "Local",
    "description": "Local testing server",
    "isActive": false
  }
]
```

### Starting a P2P Server

```csharp
var networkManager = new NetworkManager(groceryDataService);
await networkManager.StartServer(port: 8081);
```

### Connecting to P2P Network

```csharp
// Connect to specific server
await networkManager.ConnectToServer("AusPriceShare-Sydney");

// Auto-discover and connect to all active servers in region
await networkManager.DiscoverAndConnectToServers("NSW");
```

### Sharing Prices

```csharp
await networkManager.SharePrice(
    itemId: "item123",
    placeId: "place456",
    price: 3.99m,
    isOnSale: true,
    originalPrice: 5.99m,
    saleDescription: "50% Off - Special"
);
```

## 🤖 AI & Catalogue Processing

### Extract Prices from PDF Catalogues

The Python scripts use LLM APIs to automatically extract product information and pricing from supermarket catalogue PDFs:

```bash
python pdf_catalog_extractor.py --input coles_catalogue.pdf --output coles_prices.json
```

**Supported Catalogue Sources:**
- Coles weekly specials
- Woolworths catalogues
- Aldi special buys
- IGA local catalogues

### How AI Extraction Works

1. **PDF Processing**: Extract text and images from catalogue PDFs
2. **LLM Analysis**: Send pages to LLM (GPT-4, Claude, etc.) to identify products and prices
3. **Structured Output**: Generate JSON with product names, prices, brands, and categories
4. **Import to Database**: Load extracted data into LiteDB for tracking

### 🤖 ML.NET Integration (Planned)

Future ML.NET capabilities for Phase 9-11:
- **Auto-Categorization**: ML-powered product categorization during import
- **Price Prediction**: Forecast future price trends based on historical data
- **Anomaly Detection**: Identify suspicious price changes or fake discounts
- **Smart Buying Recommendations**: "Buy Now" vs "Wait" based on price forecasting

## 💾 Database Structure

### LiteDB Collections

**Items Collection**
```json
{
  "id": "item_001",
  "name": "Milk Full Cream",
  "brand": "Dairy Farmers",
  "category": "Dairy",
  "packageSize": "2L",
  "barcode": "9300632123456"
}
```

**Places Collection**
```json
{
  "id": "place_001",
  "name": "Coles Chermside",
  "chain": "Coles",
  "suburb": "Chermside",
  "state": "QLD",
  "latitude": -27.3853,
  "longitude": 153.0356
}
```

**PriceRecords Collection**
```json
{
  "id": "record_001",
  "itemId": "item_001",
  "placeId": "place_001",
  "price": 3.99,
  "isOnSale": true,
  "originalPrice": 5.99,
  "dateRecorded": "2026-02-25T10:30:00Z",
  "source": "p2p-network"
}
```

Database location: `%AppData%\AdvGenPriceComparer\GroceryPrices.db`

> **💡 Need More Scale?** For large-scale deployments with millions of records, migrate to [AdvGenNoSqlServer](https://github.com/michaelleungadvgen/AdvGenNoSQLServer) for enterprise-grade performance and scalability.

## 📁 Project Structure

```
AdvGenPriceComparer/
├── AdvGenPriceComparer.WPF/              # WPF Application (.NET 9, WPF-UI)
│   ├── Views/                            # XAML Views (Items, Stores, Dashboard)
│   ├── ViewModels/                       # MVVM ViewModels
│   ├── Services/                         # WPF-specific services
│   └── Converters/                       # XAML value converters
├── AdvGenPriceComparer.Core/             # Core Models & Interfaces
│   ├── Models/                           # Item, Place, PriceRecord
│   ├── Interfaces/                       # Repository interfaces
│   └── Helpers/                          # NetworkManager
├── AdvGenPriceComparer.Data.LiteDB/      # Data Access Layer
│   ├── Repositories/                     # LiteDB implementations
│   ├── Services/                         # JsonImportService, ExportService
│   └── Entities/                         # LiteDB entity classes
├── AdvGenPriceComparer.Tests/            # xUnit Test Suite (217+ tests)
│   ├── Services/                         # JsonImport, ServerConfig tests
│   ├── Repositories/                     # Repository layer tests
│   ├── ViewModels/                       # ViewModel tests
│   └── Integration/                      # End-to-end tests
├── TestConsole/                          # Console test application
├── NetworkTest/                          # P2P network testing
├── pdf_catalog_extractor.py              # LLM-powered PDF extraction
├── coles_catalogue_scraper.py            # Coles-specific scraper
├── woolworths_catalogue_parser.py        # Woolworths-specific parser
├── requirements.txt                      # Python dependencies
└── servers.json                          # Server configuration
```

## 🔧 Development

### Building for Different Platforms

```bash
# Windows x64 (default)
dotnet publish -c Release -r win-x64

# Windows x86
dotnet publish -c Release -r win-x86

# Windows ARM64
dotnet publish -c Release -r win-arm64
```

### Running Tests

```bash
dotnet test AdvGenPriceComparer.Tests
```

### Python Environment Setup

```bash
# Install dependencies
pip install -r requirements.txt

# Key Python packages:
# - PyPDF2, pdfplumber: PDF processing
# - openai, anthropic: LLM API clients
# - pytesseract: OCR processing
```

## 🤝 Contributing

Contributions are welcome! This is a community-driven project to combat deceptive pricing practices.

**Ways to contribute:**
- Add support for more supermarket chains
- Improve AI extraction accuracy
- Set up regional P2P nodes with AdvGenNoSqlServer
- Add price alert features
- Enhance the UI/UX
- Report bugs and request features
- Help build community price-sharing hubs

## 📋 Roadmap

### Completed ✅
- [x] ~~WinUI 3~~ **Migrated to WPF** with Fluent Design (WPF-UI)
- [x] LiteDB integration with repository pattern
- [x] **JSON Import/Export** (Coles, Woolworths, Drakes formats)
- [x] **217+ xUnit tests** with CI/CD pipeline
- [x] P2P networking with server discovery
- [x] LLM-powered catalogue extraction
- [x] Sister project: AdvGenNoSqlServer for large-scale deployments

### In Progress 🚧
- [ ] ML.NET Auto-Categorization (Phase 9)
- [ ] ML.NET Price Prediction & Forecasting (Phase 11)
- [ ] Settings Service with database provider selection (Phase 10)

### Planned 📅
- [ ] AdvGenNoSqlServer integration and migration tools
- [ ] Mobile app (Android/iOS)
- [ ] Barcode scanning
- [ ] Shopping list integration
- [ ] Browser extension for online shopping
- [ ] Public P2P node infrastructure with regional hubs

## ⚠️ Known Issues

### COM Registration Error (0x80040154)

If you encounter this error, use .NET 8.0 or run via Visual Studio. See [CLAUDE.md](CLAUDE.md) for detailed troubleshooting.

## 📝 License

[Add your license here]

## 🙏 Acknowledgments

- Built to combat misleading pricing practices in Australian supermarkets
- Inspired by consumer advocacy groups fighting illusory discounts
- Community-driven approach to transparent pricing

## 🔗 Related Projects

### AdvGenNoSqlServer
Enterprise-scale NoSQL server for large pricing datasets. When your price data grows beyond embedded database limits, AdvGenNoSqlServer provides:
- Scalable storage for millions of price records
- RESTful API for distributed access
- Regional hub deployment for P2P networks
- Advanced analytics and reporting

**Repository**: [AdvGenNoSqlServer](https://github.com/michaelleungadvgen/AdvGenNoSQLServer)

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/AdvGenPriceComparer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/AdvGenPriceComparer/discussions)

---

**Made with 💙 for Australian shoppers**

*Help us build a transparent grocery pricing network - because you deserve to know the real price!*
