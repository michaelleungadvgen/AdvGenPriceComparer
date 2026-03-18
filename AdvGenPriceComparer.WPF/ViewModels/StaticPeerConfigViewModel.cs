using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for configuring static P2P peers and discovery sources
/// </summary>
public class StaticPeerConfigViewModel : ViewModelBase
{
    private readonly PeerDiscoveryService _peerDiscoveryService;
    private readonly ILoggerService _logger;
    private readonly IDialogService _dialogService;

    private DiscoveredPeer? _selectedPeer;
    private DiscoverySource? _selectedSource;
    private bool _isEditingPeer;
    private bool _isEditingSource;
    private bool _isDiscovering;
    private int _discoveryProgress;
    private string _statusMessage = string.Empty;

    // Peer edit fields
    private string _peerId = string.Empty;
    private string _peerType = "static_peer";
    private string _peerAddress = string.Empty;
    private string? _peerLocation;
    private string? _peerDescription;
    private string? _peerRegion;
    private bool _peerIsEnabled = true;
    private bool _peerIsFavorite;

    // Source edit fields
    private string _sourceName = string.Empty;
    private DiscoverySourceType _sourceType = DiscoverySourceType.HttpUrl;
    private string _sourcePath = string.Empty;
    private bool _sourceIsEnabled = true;
    private int _sourcePriority = 100;
    private int _sourceRefreshInterval = 60;
    private string? _sourceApiKey;

    /// <summary>
    /// Collection of discovered peers
    /// </summary>
    public ObservableCollection<DiscoveredPeer> Peers { get; } = new();

    /// <summary>
    /// Collection of discovery sources
    /// </summary>
    public ObservableCollection<DiscoverySource> Sources { get; } = new();

    /// <summary>
    /// Available peer types
    /// </summary>
    public ObservableCollection<string> PeerTypes { get; } = new()
    {
        "static_peer",
        "full_peer"
    };

    /// <summary>
    /// Available discovery source types
    /// </summary>
    public ObservableCollection<DiscoverySourceType> SourceTypes { get; } = new()
    {
        DiscoverySourceType.LocalFile,
        DiscoverySourceType.HttpUrl,
        DiscoverySourceType.NetworkShare
    };

    /// <summary>
    /// Selected peer for editing
    /// </summary>
    public DiscoveredPeer? SelectedPeer
    {
        get => _selectedPeer;
        set
        {
            _selectedPeer = value;
            OnPropertyChanged();
            if (value != null)
            {
                LoadPeerForEdit(value);
            }
        }
    }

    /// <summary>
    /// Selected source for editing
    /// </summary>
    public DiscoverySource? SelectedSource
    {
        get => _selectedSource;
        set
        {
            _selectedSource = value;
            OnPropertyChanged();
            if (value != null)
            {
                LoadSourceForEdit(value);
            }
        }
    }

