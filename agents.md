# Agents in Software Architecture

## Overview
Agents in software architecture refer to autonomous entities that can perceive their environment and take actions to achieve specific goals. They are fundamental components in distributed systems, artificial intelligence, and complex software applications.

## Key Concepts

### Agent Characteristics
- **Autonomy**: Agents operate independently without direct human intervention
- **Reactivity**: Agents can sense and respond to changes in their environment
- **Proactivity**: Agents can initiate actions based on their goals and beliefs
- **Social Ability**: Agents can interact and communicate with other agents

### Types of Agents
1. **Simple Reflex Agents**: Base actions on current percept
2. **Model-Based Reflex Agents**: Use internal state to handle partial observability
3. **Goal-Based Agents**: Use goals to guide decision-making
4. **Utility-Based Agents**: Maximize a utility function
5. **Learning Agents**: Improve performance through experience

## Architecture Patterns

### Multi-Agent Systems (MAS)
- **Coordination**: How agents collaborate and communicate
- **Communication Protocols**: Standards for agent interaction
- **Decentralization**: No central control point
- **Scalability**: Ability to handle increasing numbers of agents

### Agent Communication
- **Message Passing**: Direct communication between agents
- **Shared Memory**: Agents access common data structures
- **Blackboard Systems**: Centralized problem-solving space
- **Market-Based**: Auction-like mechanisms for resource allocation

## Implementation Considerations

### Design Principles
- **Modularity**: Each agent should have well-defined responsibilities
- **Interoperability**: Agents should work together seamlessly
- **Flexibility**: Systems should adapt to changing requirements
- **Robustness**: Agents should handle failures gracefully

### Technologies
- **Agent-Oriented Programming Languages**: Languages specifically designed for agent development
- **Middleware**: Platforms that facilitate agent communication (e.g., JADE, AgentSpeak)
- **Frameworks**: Existing frameworks that support agent-based development

## Common Use Cases

### E-commerce
- Price comparison agents
- Recommendation systems
- Automated bidding agents

### IoT Systems
- Smart home automation
- Sensor network management
- Resource allocation

### Business Intelligence
- Data mining agents
- Market analysis agents
- Decision support systems

## Best Practices

1. **Define Clear Agent Boundaries**: Each agent should have a well-defined scope
2. **Implement Proper Communication**: Use appropriate protocols for agent interaction
3. **Design for Scalability**: Consider how the system will grow
4. **Plan for Failures**: Implement fault tolerance mechanisms
5. **Monitor Performance**: Track agent behavior and system efficiency

## Challenges

- **Complexity Management**: Multi-agent systems can become very complex
- **Coordination Overhead**: Communication between agents can introduce delays
- **Debugging Difficulty**: Hard to trace issues in distributed systems
- **Scalability Issues**: Performance may degrade with increasing agent count
- **Security Concerns**: Agents may be vulnerable to malicious interference

## Future Trends

- **AI Integration**: More sophisticated learning and decision-making capabilities
- **Blockchain**: Secure agent interactions using distributed ledgers
- **Edge Computing**: Agents operating closer to data sources
- **Quantum Computing**: New possibilities for agent optimization

---

# AdvGenPriceComparer Project - Agent Guidelines

## Project-Specific Information

### Build Commands
```powershell
# Build the WPF project
cd AdvGenPriceComparer.WPF
dotnet build

# Run the application
dotnet run

# Build the installer (WiX v4)
cd AdvGenPriceComparer.Installer
dotnet build -c Release -p:Platform=x64
# Output: bin/x64/Release/AdvGenPriceComparer.msi
```

### Project Structure
- **AdvGenPriceComparer.Core**: Core models and interfaces (Clean Architecture - Domain layer)
- **AdvGenPriceComparer.Application**: Application layer with use cases and DTOs (Clean Architecture)
- **AdvGenPriceComparer.Data.LiteDB**: LiteDB data access layer (Infrastructure layer)
- **AdvGenPriceComparer.ML**: ML.NET machine learning services (Infrastructure layer)
- **AdvGenPriceComparer.WPF**: WPF desktop application (Presentation layer)
- **AdvGenPriceComparer.Server**: ASP.NET Core Web API for P2P price sharing
- **AdvGenPriceComparer.Installer**: WiX v4 installer project (outputs MSI)

