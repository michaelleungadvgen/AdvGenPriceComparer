using System.Text.Json;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Services;

public class ServerConfigService
{
    private readonly string _configPath;
    private List<ServerInfo> _servers = new();

    public ServerConfigService(string? configPath = null)
    {
        _configPath = configPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer",
            "servers.json"
        );
        LoadServers();
    }

    public IReadOnlyList<ServerInfo> GetActiveServers()
    {
        return _servers.Where(s => s.IsActive).ToList();
    }

    public IReadOnlyList<ServerInfo> GetServersByRegion(string region)
    {
        return _servers.Where(s => s.IsActive && 
            (s.Region?.Equals(region, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();
    }

    public ServerInfo? GetServerByName(string name)
    {
        return _servers.FirstOrDefault(s => 
            s.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public void AddServer(ServerInfo server)
    {
        var existing = _servers.FirstOrDefault(s => s.Name == server.Name);
        if (existing != null)
        {
            existing.Host = server.Host;
            existing.Port = server.Port;
            existing.IsSecure = server.IsSecure;
            existing.Region = server.Region;
            existing.IsActive = server.IsActive;
            existing.Description = server.Description;
        }
        else
        {
            _servers.Add(server);
        }
        SaveServers();
    }

    public void UpdateServerStatus(string serverName, bool isActive, DateTime? lastSeen = null)
    {
        var server = GetServerByName(serverName);
        if (server != null)
        {
            server.IsActive = isActive;
            server.LastSeen = lastSeen ?? DateTime.UtcNow;
            SaveServers();
        }
    }

    public void RemoveServer(string serverName)
    {
        _servers.RemoveAll(s => s.Name?.Equals(serverName, StringComparison.OrdinalIgnoreCase) ?? false);
        SaveServers();
    }

    private void LoadServers()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                var servers = System.Text.Json.JsonSerializer.Deserialize<List<ServerInfo>>(json);
                _servers = servers ?? new List<ServerInfo>();
            }
            else
            {
                CreateDefaultServers();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading servers: {ex.Message}");
            CreateDefaultServers();
        }
    }

    private void SaveServers()
    {
        try
        {
            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = System.Text.Json.JsonSerializer.Serialize(_servers, options);
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving servers: {ex.Message}");
        }
    }

    private void CreateDefaultServers()
    {
        _servers = new List<ServerInfo>
        {
            new()
            {
                Name = "AusPriceShare-Sydney",
                Host = "price.aus.example.com",
                Port = 8080,
                IsSecure = false,
                Region = "NSW",
                Description = "Australian grocery price sharing - Sydney region",
                IsActive = false // Will be activated when tested
            },
            new()
            {
                Name = "AusPriceShare-Melbourne", 
                Host = "price.vic.example.com",
                Port = 8080,
                IsSecure = false,
                Region = "VIC",
                Description = "Australian grocery price sharing - Melbourne region",
                IsActive = false
            },
            new()
            {
                Name = "LocalTestServer",
                Host = "localhost",
                Port = 8081,
                IsSecure = false,
                Region = "Local",
                Description = "Local testing server",
                IsActive = false
            }
        };
        SaveServers();
    }

    public void ResetToDefaults()
    {
        CreateDefaultServers();
    }

    public async Task<bool> TestServerConnection(ServerInfo server)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            var url = $"{(server.IsSecure ? "https" : "http")}://{server.Host}:{server.Port}/health";
            var response = await client.GetAsync(url);
            
            var isHealthy = response.IsSuccessStatusCode;
            UpdateServerStatus(server.Name!, isHealthy);
            
            return isHealthy;
        }
        catch
        {
            UpdateServerStatus(server.Name!, false);
            return false;
        }
    }

    public async Task TestAllServers()
    {
        var tasks = _servers.Select(TestServerConnection);
        await Task.WhenAll(tasks);
    }
}