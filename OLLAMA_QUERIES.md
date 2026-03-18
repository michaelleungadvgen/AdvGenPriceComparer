# Ollama Chat Interface - Supported Query Types

**Version:** 1.0  
**Last Updated:** 2026-03-18  
**Applies to:** AdvGenPriceComparer WPF Application v1.0+

---

## Overview

The Ollama Chat Interface in AdvGenPriceComparer allows users to query grocery price data using natural language. This document describes all supported query types, their usage, and examples.

---

## Query Types

### 1. Price Query (`priceQuery`)

Get current price(s) for specific product(s).

**Natural Language Examples:**
- "What's the price of milk?"
- "How much does bread cost?"
- "Show me the price of Dairy Farmers milk at Coles"
- "What is the current price of eggs?"
- "Tell me how much butter costs"

**Supported Variations:**
- With or without store specification
- With or without brand specification
- Singular or plural product names
- Using "cost", "price", "how much"

**SPQL Output:**
```json
{
  "version": "1.0",
  "queryType": "priceQuery",
  "target": "Both",
  "filters": {
    "product": {
      "name": "milk",
      "nameMatch": "contains"
    },
    "store": {
      "chain": "Coles"
    }
  },
  "options": {
    "limit": 10
  }
}
```

---

### 2. Price Comparison (`priceComparison`)

Compare prices across multiple stores.

**Natural Language Examples:**
- "Compare milk prices between Coles and Woolworths"
- "Which store has cheaper bread?"
- "Price difference for eggs between stores"
- "Compare prices of butter at Coles vs Woolworths"
- "Where can I get milk for less?"
- "Show me price comparison for cheese"

**Supported Variations:**
- "Compare X at Store A and Store B"
- "Which store has cheaper X"
- "Price difference for X"
- "Where is X cheaper"
- Using "vs", "versus", "between", "at"

**SPQL Output:**
```json
{
  "version": "1.0",
  "queryType": "priceComparison",
  "target": "Both",
  "filters": {
    "product": {
      "name": "milk",
      "nameMatch": "contains"
    },
    "store": {
      "chains": ["Coles", "Woolworths"]
    }
  },
  "options": {
    "sortBy": "price",
    "sortOrder": "asc"
  }
}
```

---

### 3. Cheapest Item (`cheapestItemQuery`)

Find cheapest options for a product type.

**Natural Language Examples:**
- "What's the cheapest bread?"
- "Find the cheapest milk"
- "Show me the lowest price for eggs"
- "What is the most affordable butter?"
- "Where can I find cheap cheese?"
- "Best price for chicken"

**Supported Variations:**
- "Cheapest X"
- "Lowest price for X"
- "Most affordable X"
- "Best price for X"
- "Where can I find cheap X"

**SPQL Output:**
```json
{
  "version": "1.0",
  "queryType": "cheapestItemQuery",
  "target": "Both",
  "filters": {
    "product": {
      "keywords": ["bread"]
    }
  },
  "options": {
    "limit": 5,
    "includeUnitPrice": true
  }
}
```

---

### 4. Category Query (`categoryQuery`)

Get all items in a category.

**Natural Language Examples:**
- "Show me all dairy products"
- "What vegetables are available?"
- "List all beverages under $5"
- "Show me the meat and seafood section"
- "What bakery items do you have?"
- "Display all frozen foods"
- "Show snacks and confectionery"

**Supported Variations:**
- "Show me all X"
- "What X are available"
- "List all X"
- "Display all X"
- With or without price filters

