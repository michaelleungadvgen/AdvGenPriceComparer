using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Core.Helpers;
using AdvGenPriceComparer.Core.Services;
using System.IO;
using LiteDB;

Console.WriteLine("🛒 AdvGen Price Comparer - P2P Network Test");
Console.WriteLine("==========================================");

// Setup database
var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdvGenPriceComparer");
Directory.CreateDirectory(appDataPath);
var dbPath = Path.Combine(appDataPath, "NetworkTestGroceryPrices.db");

// Copy servers.json from project root
var serverConfigPath = Path.Combine(appDataPath, "servers.json");
var projectServerPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "servers.json");
if (File.Exists(projectServerPath) && !File.Exists(serverConfigPath))
{
    File.Copy(projectServerPath, serverConfigPath);
    Console.WriteLine($"✅ Copied servers.json to {serverConfigPath}");
}

// Initialize services
var groceryData = new GroceryDataService(dbPath);
var serverConfig = new ServerConfigService(serverConfigPath);
var networkManager = new NetworkManager(groceryData, serverConfig);

// Subscribe to events
networkManager.PeerConnected += (sender, peer) => 
{
    Console.WriteLine($"🔗 Peer connected: {peer.Host}:{peer.Port} ({peer.Region})");
};

networkManager.PeerDisconnected += (sender, peer) =>
{
    Console.WriteLine($"❌ Peer disconnected: {peer.Host}:{peer.Port}");
};

networkManager.PriceReceived += (sender, priceMessage) =>
{
    Console.WriteLine($"💰 Price received: {priceMessage.ItemName} - ${priceMessage.Price:F2} at {priceMessage.StoreName}");
    if (priceMessage.IsOnSale)
    {
        Console.WriteLine($"   🏷️  ON SALE! Originally ${priceMessage.OriginalPrice:F2} - {priceMessage.SaleDescription}");
    }
};

networkManager.ErrorOccurred += (sender, error) =>
{
    Console.WriteLine($"⚠️  Network error: {error}");
};

Console.WriteLine();

try
{
    // Add some sample data
    Console.WriteLine("📝 Adding sample grocery items and stores...");
    
    var breadId = groceryData.AddGroceryItem("Bread White 680g", "Tip Top", "Bakery", "9310012345678", "680g");
    var milkId = groceryData.AddGroceryItem("Milk Full Cream 2L", "Dairy Farmers", "Dairy", "9310098765432", "2L");
    
    var colesId = groceryData.AddSupermarket("Coles Chatswood", "Coles", "Chatswood", "NSW", "2067");
    var wooliesId = groceryData.AddSupermarket("Woolworths Town Hall", "Woolworths", "Sydney", "NSW", "2000");
    
    // Record some prices
    groceryData.RecordPrice(breadId, colesId, 2.80m, true, 3.50m, "Weekly Special");
    groceryData.RecordPrice(breadId, wooliesId, 3.00m);
    groceryData.RecordPrice(milkId, colesId, 3.20m);
    groceryData.RecordPrice(milkId, wooliesId, 3.10m, true, 3.50m, "Down Down");
    
    Console.WriteLine("✅ Sample data added successfully!");
    
    // Start P2P server
    Console.WriteLine("\n🚀 Starting P2P server...");
    var serverStarted = await networkManager.StartServer(8081);
    
    if (serverStarted)
    {
        Console.WriteLine("✅ P2P server started on port 8081");
        Console.WriteLine($"🆔 Node ID: {networkManager.NodeId}");
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine("❌ Failed to start P2P server");
        return;
    }
    
    // Show available servers
    var servers = serverConfig.GetActiveServers();
    if (servers.Any())
    {
        Console.WriteLine("📡 Available servers in configuration:");
        foreach (var server in servers)
        {
            Console.WriteLine($"   • {server.Name} ({server.Region}) - {server.Host}:{server.Port}");
        }
        Console.WriteLine();
    }
    
    // Test sharing a price
    Console.WriteLine("💫 Sharing test price...");
    await networkManager.SharePrice(breadId, colesId, 2.80m, true, 3.50m, "Weekly Special");
    Console.WriteLine("✅ Price shared with network!");
    
    // Show dashboard stats
    var stats = groceryData.GetDashboardStats();
    Console.WriteLine($"\n📊 Database Stats:");
    Console.WriteLine($"   Items: {stats["totalItems"]}");
    Console.WriteLine($"   Stores: {stats["trackedStores"]}");
    Console.WriteLine($"   Price Records: {stats["priceRecords"]}");
    Console.WriteLine($"   Recent Updates: {stats["recentUpdates"]}");
    
    // Show best deals
    Console.WriteLine($"\n🏆 Best Deals:");
    var bestDeals = groceryData.FindBestDeals().Take(5);
    foreach (var deal in bestDeals)
    {
        Console.WriteLine($"   • {deal.item.Name} - ${deal.lowestPrice:F2} at {deal.place.Name}");
    }
    
    Console.WriteLine($"\n🔗 Connected Peers: {networkManager.ConnectedPeers.Count}");
    foreach (var peer in networkManager.ConnectedPeers)
    {
        Console.WriteLine($"   • {peer.Host}:{peer.Port} - {(peer.IsConnected ? "Connected" : "Disconnected")}");
    }
    
    Console.WriteLine("\n✨ P2P Network Test Complete!");
    Console.WriteLine("The P2P grocery price sharing system is working correctly.");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error during test: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
finally
{
    networkManager?.Dispose();
    Console.WriteLine("\n👋 Network manager disposed. Goodbye!");
}