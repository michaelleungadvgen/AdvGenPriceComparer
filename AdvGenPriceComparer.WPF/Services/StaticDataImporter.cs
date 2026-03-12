using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for importing data from static packages for P2P price data sharing.
/// Imports data exported by StaticDataExporter from web servers, file shares,
/// or local directories.
/// </summary>
public class StaticDataImporter
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILoggerService _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public StaticDataImporter(
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        ILoggerService logger)
    {
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Import Methods

    /// <summary>
    /// Import data from a static package directory
    /// </summary>
    public async Task<StaticImportResult> ImportFromDirectoryAsync(
        string packageDirectory,
        StaticImportOptions options,
        IProgress<StaticImportProgress>? progress = null)
    {
        var result = new StaticImportResult();

        try
        {
            _logger.LogInfo($"Starting static data import from directory: {packageDirectory}");
            progress?.Report(new StaticImportProgress { Percentage = 0, Status = "Validating package..." });

            // Validate directory exists
            if (!Directory.Exists(packageDirectory))
            {
                throw new DirectoryNotFoundException($"Package directory not found: {packageDirectory}");
            }

            // Load and validate manifest
            var manifestPath = Path.Combine(packageDirectory, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("Package manifest not found. Expected file: manifest.json");
            }

            var manifestJson = await File.ReadAllTextAsync(manifestPath);
            var manifest = JsonSerializer.Deserialize<StaticExportManifest>(manifestJson, _jsonOptions);

            if (manifest == null)
            {
                throw new InvalidDataException("Failed to parse package manifest.");
            }

            result.PackageId = manifest.PackageId;
            result.ImportedAt = DateTime.UtcNow;

            _logger.LogInfo($"Importing package {manifest.PackageId} created at {manifest.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            progress?.Report(new StaticImportProgress { Percentage = 5, Status = $"Package: {manifest.PackageId}" });

            // Validate checksums if requested
            if (options.ValidateChecksums)
            {
                progress?.Report(new StaticImportProgress { Percentage = 10, Status = "Validating checksums..." });
                await ValidateChecksumsAsync(packageDirectory, manifest, result);

                if (!result.Success && options.FailOnChecksumError)
                {
                    result.ErrorMessage = "Checksum validation failed. Import aborted.";
                    return result;
                }
            }

            // Import stores first (they're referenced by products and prices)
            progress?.Report(new StaticImportProgress { Percentage = 15, Status = "Importing stores..." });
            var storeMap = await ImportStoresAsync(packageDirectory, options, result);

            // Import products
            progress?.Report(new StaticImportProgress { Percentage = 40, Status = "Importing products..." });
            var productMap = await ImportProductsAsync(packageDirectory, options, result, storeMap);

            // Import prices
            progress?.Report(new StaticImportProgress { Percentage = 70, Status = "Importing price records..." });
            await ImportPricesAsync(packageDirectory, options, result, productMap, storeMap);

            // Update result
            result.Success = result.Errors.Count == 0 || !options.FailOnError;
            result.Message = $"Import complete. Imported {result.StoresImported} stores, {result.ProductsImported} products, {result.PricesImported} price records. Skipped: {result.StoresSkipped + result.ProductsSkipped + result.PricesSkipped}";

            if (result.Errors.Count > 0)
            {
                result.Message += $" Errors: {result.Errors.Count}";
            }

            _logger.LogInfo(result.Message);
            progress?.Report(new StaticImportProgress { Percentage = 100, Status = "Import complete!" });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Static import failed: {ex.Message}";
            _logger.LogError(result.ErrorMessage, ex);
            progress?.Report(new StaticImportProgress { Percentage = 0, Status = $"Error: {ex.Message}" });
        }

        return result;
    }

    /// <summary>
    /// Import data from a ZIP archive
    /// </summary>
    public async Task<StaticImportResult> ImportFromArchiveAsync(
        string archivePath,
        StaticImportOptions options,
        IProgress<StaticImportProgress>? progress = null)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"advgen_import_{Guid.NewGuid():N}");

        try
        {
            _logger.LogInfo($"Extracting archive: {archivePath}");
            progress?.Report(new StaticImportProgress { Percentage = 0, Status = "Extracting archive..." });

            // Validate archive exists
            if (!File.Exists(archivePath))
            {
                throw new FileNotFoundException($"Archive not found: {archivePath}");
            }

            // Create temp directory
            Directory.CreateDirectory(tempDir);

            // Extract archive
            await Task.Run(() => ZipFile.ExtractToDirectory(archivePath, tempDir));

            progress?.Report(new StaticImportProgress { Percentage = 10, Status = "Archive extracted. Starting import..." });

            // Import from extracted directory
            var result = await ImportFromDirectoryAsync(tempDir, options, progress);
            result.ArchivePath = archivePath;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to import from archive: {ex.Message}", ex);
            return new StaticImportResult
            {
                Success = false,
                ErrorMessage = $"Failed to import from archive: {ex.Message}",
                ArchivePath = archivePath
            };
        }
        finally
        {
            // Cleanup temp directory
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to cleanup temp directory: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Import data from a URL (downloads and imports)
    /// </summary>
    public async Task<StaticImportResult> ImportFromUrlAsync(
        string url,
        StaticImportOptions options,
        IProgress<StaticImportProgress>? progress = null)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"advgen_download_{Guid.NewGuid():N}.zip");

        try
        {
            _logger.LogInfo($"Downloading package from: {url}");
            progress?.Report(new StaticImportProgress { Percentage = 0, Status = "Downloading..." });

            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            // Download file
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
            await response.Content.CopyToAsync(fs);

            progress?.Report(new StaticImportProgress { Percentage = 30, Status = "Download complete. Starting import..." });

            // Determine if it's a zip or directory
            var contentType = response.Content.Headers.ContentType?.MediaType;
            var isZip = url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                       contentType == "application/zip" ||
                       contentType == "application/octet-stream";

            StaticImportResult result;
            if (isZip)
            {
                result = await ImportFromArchiveAsync(tempFile, options, progress);
            }
            else
            {
                // Assume it's a manifest URL - need to download the whole package
                result = await ImportFromArchiveAsync(tempFile, options, progress);
            }

            result.SourceUrl = url;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to import from URL: {ex.Message}", ex);
            return new StaticImportResult
            {
                Success = false,
                ErrorMessage = $"Failed to import from URL: {ex.Message}",
                SourceUrl = url
            };
        }
        finally
        {
            // Cleanup temp file
            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to cleanup temp file: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Preview the contents of a package without importing
    /// </summary>
    public async Task<StaticImportPreview> PreviewPackageAsync(string packagePath)
    {
        var preview = new StaticImportPreview();

        try
        {
            // Determine if it's a file (archive) or directory
            var isArchive = File.Exists(packagePath) && packagePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

            string manifestPath;
            string basePath;

            if (isArchive)
            {
                // Extract just the manifest from the zip
                using var archive = ZipFile.OpenRead(packagePath);
                var manifestEntry = archive.GetEntry("manifest.json");

                if (manifestEntry == null)
                {
                    preview.ErrorMessage = "Archive does not contain a manifest.json file.";
                    return preview;
                }

                await using var entryStream = manifestEntry.Open();
                var manifest = await JsonSerializer.DeserializeAsync<StaticExportManifest>(entryStream, _jsonOptions);

                if (manifest == null)
                {
                    preview.ErrorMessage = "Failed to parse manifest.";
                    return preview;
                }

                preview.PackageId = manifest.PackageId;
                preview.Version = manifest.Version;
                preview.CreatedAt = manifest.CreatedAt;
                preview.ExportedBy = manifest.ExportedBy;
                preview.Description = manifest.Description;
                preview.Location = manifest.Location;
                preview.StoreCount = manifest.DataStats.StoreCount;
                preview.ProductCount = manifest.DataStats.ProductCount;
                preview.PriceRecordCount = manifest.DataStats.PriceRecordCount;
                preview.Files = manifest.Files.Select(f => f.Name).ToList();

                return preview;
            }
            else
            {
                // Directory
                if (!Directory.Exists(packagePath))
                {
                    preview.ErrorMessage = $"Path not found: {packagePath}";
                    return preview;
                }

                manifestPath = Path.Combine(packagePath, "manifest.json");
                if (!File.Exists(manifestPath))
                {
                    preview.ErrorMessage = "Package does not contain a manifest.json file.";
                    return preview;
                }

                var manifestJson = await File.ReadAllTextAsync(manifestPath);
                var manifest = JsonSerializer.Deserialize<StaticExportManifest>(manifestJson, _jsonOptions);

                if (manifest == null)
                {
                    preview.ErrorMessage = "Failed to parse manifest.";
                    return preview;
                }

                preview.PackageId = manifest.PackageId;
                preview.Version = manifest.Version;
                preview.CreatedAt = manifest.CreatedAt;
                preview.ExportedBy = manifest.ExportedBy;
                preview.Description = manifest.Description;
                preview.Location = manifest.Location;
                preview.StoreCount = manifest.DataStats.StoreCount;
                preview.ProductCount = manifest.DataStats.ProductCount;
                preview.PriceRecordCount = manifest.DataStats.PriceRecordCount;
                preview.Files = manifest.Files.Select(f => f.Name).ToList();

                return preview;
            }
        }
        catch (Exception ex)
        {
            preview.ErrorMessage = $"Failed to preview package: {ex.Message}";
            _logger.LogError(preview.ErrorMessage, ex);
            return preview;
        }
    }

    /// <summary>
    /// Synchronize data from a static peer, importing only new or updated data since last sync.
    /// Fetches discovery info, checks timestamps, and performs incremental import.
    /// </summary>
    /// <param name="peerBaseUrl">Base URL of the static peer (e.g., "https://example.com/data")</param>
    /// <param name="options">Import options for handling duplicates</param>
    /// <param name="lastSyncTimestamp">Timestamp of last successful sync (null for full sync)</param>
    /// <param name="progress">Progress reporter</param>
    /// <returns>Sync result with details of what was imported</returns>
    public async Task<StaticSyncResult> SyncFromStaticPeerAsync(
        string peerBaseUrl,
        StaticImportOptions options,
        DateTime? lastSyncTimestamp = null,
        IProgress<StaticImportProgress>? progress = null)
    {
        var result = new StaticSyncResult
        {
            PeerUrl = peerBaseUrl,
            SyncStartedAt = DateTime.UtcNow,
            LastSyncTimestamp = lastSyncTimestamp
        };

        try
        {
            _logger.LogInfo($"Starting sync from static peer: {peerBaseUrl}");
            progress?.Report(new StaticImportProgress { Percentage = 0, Status = "Connecting to peer..." });

            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            // Fetch discovery.json to verify peer and get info
            var discoveryUrl = $"{peerBaseUrl.TrimEnd('/')}/discovery.json";
            StaticDiscoveryDto? discovery;
            
            try
            {
                var discoveryJson = await httpClient.GetStringAsync(discoveryUrl);
                discovery = JsonSerializer.Deserialize<StaticDiscoveryDto>(discoveryJson, _jsonOptions);
                
                if (discovery == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Failed to parse discovery.json from peer.";
                    return result;
                }
                
                result.PeerPackageId = discovery.PackageId;
                result.PeerLocation = discovery.Location;
                _logger.LogInfo($"Connected to peer: {discovery.ExportedBy} at {discovery.Location}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not fetch discovery.json: {ex.Message}. Proceeding with direct manifest fetch.");
                discovery = null;
            }

            progress?.Report(new StaticImportProgress { Percentage = 10, Status = "Checking peer manifest..." });

            // Fetch manifest.json to check timestamp
            var manifestUrl = $"{peerBaseUrl.TrimEnd('/')}/manifest.json";
            StaticExportManifest? manifest;
            
            try
            {
                var manifestJson = await httpClient.GetStringAsync(manifestUrl);
                manifest = JsonSerializer.Deserialize<StaticExportManifest>(manifestJson, _jsonOptions);
                
                if (manifest == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Failed to parse manifest.json from peer.";
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Failed to fetch manifest.json: {ex.Message}";
                _logger.LogError(result.ErrorMessage, ex);
                return result;
            }

            result.PeerPackageId = manifest.PackageId;
            result.PeerCreatedAt = manifest.CreatedAt;

            // Check if peer data is newer than last sync
            if (lastSyncTimestamp.HasValue && manifest.CreatedAt <= lastSyncTimestamp.Value)
            {
                result.Success = true;
                result.IsUpToDate = true;
                result.Message = "Peer data is up to date. No sync needed.";
                result.SyncCompletedAt = DateTime.UtcNow;
                _logger.LogInfo($"Peer data ({manifest.CreatedAt:yyyy-MM-dd HH:mm:ss}) is not newer than last sync ({lastSyncTimestamp.Value:yyyy-MM-dd HH:mm:ss}). Skipping sync.");
                progress?.Report(new StaticImportProgress { Percentage = 100, Status = "Already up to date!" });
                return result;
            }

            _logger.LogInfo($"Peer data is newer than last sync. Starting import...");
            progress?.Report(new StaticImportProgress { Percentage = 20, Status = "Downloading package..." });

            // Download the full package as ZIP (for now - in future could download individual files)
            var tempFile = Path.Combine(Path.GetTempPath(), $"advgen_sync_{Guid.NewGuid():N}.zip");
            string? extractedDir = null;

            try
            {
                // Try to download ZIP archive first
                var zipUrl = $"{peerBaseUrl.TrimEnd('/')}/price-data-{manifest.CreatedAt:yyyyMMdd-HHmmss}.zip";
                
                try
                {
                    var response = await httpClient.GetAsync(zipUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        await using var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
                        await response.Content.CopyToAsync(fs);
                        _logger.LogInfo($"Downloaded ZIP archive: {zipUrl}");
                    }
                    else
                    {
                        // Fall back to downloading individual files
                        _logger.LogInfo("ZIP archive not found, downloading individual files...");
                        extractedDir = Path.Combine(Path.GetTempPath(), $"advgen_sync_{Guid.NewGuid():N}");
                        Directory.CreateDirectory(extractedDir);
                        
                        await DownloadPackageFilesAsync(httpClient, peerBaseUrl, extractedDir, manifest, progress, result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to download ZIP: {ex.Message}. Falling back to individual files.");
                    extractedDir = Path.Combine(Path.GetTempPath(), $"advgen_sync_{Guid.NewGuid():N}");
                    Directory.CreateDirectory(extractedDir);
                    
                    await DownloadPackageFilesAsync(httpClient, peerBaseUrl, extractedDir, manifest, progress, result);
                }

                progress?.Report(new StaticImportProgress { Percentage = 60, Status = "Importing downloaded data..." });

                // Import from downloaded data
                StaticImportResult importResult;
                
                if (File.Exists(tempFile))
                {
                    // Import from ZIP
                    importResult = await ImportFromArchiveAsync(tempFile, options, 
                        new Progress<StaticImportProgress>(p => 
                        {
                            var adjustedPercentage = 60 + (p.Percentage * 0.4);
                            progress?.Report(new StaticImportProgress 
                            { 
                                Percentage = (int)adjustedPercentage, 
                                Status = p.Status 
                            });
                        }));
                }
                else if (!string.IsNullOrEmpty(extractedDir) && Directory.Exists(extractedDir))
                {
                    // Import from directory
                    importResult = await ImportFromDirectoryAsync(extractedDir, options,
                        new Progress<StaticImportProgress>(p =>
                        {
                            var adjustedPercentage = 60 + (p.Percentage * 0.4);
                            progress?.Report(new StaticImportProgress
                            {
                                Percentage = (int)adjustedPercentage,
                                Status = p.Status
                            });
                        }));
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "No data files were downloaded.";
                    return result;
                }

                // Copy import results to sync result
                result.Success = importResult.Success;
                result.Message = importResult.Message;
                result.ErrorMessage = importResult.ErrorMessage;
                result.StoresImported = importResult.StoresImported;
                result.StoresSkipped = importResult.StoresSkipped;
                result.StoresUpdated = importResult.StoresUpdated;
                result.ProductsImported = importResult.ProductsImported;
                result.ProductsSkipped = importResult.ProductsSkipped;
                result.ProductsUpdated = importResult.ProductsUpdated;
                result.PricesImported = importResult.PricesImported;
                result.PricesSkipped = importResult.PricesSkipped;
                result.PricesUpdated = importResult.PricesUpdated;
                result.Errors.AddRange(importResult.Errors);
                result.NewSyncTimestamp = manifest.CreatedAt;
            }
            finally
            {
                // Cleanup temp files
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                    if (!string.IsNullOrEmpty(extractedDir) && Directory.Exists(extractedDir))
                    {
                        Directory.Delete(extractedDir, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to cleanup temp files: {ex.Message}");
                }
            }

            result.SyncCompletedAt = DateTime.UtcNow;
            
            if (result.Success)
            {
                var duration = result.SyncCompletedAt - result.SyncStartedAt;
                _logger.LogInfo($"Sync completed in {duration.TotalSeconds:F1}s. {result.Message}");
                progress?.Report(new StaticImportProgress { Percentage = 100, Status = "Sync complete!" });
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Sync failed: {ex.Message}";
            result.SyncCompletedAt = DateTime.UtcNow;
            _logger.LogError(result.ErrorMessage, ex);
            progress?.Report(new StaticImportProgress { Percentage = 0, Status = $"Error: {ex.Message}" });
            return result;
        }
    }

    /// <summary>
    /// Download individual package files from a static peer
    /// </summary>
    private async Task DownloadPackageFilesAsync(
        System.Net.Http.HttpClient httpClient,
        string peerBaseUrl,
        string outputDirectory,
        StaticExportManifest manifest,
        IProgress<StaticImportProgress>? progress,
        StaticSyncResult result)
    {
        var filesToDownload = new[] { "stores.json", "products.json", "prices.json", "manifest.json" };
        var totalFiles = filesToDownload.Length;
        var downloadedFiles = 0;

        foreach (var fileName in filesToDownload)
        {
            try
            {
                var fileUrl = $"{peerBaseUrl.TrimEnd('/')}/{fileName}";
                var outputPath = Path.Combine(outputDirectory, fileName);

                progress?.Report(new StaticImportProgress 
                { 
                    Percentage = 20 + (downloadedFiles * 40 / totalFiles), 
                    Status = $"Downloading {fileName}..." 
                });

                var fileContent = await httpClient.GetStringAsync(fileUrl);
                await File.WriteAllTextAsync(outputPath, fileContent);
                
                downloadedFiles++;
                result.FilesDownloaded.Add(fileName);
                _logger.LogDebug($"Downloaded {fileName} from peer");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to download {fileName}: {ex.Message}");
                result.Errors.Add($"Failed to download {fileName}: {ex.Message}");
            }
        }

        result.TotalFilesDownloaded = downloadedFiles;
    }

    #endregion

    #region Private Import Methods

    private async Task ValidateChecksumsAsync(string packageDirectory, StaticExportManifest manifest, StaticImportResult result)
    {
        foreach (var fileEntry in manifest.Files)
        {
            var filePath = Path.Combine(packageDirectory, fileEntry.Name);

            if (!File.Exists(filePath))
            {
                result.ChecksumErrors.Add($"File missing: {fileEntry.Name}");
                continue;
            }

            if (!string.IsNullOrEmpty(fileEntry.Checksum))
            {
                var actualChecksum = await CalculateChecksumAsync(filePath);
                if (!actualChecksum.Equals(fileEntry.Checksum, StringComparison.OrdinalIgnoreCase))
                {
                    result.ChecksumErrors.Add($"Checksum mismatch for {fileEntry.Name}");
                }
            }
        }

        if (result.ChecksumErrors.Count > 0)
        {
            result.Success = false;
            _logger.LogWarning($"Checksum validation failed for {result.ChecksumErrors.Count} files.");
        }
    }

    private async Task<Dictionary<string, string>> ImportStoresAsync(
        string packageDirectory,
        StaticImportOptions options,
        StaticImportResult result)
    {
        var storeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // Maps external ID -> internal ID
        var storesPath = Path.Combine(packageDirectory, "stores.json");

        if (!File.Exists(storesPath))
        {
            _logger.LogWarning("stores.json not found in package.");
            return storeMap;
        }

        try
        {
            var json = await File.ReadAllTextAsync(storesPath);
            var container = JsonSerializer.Deserialize<StaticStoresContainer>(json, _jsonOptions);

            if (container?.Stores == null || container.Stores.Count == 0)
            {
                _logger.LogWarning("No stores found in stores.json.");
                return storeMap;
            }

            foreach (var storeDto in container.Stores)
            {
                try
                {
                    var importResult = await ImportSingleStoreAsync(storeDto, options);

                    switch (importResult.Action)
                    {
                        case ImportAction.Imported:
                            result.StoresImported++;
                            break;
                        case ImportAction.Skipped:
                            result.StoresSkipped++;
                            break;
                        case ImportAction.Updated:
                            result.StoresUpdated++;
                            break;
                    }

                    // Map external ID to internal ID
                    storeMap[storeDto.Id] = importResult.InternalId;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to import store {storeDto.Name}: {ex.Message}");
                    _logger.LogError($"Failed to import store {storeDto.Name}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to import stores: {ex.Message}");
            _logger.LogError("Failed to import stores", ex);
        }

        return storeMap;
    }

    private async Task<ImportSingleResult> ImportSingleStoreAsync(StaticStoreDto storeDto, StaticImportOptions options)
    {
        // Check for existing store by name and suburb
        var existingStores = _placeRepository.GetAll()
            .Where(s => s.Name.Equals(storeDto.Name, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(storeDto.Suburb))
        {
            existingStores = existingStores.Where(s =>
                s.Suburb?.Equals(storeDto.Suburb, StringComparison.OrdinalIgnoreCase) == true);
        }

        var existingStore = existingStores.FirstOrDefault();

        if (existingStore != null)
        {
            switch (options.DuplicateStoreStrategy)
            {
                case DuplicateStrategy.Skip:
                    _logger.LogInfo($"Skipping existing store: {storeDto.Name}");
                    return new ImportSingleResult { Action = ImportAction.Skipped, InternalId = existingStore.Id };

                case DuplicateStrategy.Update:
                    _logger.LogInfo($"Updating existing store: {storeDto.Name}");
                    existingStore.Chain = storeDto.Chain ?? existingStore.Chain;
                    existingStore.Address = storeDto.Address ?? existingStore.Address;
                    existingStore.Suburb = storeDto.Suburb ?? existingStore.Suburb;
                    existingStore.State = storeDto.State ?? existingStore.State;
                    existingStore.Postcode = storeDto.Postcode ?? existingStore.Postcode;
                    existingStore.Latitude = storeDto.Latitude ?? existingStore.Latitude;
                    existingStore.Longitude = storeDto.Longitude ?? existingStore.Longitude;
                    existingStore.Phone = storeDto.Phone ?? existingStore.Phone;
                    existingStore.IsActive = storeDto.IsActive;

                    _placeRepository.Update(existingStore);
                    return new ImportSingleResult { Action = ImportAction.Updated, InternalId = existingStore.Id };

                case DuplicateStrategy.CreateNew:
                    // Fall through to create new
                    break;
            }
        }

        // Create new store
        var newStore = new Place
        {
            Id = Guid.NewGuid().ToString(),
            Name = storeDto.Name,
            Chain = storeDto.Chain,
            Address = storeDto.Address,
            Suburb = storeDto.Suburb,
            State = storeDto.State,
            Postcode = storeDto.Postcode,
            Latitude = storeDto.Latitude,
            Longitude = storeDto.Longitude,
            Phone = storeDto.Phone,
            IsActive = storeDto.IsActive,
            DateAdded = DateTime.UtcNow
        };

        var id = _placeRepository.Add(newStore);
        _logger.LogInfo($"Imported store: {storeDto.Name} (ID: {id})");

        return new ImportSingleResult { Action = ImportAction.Imported, InternalId = id };
    }

    private async Task<Dictionary<string, string>> ImportProductsAsync(
        string packageDirectory,
        StaticImportOptions options,
        StaticImportResult result,
        Dictionary<string, string> storeMap)
    {
        var productMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // Maps external ID -> internal ID
        var productsPath = Path.Combine(packageDirectory, "products.json");

        if (!File.Exists(productsPath))
        {
            _logger.LogWarning("products.json not found in package.");
            return productMap;
        }

        try
        {
            var json = await File.ReadAllTextAsync(productsPath);
            var container = JsonSerializer.Deserialize<StaticProductsContainer>(json, _jsonOptions);

            if (container?.Products == null || container.Products.Count == 0)
            {
                _logger.LogWarning("No products found in products.json.");
                return productMap;
            }

            foreach (var productDto in container.Products)
            {
                try
                {
                    var importResult = await ImportSingleProductAsync(productDto, options);

                    switch (importResult.Action)
                    {
                        case ImportAction.Imported:
                            result.ProductsImported++;
                            break;
                        case ImportAction.Skipped:
                            result.ProductsSkipped++;
                            break;
                        case ImportAction.Updated:
                            result.ProductsUpdated++;
                            break;
                    }

                    // Map external ID to internal ID
                    productMap[productDto.Id] = importResult.InternalId;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to import product {productDto.Name}: {ex.Message}");
                    _logger.LogError($"Failed to import product {productDto.Name}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to import products: {ex.Message}");
            _logger.LogError("Failed to import products", ex);
        }

        return productMap;
    }

    private async Task<ImportSingleResult> ImportSingleProductAsync(StaticProductDto productDto, StaticImportOptions options)
    {
        // Check for existing product by barcode (most reliable) or name + brand
        Item? existingProduct = null;

        if (!string.IsNullOrEmpty(productDto.Barcode))
        {
            existingProduct = _itemRepository.GetByBarcode(productDto.Barcode).FirstOrDefault();
        }

        if (existingProduct == null && !string.IsNullOrEmpty(productDto.Name))
        {
            existingProduct = _itemRepository.GetAll()
                .FirstOrDefault(i =>
                    i.Name.Equals(productDto.Name, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrEmpty(productDto.Brand) ||
                     i.Brand?.Equals(productDto.Brand, StringComparison.OrdinalIgnoreCase) == true));
        }

        if (existingProduct != null)
        {
            switch (options.DuplicateProductStrategy)
            {
                case DuplicateStrategy.Skip:
                    _logger.LogInfo($"Skipping existing product: {productDto.Name}");
                    return new ImportSingleResult { Action = ImportAction.Skipped, InternalId = existingProduct.Id };

                case DuplicateStrategy.Update:
                    _logger.LogInfo($"Updating existing product: {productDto.Name}");
                    existingProduct.Brand = productDto.Brand ?? existingProduct.Brand;
                    existingProduct.Category = productDto.Category ?? existingProduct.Category;
                    existingProduct.SubCategory = productDto.SubCategory ?? existingProduct.SubCategory;
                    existingProduct.Description = productDto.Description ?? existingProduct.Description;
                    existingProduct.PackageSize = productDto.PackageSize ?? existingProduct.PackageSize;
                    existingProduct.Unit = productDto.Unit ?? existingProduct.Unit;
                    existingProduct.ImageUrl = productDto.ImageUrl ?? existingProduct.ImageUrl;
                    existingProduct.IsActive = productDto.IsActive;

                    if (productDto.Tags?.Count > 0)
                    {
                        foreach (var tag in productDto.Tags.Where(t => !existingProduct.Tags.Contains(t)))
                        {
                            existingProduct.Tags.Add(tag);
                        }
                    }

                    existingProduct.LastUpdated = DateTime.UtcNow;
                    _itemRepository.Update(existingProduct);
                    return new ImportSingleResult { Action = ImportAction.Updated, InternalId = existingProduct.Id };

                case DuplicateStrategy.CreateNew:
                    // Fall through to create new
                    break;
            }
        }

        // Create new product
        var newProduct = new Item
        {
            Id = Guid.NewGuid().ToString(),
            Name = productDto.Name,
            Brand = productDto.Brand,
            Category = productDto.Category,
            SubCategory = productDto.SubCategory,
            Description = productDto.Description,
            PackageSize = productDto.PackageSize,
            Unit = productDto.Unit,
            Barcode = productDto.Barcode,
            ImageUrl = productDto.ImageUrl,
            Tags = productDto.Tags ?? new List<string>(),
            IsActive = productDto.IsActive,
            DateAdded = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        var id = _itemRepository.Add(newProduct);
        _logger.LogInfo($"Imported product: {productDto.Name} (ID: {id})");

        return new ImportSingleResult { Action = ImportAction.Imported, InternalId = id };
    }

    private async Task ImportPricesAsync(
        string packageDirectory,
        StaticImportOptions options,
        StaticImportResult result,
        Dictionary<string, string> productMap,
        Dictionary<string, string> storeMap)
    {
        var pricesPath = Path.Combine(packageDirectory, "prices.json");

        if (!File.Exists(pricesPath))
        {
            _logger.LogWarning("prices.json not found in package.");
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(pricesPath);
            var container = JsonSerializer.Deserialize<StaticPricesContainer>(json, _jsonOptions);

            if (container?.Prices == null || container.Prices.Count == 0)
            {
                _logger.LogWarning("No prices found in prices.json.");
                return;
            }

            foreach (var priceDto in container.Prices)
            {
                try
                {
                    // Map external IDs to internal IDs
                    if (!productMap.TryGetValue(priceDto.ProductId, out var internalProductId))
                    {
                        _logger.LogWarning($"Product {priceDto.ProductId} not found in import map. Skipping price record.");
                        result.PricesSkipped++;
                        continue;
                    }

                    if (!storeMap.TryGetValue(priceDto.StoreId, out var internalStoreId))
                    {
                        _logger.LogWarning($"Store {priceDto.StoreId} not found in import map. Skipping price record.");
                        result.PricesSkipped++;
                        continue;
                    }

                    var importResult = await ImportSinglePriceAsync(priceDto, internalProductId, internalStoreId, options);

                    switch (importResult.Action)
                    {
                        case ImportAction.Imported:
                            result.PricesImported++;
                            break;
                        case ImportAction.Skipped:
                            result.PricesSkipped++;
                            break;
                        case ImportAction.Updated:
                            result.PricesUpdated++;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to import price record: {ex.Message}");
                    _logger.LogError("Failed to import price record", ex);
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to import prices: {ex.Message}");
            _logger.LogError("Failed to import prices", ex);
        }
    }

    private async Task<ImportSingleResult> ImportSinglePriceAsync(
        StaticPriceDto priceDto,
        string internalProductId,
        string internalStoreId,
        StaticImportOptions options)
    {
        // Check for existing price record for same product/store on same date
        var existingPrices = _priceRecordRepository.GetByItemAndPlace(internalProductId, internalStoreId);
        var existingPrice = existingPrices
            .FirstOrDefault(p => p.DateRecorded.Date == priceDto.RecordedAt.Date);

        if (existingPrice != null)
        {
            switch (options.DuplicatePriceStrategy)
            {
                case DuplicateStrategy.Skip:
                    _logger.LogDebug($"Skipping existing price record for product {internalProductId} at store {internalStoreId}");
                    return new ImportSingleResult { Action = ImportAction.Skipped, InternalId = existingPrice.Id };

                case DuplicateStrategy.Update:
                    _logger.LogDebug($"Updating existing price record for product {internalProductId}");
                    existingPrice.Price = priceDto.Price;
                    existingPrice.OriginalPrice = priceDto.OriginalPrice;
                    existingPrice.IsOnSale = priceDto.IsOnSale;
                    existingPrice.SaleDescription = priceDto.SaleDescription ?? existingPrice.SaleDescription;
                    existingPrice.ValidFrom = priceDto.ValidFrom ?? existingPrice.ValidFrom;
                    existingPrice.ValidTo = priceDto.ValidTo ?? existingPrice.ValidTo;
                    existingPrice.Source = priceDto.Source ?? existingPrice.Source;

                    _priceRecordRepository.Update(existingPrice);
                    return new ImportSingleResult { Action = ImportAction.Updated, InternalId = existingPrice.Id };

                case DuplicateStrategy.CreateNew:
                    // Fall through to create new
                    break;
            }
        }

        // Create new price record
        var newPriceRecord = new PriceRecord
        {
            Id = Guid.NewGuid().ToString(),
            ItemId = internalProductId,
            PlaceId = internalStoreId,
            Price = priceDto.Price,
            OriginalPrice = priceDto.OriginalPrice,
            IsOnSale = priceDto.IsOnSale,
            SaleDescription = priceDto.SaleDescription,
            ValidFrom = priceDto.ValidFrom,
            ValidTo = priceDto.ValidTo,
            DateRecorded = priceDto.RecordedAt,
            Source = priceDto.Source ?? "import"
        };

        var id = _priceRecordRepository.Add(newPriceRecord);
        _logger.LogDebug($"Imported price record: {id} (Product: {internalProductId}, Store: {internalStoreId}, Price: {priceDto.Price:C})");

        return new ImportSingleResult { Action = ImportAction.Imported, InternalId = id };
    }

    private async Task<string> CalculateChecksumAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    #endregion
}

#region Options and Results

/// <summary>
/// Options for static import operations
/// </summary>
public class StaticImportOptions
{
    /// <summary>
    /// Strategy for handling duplicate stores
    /// </summary>
    public DuplicateStrategy DuplicateStoreStrategy { get; set; } = DuplicateStrategy.Update;

    /// <summary>
    /// Strategy for handling duplicate products
    /// </summary>
    public DuplicateStrategy DuplicateProductStrategy { get; set; } = DuplicateStrategy.Update;

    /// <summary>
    /// Strategy for handling duplicate price records
    /// </summary>
    public DuplicateStrategy DuplicatePriceStrategy { get; set; } = DuplicateStrategy.Skip;

    /// <summary>
    /// Whether to validate file checksums
    /// </summary>
    public bool ValidateChecksums { get; set; } = true;

    /// <summary>
    /// Whether to fail the entire import if checksum validation fails
    /// </summary>
    public bool FailOnChecksumError { get; set; } = false;

    /// <summary>
    /// Whether to fail the entire import if any individual import fails
    /// </summary>
    public bool FailOnError { get; set; } = false;
}

/// <summary>
/// Strategy for handling duplicate entries during import
/// </summary>
public enum DuplicateStrategy
{
    /// <summary>
    /// Skip duplicate entries (keep existing)
    /// </summary>
    Skip,

    /// <summary>
    /// Update existing entries with new data
    /// </summary>
    Update,

    /// <summary>
    /// Create new entries even if duplicates exist
    /// </summary>
    CreateNew
}

/// <summary>
/// Result of a static import operation
/// </summary>
public class StaticImportResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PackageId { get; set; }
    public string? SourceUrl { get; set; }
    public string? ArchivePath { get; set; }
    public DateTime ImportedAt { get; set; }

    public int StoresImported { get; set; }
    public int StoresSkipped { get; set; }
    public int StoresUpdated { get; set; }

    public int ProductsImported { get; set; }
    public int ProductsSkipped { get; set; }
    public int ProductsUpdated { get; set; }

    public int PricesImported { get; set; }
    public int PricesSkipped { get; set; }
    public int PricesUpdated { get; set; }

    public List<string> Errors { get; set; } = new();
    public List<string> ChecksumErrors { get; set; } = new();
}

/// <summary>
/// Progress information for static import operations
/// </summary>
public class StaticImportProgress
{
    public int Percentage { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Preview of a package before importing
/// </summary>
public class StaticImportPreview
{
    public string? PackageId { get; set; }
    public string? Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ExportedBy { get; set; }
    public string? Description { get; set; }
    public ExportLocationInfo? Location { get; set; }
    public int StoreCount { get; set; }
    public int ProductCount { get; set; }
    public int PriceRecordCount { get; set; }
    public List<string> Files { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Internal result of importing a single entity
/// </summary>
internal class ImportSingleResult
{
    public ImportAction Action { get; set; }
    public string InternalId { get; set; } = string.Empty;
}

/// <summary>
/// Action taken during import of a single entity
/// </summary>
internal enum ImportAction
{
    Imported,
    Skipped,
    Updated
}

#endregion

#region Sync Result

/// <summary>
/// Result of a sync operation from a static peer
/// </summary>
public class StaticSyncResult
{
    /// <summary>
    /// Whether the sync was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable status message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Error message if sync failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// URL of the peer that was synced
    /// </summary>
    public string? PeerUrl { get; set; }

    /// <summary>
    /// Package ID from the peer
    /// </summary>
    public string? PeerPackageId { get; set; }

    /// <summary>
    /// Location of the peer
    /// </summary>
    public string? PeerLocation { get; set; }

    /// <summary>
    /// Timestamp when the peer's package was created
    /// </summary>
    public DateTime PeerCreatedAt { get; set; }

    /// <summary>
    /// Whether the peer data was up to date (no sync needed)
    /// </summary>
    public bool IsUpToDate { get; set; }

    /// <summary>
    /// When the sync started
    /// </summary>
    public DateTime SyncStartedAt { get; set; }

    /// <summary>
    /// When the sync completed
    /// </summary>
    public DateTime SyncCompletedAt { get; set; }

    /// <summary>
    /// The last sync timestamp that was provided (for incremental sync)
    /// </summary>
    public DateTime? LastSyncTimestamp { get; set; }

    /// <summary>
    /// The new sync timestamp to save for future incremental syncs
    /// </summary>
    public DateTime? NewSyncTimestamp { get; set; }

    // Import counts
    public int StoresImported { get; set; }
    public int StoresSkipped { get; set; }
    public int StoresUpdated { get; set; }

    public int ProductsImported { get; set; }
    public int ProductsSkipped { get; set; }
    public int ProductsUpdated { get; set; }

    public int PricesImported { get; set; }
    public int PricesSkipped { get; set; }
    public int PricesUpdated { get; set; }

    /// <summary>
    /// Number of files downloaded from peer
    /// </summary>
    public int TotalFilesDownloaded { get; set; }

    /// <summary>
    /// List of files that were downloaded
    /// </summary>
    public List<string> FilesDownloaded { get; set; } = new();

    /// <summary>
    /// List of errors encountered during sync
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

#endregion