### Key Services Location
| Service | Location |
|---------|----------|
| JsonImportService | AdvGenPriceComparer.Application/Services/ |
| JsonExportService | AdvGenPriceComparer.Application/Services/ |
| ServerConfigService | AdvGenPriceComparer.Core/Services/ |
| ExportService | AdvGenPriceComparer.WPF/Services/ |
| SettingsService | AdvGenPriceComparer.WPF/Services/ |
| StaticDataExporter | AdvGenPriceComparer.WPF/Services/ |
| StaticDataImporter | AdvGenPriceComparer.WPF/Services/ |
| PeerDiscoveryService | AdvGenPriceComparer.WPF/Services/ |
| UpdateService | AdvGenPriceComparer.WPF/Services/ |
| NetworkManager | AdvGenPriceComparer.WPF/Services/ |
| IP2PNetworkService | AdvGenPriceComparer.Core/Interfaces/ |

### Clean Architecture Principles
The project follows Clean Architecture with these dependency rules:
1. **Core (Domain)**: Contains entities, interfaces, and domain logic. No external dependencies.
2. **Application**: Contains use cases, DTOs, and business logic. Depends only on Core.
3. **Infrastructure** (Data.LiteDB, ML, WPF/Services): Contains implementations. Depends on Core and Application.
4. **Presentation** (WPF): UI layer. Depends on all inner layers.

**Key Interfaces in Core:**
- `IP2PNetworkService`: P2P networking operations (implemented by NetworkManager in WPF)
- `IDatabaseProvider`: Database abstraction for LiteDB and AdvGenNoSqlServer
- `IGroceryDataService`: Main data service interface
- `ISettingsService`: Application settings management

### WPF Converters
XAML value converters are located in `AdvGenPriceComparer.WPF/Converters/`:
- **BooleanToVisibilityConverter**: Converts bool to Visibility
- **InverseBooleanConverter**: Inverts boolean values

Converters are registered in `App.xaml` as static resources.

### Dependency Injection
Services are registered in `AdvGenPriceComparer.WPF/App.xaml.cs` in the `ConfigureServices()` method.

### Task Coordination
- Use `multiagents.md` to track task assignments
- Update `plan.md` when completing tasks
- NEVER use `git push`, ONLY `git commit`

### Adding New Services
1. Create service class in appropriate project
2. Add to DI container in App.xaml.cs
3. Update multiagents.md with status
4. Update plan.md with progress

### Adding New Dialog Windows
To add a new dialog window to the application:

1. **Create ViewModel** (`AdvGenPriceComparer.WPF/ViewModels/YourDialogViewModel.cs`):
   - Inherit from `ViewModelBase` for INotifyPropertyChanged support
   - Accept required services via constructor injection
   - Implement `ICommand` properties for button actions
   - Raise `RequestClose` event when dialog should close

2. **Create View** (`AdvGenPriceComparer.WPF/Views/YourDialogWindow.xaml` and `.xaml.cs`):
   - XAML: Use `<Window>` root element (not FluentWindow)
   - Code-behind: Inherit from `Window`, accept ViewModel in constructor
   - Set `DataContext = viewModel` and subscribe to `RequestClose` event
   - Set `Owner = Application.Current.MainWindow` when showing dialog

3. **Add to IDialogService** (`AdvGenPriceComparer.WPF/Services/IDialogService.cs`):
   - Add method signature: `void ShowYourDialog();`

4. **Implement in SimpleDialogService** (`AdvGenPriceComparer.WPF/Services/SimpleDialogService.cs`):
   - Get services from DI: `((App)Application.Current).Services.GetRequiredService<YourService>()`
   - Create ViewModel and Window instances
   - Call `window.ShowDialog()`

5. **Register in DI** (`AdvGenPriceComparer.WPF/App.xaml.cs`):
   - Add ViewModel registration: `services.AddTransient<YourDialogViewModel>(provider => { ... })`
   - Add Window registration: `services.AddTransient<YourDialogWindow>(provider => { ... })`

6. **Add Menu Item** (`AdvGenPriceComparer.WPF/MainWindow.xaml` and `.xaml.cs`):
   - Add MenuItem to appropriate menu (File, Data, Tools, Help)
   - Create Click event handler in code-behind
   - Call `_dialogService.ShowYourDialog()`

7. **Update Tests** (if tests have mock IDialogService implementations):
   - Add stub method to TestDialogService classes in test files