    /// <summary>
    /// Whether currently editing a peer
    /// </summary>
    public bool IsEditingPeer
    {
        get => _isEditingPeer;
        set
        {
            _isEditingPeer = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether currently editing a source
    /// </summary>
    public bool IsEditingSource
    {
        get => _isEditingSource;
        set
        {
            _isEditingSource = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether a discovery operation is in progress
    /// </summary>
    public bool IsDiscovering
    {
        get => _isDiscovering;
        set
        {
            _isDiscovering = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Discovery progress percentage
    /// </summary>
    public int DiscoveryProgress
    {
        get => _discoveryProgress;
        set
        {
            _discoveryProgress = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Status message for operations
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    // Peer edit properties
    public string PeerId
    {
        get => _peerId;
        set { _peerId = value; OnPropertyChanged(); }
    }

    public string PeerType
    {
        get => _peerType;
        set { _peerType = value; OnPropertyChanged(); }
    }

    public string PeerAddress
    {
        get => _peerAddress;
        set { _peerAddress = value; OnPropertyChanged(); }
    }

    public string? PeerLocation
    {
        get => _peerLocation;
        set { _peerLocation = value; OnPropertyChanged(); }
    }

    public string? PeerDescription
    {
        get => _peerDescription;
        set { _peerDescription = value; OnPropertyChanged(); }
    }

    public string? PeerRegion
    {
        get => _peerRegion;
        set { _peerRegion = value; OnPropertyChanged(); }
    }

    public bool PeerIsEnabled
    {
        get => _peerIsEnabled;
        set { _peerIsEnabled = value; OnPropertyChanged(); }
    }

    public bool PeerIsFavorite
    {
        get => _peerIsFavorite;
        set { _peerIsFavorite = value; OnPropertyChanged(); }
    }

    // Source edit properties
    public string SourceName
    {
        get => _sourceName;
        set { _sourceName = value; OnPropertyChanged(); }
    }

    public DiscoverySourceType SourceType
    {
        get => _sourceType;
        set { _sourceType = value; OnPropertyChanged(); }
    }

    public string SourcePath
    {
        get => _sourcePath;
        set { _sourcePath = value; OnPropertyChanged(); }
    }

    public bool SourceIsEnabled
    {
        get => _sourceIsEnabled;
        set { _sourceIsEnabled = value; OnPropertyChanged(); }
    }

    public int SourcePriority
    {
        get => _sourcePriority;
        set { _sourcePriority = value; OnPropertyChanged(); }
    }

    public int SourceRefreshInterval
    {
        get => _sourceRefreshInterval;
        set { _sourceRefreshInterval = value; OnPropertyChanged(); }
    }

    public string? SourceApiKey
    {
        get => _sourceApiKey;
        set { _sourceApiKey = value; OnPropertyChanged(); }
    }

    // Commands
    public ICommand AddPeerCommand { get; }
    public ICommand EditPeerCommand { get; }
    public ICommand SavePeerCommand { get; }
    public ICommand CancelPeerEditCommand { get; }
    public ICommand DeletePeerCommand { get; }
    public ICommand TogglePeerEnabledCommand { get; }
    public ICommand TogglePeerFavoriteCommand { get; }
    public ICommand CheckPeerHealthCommand { get; }

    public ICommand AddSourceCommand { get; }
    public ICommand EditSourceCommand { get; }
    public ICommand SaveSourceCommand { get; }
    public ICommand CancelSourceEditCommand { get; }
    public ICommand DeleteSourceCommand { get; }
    public ICommand ToggleSourceEnabledCommand { get; }

    public ICommand DiscoverPeersCommand { get; }
    public ICommand CheckAllHealthCommand { get; }
    public ICommand CloseCommand { get; }

    public event EventHandler? RequestClose;

    public StaticPeerConfigViewModel(
        PeerDiscoveryService peerDiscoveryService,
        ILoggerService logger,
        IDialogService dialogService)
    {
        _peerDiscoveryService = peerDiscoveryService;
        _logger = logger;
        _dialogService = dialogService;

        // Peer commands
        AddPeerCommand = new RelayCommand(() => StartAddPeer());
        EditPeerCommand = new RelayCommand<DiscoveredPeer>(p => StartEditPeer(p!), p => p != null);
        SavePeerCommand = new RelayCommand(async () => await SavePeerAsync(), () => CanSavePeer());
        CancelPeerEditCommand = new RelayCommand(() => CancelPeerEdit());
        DeletePeerCommand = new RelayCommand<DiscoveredPeer>(p => DeletePeer(p!), p => p != null);
        TogglePeerEnabledCommand = new RelayCommand<DiscoveredPeer>(p => TogglePeerEnabled(p!), p => p != null);
        TogglePeerFavoriteCommand = new RelayCommand<DiscoveredPeer>(p => TogglePeerFavorite(p!), p => p != null);
        CheckPeerHealthCommand = new RelayCommand<DiscoveredPeer>(async p => await CheckPeerHealthAsync(p!), p => p != null);

        // Source commands
        AddSourceCommand = new RelayCommand(() => StartAddSource());
        EditSourceCommand = new RelayCommand<DiscoverySource>(s => StartEditSource(s!), s => s != null);
        SaveSourceCommand = new RelayCommand(() => SaveSource(), () => CanSaveSource());
        CancelSourceEditCommand = new RelayCommand(() => CancelSourceEdit());
        DeleteSourceCommand = new RelayCommand<DiscoverySource>(s => DeleteSource(s!), s => s != null);
        ToggleSourceEnabledCommand = new RelayCommand<DiscoverySource>(s => ToggleSourceEnabled(s!), s => s != null);

        // Discovery commands
        DiscoverPeersCommand = new RelayCommand(async () => await DiscoverPeersAsync(), () => !IsDiscovering);
        CheckAllHealthCommand = new RelayCommand(async () => await CheckAllHealthAsync(), () => !IsDiscovering);
        CloseCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));

        LoadData();
    }

    private void LoadData()
    {
        Peers.Clear();
        foreach (var peer in _peerDiscoveryService.Peers.OrderByDescending(p => p.IsFavorite).ThenBy(p => p.Id))
        {
            Peers.Add(peer);
        }

        Sources.Clear();
        foreach (var source in _peerDiscoveryService.Sources.OrderBy(s => s.Priority))
        {
            Sources.Add(source);
        }
    }

    #region Peer Management

    private void StartAddPeer()
    {
        SelectedPeer = null;
        PeerId = string.Empty;
        PeerType = "static_peer";
        PeerAddress = string.Empty;
        PeerLocation = null;
        PeerDescription = null;
        PeerRegion = null;
        PeerIsEnabled = true;
        PeerIsFavorite = false;
        IsEditingPeer = true;
    }

    private void StartEditPeer(DiscoveredPeer peer)
    {
        LoadPeerForEdit(peer);
        IsEditingPeer = true;
    }

    private void LoadPeerForEdit(DiscoveredPeer peer)
    {
        PeerId = peer.Id;
        PeerType = peer.Type;
        PeerAddress = peer.Address;
        PeerLocation = peer.Location;
        PeerDescription = peer.Description;
        PeerRegion = peer.Region;
        PeerIsEnabled = peer.IsEnabled;
        PeerIsFavorite = peer.IsFavorite;
    }

    private bool CanSavePeer()
    {
        return !string.IsNullOrWhiteSpace(PeerId) &&
               !string.IsNullOrWhiteSpace(PeerAddress) &&
               Uri.TryCreate(PeerAddress, UriKind.Absolute, out _);
    }

    private async Task SavePeerAsync()
    {
        try
        {
            var peer = new DiscoveredPeer
            {
                Id = PeerId.Trim(),
                Type = PeerType,
                Address = PeerAddress.Trim(),
                Location = PeerLocation?.Trim(),
                Description = PeerDescription?.Trim(),
                Region = PeerRegion?.Trim(),
                IsEnabled = PeerIsEnabled,
                IsFavorite = PeerIsFavorite,
                Source = "Manual"
            };

            // Remove existing peer with same ID if editing
            if (SelectedPeer != null && SelectedPeer.Id != peer.Id)
            {
                _peerDiscoveryService.RemovePeer(SelectedPeer.Id);
            }

            _peerDiscoveryService.RemovePeer(peer.Id);

            // Add to PeerDiscoveryService via reflection or public method
            // Since there's no direct AddPeer method, we'll add it as a source and discover
            var tempSource = new DiscoverySource
            {
                Name = $"Manual_{peer.Id}",
                Type = DiscoverySourceType.HttpUrl,
                Path = peer.Address + "/discovery.json",
                IsEnabled = true,
                Priority = 999
            };

            _peerDiscoveryService.AddSource(tempSource);

            // Try to discover from this peer
            await _peerDiscoveryService.DiscoverPeersFromSourceAsync(tempSource);

            _logger.LogInfo($"Saved peer: {peer.Id}");
            LoadData();
            IsEditingPeer = false;
            SelectedPeer = null;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save peer", ex);
            _dialogService.ShowError($"Failed to save peer: {ex.Message}", "Error");
        }
    }

    private void CancelPeerEdit()
    {
        IsEditingPeer = false;
        SelectedPeer = null;
    }

    private void DeletePeer(DiscoveredPeer peer)
    {
        if (_dialogService.ShowConfirmation($"Are you sure you want to delete the peer '{peer.Id}'?", "Confirm Delete"))
        {
            _peerDiscoveryService.RemovePeer(peer.Id);
            Peers.Remove(peer);
            _logger.LogInfo($"Deleted peer: {peer.Id}");
        }
    }

    private void TogglePeerEnabled(DiscoveredPeer peer)
    {
        if (peer.IsEnabled)
        {
            _peerDiscoveryService.DisablePeer(peer.Id);
            peer.IsEnabled = false;
        }
        else
        {
            _peerDiscoveryService.EnablePeer(peer.Id);
            peer.IsEnabled = true;
        }

        // Refresh the collection
        var index = Peers.IndexOf(peer);
        if (index >= 0)
        {
            Peers.RemoveAt(index);
            Peers.Insert(index, peer);
        }
    }

    private void TogglePeerFavorite(DiscoveredPeer peer)
    {
        _peerDiscoveryService.SetPeerFavorite(peer.Id, !peer.IsFavorite);
        peer.IsFavorite = !peer.IsFavorite;
        LoadData(); // Re-sort
    }

    private async Task CheckPeerHealthAsync(DiscoveredPeer peer)
    {
        StatusMessage = $"Checking health of {peer.Id}...";
        var status = await _peerDiscoveryService.CheckPeerHealthAsync(peer);
        StatusMessage = $"Health check for {peer.Id}: {status}";
        _logger.LogInfo($"Health check for {peer.Id}: {status}");

        // Refresh display
        var index = Peers.IndexOf(peer);
        if (index >= 0)
        {
            Peers.RemoveAt(index);
            Peers.Insert(index, peer);
        }
    }

    #endregion

    #region Source Management

    private void StartAddSource()
    {
        SelectedSource = null;
        SourceName = string.Empty;
        SourceType = DiscoverySourceType.HttpUrl;
        SourcePath = string.Empty;
        SourceIsEnabled = true;
        SourcePriority = 100;
        SourceRefreshInterval = 60;
        SourceApiKey = null;
        IsEditingSource = true;
    }

    private void StartEditSource(DiscoverySource source)
    {
        LoadSourceForEdit(source);
        IsEditingSource = true;
    }

    private void LoadSourceForEdit(DiscoverySource source)
    {
        SourceName = source.Name;
        SourceType = source.Type;
        SourcePath = source.Path;
        SourceIsEnabled = source.IsEnabled;
        SourcePriority = source.Priority;
        SourceRefreshInterval = source.RefreshIntervalMinutes;
        SourceApiKey = source.ApiKey;
    }

    private bool CanSaveSource()
    {
        return !string.IsNullOrWhiteSpace(SourceName) &&
               !string.IsNullOrWhiteSpace(SourcePath);
    }

    private void SaveSource()
    {
        try
        {
            var source = new DiscoverySource
            {
                Name = SourceName.Trim(),
                Type = SourceType,
                Path = SourcePath.Trim(),
                IsEnabled = SourceIsEnabled,
                Priority = SourcePriority,
                RefreshIntervalMinutes = SourceRefreshInterval,
                ApiKey = SourceApiKey?.Trim()
            };

            _peerDiscoveryService.AddSource(source);

            _logger.LogInfo($"Saved source: {source.Name}");
            LoadData();
            IsEditingSource = false;
            SelectedSource = null;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save source", ex);
            _dialogService.ShowError($"Failed to save source: {ex.Message}", "Error");
        }
    }

    private void CancelSourceEdit()
    {
        IsEditingSource = false;
        SelectedSource = null;
    }

    private void DeleteSource(DiscoverySource source)
    {
        if (_dialogService.ShowConfirmation($"Are you sure you want to delete the source '{source.Name}'?", "Confirm Delete"))
        {
            _peerDiscoveryService.RemoveSource(source.Name);
            Sources.Remove(source);
            _logger.LogInfo($"Deleted source: {source.Name}");
        }
    }

    private void ToggleSourceEnabled(DiscoverySource source)
    {
        if (source.IsEnabled)
        {
            _peerDiscoveryService.DisableSource(source.Name);
            source.IsEnabled = false;
        }
        else
        {
            _peerDiscoveryService.EnableSource(source.Name);
            source.IsEnabled = true;
        }

        var index = Sources.IndexOf(source);
        if (index >= 0)
        {
            Sources.RemoveAt(index);
            Sources.Insert(index, source);
        }
    }

    #endregion

    #region Discovery

    private async Task DiscoverPeersAsync()
    {
        IsDiscovering = true;
        StatusMessage = "Discovering peers from all sources...";

        try
        {
            var progress = new Progress<DiscoveryProgress>(p =>
            {
                DiscoveryProgress = p.Percentage;
                StatusMessage = p.Message;
            });

            var results = await _peerDiscoveryService.DiscoverPeersFromAllSourcesAsync(false, progress);

            var successCount = results.Count(r => r.Value.Success);
            var totalPeers = results.Sum(r => r.Value.Peers.Count);

            StatusMessage = $"Discovery complete. Found {totalPeers} peers from {successCount} sources.";
            _logger.LogInfo($"Discovery complete: {totalPeers} peers from {successCount} sources");

            LoadData();
        }
        catch (Exception ex)
        {
            _logger.LogError("Discovery failed", ex);
            StatusMessage = $"Discovery failed: {ex.Message}";
            _dialogService.ShowError($"Discovery failed: {ex.Message}", "Error");
        }
        finally
        {
            IsDiscovering = false;
        }
    }

    private async Task CheckAllHealthAsync()
    {
        IsDiscovering = true;
        StatusMessage = "Checking health of all enabled peers...";

        try
        {
            var progress = new Progress<string>(msg =>
            {
                StatusMessage = msg;
            });

            var results = await _peerDiscoveryService.CheckAllPeersHealthAsync(progress);

            var healthyCount = results.Count(r => r.Value == PeerHealthStatus.Healthy);
            StatusMessage = $"Health check complete. {healthyCount}/{results.Count} peers healthy.";
            _logger.LogInfo($"Health check complete: {healthyCount}/{results.Count} peers healthy");

            LoadData();
        }
        catch (Exception ex)
        {
            _logger.LogError("Health check failed", ex);
            StatusMessage = $"Health check failed: {ex.Message}";
        }
        finally
        {
            IsDiscovering = false;
        }
    }

    #endregion
}
