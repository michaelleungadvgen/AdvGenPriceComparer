# AdvGen Price Comparer - User Guide

**Version:** 1.0  
**Last Updated:** February 2026

---

## Table of Contents

1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [Dashboard Overview](#dashboard-overview)
4. [Managing Items](#managing-items)
5. [Managing Stores](#managing-stores)
6. [Price History](#price-history)
7. [Shopping Lists](#shopping-lists)
8. [Reports & Analytics](#reports--analytics)
9. [Import & Export](#import--export)
10. [Special Features](#special-features)
11. [Tips & Tricks](#tips--tricks)
12. [Troubleshooting](#troubleshooting)

---

## Introduction

AdvGen Price Comparer is a Windows desktop application that helps you track and compare grocery prices across Australian supermarkets including Coles, Woolworths, ALDI, and Drakes. Save money by identifying genuine deals versus "illusory discounts" and make informed shopping decisions.

### Key Features

- **Price Tracking:** Monitor prices over time to spot real deals
- **Store Comparison:** Compare prices across multiple supermarkets
- **Shopping Lists:** Create and manage shopping lists with price estimates
- **Deal Alerts:** Get notified when prices drop on your favorite items
- **Weekly Specials:** View curated weekly specials from major retailers
- **Global Search:** Search across all items, stores, and price records
- **Barcode Scanning:** Quickly add items by scanning barcodes
- **Import/Export:** Share price data with others

---

## Getting Started

### System Requirements

- **Operating System:** Windows 10 or Windows 11
- **.NET Runtime:** .NET 9.0 or later
- **RAM:** 4 GB minimum, 8 GB recommended
- **Disk Space:** 100 MB for application, additional space for database

### Installation

1. Download the installer from the releases page
2. Run `AdvGenPriceComparer.Setup.exe`
3. Follow the installation wizard
4. Launch the application from the Start menu or desktop shortcut

### First Launch

On first launch, the application will:
1. Create a local database in `%AppData%\AdvGenPriceComparer\`
2. Set up default configuration files
3. Display the main dashboard

---

## Dashboard Overview

The main dashboard provides a quick overview of your price tracking data.

### Statistics Cards

- **Total Items:** Number of grocery items being tracked
- **Tracked Stores:** Number of supermarkets monitored
- **Price Updates:** Recent price changes in the system

### Charts

- **Items by Category:** Pie chart showing distribution across categories
- **Price Updates Over Time:** Line chart showing recent activity

### Navigation Sidebar

- **Dashboard:** Return to the main overview
- **Items:** Manage your grocery items database
- **Stores:** Manage supermarket locations
- **Categories:** Browse items by category
- **Price History:** View detailed price history
- **Reports:** View analytics and best deals

### Quick Actions

Access frequently used features from the sidebar:
- Global Search
- Add Item/Store
- Compare Prices
- Favorites
- Scan Barcode
- Price Drop Alerts
- Deal Reminders
- Weekly Specials
- Shopping Lists

---

## Managing Items

### Adding Items

1. Click "Add Item" in the Quick Actions or navigate to Items page
2. Fill in the item details:
   - **Name:** Product name (required)
   - **Brand:** Product brand
   - **Category:** Select from dropdown or type new category
   - **Barcode:** For quick lookup (optional)
   - **Package Size:** e.g., "500g", "2L"
   - **Description:** Additional details
3. Click "Save"

### Editing Items

1. Navigate to the Items page
2. Find the item you want to edit
3. Click the "Edit" button
4. Modify the details
5. Click "Save"

### Searching Items

- Use the search box to filter by name, brand, or category
- Use category filters to narrow down results
- Sort by name, date added, or last updated

### Marking Favorites

Click the ⭐ icon next to an item to add it to your favorites for quick access.

---

## Managing Stores

### Adding Stores

1. Click "Add Store" in the Quick Actions
2. Enter store details:
   - **Name:** Store name (e.g., "Coles Indooroopilly")
   - **Chain:** Store chain (e.g., "Coles", "Woolworths")
   - **Address:** Street address
   - **Suburb:** Suburb name
   - **State:** State abbreviation (QLD, NSW, etc.)
   - **Postcode:** ZIP code
3. Click "Save"

### Store Management

- View all stores in the Stores page
- Edit store details as needed
- Stores are used when recording prices

---

## Price History

### Viewing Price History

1. Navigate to Price History page
2. Use filters to find specific items:
   - Select item from dropdown
   - Select store
   - Set date range
3. View price trends in the chart

### Adding Price Records

1. Click "Add Price Record"
2. Select the item and store
3. Enter price details:
   - **Price:** Current price
   - **Original Price:** For sale items (optional)
   - **Sale Description:** e.g., "Half Price", "Buy 2 Get 1 Free"
   - **Valid From/To:** Sale period
4. Click "Save"

### Understanding Price Statistics

- **Current Price:** Latest recorded price
- **Lowest Price:** Historical minimum
- **Highest Price:** Historical maximum
- **Average Price:** Mean price over time

---

## Shopping Lists

### Creating a Shopping List

1. Click "🛒 Shopping Lists" in Quick Actions
2. Enter a name for your list (e.g., "Weekly Groceries")
3. Click "➕ Create"

### Adding Items to Lists

1. Select a shopping list from the left panel
2. Type item name in the "Add an item" box
3. Press Enter or click "➕ Add"
4. Items can be:
   - Checked off when purchased
   - Removed if not needed
   - Reordered by priority

### Managing Lists

- **Duplicate:** Create a copy of an existing list
- **Favorite:** Star important lists for quick access
- **Export:** Save list as Markdown file
- **Clear Completed:** Remove checked items
- **Clear All:** Remove all items

### Progress Tracking

The progress bar shows how many items you've checked off. Use this to track your shopping progress.

---

## Reports & Analytics

### Viewing Reports

1. Click "Reports" in the navigation sidebar
2. View key statistics:
   - Total items tracked
   - Stores monitored
   - Active deals
   - Average savings

### Best Deals

The "🔥 Best Deals This Week" section shows current specials ranked by savings amount. Each deal displays:
- Item name and store
- Sale price
- Savings amount
- Discount percentage

---

## Import & Export

### Importing Price Data

1. Click "Import JSON Data" from the File menu
2. Select a JSON file from supported sources:
   - Coles catalogues
   - Woolworths catalogues
   - Drakes markdown files
3. Preview the import
4. Click "Import" to add to your database

### Supported Import Formats

**Coles/Woolworths JSON:**
```json
[{
  "productID": "CL001",
  "productName": "Product Name",
  "brand": "Brand",
  "category": "Category",
  "price": "$2.75",
  "originalPrice": "$5.50",
  "savings": "$2.75",
  "specialType": "Half Price Special"
}]
```

**Drakes Markdown:**
```markdown
# Drakes Supermarket Specials
**Valid: 28 January 2026 - 3 February 2026**

## Groceries
- **Product Name** 500g - $5 ea - SAVE $2
```

### Exporting Data

1. Click "Export Data" from the Quick Actions
2. Choose export options:
   - Date range
   - Stores to include
   - Categories to include
   - Include only sale items
3. Choose format (JSON or compressed JSON.GZ)
4. Select save location
5. Click "Export"

---

## Special Features

### Global Search

Press `Ctrl+F` or click "🔍 Global Search" to search across:
- Items (by name, brand, barcode, category)
- Stores (by name, suburb, chain)
- Price Records (by price value)

Results are ranked by relevance. Click any result to navigate to it.

### Barcode Scanning

1. Click "📷 Scan Barcode"
2. Point your webcam at a product barcode
3. The app will automatically detect and search for the item
4. Add to database or view existing price information

### Price Drop Alerts

1. Navigate to Price History
2. Find the item you want to monitor
3. Click "Create Alert"
4. Set conditions:
   - Target price
   - Percentage drop
   - Specific stores
5. Click "Create Alert"

You'll be notified when the price meets your criteria.

### Deal Expiration Reminders

1. Click "⏰ Deal Reminders"
2. View deals expiring soon
3. Click "Dismiss" to remove reminders for deals you're not interested in
4. Use "Clear All Dismissed" to reset

### Weekly Specials Digest

1. Click "📰 Weekly Specials"
2. View curated specials from major retailers
3. Filter by category or store
4. Export to Markdown or copy to clipboard
5. Use the digest to plan your shopping

### Compare Prices

1. Click "Compare Prices" in Quick Actions
2. Select a category (optional)
3. View side-by-side price comparison across stores
4. Identify the best value stores

---

## Tips & Tricks

### Saving Time

- **Use Barcode Scanning:** Quickly add items without typing
- **Create Templates:** Duplicate shopping lists for recurring trips
- **Favorite Items:** Star frequently purchased items for quick access
- **Global Search:** Use `Ctrl+F` to find anything instantly

### Saving Money

- **Track Historical Prices:** Know what a "good" price really is
- **Set Price Alerts:** Get notified of genuine price drops
- **Compare Before Buying:** Use the Compare Prices feature
- **Check Weekly Specials:** Plan shopping around current deals

### Best Practices

- **Regular Updates:** Record prices weekly for accurate tracking
- **Categorize Items:** Proper categorization helps with reports
- **Add Sale Dates:** Include valid-from/to dates for sale items
- **Export Backups:** Regularly export your data as backup

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+F` | Global Search |
| `Ctrl+I` | Add Item |
| `Ctrl+S` | Add Store |
| `F5` | Refresh Dashboard |
| `Ctrl+O` | Import Data |
| `Ctrl+E` | Export Data |

---

## Troubleshooting

### Application Won't Start

**Problem:** Application crashes on startup  
**Solution:**
1. Check that .NET 9.0 Runtime is installed
2. Delete `%AppData%\AdvGenPriceComparer\` and restart
3. Check Windows Event Viewer for errors

### Import Fails

**Problem:** JSON import returns errors  
**Solution:**
1. Verify JSON file format matches supported formats
2. Check file encoding (UTF-8 recommended)
3. Ensure required fields are present (productName, price)
4. Try smaller batches if file is very large

### Database Issues

**Problem:** Items not saving or disappearing  
**Solution:**
1. Check disk space availability
2. Verify write permissions to `%AppData%\AdvGenPriceComparer\`
3. Try repairing the database from Settings

### Performance Issues

**Problem:** Application is slow or unresponsive  
**Solution:**
1. Close other applications to free RAM
2. Reduce date range in Price History filters
3. Archive old price records via export/delete

### Barcode Scanning Not Working

**Problem:** Barcode scanner doesn't detect codes  
**Solution:**
1. Ensure webcam is connected and accessible
2. Check lighting - avoid glare on barcode
3. Hold product steady 10-15cm from camera
4. Try different angles

### Getting Help

- **Check Logs:** View logs at `%AppData%\AdvGenPriceComparer\Logs\`
- **GitHub Issues:** Report bugs on the project GitHub page
- **Documentation:** Review this guide and the README.md

---

## Data Privacy

AdvGen Price Comparer stores all data locally on your computer:
- Database: `%AppData%\AdvGenPriceComparer\GroceryPrices.db`
- Settings: `%AppData%\AdvGenPriceComparer\`
- Logs: `%AppData%\AdvGenPriceComparer\Logs\`

No data is sent to external servers unless you explicitly use the P2P sharing feature (if enabled).

---

## Glossary

| Term | Definition |
|------|------------|
| **Illusory Discount** | A "sale" price that is actually the normal price or higher |
| **Price Record** | A specific price observation for an item at a store |
| **Category** | Grouping for items (e.g., "Dairy", "Produce") |
| **Chain** | Store brand (e.g., "Coles", "Woolworths") |
| **Special** | A temporary price reduction or promotion |

---

**Happy Price Tracking!** 🛒💰
