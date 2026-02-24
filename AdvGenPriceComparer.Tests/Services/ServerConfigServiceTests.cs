using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Services;
using Xunit;

namespace AdvGenPriceComparer.Tests.Services
{
    // Collection to prevent parallel execution of these tests
    // This ensures file system operations don't interfere with each other
    [CollectionDefinition("ServerConfigServiceTests", DisableParallelization = true)]
    public class ServerConfigServiceCollection : ICollectionFixture<object>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("ServerConfigServiceTests")]
    public class ServerConfigServiceTests : IDisposable
    {
        private readonly string _tempConfigPath;
        private readonly string _testDir;

        public ServerConfigServiceTests()
        {
            // Create a temporary directory for test config files
            _testDir = Path.Combine(Path.GetTempPath(), $"AdvGenTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDir);
            _tempConfigPath = Path.Combine(_testDir, "test_servers.json");
        }

        public void Dispose()
        {
            // Cleanup temporary files
            try
            {
                if (Directory.Exists(_testDir))
                {
                    Directory.Delete(_testDir, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ConfigFileNotFound_CreatesDefaultServers()
        {
            // Arrange & Act
            var service = new ServerConfigService(_tempConfigPath);

            // Assert
            // Default servers are created with IsActive = false, so GetActiveServers returns 0
            var activeServers = service.GetActiveServers();
            Assert.Empty(activeServers);
            
            // But we can still get them by name
            Assert.NotNull(service.GetServerByName("AusPriceShare-Sydney"));
            Assert.NotNull(service.GetServerByName("AusPriceShare-Melbourne"));
            Assert.NotNull(service.GetServerByName("LocalTestServer"));
            
            Assert.True(File.Exists(_tempConfigPath), "Config file should be created");
        }

        [Fact]
        public void Constructor_ConfigFileExists_LoadsServersFromFile()
        {
            // Arrange
            var testServers = @"[{
                ""name"": ""TestServer1"",
                ""host"": ""test1.example.com"",
                ""port"": 8080,
                ""isSecure"": true,
                ""region"": ""NSW"",
                ""isActive"": true,
                ""description"": ""Test server 1""
            }]";
            File.WriteAllText(_tempConfigPath, testServers);

            // Act
            var service = new ServerConfigService(_tempConfigPath);

            // Assert
            var server = service.GetServerByName("TestServer1");
            Assert.NotNull(server);
            Assert.Equal("test1.example.com", server.Host);
            Assert.Equal(8080, server.Port);
            Assert.True(server.IsSecure);
            Assert.Equal("NSW", server.Region);
            Assert.True(server.IsActive);
        }

        [Fact]
        public void Constructor_InvalidJson_CreatesDefaultServers()
        {
            // Arrange
            File.WriteAllText(_tempConfigPath, "invalid json content");

            // Act
            var service = new ServerConfigService(_tempConfigPath);

            // Assert
            var servers = service.GetActiveServers();
            Assert.NotNull(servers);
            Assert.True(File.Exists(_tempConfigPath));
        }

        #endregion

        #region GetActiveServers Tests

        [Fact]
        public void GetActiveServers_ReturnsOnlyActiveServers()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "ActiveServer",
                Host = "active.example.com",
                Port = 8080,
                IsActive = true
            });
            service.AddServer(new ServerInfo
            {
                Name = "InactiveServer",
                Host = "inactive.example.com",
                Port = 8081,
                IsActive = false
            });

            // Act
            var activeServers = service.GetActiveServers();

            // Assert
            Assert.Contains(activeServers, s => s.Name == "ActiveServer");
            Assert.DoesNotContain(activeServers, s => s.Name == "InactiveServer");
        }

        [Fact]
        public void GetActiveServers_NoActiveServers_ReturnsEmptyList()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            // Reset and add only inactive servers
            service.ResetToDefaults();
            var allServers = new[] { "AusPriceShare-Sydney", "AusPriceShare-Melbourne", "LocalTestServer" };
            foreach (var name in allServers)
            {
                service.UpdateServerStatus(name, false);
            }

            // Act
            var activeServers = service.GetActiveServers();

            // Assert
            Assert.Empty(activeServers);
        }

        #endregion

        #region GetServersByRegion Tests

        [Fact]
        public void GetServersByRegion_ReturnsServersInSpecifiedRegion()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "NSWServer",
                Host = "nsw.example.com",
                Port = 8080,
                Region = "NSW",
                IsActive = true
            });
            service.AddServer(new ServerInfo
            {
                Name = "VICServer",
                Host = "vic.example.com",
                Port = 8081,
                Region = "VIC",
                IsActive = true
            });

            // Act
            var nswServers = service.GetServersByRegion("NSW");

            // Assert
            Assert.Single(nswServers);
            Assert.Equal("NSWServer", nswServers[0].Name);
        }

        [Fact]
        public void GetServersByRegion_CaseInsensitiveMatch()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "NSWServer",
                Host = "nsw.example.com",
                Port = 8080,
                Region = "NSW",
                IsActive = true
            });

            // Act
            var nswServersLower = service.GetServersByRegion("nsw");
            var nswServersUpper = service.GetServersByRegion("NSW");
            var nswServersMixed = service.GetServersByRegion("Nsw");

            // Assert
            Assert.Single(nswServersLower);
            Assert.Single(nswServersUpper);
            Assert.Single(nswServersMixed);
        }

        [Fact]
        public void GetServersByRegion_InactiveServersExcluded()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "InactiveNSWServer",
                Host = "nsw.example.com",
                Port = 8080,
                Region = "NSW",
                IsActive = false
            });

            // Act
            var nswServers = service.GetServersByRegion("NSW");

            // Assert
            Assert.DoesNotContain(nswServers, s => s.Name == "InactiveNSWServer");
        }

        [Fact]
        public void GetServersByRegion_NonExistentRegion_ReturnsEmptyList()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);

            // Act
            var servers = service.GetServersByRegion("NONEXISTENT");

            // Assert
            Assert.Empty(servers);
        }

        #endregion

        #region GetServerByName Tests

        [Fact]
        public void GetServerByName_ExistingServer_ReturnsServer()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "TestServer",
                Host = "test.example.com",
                Port = 8080
            });

            // Act
            var server = service.GetServerByName("TestServer");

            // Assert
            Assert.NotNull(server);
            Assert.Equal("test.example.com", server.Host);
        }

        [Fact]
        public void GetServerByName_NonExistentServer_ReturnsNull()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);

            // Act
            var server = service.GetServerByName("NonExistent");

            // Assert
            Assert.Null(server);
        }

        [Fact]
        public void GetServerByName_CaseInsensitiveMatch()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "TestServer",
                Host = "test.example.com",
                Port = 8080
            });

            // Act
            var serverLower = service.GetServerByName("testserver");
            var serverUpper = service.GetServerByName("TESTSERVER");
            var serverMixed = service.GetServerByName("TestServer");

            // Assert
            Assert.NotNull(serverLower);
            Assert.NotNull(serverUpper);
            Assert.NotNull(serverMixed);
        }

        #endregion

        #region AddServer Tests

        [Fact]
        public void AddServer_NewServer_AddsToList()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            var newServer = new ServerInfo
            {
                Name = "NewServer",
                Host = "new.example.com",
                Port = 9090,
                Region = "QLD",
                IsActive = true,
                Description = "A new test server"
            };

            // Act
            service.AddServer(newServer);

            // Assert
            var retrieved = service.GetServerByName("NewServer");
            Assert.NotNull(retrieved);
            Assert.Equal("new.example.com", retrieved.Host);
            Assert.Equal(9090, retrieved.Port);
            Assert.Equal("QLD", retrieved.Region);
        }

        [Fact]
        public void AddServer_ExistingServer_UpdatesProperties()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "ExistingServer",
                Host = "old.example.com",
                Port = 8080,
                Region = "NSW"
            });

            // Act
            service.AddServer(new ServerInfo
            {
                Name = "ExistingServer",
                Host = "new.example.com",
                Port = 9090,
                Region = "VIC",
                IsSecure = true,
                IsActive = true
            });

            // Assert
            var updated = service.GetServerByName("ExistingServer");
            Assert.NotNull(updated);
            Assert.Equal("new.example.com", updated.Host);
            Assert.Equal(9090, updated.Port);
            Assert.Equal("VIC", updated.Region);
            Assert.True(updated.IsSecure);
        }

        [Fact]
        public void AddServer_SavesToFile()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            var newServer = new ServerInfo
            {
                Name = "PersistentServer",
                Host = "persistent.example.com",
                Port = 8080,
                IsActive = true
            };

            // Act
            service.AddServer(newServer);

            // Assert - File should exist and contain the server
            Assert.True(File.Exists(_tempConfigPath), "Config file should be created");
            var json = File.ReadAllText(_tempConfigPath);
            Assert.Contains("PersistentServer", json);
            Assert.Contains("persistent.example.com", json);
            
            // Also verify by creating new service instance
            var newService = new ServerConfigService(_tempConfigPath);
            var retrieved = newService.GetServerByName("PersistentServer");
            Assert.NotNull(retrieved);
            Assert.Equal("persistent.example.com", retrieved.Host);
        }

        #endregion

        #region UpdateServerStatus Tests

        [Fact]
        public void UpdateServerStatus_ExistingServer_UpdatesStatus()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "StatusServer",
                Host = "status.example.com",
                Port = 8080,
                IsActive = false
            });

            // Act
            service.UpdateServerStatus("StatusServer", true);

            // Assert
            var server = service.GetActiveServers().FirstOrDefault(s => s.Name == "StatusServer");
            Assert.NotNull(server);
            Assert.True(server.IsActive);
        }

        [Fact]
        public void UpdateServerStatus_ExistingServer_UpdatesLastSeen()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "LastSeenServer",
                Host = "lastseen.example.com",
                Port = 8080,
                IsActive = true
            });
            var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);

            // Act
            service.UpdateServerStatus("LastSeenServer", true);

            // Assert
            var server = service.GetServerByName("LastSeenServer");
            Assert.NotNull(server);
            Assert.True(server.LastSeen >= beforeUpdate);
        }

        [Fact]
        public void UpdateServerStatus_NonExistentServer_DoesNothing()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);

            // Act & Assert - Should not throw
            service.UpdateServerStatus("NonExistent", true);
            Assert.Null(service.GetServerByName("NonExistent"));
        }

        [Fact]
        public void UpdateServerStatus_CustomLastSeen_UsesProvidedValue()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "CustomLastSeen",
                Host = "custom.example.com",
                Port = 8080
            });
            var customTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            // Act
            service.UpdateServerStatus("CustomLastSeen", true, customTime);

            // Assert
            var server = service.GetServerByName("CustomLastSeen");
            Assert.NotNull(server);
            Assert.Equal(customTime, server.LastSeen);
        }

        #endregion

        #region RemoveServer Tests

        [Fact]
        public void RemoveServer_ExistingServer_RemovesFromList()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "ToRemove",
                Host = "remove.example.com",
                Port = 8080
            });

            // Act
            service.RemoveServer("ToRemove");

            // Assert
            Assert.Null(service.GetServerByName("ToRemove"));
        }

        [Fact]
        public void RemoveServer_NonExistentServer_DoesNothing()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);

            // Act & Assert - Should not throw
            service.RemoveServer("NonExistent");
        }

        [Fact]
        public void RemoveServer_CaseInsensitiveMatch()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "CaseTest",
                Host = "case.example.com",
                Port = 8080
            });

            // Act
            service.RemoveServer("CASETEST");

            // Assert
            Assert.Null(service.GetServerByName("CaseTest"));
        }

        [Fact]
        public void RemoveServer_SavesToFile()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "ToRemovePersistent",
                Host = "remove.example.com",
                Port = 8080
            });

            // Act
            service.RemoveServer("ToRemovePersistent");

            // Assert
            var newService = new ServerConfigService(_tempConfigPath);
            Assert.Null(newService.GetServerByName("ToRemovePersistent"));
        }

        #endregion

        #region ResetToDefaults Tests

        [Fact]
        public void ResetToDefaults_ClearsCustomServers()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "CustomServer",
                Host = "custom.example.com",
                Port = 8080
            });

            // Act
            service.ResetToDefaults();

            // Assert
            Assert.Null(service.GetServerByName("CustomServer"));
            Assert.NotNull(service.GetServerByName("AusPriceShare-Sydney"));
            Assert.NotNull(service.GetServerByName("AusPriceShare-Melbourne"));
            Assert.NotNull(service.GetServerByName("LocalTestServer"));
        }

        [Fact]
        public void ResetToDefaults_SavesDefaultsToFile()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "CustomServer",
                Host = "custom.example.com",
                Port = 8080,
                IsActive = true
            });
            Assert.NotNull(service.GetServerByName("CustomServer"));

            // Act
            service.ResetToDefaults();

            // Assert - File should exist and contain default servers
            Assert.True(File.Exists(_tempConfigPath), "Config file should exist after reset");
            var json = File.ReadAllText(_tempConfigPath);
            Assert.Contains("LocalTestServer", json);
            Assert.Contains("AusPriceShare-Sydney", json);
            Assert.DoesNotContain("CustomServer", json);
            
            // Also verify via new service instance
            var newService = new ServerConfigService(_tempConfigPath);
            Assert.Null(newService.GetServerByName("CustomServer")); // Custom should be gone
            Assert.NotNull(newService.GetServerByName("LocalTestServer")); // Default should exist
        }

        #endregion

        #region TestServerConnection Tests

        [Fact]
        public async Task TestServerConnection_UnreachableServer_ReturnsFalse()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            var unreachableServer = new ServerInfo
            {
                Name = "Unreachable",
                Host = "192.0.2.1", // TEST-NET-1, should not be reachable
                Port = 59999,
                IsActive = true
            };

            // Act
            var result = await service.TestServerConnection(unreachableServer);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task TestServerConnection_UpdatesServerStatus()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "StatusUpdateTest",
                Host = "192.0.2.1",
                Port = 59999,
                IsActive = true
            });
            var server = service.GetServerByName("StatusUpdateTest")!;

            // Act
            await service.TestServerConnection(server);

            // Assert
            var updated = service.GetServerByName("StatusUpdateTest");
            Assert.NotNull(updated);
            Assert.False(updated.IsActive); // Should be marked inactive after failed connection
        }

        #endregion

        #region TestAllServers Tests

        [Fact]
        public async Task TestAllServers_TestsAllConfiguredServers()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.ResetToDefaults(); // Reset to have known servers

            // Act
            await service.TestAllServers();

            // Assert - All default servers should have been tested and marked inactive
            var servers = new[] { "AusPriceShare-Sydney", "AusPriceShare-Melbourne", "LocalTestServer" };
            foreach (var name in servers)
            {
                var server = service.GetServerByName(name);
                Assert.NotNull(server);
                // After testing, IsActive should be false (since these are example servers)
                Assert.False(server.IsActive);
            }
        }

        #endregion

        #region Persistence Tests

        [Fact]
        public void Persistence_RoundTrip_PreservesAllData()
        {
            // Arrange
            var originalService = new ServerConfigService(_tempConfigPath);
            originalService.ResetToDefaults();
            
            // Add custom server
            originalService.AddServer(new ServerInfo
            {
                Name = "RoundTripServer",
                Host = "roundtrip.example.com",
                Port = 7777,
                IsSecure = true,
                Region = "WA",
                IsActive = true,
                Description = "Test round trip persistence"
            });
            
            // Update the custom server's LastSeen
            var customTime = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            originalService.UpdateServerStatus("RoundTripServer", true, customTime);

            // Verify file was written with correct data
            Assert.True(File.Exists(_tempConfigPath), "Config file should exist");
            var json = File.ReadAllText(_tempConfigPath);
            Assert.Contains("RoundTripServer", json);
            Assert.Contains("roundtrip.example.com", json);
            Assert.Contains("Test round trip persistence", json);

            // Act - Create new service instance to load from file
            var newService = new ServerConfigService(_tempConfigPath);

            // Assert
            var server = newService.GetServerByName("RoundTripServer");
            Assert.NotNull(server);
            Assert.Equal("roundtrip.example.com", server.Host);
            Assert.Equal(7777, server.Port);
            Assert.True(server.IsSecure);
            Assert.Equal("WA", server.Region);
            Assert.True(server.IsActive);
            Assert.Equal("Test round trip persistence", server.Description);
            Assert.Equal(customTime, server.LastSeen);
        }

        [Fact]
        public void Persistence_JsonFormat_UsesCamelCase()
        {
            // Arrange
            var service = new ServerConfigService(_tempConfigPath);
            service.AddServer(new ServerInfo
            {
                Name = "JsonTest",
                Host = "json.example.com",
                Port = 8080
            });

            // Act
            var json = File.ReadAllText(_tempConfigPath);

            // Assert
            Assert.Contains("\"name\":", json);
            Assert.Contains("\"host\":", json);
            Assert.Contains("\"port\":", json);
            Assert.Contains("\"isSecure\":", json);
            Assert.Contains("\"isActive\":", json);
        }

        #endregion
    }
}
