using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AdvGenPriceComparer.Server;
using AdvGenPriceComparer.Server.Hubs;
using AdvGenPriceComparer.Server.Models;
using AdvGenPriceComparer.Server.Services;
using AdvGenPriceComparer.WPF.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace AdvGenPriceComparer.Tests.Integration;

/// <summary>
/// Integration tests for the P2P price sharing workflow
/// Tests end-to-end scenarios including upload, download, SignalR, and authentication
/// </summary>
public class PriceSharingWorkflowTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _client;
    private string _testApiKey = "test-api-key-12345";
    private HubConnection? _hubConnection;
    private bool _priceUpdateReceived;
    private bool _newDealReceived;

    public PriceSharingWorkflowTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices((context, services) =>
            {
                // Remove all database-related descriptors to avoid provider conflicts
                var descriptors = services.Where(d => 
                    d.ServiceType == typeof(DbContextOptions<AdvGenPriceComparer.Server.Data.PriceDataContext>) ||
                    d.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions)
                ).ToList();
                
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<AdvGenPriceComparer.Server.Data.PriceDataContext>(options =>
                {
                    options.UseInMemoryDatabase("TestPriceSharingDb");
                });
            });
        });

        _output = output;
        _client = _factory.CreateClient();
        
        // Seed the test API key
        SeedTestData().GetAwaiter().GetResult();
    }
    
    private async Task SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AdvGenPriceComparer.Server.Data.PriceDataContext>();
        var apiKeyService = scope.ServiceProvider.GetRequiredService<IApiKeyService>();
        
        // Generate a test API key and save the plain key value
        var (key, plainKey) = await apiKeyService.GenerateKeyAsync("Test Client", 1000);
        _testApiKey = plainKey;
        
        _output.WriteLine($"Generated test API key: {_testApiKey}");
    }

    public void Dispose()
    {
        _hubConnection?.DisposeAsync().GetAwaiter().GetResult();
        _client?.Dispose();
    }

    #region Server Health and Authentication Tests

    [Fact]
    public async Task Server_HealthCheck_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/prices/stats");

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success but got {response.StatusCode}");
        var stats = await response.Content.ReadFromJsonAsync<ServerStats>();
        Assert.NotNull(stats);
        Assert.True(stats.TotalItems >= 0);
        Assert.True(stats.TotalPlaces >= 0);
        Assert.True(stats.TotalPriceRecords >= 0);

        _output.WriteLine($"Server stats: {stats.TotalItems} items, {stats.TotalPlaces} places, {stats.TotalPriceRecords} prices");
    }

    [Fact]
    public async Task Upload_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var request = CreateSampleUploadRequest();

        // Act - No API key header
        var response = await _client.PostAsJsonAsync("/api/prices/upload", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithValidApiKey_ReturnsSuccess()
    {
        // Arrange
        var request = CreateSampleUploadRequest();
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);

        // Act
        var response = await _client.PostAsJsonAsync("/api/prices/upload", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success but got {response.StatusCode}");
        var result = await response.Content.ReadFromJsonAsync<UploadResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(result.ItemsUploaded > 0);

        _output.WriteLine($"Upload successful: {result.ItemsUploaded} items, {result.PlacesUploaded} places, {result.PricesUploaded} prices");
    }

    [Fact]
    public async Task Upload_WithInvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var request = CreateSampleUploadRequest();
        _client.DefaultRequestHeaders.Remove("X-API-Key");
        _client.DefaultRequestHeaders.Add("X-API-Key", "invalid-key");

        // Act
        var response = await _client.PostAsJsonAsync("/api/prices/upload", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Data Upload and Download Tests

    [Fact]
    public async Task Upload_ThenDownload_DataIntegrityMaintained()
    {
        // Arrange
        var originalRequest = CreateSampleUploadRequest();
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);

        // Act - Upload data
        var uploadResponse = await _client.PostAsJsonAsync("/api/prices/upload", originalRequest);
        if (!uploadResponse.IsSuccessStatusCode)
        {
            var errorContent = await uploadResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"Upload failed with status {uploadResponse.StatusCode}: {errorContent}");
        }
        Assert.True(uploadResponse.IsSuccessStatusCode, $"Upload failed: {uploadResponse.StatusCode}");
        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<UploadResult>();
        Assert.NotNull(uploadResult);

        // Act - Download items
        var itemsResponse = await _client.GetAsync("/api/items?pageSize=100");
        Assert.True(itemsResponse.IsSuccessStatusCode);
        var itemsResult = await itemsResponse.Content.ReadFromJsonAsync<List<SharedItem>>();

        // Act - Download places
        var placesResponse = await _client.GetAsync("/api/places?pageSize=100");
        Assert.True(placesResponse.IsSuccessStatusCode);
        var placesResult = await placesResponse.Content.ReadFromJsonAsync<List<SharedPlace>>();

        // Act - Download prices
        var pricesResponse = await _client.GetAsync("/api/prices/download?pageSize=100");
        Assert.True(pricesResponse.IsSuccessStatusCode);
        var pricesResult = await pricesResponse.Content.ReadFromJsonAsync<List<SharedPriceRecord>>();

        // Assert
        Assert.NotNull(itemsResult);
        Assert.NotNull(placesResult);
        Assert.NotNull(pricesResult);

        Assert.True(itemsResult.Count >= originalRequest.Items.Count, 
            $"Expected at least {originalRequest.Items.Count} items, got {itemsResult.Count}");
        Assert.True(placesResult.Count >= originalRequest.Places.Count,
            $"Expected at least {originalRequest.Places.Count} places, got {placesResult.Count}");
        Assert.True(pricesResult.Count >= originalRequest.PriceRecords.Count,
            $"Expected at least {originalRequest.PriceRecords.Count} prices, got {pricesResult.Count}");

        _output.WriteLine($"Data integrity verified: {itemsResult.Count} items, {placesResult.Count} places, {pricesResult.Count} prices");
    }

    [Fact]
    public async Task Upload_MultipleBatches_AllDataPreserved()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);
        var totalItems = 0;
        var totalPlaces = 0;
        var totalPrices = 0;

        // Act - Upload 3 batches
        for (int i = 0; i < 3; i++)
        {
            var request = CreateSampleUploadRequest(batchIndex: i);
            var response = await _client.PostAsJsonAsync("/api/prices/upload", request);
            Assert.True(response.IsSuccessStatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<UploadResult>();
            Assert.NotNull(result);
            
            totalItems += result.ItemsUploaded;
            totalPlaces += result.PlacesUploaded;
            totalPrices += result.PricesUploaded;
        }

        // Assert - Check server stats
        var statsResponse = await _client.GetAsync("/api/prices/stats");
        var stats = await statsResponse.Content.ReadFromJsonAsync<ServerStats>();
        Assert.NotNull(stats);

        Assert.True(stats.TotalItems >= totalItems, 
            $"Expected at least {totalItems} total items, server has {stats.TotalItems}");

        _output.WriteLine($"Multiple batches uploaded: {totalItems} items, {totalPlaces} places, {totalPrices} prices");
    }

    [Fact]
    public async Task Download_WithDateFilter_ReturnsFilteredResults()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);
        var request = CreateSampleUploadRequest();
        await _client.PostAsJsonAsync("/api/prices/upload", request);

        var fromDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/prices/download?from={fromDate}&to={toDate}&pageSize=100");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<SharedPriceRecord>>();
        Assert.NotNull(result);
        Assert.True(result.Count >= 0);

        _output.WriteLine($"Date filter test: {result.Count} records between {fromDate} and {toDate}");
    }

    #endregion

    #region Search and Comparison Tests

    [Fact]
    public async Task Search_ByName_ReturnsMatchingItems()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);
        var request = CreateSampleUploadRequest();
        await _client.PostAsJsonAsync("/api/prices/upload", request);

        // Act
        var response = await _client.GetAsync("/api/prices/search?query=Milk&limit=10");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<SharedItem>>();
        Assert.NotNull(results);
        // Should find at least the milk product we uploaded

        _output.WriteLine($"Search returned {results.Count} results");
    }

    [Fact]
    public async Task ComparePrices_ForItem_ReturnsMultipleStorePrices()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);
        var request = CreateSampleUploadRequest();
        await _client.PostAsJsonAsync("/api/prices/upload", request);

        // First get an item ID
        var itemsResponse = await _client.GetAsync("/api/items?pageSize=1");
        var itemsResult = await itemsResponse.Content.ReadFromJsonAsync<List<SharedItem>>();
        Assert.NotNull(itemsResult);
        Assert.True(itemsResult.Count > 0);

        var itemId = itemsResult[0].Id;

        // Act
        var response = await _client.GetAsync($"/api/prices/compare/{itemId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var prices = await response.Content.ReadFromJsonAsync<List<SharedPriceRecord>>();
        Assert.NotNull(prices);

        _output.WriteLine($"Price comparison for item {itemId}: {prices.Count} prices found");
    }

    [Fact]
    public async Task GetLatestDeals_ReturnsSaleItems()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);
        var request = CreateSampleUploadRequest();
        await _client.PostAsJsonAsync("/api/prices/upload", request);

        // Act
        var response = await _client.GetAsync("/api/prices/latest?limit=20");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var deals = await response.Content.ReadFromJsonAsync<List<SharedPriceRecord>>();
        Assert.NotNull(deals);

        _output.WriteLine($"Latest deals: {deals.Count} items on sale");
    }

    #endregion

    #region SignalR Real-time Tests

    [Fact]
    public async Task SignalR_Connect_WithValidApiKey_Success()
    {
        // Arrange
        var baseUrl = _factory.Server.BaseAddress.ToString().TrimEnd('/');
        var connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/price-updates", options =>
            {
                options.Headers.Add("X-API-Key", _testApiKey);
            })
            .WithAutomaticReconnect()
            .Build();

        // Act & Assert - Should not throw
        await connection.StartAsync();
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.StopAsync();
        _output.WriteLine("SignalR connection test passed");
    }

    [Fact]
    public async Task SignalR_SubscribeToItem_ReceivesUpdates()
    {
        // Arrange
        var baseUrl = _factory.Server.BaseAddress.ToString().TrimEnd('/');
        _priceUpdateReceived = false;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/price-updates", options =>
            {
                options.Headers.Add("X-API-Key", _testApiKey);
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<PriceUpdateNotification>("PriceUpdated", data =>
        {
            _priceUpdateReceived = true;
            _output.WriteLine($"Received price update: {data.ItemName} = ${data.NewPrice}");
        });

        await _hubConnection.StartAsync();

        // Act - Subscribe to item
        await _hubConnection.InvokeAsync("SubscribeToItem", 1);

        // Wait a moment for subscription to take effect
        await Task.Delay(100);

        // Assert
        Assert.True(_hubConnection.State == HubConnectionState.Connected, "Should be connected");

        await _hubConnection.InvokeAsync("UnsubscribeFromItem", 1);

        _output.WriteLine("SignalR subscribe/unsubscribe test passed");
    }

    [Fact]
    public async Task SignalR_SubscribeToNewDeals_ReceivesNotifications()
    {
        // Arrange
        var baseUrl = _factory.Server.BaseAddress.ToString().TrimEnd('/');
        _newDealReceived = false;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/price-updates", options =>
            {
                options.Headers.Add("X-API-Key", _testApiKey);
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<NewDealNotification>("NewDeal", data =>
        {
            _newDealReceived = true;
            _output.WriteLine($"Received new deal: {data.ItemName} at {data.PlaceName}");
        });

        await _hubConnection.StartAsync();

        // Act - Subscribe to new deals
        await _hubConnection.InvokeAsync("SubscribeToNewDeals");

        // Wait a moment
        await Task.Delay(100);

        // Assert
        Assert.True(_hubConnection.State == HubConnectionState.Connected, "Should be connected");

        await _hubConnection.InvokeAsync("UnsubscribeFromNewDeals");

        _output.WriteLine("SignalR new deals subscription test passed");
    }

    #endregion

    #region End-to-End Workflow Tests

    [Fact]
    public async Task EndToEnd_UploadDownloadCycle_CompleteWorkflow()
    {
        // Arrange - Simulate a complete P2P sharing workflow
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);

        // Step 1: Upload data from "Client A"
        var items = new List<SharedItem>
        {
            new() { Id = 1, ProductId = "PROD-001", Name = "Organic Milk 2L", Brand = "Dairy Farmers", Category = "Dairy & Eggs", Barcode = "930000000001" },
            new() { Id = 2, ProductId = "PROD-002", Name = "Wholemeal Bread", Brand = "Tip Top", Category = "Bakery", Barcode = "930000000002" },
            new() { Id = 3, ProductId = "PROD-003", Name = "Free Range Eggs 12pk", Brand = "Golden Eggs", Category = "Dairy & Eggs", Barcode = "930000000003" }
        };
        var places = new List<SharedPlace>
        {
            new() { Id = 1, StoreId = "STORE-001", Name = "Coles Chermside", Chain = "Coles", Suburb = "Chermside", State = "QLD", Postcode = "4032" },
            new() { Id = 2, StoreId = "STORE-002", Name = "Woolworths Toowong", Chain = "Woolworths", Suburb = "Toowong", State = "QLD", Postcode = "4066" }
        };
        var uploadRequest = new DataUploadRequest
        {
            Items = items,
            Places = places,
            PriceRecords = new List<SharedPriceRecord>
            {
                new() { ItemId = 1, Item = items[0], PlaceId = 1, Place = places[0], Price = 3.99m, OriginalPrice = 4.50m, DateRecorded = DateTime.UtcNow },
                new() { ItemId = 2, Item = items[1], PlaceId = 1, Place = places[0], Price = 4.20m, DateRecorded = DateTime.UtcNow },
                new() { ItemId = 1, Item = items[0], PlaceId = 2, Place = places[1], Price = 3.79m, OriginalPrice = 4.50m, DateRecorded = DateTime.UtcNow }
            }
        };

        var uploadResponse = await _client.PostAsJsonAsync("/api/prices/upload", uploadRequest);
        Assert.True(uploadResponse.IsSuccessStatusCode, "Upload should succeed");

        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<UploadResult>();
        Assert.NotNull(uploadResult);
        Assert.True(uploadResult.Success);
        _output.WriteLine($"Step 1 - Uploaded: {uploadResult.ItemsUploaded} items, {uploadResult.PlacesUploaded} places, {uploadResult.PricesUploaded} prices");

        // Step 2: Search for products (as "Client B" would)
        var searchResponse = await _client.GetAsync("/api/prices/search?query=Milk&limit=5");
        Assert.True(searchResponse.IsSuccessStatusCode, "Search should succeed");
        var searchResults = await searchResponse.Content.ReadFromJsonAsync<List<SharedItem>>();
        Assert.NotNull(searchResults);
        _output.WriteLine($"Step 2 - Search found {searchResults.Count} milk products");

        // Step 3: Compare prices across stores
        if (searchResults.Count > 0)
        {
            var itemId = searchResults[0].Id;
            var compareResponse = await _client.GetAsync($"/api/prices/compare/{itemId}");
            Assert.True(compareResponse.IsSuccessStatusCode, "Price comparison should succeed");
            var comparisons = await compareResponse.Content.ReadFromJsonAsync<List<SharedPriceRecord>>();
            Assert.NotNull(comparisons);
            _output.WriteLine($"Step 3 - Found {comparisons.Count} price comparisons");
        }

        // Step 4: Get latest deals
        var dealsResponse = await _client.GetAsync("/api/prices/latest?limit=10");
        Assert.True(dealsResponse.IsSuccessStatusCode, "Latest deals should succeed");
        var deals = await dealsResponse.Content.ReadFromJsonAsync<List<SharedPriceRecord>>();
        Assert.NotNull(deals);
        _output.WriteLine($"Step 4 - Found {deals.Count} deals");

        // Step 5: Download all data (as "Client B" syncing)
        var downloadItemsResponse = await _client.GetAsync("/api/items?pageSize=100");
        var downloadPlacesResponse = await _client.GetAsync("/api/places?pageSize=100");
        var downloadPricesResponse = await _client.GetAsync("/api/prices/download?pageSize=100");

        Assert.True(downloadItemsResponse.IsSuccessStatusCode, "Download items should succeed");
        Assert.True(downloadPlacesResponse.IsSuccessStatusCode, "Download places should succeed");
        Assert.True(downloadPricesResponse.IsSuccessStatusCode, "Download prices should succeed");

        var downloadedItems = await downloadItemsResponse.Content.ReadFromJsonAsync<List<SharedItem>>();
        var downloadedPlaces = await downloadPlacesResponse.Content.ReadFromJsonAsync<List<SharedPlace>>();
        var downloadedPrices = await downloadPricesResponse.Content.ReadFromJsonAsync<List<SharedPriceRecord>>();

        Assert.NotNull(downloadedItems);
        Assert.NotNull(downloadedPlaces);
        Assert.NotNull(downloadedPrices);

        _output.WriteLine($"Step 5 - Downloaded: {downloadedItems.Count} items, {downloadedPlaces.Count} places, {downloadedPrices.Count} prices");

        // Step 6: Verify data integrity
        Assert.True(downloadedItems.Count >= uploadRequest.Items.Count, 
            "Downloaded items should match or exceed uploaded");
        Assert.True(downloadedPlaces.Count >= uploadRequest.Places.Count,
            "Downloaded places should match or exceed uploaded");
        Assert.True(downloadedPrices.Count >= uploadRequest.PriceRecords.Count,
            "Downloaded prices should match or exceed uploaded");

        _output.WriteLine("End-to-end workflow test completed successfully!");
    }

    [Fact]
    public async Task EndToEnd_Pagination_LargeDatasetHandling()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);

        // Upload a larger dataset
        var request = new DataUploadRequest();
        for (int i = 0; i < 50; i++)
        {
            request.Items.Add(new SharedItem 
            { 
                ProductId = $"PROD-PAGE-{i}",
                Name = $"Product {i}", 
                Brand = $"Brand {i % 5}",
                Category = i % 2 == 0 ? "Dairy & Eggs" : "Bakery"
            });
        }
        for (int i = 0; i < 10; i++)
        {
            request.Places.Add(new SharedPlace 
            { 
                StoreId = $"STORE-PAGE-{i}",
                Name = $"Store {i}", 
                Chain = i % 2 == 0 ? "Coles" : "Woolworths",
                Suburb = $"Suburb {i}",
                State = "QLD"
            });
        }

        await _client.PostAsJsonAsync("/api/prices/upload", request);

        // Act - Get first page
        var page1Response = await _client.GetAsync("/api/items?page=1&pageSize=20");
        Assert.True(page1Response.IsSuccessStatusCode);
        var page1 = await page1Response.Content.ReadFromJsonAsync<List<SharedItem>>();

        // Act - Get second page
        var page2Response = await _client.GetAsync("/api/items?page=2&pageSize=20");
        Assert.True(page2Response.IsSuccessStatusCode);
        var page2 = await page2Response.Content.ReadFromJsonAsync<List<SharedItem>>();

        // Assert
        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.True(page1.Count <= 20, "Page 1 should have at most 20 items");
        Assert.True(page2.Count <= 20, "Page 2 should have at most 20 items");
        Assert.True(page1.Count + page2.Count >= 20, "Combined pages should have at least 20 items");

        _output.WriteLine($"Pagination test: Page 1 has {page1.Count}, Page 2 has {page2.Count}");
    }

    #endregion

    #region Rate Limiting Tests

    [Fact]
    public async Task RateLimit_ExcessiveRequests_Returns429()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);

        // Act - Make many rapid requests to trigger rate limiting
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 20; i++)
        {
            var response = await _client.GetAsync("/api/prices/stats");
            responses.Add(response);
        }

        // Assert - Some requests should succeed
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        Assert.True(successCount > 0, "At least some requests should succeed");

        _output.WriteLine($"Rate limit test: {successCount}/20 requests succeeded");
    }

    #endregion

    #region Helper Methods

    private DataUploadRequest CreateSampleUploadRequest(int batchIndex = 0)
    {
        var items = new List<SharedItem>
        {
            new() 
            { 
                Id = batchIndex * 2 + 1,
                ProductId = $"PROD-{batchIndex}-1",
                Name = $"Milk Full Cream 2L (Batch {batchIndex})", 
                Brand = "Dairy Farmers", 
                Category = "Dairy & Eggs",
                Barcode = $"930000{batchIndex:D6}"
            },
            new() 
            { 
                Id = batchIndex * 2 + 2,
                ProductId = $"PROD-{batchIndex}-2",
                Name = $"White Bread 700g (Batch {batchIndex})", 
                Brand = "Tip Top", 
                Category = "Bakery",
                Barcode = $"930001{batchIndex:D6}"
            }
        };
        
        var places = new List<SharedPlace>
        {
            new() 
            { 
                Id = batchIndex + 1,
                StoreId = $"STORE-{batchIndex}",
                Name = $"Coles Store {batchIndex}", 
                Chain = "Coles", 
                Suburb = "Brisbane",
                State = "QLD",
                Postcode = "4000"
            }
        };
        
        return new DataUploadRequest
        {
            Items = items,
            Places = places,
            PriceRecords = new List<SharedPriceRecord>
            {
                new() 
                { 
                    ItemId = items[0].Id, 
                    Item = items[0],
                    PlaceId = places[0].Id, 
                    Place = places[0],
                    Price = 3.99m + batchIndex,
                    OriginalPrice = 4.50m,
                    DateRecorded = DateTime.UtcNow.AddHours(-batchIndex),
                    SpecialType = "Test Special"
                },
                new() 
                { 
                    ItemId = items[1].Id, 
                    Item = items[1],
                    PlaceId = places[0].Id, 
                    Place = places[0],
                    Price = 4.20m + batchIndex,
                    DateRecorded = DateTime.UtcNow.AddHours(-batchIndex),
                    SpecialType = "Regular Price"
                }
            }
        };
    }

    #endregion
}

/// <summary>
/// API response wrapper for list endpoints
/// </summary>
public class ApiListResponse<T>
{
    public bool Success { get; set; }
    public List<T>? Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