**SPQL Output:**
```json
{
  "version": "1.0",
  "queryType": "categoryQuery",
  "target": "LiteDB",
  "filters": {
    "category": {
      "name": "Dairy & Eggs"
    },
    "price": {
      "max": 5.00
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

### 5. Items On Sale (`saleItemsQuery`)

Find items currently on sale.

**Natural Language Examples:**
- "What's on sale this week?"
- "Show me all items on sale"
- "What are the current specials?"
- "Show me deals at Woolworths"
- "What's on special at Coles?"
- "List all discounted items"
- "Show me the sales"

**Supported Variations:**
- "On sale"
- "On special"
- "Discounted"
- "Deals"
- "Specials"
- With or without store specification

**SPQL Output:**
```json
{
  "version": "1.0",
  "queryType": "saleItemsQuery",
  "target": "Both",
  "filters": {
    "store": {
      "chain": "Woolworths"
    },
    "price": {
      "minDiscount": 20,
      "minDiscountType": "percent"
    }
  },
  "options": {
    "limit": 50,
    "sortBy": "discountPercent",
    "sortOrder": "desc"
  }
}
```

---

### 6. Price History (`priceHistoryQuery`)

Get historical price data for a product.

**Natural Language Examples:**
- "Show milk price history"
- "How has the price of bread changed?"
- "Price trends for eggs over the last month"
- "Show me historical prices for butter"
- "What was the price of milk last month?"
- "Track price changes for cheese"

**Supported Variations:**
- "X price history"
- "How has X price changed"
- "Price trends for X"
- "Historical prices for X"
- With or without time period specification

**SPQL Output:**
```json
{
  "version": "1.0",
  "queryType": "priceHistoryQuery",
  "target": "Both",
  "filters": {
    "product": {
      "name": "milk"
    },
    "time": {
      "daysBack": 30
    }
  },
  "options": {
    "sortBy": "date",
    "sortOrder": "desc",
    "aggregation": "daily"
  }
}
```

---

### 7. Best Deals (`bestDealsQuery`)

Find the best current deals.

**Natural Language Examples:**
- "What are the best deals?"
- "Show me the biggest discounts"
- "Find the best savings"
- "What has the highest discount?"
- "Show me half price specials"
- "Find the best bargains"

**Supported Variations:**
- "Best deals"
- "Biggest discounts"
- "Best savings"
- "Highest discount"
- "Bargains"
- With or without category/store filters

**SPQL Output:**
```json
{
  "version": "1.0",
  "queryType": "bestDealsQuery",
  "target": "Both",
  "filters": {
    "price": {
      "minDiscount": 30,
      "minDiscountType": "percent"
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

### 8. Store Inventory (`storeInventoryQuery`)

Get available items at a specific store.

**Natural Language Examples:**
- "What products are available at Coles?"
- "Show me what Woolworths has"
- "What does Aldi sell?"
- "List all items at Drakes"
- "Show me the inventory at Coles Chermside"
- "What can I buy at Woolworths?"

**Supported Variations:**
- "What products at Store X"
- "What does Store X have"
- "What can I buy at Store X"
- "Show me Store X inventory"
- With or without category filters

**SPQL Output:**
```json
{
  "version": "1.0",
  "queryType": "storeInventoryQuery",
  "target": "Both",
  "filters": {
    "store": {
      "chain": "Coles"
    },
    "category": {
      "names": ["Dairy & Eggs"]
    }
  },
  "options": {
    "limit": 100
  }
}
```

---

### 9. Budget Query (`budgetQuery`)

Find items within a budget.

**Natural Language Examples:**
- "What can I buy for $50?"
- "Show me items under $10"
- "What groceries can I get for $100?"
- "Find products within my $30 budget"
- "What can I afford with $25?"
- "Show me budget-friendly groceries"

**Supported Variations:**
- "What can I buy for $X"
- "Items under $X"
- "Within $X budget"
- "Affordable with $X"
- "Budget-friendly"

**SPQL Output:**
```json
{
  "version": "1.0",
  "queryType": "budgetQuery",
  "target": "LiteDB",
  "filters": {
    "price": {
      "totalBudget": 50.00,
      "maxItemPrice": 10.00
    }
  },
  "options": {
    "limit": 100,
    "sortBy": "price",
    "sortOrder": "asc",
    "optimizeFor": "quantity"
  }
}
```

---

## Query Parameters

### Product Filters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `name` | Product name | "milk", "bread" |
| `nameMatch` | Match type: `exact`, `contains`, `startsWith` | "contains" |
| `brand` | Brand name | "Dairy Farmers", "Helga's" |
| `barcode` | EAN/UPC barcode | "9300632123456" |
| `keywords` | Search keywords | ["bread", "white"] |

### Price Filters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `min` | Minimum price | 0.00 |
| `max` | Maximum price | 10.00 |
| `minDiscount` | Minimum discount amount/percent | 20 |
| `minDiscountType` | `amount` or `percent` | "percent" |
| `totalBudget` | Total budget constraint | 50.00 |

### Store Filters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `chain` | Supermarket chain | "Coles", "Woolworths" |
| `name` | Exact store name | "Coles Chermside" |
| `suburb` | Suburb name | "Chermside" |
| `state` | State code | "QLD", "NSW" |

### Time Filters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `from` | Start date (ISO 8601) | "2026-01-01T00:00:00Z" |
| `to` | End date (ISO 8601) | "2026-02-25T23:59:59Z" |
| `daysBack` | Last N days | 30 |

### Options

| Parameter | Description | Default |
|-----------|-------------|---------|
| `limit` | Maximum results | 10 |
| `sortBy` | Sort field: `price`, `name`, `date`, `discount` | "price" |
| `sortOrder` | Sort direction: `asc`, `desc` | "asc" |
| `includeHistory` | Include price history | false |
| `verifyDiscount` | Verify discount against history | true |

---

## Tips for Best Results

### 1. Be Specific
- ✅ "What's the price of Dairy Farmers milk 2L at Coles?"
- ❌ "Tell me about milk"

### 2. Include Store Names
- ✅ "Compare bread prices at Coles and Woolworths"
- ❌ "Compare bread prices"

### 3. Use Price Filters
- ✅ "Show me dairy products under $5"
- ❌ "Show me dairy products"

### 4. Specify Time Periods for History
- ✅ "Show me milk price history for the last 3 months"
- ❌ "Show me milk price history"

### 5. Category Names
Use these standard category names for best results:
- Meat & Seafood
- Dairy & Eggs
- Fruits & Vegetables
- Bakery
- Pantry Staples
- Snacks & Confectionery
- Beverages
- Frozen Foods
- Household Products
- Personal Care
- Baby Products
- Pet Care
- Health & Wellness

---

## Error Handling

If a query cannot be understood, the system will:
1. Return a friendly error message
2. Suggest alternative query formats
3. Provide examples of supported queries

**Example Error Response:**
```
I'm sorry, I didn't understand your query. Try asking:
- "What's the price of milk?"
- "What's the cheapest bread?"
- "What's on sale at Coles?"
```

---

## Related Documentation

- [SPQL Specification](plan.md#1216-standard-price-query-language-spql---json-specification) - Complete JSON query specification
- [ML.NET Price Prediction](PRICE_FORECASTING.md) - Price forecasting documentation
- [User Guide](USER_GUIDE.md) - General application usage

---

**Note:** This documentation applies to Ollama Chat Interface with llama3.2 model or compatible. Query recognition accuracy may vary based on model and phrasing.