### Testing
- **AdvGenPriceComparer.Tests**: xUnit test project with 217+ tests
  - Build: `dotnet build AdvGenPriceComparer.Tests/AdvGenPriceComparer.Tests.csproj`
  - Run: `dotnet test AdvGenPriceComparer.Tests/AdvGenPriceComparer.Tests.csproj`
  - Coverage: Uses coverlet with runsettings file
  - **Note**: When adding ASP.NET Core integration tests, reference `Microsoft.AspNetCore.Mvc.Testing` and `Microsoft.AspNetCore.SignalR.Client`

### Test Isolation Critical Note
- **`Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)` does NOT respect the `APPDATA` environment variable on .NET 5+**. This causes test isolation failures when trying to redirect settings to a temp directory.
- **Solution**: Use a helper method that checks the `APPDATA` environment variable first:
  ```csharp
  private static string GetAppDataPath()
  {
      var appDataEnv = Environment.GetEnvironmentVariable("APPDATA");
      if (!string.IsNullOrEmpty(appDataEnv))
          return appDataEnv;
      return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
  }
  ```
- **Example**: See `SettingsService.cs` (lines 243-261) for the implementation that fixed 53 failing tests.

### Known Issues
- **WPF Namespace Conflicts**: The `AdvGenPriceComparer.Application` project namespace conflicts with `System.Windows.Application` in WPF. When working with WPF files, use fully qualified type names like `System.Windows.Application` instead of just `Application`.
  
- **Core.Interfaces CultureInfo Conflict**: The `AdvGenPriceComparer.Core.Interfaces` namespace contains a custom `CultureInfo` class that conflicts with `System.Globalization.CultureInfo`. When using CultureInfo in WPF services that import `AdvGenPriceComparer.Core.Interfaces`, use fully qualified names like `System.Globalization.CultureInfo.DefaultThreadCurrentCulture` instead of `CultureInfo.DefaultThreadCurrentCulture`.
  
- **TestExportWorkflow**: CLI test project for export workflow testing
  - Location: `TestExportWorkflow/`
  - Build: `dotnet build TestExportWorkflow/TestExportWorkflow.csproj`
  - Run: `dotnet run --project TestExportWorkflow/TestExportWorkflow.csproj`
  - Tests: 10 comprehensive export tests covering empty DB, filters, compression, etc.

### Repository Pattern
- Repositories are synchronous (not async) - use direct method calls
- Create repositories by passing DatabaseService to constructor:
  ```csharp
  var itemRepo = new ItemRepository(dbService);
  var placeRepo = new PlaceRepository(dbService);
  var priceRepo = new PriceRecordRepository(dbService);
  ```
- Repository methods: `Add()`, `GetAll()`, `GetById()`, `Update()`, `Delete()`

### JSON Serialization
When using `System.Text.Json` for configuration files:
- For SERIALIZING (writing): Use `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`
- For DESERIALIZING (reading): Use `PropertyNameCaseInsensitive = true`
- Example from ServerConfigService:
  ```csharp
  // Writing
  var writeOptions = new JsonSerializerOptions
  {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };
  var json = JsonSerializer.Serialize(servers, writeOptions);
  
  // Reading
  var readOptions = new JsonSerializerOptions
  {
      PropertyNameCaseInsensitive = true
  };
  var servers = JsonSerializer.Deserialize<List<ServerInfo>>(json, readOptions);
  ```

### Static Data Import/Export (P2P Sharing)
- **StaticDataExporter**: Exports stores, products, prices to JSON for P2P sharing
  - Location: `AdvGenPriceComparer.WPF/Services/StaticDataExporter.cs`
  - Creates: stores.json, products.json, prices.json, manifest.json, discovery.json
  - Supports ZIP compression and checksum validation
  
- **StaticDataImporter**: Imports data from static packages
  - Location: `AdvGenPriceComparer.WPF/Services/StaticDataImporter.cs`
  - Import methods:
    - `ImportFromDirectoryAsync()` - Import extracted package
    - `ImportFromArchiveAsync()` - Import from ZIP file
    - `ImportFromUrlAsync()` - Download and import from URL
    - `PreviewPackageAsync()` - Preview contents before import
  - Duplicate handling strategies: Skip, Update, CreateNew
  - Validates checksums and maps external IDs to internal IDs

### Theme Service (Dark Mode)
- **IThemeService/ThemeService**: Runtime theme switching (Light/Dark/System)
  - Location: `AdvGenPriceComparer.WPF/Services/ThemeService.cs`
  - Enum: `AdvGenPriceComparer.Core.Models.ApplicationTheme` (Light, Dark, System)
  - Settings persistence via `ISettingsService.ApplicationTheme`
  - Automatically detects Windows system theme preference from registry
  - Applies theme immediately when changed in Settings UI
  - Registered in DI container in `App.xaml.cs`
  - Theme is applied on startup from saved settings

