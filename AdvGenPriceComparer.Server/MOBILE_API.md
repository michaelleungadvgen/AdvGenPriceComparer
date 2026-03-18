# Mobile Companion App API Documentation

**Version:** 1.0  
**Last Updated:** 2026-03-18  
**Protocol:** HTTP/REST with JWT Authentication

---

## Overview

The Mobile Companion App API provides optimized endpoints specifically designed for mobile clients. These endpoints offer:

- **Compact Data Format**: Reduced payload size for mobile networks
- **Offline-First Support**: Sync endpoints for local data caching
- **Battery-Efficient**: Batched operations and efficient pagination
- **Location-Aware**: Nearby store discovery
- **Quick Actions**: Fast price checks and barcode scanning

---

## Base URL

```
https://{server-host}:{port}/api/mobile
```

**Example:**
```
https://localhost:5001/api/mobile
```

---

## Authentication

Mobile API uses the same API key authentication as the main API.

### Header Format

```http
X-API-Key: your-api-key-here
```

---

## Mobile-Specific Endpoints

### 1. Dashboard Summary

Get a compact summary for the mobile app dashboard.

```http
GET /api/mobile/dashboard
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalItems": 1500,
    "totalStores": 45,
    "activeDeals": 320,
    "averageSavings": 24.5,
    "recentUpdates": [
      {
        "itemId": 1,
        "itemName": "Milk 2L",
        "storeName": "Coles",
        "oldPrice": 3.50,
        "newPrice": 2.80,
        "changePercent": -20,
        "timestamp": "2026-03-18T10:30:00Z"
      }
    ],
    "bestDealsToday": [
      {
        "itemId": 2,
        "itemName": "Bread 700g",
        "storeName": "Woolworths",
        "price": 2.50,
        "originalPrice": 4.00,
        "savingsPercent": 37.5
      }
    ]
  }
}
```

---

### 2. Quick Price Check

Fast price lookup by item name or barcode.

```http
GET /api/mobile/price-check?query={search}&barcode={barcode}
```

**Parameters:**
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `query` | string | No* | Item name to search |
| `barcode` | string | No* | Barcode to lookup |

*At least one parameter required

**Response:**
```json
{
  "success": true,
  "data": {
    "itemId": 1,
    "itemName": "Full Cream Milk 2L",
    "brand": "Dairy Farmers",
    "category": "Dairy & Eggs",
    "barcode": "9300632123456",
    "prices": [
      {
        "storeId": 1,
        "storeName": "Coles Chermside",
        "chain": "Coles",
        "price": 2.80,
        "originalPrice": 3.50,
        "specialType": "Half Price",
        "validUntil": "2026-03-25T00:00:00Z",
        "distance": 1.2
      },
      {
        "storeId": 2,
        "storeName": "Woolworths Chermside",
        "chain": "Woolworths",
        "price": 3.20,
        "originalPrice": null,
        "specialType": null,
        "validUntil": null,
        "distance": 1.5
      }
    ],
    "bestPrice": {
      "storeName": "Coles Chermside",
      "price": 2.80,
      "savings": 0.70
    }
  }
}
```

---

### 3. Nearby Stores

Find stores near the user's location.

```http
GET /api/mobile/nearby-stores?lat={latitude}&lng={longitude}&radius={km}
```

**Parameters:**
| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `lat` | double | Yes | - | User's latitude |
| `lng` | double | Yes | - | User's longitude |
| `radius` | double | No | 10 | Search radius in kilometers |
| `chain` | string | No | null | Filter by chain (e.g., "Coles") |

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Coles Chermside",
      "chain": "Coles",
      "address": "123 Main St",
      "suburb": "Chermside",
      "state": "QLD",
      "postcode": "4032",
      "latitude": -27.3858,
      "longitude": 153.0314,
      "distance": 1.2,
      "bearing": "NE",
      "currentDeals": 45
    }
  ]
}
```

---

### 4. Compact Items List

Get items in a compact format optimized for mobile.

```http
GET /api/mobile/items?page={page}&pageSize={pageSize}&category={category}
```

**Parameters:**
| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `page` | int | No | 1 | Page number |
| `pageSize` | int | No | 50 | Items per page (max 100) |
| `category` | string | No | null | Filter by category |
| `search` | string | No | null | Search term |

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Milk 2L",
      "brand": "Dairy Farmers",
      "category": "Dairy & Eggs",
      "barcode": "9300632123456",
      "bestPrice": 2.80,
      "bestStore": "Coles Chermside",
      "avgPrice": 3.15,
      "priceChange": -0.35
    }
  ],
  "totalCount": 1500,
  "page": 1,
  "pageSize": 50
}
```

