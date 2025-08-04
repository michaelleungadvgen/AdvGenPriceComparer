# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AdvGenPriceComparer is a Windows application for automated price extraction and comparison from retail catalogues. The project combines:

1. **C# WinUI 3 Desktop Application** - Main UI application for price comparison
2. **Core Library** - Shared models and business logic
3. **Python PDF Processing** - Scripts for extracting price data from PDF catalogues using LLMs

The project aims to combat "illusory discounts" by tracking historical price data and enabling transparent price comparison across retailers.

## Architecture

- **AdvGenPriceComparer.Desktop.WinUI** - WinUI 3 desktop application (.NET 9.0)
- **AdvGenPriceComparer.Core** - Class library (.NET 9.0) containing:
  - Models: `Item`, `Place`, `PriceRecord`
  - Helpers: `NetworkManager` (for future P2P functionality)
- **Python Scripts** - PDF processing and LLM-based price extraction

## Development Commands

### Building the Application
```
dotnet build AdvGenPriceComparer.sln
```

### Running the Application
```
dotnet run --project AdvGenPriceComparer.Desktop.WinUI
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
Represents a product with comprehensive store and pricing information. Contains both required fields (`Name`, `Price`) and extensive optional metadata for store details and geographic information.

### Place (AdvGenPriceComparer.Core.Models.Place)
Represents a retail location with store information, contact details, and geographic coordinates.

### PriceRecord (AdvGenPriceComparer.Core.Models.PriceRecord)
Links an `Item` to a `Place` with timestamp and price data - core entity for price tracking and historical analysis.

## Project Features

- **PDF Catalogue Processing** - Automated extraction from retailer catalogues
- **Historical Price Tracking** - Combat misleading discount practices
- **P2P Price Sharing** - Decentralized price data sharing (planned)
- **Multi-platform Support** - Windows x86, x64, and ARM64

## Data Files

- `catalog_extracted.txt`, `metro_catalog_extracted.txt` - Extracted catalogue text
- `catalog_products.txt` - Processed product data
- `data/coles_24072025.json` - Sample extracted price data
- `pdf_catalog_extractor.py` - Main Python extraction script

## Development Notes

- Uses WinUI 3 with .NET 9.0
- Python scripts utilize PyPDF2, pdfplumber, and OCR libraries
- Core models use nullable reference types
- Extensive metadata support for comprehensive price comparison