### Server Project (ASP.NET Core Web API)
- **Location**: `AdvGenPriceComparer.Server/`
- **Database**: SQLite with Entity Framework Core
- **EF Core Migrations**: To create or update database schema:
  ```powershell
  cd AdvGenPriceComparer.Server
  dotnet ef migrations add <MigrationName> --output-dir Data\Migrations
  dotnet ef database update  # Apply migrations to database
  ```
- **Key Components**:
  - **PriceDataContext**: EF Core DbContext with DbSets for Items, Places, PriceRecords, ApiKeys, UploadSessions
  - **ApiKeyService**: API key generation, validation, and management
  - **RateLimitService**: In-memory sliding window rate limiting
  - **ApiKeyMiddleware**: Validates X-API-Key header for protected endpoints
  - **RateLimitMiddleware**: Enforces rate limits per API key or IP address
- **API Documentation**: See `AdvGenPriceComparer.Server/API_PROTOCOL.md` for complete API specification including:
  - REST endpoints for Items, Places, and Prices
  - SignalR real-time update hub specification
  - Authentication (X-API-Key header)
  - Rate limiting details
  - Data models (SharedItem, SharedPlace, SharedPriceRecord)
  - C# client implementation examples
- **Build**:
  ```powershell
  cd AdvGenPriceComparer.Server
  dotnet build
  dotnet run  # Runs on https://localhost:5001 and http://localhost:5000
  ```

### Security Features
- **API Key Encryption**: SettingsService encrypts sensitive data using Windows DPAPI
  - Location: `AdvGenPriceComparer.WPF/Services/SettingsService.cs` (lines 646-707)
  - Encryption: Uses `System.Security.Cryptography.ProtectedData` with `DataProtectionScope.CurrentUser`
  - Methods: `EncryptString()` and `DecryptString()` for secure API key storage
  - Migration support: Gracefully handles plaintext-to-encrypted migration
  - Cross-platform: Falls back to plaintext on non-Windows platforms (for tests)

### Auto-Update Mechanism
- **UpdateService**: Checks for application updates from a remote JSON file
  - Location: `AdvGenPriceComparer.WPF/Services/UpdateService.cs`
  - Default URL: `https://raw.githubusercontent.com/advgen/pricecomparer/main/updates.json`
  - Configurable via `UpdateService.UpdateInfoUrl` property
  - **updates.json format** (see `updates.json` in project root):
    ```json
    {
      "latestVersion": "1.0.0",
      "downloadUrl": "https://github.com/advgen/pricecomparer/releases/download/v1.0.0/AdvGenPriceComparer.msi",
      "releaseNotes": "Description of changes...",
      "isMandatory": false,
      "fileSize": 124092808,
      "releaseDate": "2026-04-10T00:00:00Z",
      "fileHash": "sha256:hash_of_file"
    }
    ```
  - Update check runs automatically on startup (if enabled in settings)
  - Throttled to once per 24 hours via `last_update_check.txt` in AppData
  - Shows `UpdateNotificationWindow` when update is available

### Installer Build (WiX v4)
- **Installer Project**: `AdvGenPriceComparer.Installer/`
  - WiX v4 SDK-style project
  - Outputs MSI package (~124MB)
  - Creates Start Menu and Desktop shortcuts
  - Supports major upgrades
- **Build Commands**:
  ```powershell
  cd AdvGenPriceComparer.Installer
  dotnet build -c Release -p:Platform=x64
  # Output: bin/x64/Release/AdvGenPriceComparer.msi
  ```
- **Silent Installation**:
  ```powershell
  msiexec /i AdvGenPriceComparer.msi /qn
  # With desktop shortcut:
  msiexec /i AdvGenPriceComparer.msi INSTALLDESKTOPSHORTCUT=1 /qn
  ```
- **To create a new release**:
  1. Update version in `AdvGenPriceComparer.Installer/Package.wxs` (line 9)
  2. Build the installer (see command above)
  3. Calculate SHA256 hash of the MSI file
  4. Upload MSI to GitHub Releases
  5. Update `updates.json` with new version, URL, hash, and release notes
  6. Commit and push `updates.json` to trigger auto-update for users