---

### 5. Shopping List Sync

Synchronize shopping lists between mobile app and server.

#### Get Shopping Lists

```http
GET /api/mobile/shopping-lists
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "list-1",
      "name": "Weekly Groceries",
      "isFavorite": true,
      "itemCount": 12,
      "completedCount": 5,
      "totalEstimatedCost": 85.50,
      "lastModified": "2026-03-18T09:00:00Z",
      "items": [
        {
          "id": "item-1",
          "name": "Milk",
          "quantity": 2,
          "unit": "L",
          "isChecked": false,
          "estimatedPrice": 2.80,
          "bestStore": "Coles"
        }
      ]
    }
  ]
}
```

#### Create/Update Shopping List

```http
POST /api/mobile/shopping-lists
Content-Type: application/json

{
  "id": "list-1",
  "name": "Weekly Groceries",
  "items": [
    {
      "name": "Milk",
      "quantity": 2,
      "unit": "L",
      "isChecked": false
    }
  ]
}
```

#### Delete Shopping List

```http
DELETE /api/mobile/shopping-lists/{listId}
```

#### Sync Shopping List (Delta Sync)

```http
POST /api/mobile/shopping-lists/sync
Content-Type: application/json

{
  "clientLastSync": "2026-03-18T08:00:00Z",
  "lists": [
    {
      "id": "list-1",
      "lastModified": "2026-03-18T09:00:00Z"
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "serverTime": "2026-03-18T10:00:00Z",
    "listsToUpdate": [
      {
        "id": "list-1",
        "action": "update",
        "data": { ... }
      }
    ],
    "listsToDelete": ["list-2"]
  }
}
```

---

### 6. Barcode Lookup

Lookup item by barcode with price comparison.

```http
GET /api/mobile/barcode/{barcode}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "found": true,
    "item": {
      "id": 1,
      "name": "Coca-Cola 2L",
      "brand": "Coca-Cola",
      "category": "Beverages",
      "barcode": "9300632001234",
      "size": "2L"
    },
    "prices": [
      {
        "storeId": 1,
        "storeName": "Coles",
        "chain": "Coles",
        "price": 2.50,
        "originalPrice": 3.40,
        "specialType": "Special",
        "validUntil": "2026-03-25T00:00:00Z"
      }
    ],
    "bestDeal": {
      "storeName": "Coles",
      "price": 2.50,
      "savings": 0.90
    },
    "priceHistory": {
      "average30Day": 3.10,
      "lowest30Day": 2.50,
      "highest30Day": 3.50
    }
  }
}
```

---

### 7. Quick Deal Feed

Get a scrolling feed of current deals (optimized for mobile viewing).

```http
GET /api/mobile/deals?limit={limit}&category={category}
```

**Parameters:**
| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `limit` | int | No | 20 | Number of deals |
| `category` | string | No | null | Filter by category |
| `storeId` | int | No | null | Filter by store |

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "itemId": 1,
      "itemName": "Tim Tams",
      "brand": "Arnott's",
      "size": "200g",
      "storeName": "Woolworths",
      "chain": "Woolworths",
      "price": 2.50,
      "originalPrice": 4.50,
      "savings": 2.00,
      "savingsPercent": 44.4,
      "specialType": "Half Price",
      "validUntil": "2026-03-25T00:00:00Z",
      "category": "Snacks & Confectionery"
    }
  ]
}
```

---

### 8. Push Notification Registration

Register device for push notifications.

```http
POST /api/mobile/push-register
Content-Type: application/json

