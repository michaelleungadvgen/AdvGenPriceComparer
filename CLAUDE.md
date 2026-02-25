# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AdvGenPriceComparer is a Windows application for automated **grocery price tracking and comparison** across Australian supermarkets. The project combines:

1. **C# WinUI 3 Desktop Application** - Main UI application for grocery price comparison
2. **Core Library** - Shared models and business logic for grocery items and store locations
3. **Python PDF Processing** - Scripts for extracting grocery prices from supermarket catalogues using LLMs

The project aims to combat "illusory discounts" by tracking historical grocery prices and enabling transparent price comparison across major Australian supermarkets like Coles, Woolworths, IGA, and others.

## Architecture

- **AdvGenPriceComparer.Desktop.WinUI** - WinUI 3 desktop application (.NET 9.0)
- **AdvGenPriceComparer.Core** - Class library (.NET 9.0) containing:
  - Models: `Item`, `Place`, `PriceRecord`
  - Helpers: `NetworkManager` (for future P2P functionality)
- **Python Scripts** - PDF processing and LLM-based price extraction

## Development Commands

### Building the Application
```
cd AdvGenPriceComparer
dotnet build -p:Platform=x64
```

### Running the Application
Due to Windows App SDK COM registration requirements with .NET 9, use Visual Studio to run the application or build a packaged deployment:

**Option 1: Visual Studio** (Recommended)
- Open `AdvGenPriceComparer.sln` in Visual Studio
- Set platform to x64
- Run with F5 or Ctrl+F5

**Option 2: Packaged Deployment**
```
cd AdvGenPriceComparer
dotnet publish -c Release -p:Platform=x64 --self-contained true
# Run from: bin\Release\net9.0-windows10.0.19041.0\win-x64\publish\AdvGenPriceComparer.Desktop.WinUI.exe
```

### Building for Different Platforms
The WinUI app supports multiple platforms:
```
dotnet publish AdvGenPriceComparer.Desktop.WinUI -c Release -r win-x64
dotnet publish AdvGenPriceComparer.Desktop.WinUI -c Release -r win-x86
dotnet publish AdvGenPriceComparer.Desktop.WinUI -c Release -r win-arm64
```

### Python Environment Setup
For PDF processing scripts:
```
pip install -r requirements.txt
```

## Key Models

### Item (AdvGenPriceComparer.Core.Models.Item)
Represents a **grocery product** with comprehensive store and pricing information. Contains required fields (`Name`, `Price`) and extensive optional metadata for store details, brand information, package sizes, and geographic information.

### Place (AdvGenPriceComparer.Core.Models.Place)
Represents a **supermarket location** with store information, contact details, geographic coordinates, and chain identification (Coles, Woolworths, IGA, etc.).

### PriceRecord (AdvGenPriceComparer.Core.Models.PriceRecord)
Links a grocery `Item` to a supermarket `Place` with timestamp and price data - core entity for **grocery price tracking** and historical analysis to identify genuine vs. illusory discounts.

## Project Features

- **Grocery Catalogue Processing** - Automated extraction from supermarket catalogues (Coles, Woolworths, IGA)
- **Historical Grocery Price Tracking** - Combat misleading "sale" prices and illusory discounts
- **Multi-Store Price Comparison** - Compare grocery prices across different supermarket chains
- **Network Price Sharing** - Share grocery price data with other users (planned P2P functionality)
- **Multi-platform Support** - Windows x86, x64, and ARM64

## Data Files

- `catalog_extracted.txt`, `metro_catalog_extracted.txt` - Extracted **supermarket catalogue** text
- `catalog_products.txt` - Processed **grocery product** data
- `data/coles_24072025.json` - Sample extracted **Coles grocery prices**
- `pdf_catalog_extractor.py` - Main Python script for **supermarket catalogue** extraction

## Development Notes

- Uses WinUI 3 with .NET 8.0 (downgraded from .NET 9.0 due to Windows App SDK compatibility issues)
- **LiteDB Database** - Lightweight embedded database for storing grocery items, supermarket locations, and price records
- Python scripts utilize PyPDF2, pdfplumber, and OCR libraries
- Core models use nullable reference types with LiteDB BSON attributes
- Extensive metadata support for comprehensive grocery price comparison

## Database Structure

### LiteDB Implementation
- **Database File**: `GroceryPrices.db` stored in `%AppData%\AdvGenPriceComparer\`
- **Collections**: `items`, `places`, `price_records`
- **Indexes**: Optimized for name, brand, category, chain, suburb, price, and date queries

### Key Services
- **DatabaseService**: Core LiteDB connection and configuration
- **ItemRepository**: CRUD operations for grocery items
- **PlaceRepository**: CRUD operations for supermarket locations with geospatial support
- **PriceRecordRepository**: Price tracking with historical analysis
- **GroceryDataService**: High-level service combining all repositories
- **SettingsService**: Application settings management with JSON persistence (ISettingsService)

## Known Issues

### COM Registration Error (0x80040154)
If you encounter `System.Runtime.InteropServices.COMException: 'Class not registered (0x80040154)'`:

**Root Cause:** Windows App SDK compatibility issues with .NET 9.0

**Solutions:**
1. **Use .NET 8.0** (Current configuration) - Most reliable
2. **Run via Visual Studio** with x64 platform selected
3. **Use packaged deployment** with `--self-contained true`

**Technical Details:**
- Windows App SDK requires proper COM registration for WinUI 3 components
- .NET 9.0 has known compatibility issues with Windows App SDK runtime initialization
- The error occurs during Application.Start() when WinUI tries to initialize XAML components