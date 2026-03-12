# AdvGenNoSQLServer API Protocol Documentation

**Version:** 1.0  
**Last Updated:** 2026-03-06  
**Protocol:** HTTP/REST with SignalR WebSocket support

---

## Table of Contents

1. [Overview](#overview)
2. [Base URL](#base-url)
3. [Authentication](#authentication)
4. [Common Headers](#common-headers)
5. [Response Format](#response-format)
6. [API Endpoints](#api-endpoints)
   - [Items API](#items-api)
   - [Places API](#places-api)
   - [Prices API](#prices-api)
7. [Real-Time Updates (SignalR)](#real-time-updates-signalr)
8. [Error Handling](#error-handling)
9. [Rate Limiting](#rate-limiting)
10. [Data Models](#data-models)
11. [Client Implementation](#client-implementation)

---

## Overview

The AdvGenNoSQLServer API provides a RESTful interface for the AdvGenPriceComparer WPF application to share grocery price data across multiple clients. The API supports:

- CRUD operations for items (products), places (stores), and price records
- Price comparison across stores
- Real-time price updates via SignalR
- API key-based authentication
- Rate limiting for fair usage

---

## Base URL

```
https://{server-host}:{port}/api
```

**Example:**
```
https://localhost:5001/api
```

**Note:** The client implementation uses `/api/v1/` prefix, but the current server uses `/api/` directly. Ensure your client is configured with the correct base path.

---

## Authentication

All API requests (except health checks) require authentication via an API key.

### Header Format

```http
X-API-Key: your-api-key-here
```

### Obtaining an API Key

API keys are managed server-side through the `ApiKeyService`. Contact your server administrator to obtain a valid API key.

### API Key Validation

The server validates API keys using the `ApiKeyMiddleware`. Invalid or missing keys will result in:

```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "error": "Invalid or missing API key"
}
```

---

## Common Headers

### Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| `X-API-Key` | Yes | Your API key for authentication |
| `X-Database-Name` | No | Target database name (default: "GroceryPrices") |
| `Accept` | No | Should be `application/json` |
| `Content-Type` | Yes (for POST/PUT) | Must be `application/json` |

### Response Headers

| Header | Description |
|--------|-------------|
| `Content-Type` | `application/json` |
| `X-RateLimit-Limit` | Maximum requests allowed per window |
| `X-RateLimit-Remaining` | Remaining requests in current window |
| `X-RateLimit-Reset` | Unix timestamp when limit resets |

---

## Response Format

### Success Response

All successful responses follow this wrapper format:

```json
{
  "success": true,
  "message": "Optional success message",
  "data": { ... }
}
```

For list responses:

```json
{
  "success": true,
  "message": null,
  "data": [ ... ],
  "totalCount": 100
}
```

### Error Response

```json
{
  "error": "Error description",
  "details": "Optional detailed error message"
}
```

---

## API Endpoints

### Items API

Base path: `/api/items`

#### Get All Items

```http
GET /api/items?page={page}&pageSize={pageSize}&category={category}&brand={brand}&search={search}
```

**Parameters:**
| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `page` | int | No | 1 | Page number (1-based) |
| `pageSize` | int | No | 100 | Items per page (max 1000) |
| `category` | string | No | null | Filter by category |
| `brand` | string | No | null | Filter by brand |
| `search` | string | No | null | Search in item name |

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "productId": "PROD001",
      "name": "Full Cream Milk 2L",
      "brand": "Dairy Farmers",
      "category": "Dairy & Eggs",
      "description": "Fresh full cream milk",
      "barcode": "9300632123456",
      "unit": "L",
      "size": "2L",
      "createdAt": "2026-01-01T00:00:00Z",
      "updatedAt": "2026-03-01T00:00:00Z"
    }
  ],
  "totalCount": 150
}
```

#### Get Item by ID

```http
GET /api/items/{id}
```

**Response:** `200 OK` with item object, or `404 Not Found`

#### Get Item by Product ID

```http
GET /api/items/by-product-id/{productId}
```

#### Create/Update Item (Upsert)

```http
POST /api/items
Content-Type: application/json

{
  "productId": "PROD001",
  "name": "Full Cream Milk 2L",
  "brand": "Dairy Farmers",
  "category": "Dairy & Eggs",
  "description": "Fresh full cream milk",
  "barcode": "9300632123456",
  "unit": "L",
  "size": "2L"
}
```

**Response:** `200 OK` with created/updated item

#### Batch Upsert Items

```http
POST /api/items/batch
Content-Type: application/json

[
  { ... item data ... },
  { ... item data ... }
]
```

**Limits:** Maximum 1000 items per batch

#### Search Items

```http
GET /api/items/search?query={query}&limit={limit}
```

**Parameters:**
| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `query` | string | Yes | - | Search query |
| `limit` | int | No | 20 | Maximum results (max 100) |

#### Get Items by Category

```http
GET /api/items/by-category/{category}?page={page}&pageSize={pageSize}
```

#### Get Items by Brand

```http
GET /api/items/by-brand/{brand}?page={page}&pageSize={pageSize}
```

---

### Places API

Base path: `/api/places`

#### Get All Places

```http
GET /api/places?page={page}&pageSize={pageSize}&chain={chain}&state={state}&search={search}
```

**Parameters:**
| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `page` | int | No | 1 | Page number |
| `pageSize` | int | No | 100 | Places per page (max 1000) |
| `chain` | string | No | null | Filter by chain (e.g., "Coles") |
| `state` | string | No | null | Filter by state (e.g., "QLD") |
| `search` | string | No | null | Search query |

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "storeId": "STORE001",
      "name": "Coles Chermside",
      "chain": "Coles",
      "address": "123 Main St",
      "suburb": "Chermside",
      "state": "QLD",
      "postcode": "4032",
      "country": "Australia",
      "latitude": -27.3858,
      "longitude": 153.0314,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ]
}
```

#### Get Place by ID

```http
GET /api/places/{id}
```

#### Create/Update Place

```http
POST /api/places
Content-Type: application/json

{
  "storeId": "STORE001",
  "name": "Coles Chermside",
  "chain": "Coles",
  "address": "123 Main St",
  "suburb": "Chermside",
  "state": "QLD",
  "postcode": "4032",
  "country": "Australia",
  "latitude": -27.3858,
  "longitude": 153.0314
}
```

#### Batch Upsert Places

```http
POST /api/places/batch
Content-Type: application/json

[
  { ... place data ... }
]
```

**Limits:** Maximum 500 places per batch

#### Get Places by Chain

```http
GET /api/places/by-chain/{chain}?page={page}&pageSize={pageSize}
```

#### Get Places by State

```http
GET /api/places/by-state/{state}?page={page}&pageSize={pageSize}
```

#### Search Places

```http
GET /api/places/search?query={query}&limit={limit}
```

#### Get All Chains

```http
GET /api/places/chains
```

**Response:** Array of unique chain names

#### Get All States

```http
GET /api/places/states
```

---

### Prices API

Base path: `/api/prices`

#### Upload Price Data

```http
POST /api/prices/upload
Content-Type: application/json

{
  "items": [ ... ],
  "places": [ ... ],
  "priceRecords": [ ... ]
}
```

**Response:**
```json
{
  "success": true,
  "itemsUploaded": 50,
  "placesUploaded": 3,
  "pricesUploaded": 150,
  "errors": []
}
```

#### Download Price Data

```http
GET /api/prices/download?storeId={storeId}&itemId={itemId}&from={from}&to={to}&page={page}&pageSize={pageSize}
```

**Parameters:**
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `storeId` | int | No | Filter by place/store ID |
| `itemId` | int | No | Filter by item ID |
| `from` | datetime | No | Start date (ISO 8601) |
| `to` | datetime | No | End date (ISO 8601) |
| `page` | int | No | Page number (default: 1) |
| `pageSize` | int | No | Items per page (default: 100, max: 1000) |

#### Search Products

```http
GET /api/prices/search?query={query}&limit={limit}
```

#### Compare Prices

```http
GET /api/prices/compare/{itemId}
```

Returns price records for the item across all stores.

#### Get Latest Deals

```http
GET /api/prices/latest?limit={limit}
```

**Parameters:**
| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `limit` | int | No | 50 | Maximum deals (max 200) |

#### Get Price History

```http
GET /api/prices/history/{itemId}?placeId={placeId}&from={from}&to={to}
```

**Parameters:**
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `itemId` | int | Yes | Item ID |
| `placeId` | int | No | Filter by place |
| `from` | datetime | No | Start date |
| `to` | datetime | No | End date |

#### Get Current Price

```http
GET /api/prices/current/{itemId}/{placeId}
```

Returns the most current price for an item at a specific store.

#### Get Server Statistics

```http
GET /api/prices/stats
```

**Response:**
```json
{
  "totalItems": 1000,
  "totalPlaces": 50,
  "totalPriceRecords": 5000,
  "latestUpdate": "2026-03-06T06:00:00Z"
}
```

---

## Real-Time Updates (SignalR)

The server supports real-time price updates via SignalR WebSocket connections.

### Hub Endpoint

```
/hubs/price-updates
```

### Connection URL

```
wss://{server-host}:{port}/hubs/price-updates
```

### Client Methods (Server → Client)

| Method | Description | Parameters |
|--------|-------------|------------|
| `PriceUpdated` | Notifies when a price changes | `itemId`, `placeId`, `newPrice`, `oldPrice` |
| `NewDealAvailable` | Notifies of new deals | `itemId`, `placeId`, `discountPercent` |
| `ItemAdded` | Notifies when new item added | `item` object |

### Server Methods (Client → Server)

| Method | Description | Parameters |
|--------|-------------|------------|
| `SubscribeToItem` | Subscribe to price updates for an item | `itemId` |
| `SubscribeToPlace` | Subscribe to updates from a store | `placeId` |
| `Unsubscribe` | Unsubscribe from updates | `itemId`, `placeId` |

### Example Connection (JavaScript)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/price-updates", {
        headers: { "X-API-Key": "your-api-key" }
    })
    .build();

connection.on("PriceUpdated", (itemId, placeId, newPrice, oldPrice) => {
    console.log(`Price updated: Item ${itemId} at ${placeId}: $${newPrice}`);
});

await connection.start();
await connection.invoke("SubscribeToItem", 123);
```

---

## Error Handling

### HTTP Status Codes

| Code | Meaning | Description |
|------|---------|-------------|
| `200 OK` | Success | Request succeeded |
| `400 Bad Request` | Invalid request | Missing or invalid parameters |
| `401 Unauthorized` | Authentication failed | Invalid or missing API key |
| `404 Not Found` | Resource not found | Item/Place doesn't exist |
| `429 Too Many Requests` | Rate limited | Request limit exceeded |
| `500 Internal Server Error` | Server error | Unexpected server error |

### Error Response Format

```json
{
  "error": "Human-readable error message",
  "details": "Additional error details (optional)"
}
```

---

## Rate Limiting

The API implements rate limiting via `RateLimitMiddleware` using a sliding window algorithm.

### Limits

- **Default:** 100 requests per minute per API key
- **Upload endpoint:** 10 requests per minute
- **WebSocket:** 1000 messages per minute

### Rate Limit Response

```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1709701200
Retry-After: 60

{
  "error": "Rate limit exceeded. Please try again in 60 seconds."
}
```

---

## Data Models

### SharedItem

```json
{
  "id": 1,
  "productId": "string",
  "name": "string",
  "brand": "string?",
  "category": "string?",
  "description": "string?",
  "barcode": "string?",
  "unit": "string?",
  "size": "string?",
  "createdAt": "2026-01-01T00:00:00Z",
  "updatedAt": "2026-01-01T00:00:00Z",
  "priceRecords": []
}
```

### SharedPlace

```json
{
  "id": 1,
  "storeId": "string",
  "name": "string",
  "chain": "string?",
  "address": "string?",
  "suburb": "string?",
  "state": "string?",
  "postcode": "string?",
  "country": "string",
  "latitude": 0.0,
  "longitude": 0.0,
  "createdAt": "2026-01-01T00:00:00Z",
  "priceRecords": []
}
```

### SharedPriceRecord

```json
{
  "id": 1,
  "itemId": 1,
  "item": { /* SharedItem */ },
  "placeId": 1,
  "place": { /* SharedPlace */ },
  "price": 9.99,
  "originalPrice": 14.99,
  "currency": "AUD",
  "specialType": "Half Price",
  "validFrom": "2026-03-01T00:00:00Z",
  "validUntil": "2026-03-07T00:00:00Z",
  "dateRecorded": "2026-03-01T00:00:00Z",
  "isCurrent": true,
  "clientVersion": "1.0.0",
  "source": "Coles Catalog"
}
```

### DataUploadRequest

```json
{
  "items": [ /* SharedItem[] */ ],
  "places": [ /* SharedPlace[] */ ],
  "priceRecords": [ /* SharedPriceRecord[] */ ]
}
```

### UploadResult

```json
{
  "success": true,
  "itemsUploaded": 50,
  "placesUploaded": 3,
  "pricesUploaded": 150,
  "errorMessage": null
}
```

---

## Client Implementation

### C# Client Example

```csharp
using System.Net.Http.Headers;
using System.Text.Json;

public class AdvGenNoSqlClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    public AdvGenNoSqlClient(string baseUrl, string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    public async Task<List<SharedItem>> GetItemsAsync(int page = 1, int pageSize = 100)
    {
        var response = await _httpClient.GetAsync($"/api/items?page={page}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiListResponse<SharedItem>>(json);
        return result?.Data ?? new List<SharedItem>();
    }
    
    public async Task<SharedItem> CreateItemAsync(SharedItem item)
    {
        var json = JsonSerializer.Serialize(item);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/api/items", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<SharedItem>>(responseJson);
        return result?.Data;
    }
}
```

### Retry Logic

The reference implementation (`AdvGenNoSqlProvider`) includes automatic retry logic with exponential backoff:

- **Max retries:** 3 (configurable)
- **Retry delay:** 100ms, 200ms, 400ms (exponential)
- **Retry conditions:** Server errors (5xx), timeouts, connection failures
- **No retry:** Client errors (4xx)

---

## Notes for Developers

1. **API Version Mismatch:** The client repositories use `/api/v1/` paths, but the current server uses `/api/`. Update either the client or server to ensure path compatibility.

2. **Pagination:** All list endpoints support pagination. Use `page` and `pageSize` parameters to navigate large datasets.

3. **Date Format:** All dates are in ISO 8601 format (UTC).

4. **Currency:** Default currency is AUD (Australian Dollar).

5. **WebSocket Reconnection:** Implement automatic reconnection for SignalR connections with exponential backoff.

---

## Related Files

- Server Controllers:
  - `AdvGenPriceComparer.Server/Controllers/ItemsController.cs`
  - `AdvGenPriceComparer.Server/Controllers/PlacesController.cs`
  - `AdvGenPriceComparer.Server/Controllers/PricesController.cs`
- Client Provider:
  - `AdvGenPriceComparer.WPF/Services/AdvGenNoSqlProvider.cs`
  - `AdvGenPriceComparer.WPF/Services/AdvGenNoSqlItemRepository.cs`
  - `AdvGenPriceComparer.WPF/Services/AdvGenNoSqlPlaceRepository.cs`
  - `AdvGenPriceComparer.WPF/Services/AdvGenNoSqlPriceRecordRepository.cs`
- Middleware:
  - `AdvGenPriceComparer.Server/Middleware/ApiKeyMiddleware.cs`
  - `AdvGenPriceComparer.Server/Middleware/RateLimitMiddleware.cs`