{
  "deviceToken": "fcm-device-token-here",
  "platform": "android",
  "deviceId": "unique-device-id",
  "preferences": {
    "priceDrops": true,
    "dealAlerts": true,
    "weeklyDigest": false
  }
}
```

### Unregister Device

```http
POST /api/mobile/push-unregister
Content-Type: application/json

{
  "deviceToken": "fcm-device-token-here"
}
```

---

### 9. Price Alert Management

Manage price alerts from mobile app.

#### Get Price Alerts

```http
GET /api/mobile/price-alerts
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "alert-1",
      "itemId": 1,
      "itemName": "Milk 2L",
      "targetPrice": 2.50,
      "currentPrice": 2.80,
      "condition": "Below",
      "isActive": true,
      "createdAt": "2026-03-15T00:00:00Z"
    }
  ]
}
```

#### Create Price Alert

```http
POST /api/mobile/price-alerts
Content-Type: application/json

{
  "itemId": 1,
  "targetPrice": 2.50,
  "condition": "Below"
}
```

#### Delete Price Alert

```http
DELETE /api/mobile/price-alerts/{alertId}
```

---

## Data Models

### MobileDashboardSummary
```json
{
  "totalItems": 1500,
  "totalStores": 45,
  "activeDeals": 320,
  "averageSavings": 24.5,
  "recentUpdates": [MobilePriceUpdate],
  "bestDealsToday": [MobileDeal]
}
```

### MobilePriceCheckResult
```json
{
  "itemId": 1,
  "itemName": "string",
  "brand": "string",
  "category": "string",
  "barcode": "string",
  "prices": [MobileStorePrice],
  "bestPrice": MobileBestPrice
}
```

### MobileStorePrice
```json
{
  "storeId": 1,
  "storeName": "string",
  "chain": "string",
  "price": 2.80,
  "originalPrice": 3.50,
  "specialType": "string",
  "validUntil": "2026-03-25T00:00:00Z",
  "distance": 1.2
}
```

### MobileShoppingList
```json
{
  "id": "string",
  "name": "string",
  "isFavorite": true,
  "itemCount": 12,
  "completedCount": 5,
  "totalEstimatedCost": 85.50,
  "lastModified": "2026-03-18T09:00:00Z",
  "items": [MobileShoppingListItem]
}
```

### MobileShoppingListItem
```json
{
  "id": "string",
  "name": "string",
  "quantity": 2,
  "unit": "string",
  "isChecked": false,
  "estimatedPrice": 2.80,
  "bestStore": "string"
}
```

---

## Error Handling

Mobile API uses the same error format as the main API:

```json
{
  "error": "Error description",
  "details": "Optional detailed error message"
}
```

### Mobile-Specific Error Codes

| Code | Description |
|------|-------------|
| `MOBILE_INVALID_LOCATION` | Invalid latitude/longitude |
| `MOBILE_SYNC_CONFLICT` | Shopping list sync conflict |
| `MOBILE_BARCODE_NOT_FOUND` | Barcode not in database |

---

## Rate Limiting

Mobile API has specific rate limits:

- **Dashboard:** 30 requests per minute
- **Price Check:** 60 requests per minute
- **Barcode Lookup:** 100 requests per minute
- **Shopping List Sync:** 30 requests per minute
- **Nearby Stores:** 30 requests per minute

---

## Implementation Notes

1. **Data Compression**: Mobile API supports gzip compression. Add `Accept-Encoding: gzip` header.

2. **Caching**: Mobile clients should cache:
   - Dashboard data: 5 minutes
   - Nearby stores: 15 minutes
   - Price checks: 1 minute
   - Shopping lists: Sync-based

3. **Offline Support**: Shopping lists support offline-first with delta sync.

4. **Location**: Nearby stores endpoint requires location permissions.

5. **Push Notifications**: Device tokens should be refreshed periodically.

---

## Related Files

- Controller: `AdvGenPriceComparer.Server/Controllers/MobileApiController.cs`
- Models: `AdvGenPriceComparer.Server/Models/Mobile*.cs`
- Main API: `AdvGenPriceComparer.Server/API_PROTOCOL.md`
