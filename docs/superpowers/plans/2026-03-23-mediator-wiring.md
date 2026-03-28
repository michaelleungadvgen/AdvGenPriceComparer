# Mediator Wiring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Wire the WPF project to route data operations through `IMediator` (AdvGenFlow) instead of calling `IGroceryDataService` and repositories directly.

**Architecture:** The Application layer already defines Commands, Queries, and Handlers using AdvGenFlow. The WPF `.csproj` must reference `AdvGenPriceComparer.Application`, `AddApplicationServices()` must be called in DI setup, and ViewModels must inject `IMediator` and call `await _mediator.Send(new SomeQuery())` instead of calling repositories directly.

**Tech Stack:** C# 12, .NET 9, WPF, AdvGenFlow 1.0.1 (`IMediator`, `IRequest<T>`, `IRequestHandler<TReq,TResp>`), Microsoft.Extensions.DependencyInjection

---

## Background Context

### What already happened (git status context)
- `AdvGenPriceComparer.Application/Mediator/IMediator.cs` — **deleted** (was a custom local mediator)
- `AdvGenPriceComparer.Application/Mediator/Mediator.cs` — **deleted** (was a custom local mediator)
- `ServiceRegistration.cs` was updated to use `AdvGenFlow.IMediator` and `AdvGenFlow.Mediator` from NuGet instead
- All Commands/Queries/Handlers already import `using AdvGenFlow;` — this is correct

When you see these deletions in `git status`, that is expected and correct. Do not restore them.

### Key facts about App.xaml.cs
- `CategoryViewModel` is **NOT registered in DI** — it is newed up directly at the call site (`MainWindow.xaml.cs`)
- `ItemViewModel`, `PlaceViewModel`, `MainWindowViewModel` are registered with **implicit constructor injection** (`services.AddTransient<ItemViewModel>()`) — after adding `IMediator`, DI will resolve it automatically with no lambda needed
- `AddStoreViewModel`, `AddItemViewModel` may be registered with explicit factory lambdas — check before editing
- `services.AddLogging()` is **NOT called** — must be added or the new handlers' `ILogger<T>` injection will throw

### AdvGenFlow mediator Send signature
```csharp
Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
```

---

## Scope

### What has mediator coverage (migrate these):
- Items: `GetAllItemsQuery`, `GetItemByIdQuery`, `GetItemsByCategoryQuery`, `SearchItemsQuery`, `CreateItemCommand`, `UpdateItemCommand`, `DeleteItemCommand`
- Places: `GetAllPlacesQuery`, `GetPlaceByIdQuery`, `CreatePlaceCommand` *(no Delete/Update yet — add them)*
- Prices: `RecordPriceCommand`, `GetPriceHistoryQuery`, `GetRecentPriceUpdatesQuery` *(no Delete yet — add it)*
- Analytics: `GetDashboardStatsQuery`, `GetCategoryStatsQuery`, `GetStoreComparisonStatsQuery`, `FindBestDealsQuery`

### `DashboardStatsResult` fields (read before Task 8)
`GetDashboardStatsQuery` returns `DashboardStatsResult` with: `TotalItems`, `TotalPlaces`, `TotalPriceRecords`, `ActiveDeals`, `AverageSavings`, `ItemsByCategory`, `AveragePriceByCategory`. It does NOT include `RecentPriceUpdates` count or raw chart series data. `MainWindowViewModel` needs multiple queries, not just this one — see Task 8.

### ViewModels to migrate (in order):
1. `CategoryViewModel` — `GetAllItemsQuery`, `GetItemsByCategoryQuery` (instantiated directly, not via DI)
2. `ItemViewModel` — `GetAllItemsQuery`, `DeleteItemCommand`
3. `PlaceViewModel` — `GetAllPlacesQuery`, `DeletePlaceCommand` (new); also newing up `AddStoreViewModel` — update that call site too
4. `AddItemViewModel` — `CreateItemCommand`, `UpdateItemCommand`, `GetItemByIdQuery`, `GetAllPlacesQuery`, `RecordPriceCommand`, `GetPlaceByIdQuery`, `GetPriceHistoryQuery`
5. `AddStoreViewModel` — `CreatePlaceCommand`, `UpdatePlaceCommand` (new)
6. `PriceComparisonViewModel` — `GetStoreComparisonStatsQuery` (note: result uses PascalCase `StoreName`/`AveragePrice`/`ProductCount`, not old tuple fields)
7. `MainWindowViewModel` — `GetAllItemsQuery`, `GetAllPlacesQuery`, `GetRecentPriceUpdatesQuery`, `GetCategoryStatsQuery`, `GetPriceHistoryQuery`; also update internal child ViewModel instantiations

### Out of scope (use domain-specific services, no handler coverage):
- `ShoppingListViewModel`, `TripOptimizerViewModel`, `AlertViewModel`, `PriceAlertViewModel`, `FavoritesViewModel`, `WeeklySpecialsViewModel`, `CloudSyncViewModel`, `GlobalSearchViewModel`, etc.

---

## File Map

| Action | File |
|--------|------|
| Modify | `AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj` — add Application project reference |
| Modify | `AdvGenPriceComparer.WPF/App.xaml.cs` — add `AddLogging()`, call `AddApplicationServices()`, update ViewModel registrations |
| Modify | `AdvGenPriceComparer.WPF/MainWindow.xaml.cs` — update `CategoriesNav_Click` (already partially fixed) |
| Create | `AdvGenPriceComparer.Application/Commands/DeletePlaceCommand.cs` |
| Create | `AdvGenPriceComparer.Application/Commands/UpdatePlaceCommand.cs` |
| Create | `AdvGenPriceComparer.Application/Commands/DeletePriceRecordCommand.cs` |
| Create | `AdvGenPriceComparer.Application/Handlers/PlaceDeleteUpdateCommandHandlers.cs` |
| Create | `AdvGenPriceComparer.Application/Handlers/PriceRecordDeleteCommandHandler.cs` |
| Modify | `AdvGenPriceComparer.WPF/ViewModels/CategoryViewModel.cs` |
| Modify | `AdvGenPriceComparer.WPF/ViewModels/ItemViewModel.cs` |
| Modify | `AdvGenPriceComparer.WPF/ViewModels/PlaceViewModel.cs` |
| Modify | `AdvGenPriceComparer.WPF/ViewModels/AddItemViewModel.cs` |
| Modify | `AdvGenPriceComparer.WPF/ViewModels/AddStoreViewModel.cs` |
| Modify | `AdvGenPriceComparer.WPF/ViewModels/PriceComparisonViewModel.cs` |
| Modify | `AdvGenPriceComparer.WPF/ViewModels/MainWindowViewModel.cs` |

---

## Task 1: Add Application Project Reference and Wire DI

**Files:**
- Modify: `AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj`
- Modify: `AdvGenPriceComparer.WPF/App.xaml.cs`

- [ ] **Step 1: Add project reference**

In `AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj`, add inside the existing `<ItemGroup>` with ProjectReferences:
```xml
<ProjectReference Include="..\AdvGenPriceComparer.Application\AdvGenPriceComparer.Application.csproj" />
```

- [ ] **Step 2: Register the mediator and logging in DI**

In `AdvGenPriceComparer.WPF/App.xaml.cs`, find `ConfigureServices()`. Add these two lines after the existing repository registrations (logging must come before Application services since handlers depend on `ILogger<T>`):

```csharp
// Required: handlers inject ILogger<T>
services.AddLogging();

// Register Application layer mediator and all handlers (via reflection)
services.AddApplicationServices();
```

Add usings at the top of App.xaml.cs:
```csharp
using AdvGenFlow;
using AdvGenPriceComparer.Application;
```

- [ ] **Step 3: Build to verify no compilation errors**
```
dotnet build AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj
```
Expected: Build succeeds. The `IMediator` type resolves from `AdvGenFlow` via the Application project reference.

- [ ] **Step 4: Commit**
```bash
git add AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj AdvGenPriceComparer.WPF/App.xaml.cs
git commit -m "feat: add Application project reference, IMediator, and AddLogging in WPF DI"
```

---

## Task 2: Add Missing Commands (DeletePlace, UpdatePlace, DeletePriceRecord)

**Files:**
- Create: `AdvGenPriceComparer.Application/Commands/DeletePlaceCommand.cs`
- Create: `AdvGenPriceComparer.Application/Commands/UpdatePlaceCommand.cs`
- Create: `AdvGenPriceComparer.Application/Commands/DeletePriceRecordCommand.cs`
- Create: `AdvGenPriceComparer.Application/Handlers/PlaceDeleteUpdateCommandHandlers.cs`
- Create: `AdvGenPriceComparer.Application/Handlers/PriceRecordDeleteCommandHandler.cs`

Note: `ServiceRegistration.cs` auto-registers all `IRequestHandler<,>` implementations via reflection. No manual registration needed for new handlers.

- [ ] **Step 1: Create DeletePlaceCommand**

`AdvGenPriceComparer.Application/Commands/DeletePlaceCommand.cs`:
```csharp
using AdvGenFlow;

namespace AdvGenPriceComparer.Application.Commands;

public record DeletePlaceCommand(string PlaceId) : IRequest<DeletePlaceResult>;

public record DeletePlaceResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static DeletePlaceResult SuccessResult() => new() { Success = true };
    public static DeletePlaceResult NotFound(string placeId) =>
        new() { Success = false, ErrorMessage = $"Place not found: {placeId}" };
    public static DeletePlaceResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
```

- [ ] **Step 2: Create UpdatePlaceCommand**

`AdvGenPriceComparer.Application/Commands/UpdatePlaceCommand.cs`:
```csharp
using AdvGenFlow;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Application.Commands;

public record UpdatePlaceCommand(
    string PlaceId,
    string? Name = null,
    string? Chain = null,
    string? Address = null,
    string? Suburb = null,
    string? State = null,
    string? Postcode = null,
    string? Phone = null
) : IRequest<UpdatePlaceResult>;

public record UpdatePlaceResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Place? Place { get; init; }

    public static UpdatePlaceResult SuccessResult(Place place) => new() { Success = true, Place = place };
    public static UpdatePlaceResult NotFound(string placeId) =>
        new() { Success = false, ErrorMessage = $"Place not found: {placeId}" };
    public static UpdatePlaceResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
```

- [ ] **Step 3: Create DeletePriceRecordCommand**

`AdvGenPriceComparer.Application/Commands/DeletePriceRecordCommand.cs`:
```csharp
using AdvGenFlow;

namespace AdvGenPriceComparer.Application.Commands;

public record DeletePriceRecordCommand(string PriceRecordId) : IRequest<DeletePriceRecordResult>;

public record DeletePriceRecordResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static DeletePriceRecordResult SuccessResult() => new() { Success = true };
    public static DeletePriceRecordResult NotFound(string id) =>
        new() { Success = false, ErrorMessage = $"Price record not found: {id}" };
    public static DeletePriceRecordResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
```

- [ ] **Step 4: Create PlaceDeleteUpdateCommandHandlers**

`AdvGenPriceComparer.Application/Handlers/PlaceDeleteUpdateCommandHandlers.cs`:
```csharp
using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

public class DeletePlaceCommandHandler : IRequestHandler<DeletePlaceCommand, DeletePlaceResult>
{
    private readonly IPlaceRepository _placeRepository;
    private readonly ILogger<DeletePlaceCommandHandler> _logger;

    public DeletePlaceCommandHandler(IPlaceRepository placeRepository, ILogger<DeletePlaceCommandHandler> logger)
    {
        _placeRepository = placeRepository;
        _logger = logger;
    }

    public Task<DeletePlaceResult> Handle(DeletePlaceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var place = _placeRepository.GetById(request.PlaceId);
            if (place == null)
                return Task.FromResult(DeletePlaceResult.NotFound(request.PlaceId));

            _placeRepository.Delete(request.PlaceId);
            _logger.LogInformation("Deleted place with ID: {PlaceId}", request.PlaceId);
            return Task.FromResult(DeletePlaceResult.SuccessResult());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting place: {PlaceId}", request.PlaceId);
            return Task.FromResult(DeletePlaceResult.Failure($"Failed to delete place: {ex.Message}"));
        }
    }
}

public class UpdatePlaceCommandHandler : IRequestHandler<UpdatePlaceCommand, UpdatePlaceResult>
{
    private readonly IPlaceRepository _placeRepository;
    private readonly ILogger<UpdatePlaceCommandHandler> _logger;

    public UpdatePlaceCommandHandler(IPlaceRepository placeRepository, ILogger<UpdatePlaceCommandHandler> logger)
    {
        _placeRepository = placeRepository;
        _logger = logger;
    }

    public Task<UpdatePlaceResult> Handle(UpdatePlaceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var place = _placeRepository.GetById(request.PlaceId);
            if (place == null)
                return Task.FromResult(UpdatePlaceResult.NotFound(request.PlaceId));

            if (request.Name != null) place.Name = request.Name.Trim();
            if (request.Chain != null) place.Chain = request.Chain.Trim();
            if (request.Address != null) place.Address = request.Address.Trim();
            if (request.Suburb != null) place.Suburb = request.Suburb.Trim();
            if (request.State != null) place.State = request.State.Trim();
            if (request.Postcode != null) place.Postcode = request.Postcode.Trim();
            if (request.Phone != null) place.Phone = request.Phone.Trim();

            _placeRepository.Update(place);
            _logger.LogInformation("Updated place: {PlaceName} with ID: {PlaceId}", place.Name, place.Id);
            return Task.FromResult(UpdatePlaceResult.SuccessResult(place));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating place: {PlaceId}", request.PlaceId);
            return Task.FromResult(UpdatePlaceResult.Failure($"Failed to update place: {ex.Message}"));
        }
    }
}
```

- [ ] **Step 5: Create PriceRecordDeleteCommandHandler**

`AdvGenPriceComparer.Application/Handlers/PriceRecordDeleteCommandHandler.cs`:
```csharp
using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

public class DeletePriceRecordCommandHandler : IRequestHandler<DeletePriceRecordCommand, DeletePriceRecordResult>
{
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<DeletePriceRecordCommandHandler> _logger;

    public DeletePriceRecordCommandHandler(
        IPriceRecordRepository priceRecordRepository,
        ILogger<DeletePriceRecordCommandHandler> logger)
    {
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<DeletePriceRecordResult> Handle(DeletePriceRecordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var record = _priceRecordRepository.GetById(request.PriceRecordId);
            if (record == null)
                return Task.FromResult(DeletePriceRecordResult.NotFound(request.PriceRecordId));

            _priceRecordRepository.Delete(request.PriceRecordId);
            _logger.LogInformation("Deleted price record with ID: {PriceRecordId}", request.PriceRecordId);
            return Task.FromResult(DeletePriceRecordResult.SuccessResult());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting price record: {PriceRecordId}", request.PriceRecordId);
            return Task.FromResult(DeletePriceRecordResult.Failure($"Failed to delete price record: {ex.Message}"));
        }
    }
}
```

- [ ] **Step 6: Build Application layer**
```
dotnet build AdvGenPriceComparer.Application/AdvGenPriceComparer.Application.csproj
```
Expected: Build succeeds. The 3 new handlers auto-register via the existing reflection loop in `ServiceRegistration.cs`.

- [ ] **Step 7: Commit**
```bash
git add AdvGenPriceComparer.Application/Commands/ AdvGenPriceComparer.Application/Handlers/
git commit -m "feat: add DeletePlace, UpdatePlace, DeletePriceRecord commands and handlers"
```

---

## Task 3: Migrate CategoryViewModel

**Files:**
- Modify: `AdvGenPriceComparer.WPF/ViewModels/CategoryViewModel.cs`
- Modify: `AdvGenPriceComparer.WPF/MainWindow.xaml.cs` (update call site — CategoryViewModel is NOT in DI)

**IMPORTANT:** `CategoryViewModel` is **not registered in `App.xaml.cs`**. It is instantiated directly in `MainWindow.xaml.cs` `CategoriesNav_Click`. The previous fix to that method already removed the `GroceryDataService` cast — now update it to pass `IMediator` instead of `IGroceryDataService`.

**Current state:** Uses `IGroceryDataService` → `Items.GetAll()`.
**Target:** Uses `IMediator` → `Send(new GetAllItemsQuery())`.

`CategoryViewModel` calls `LoadCategories()` from its constructor (synchronous). Since `IMediator.Send` returns `Task<T>`, use `.GetAwaiter().GetResult()` — this is acceptable here because we are in a synchronous constructor on the UI thread before the window is shown.

- [ ] **Step 1: Read CategoryViewModel and MainWindow.xaml.cs CategoriesNav_Click**

Read both files completely before editing.

- [ ] **Step 2: Update CategoryViewModel**

Replace usings:
```csharp
// Remove:
using AdvGenPriceComparer.Core.Interfaces;
// Add:
using AdvGenFlow;
using AdvGenPriceComparer.Application.Queries;
```

Replace field and constructor:
```csharp
private readonly IMediator _mediator;

public CategoryViewModel(IMediator mediator, ILoggerService logger)
{
    _mediator = mediator;
    // ... rest unchanged
}
```

Replace `LoadCategories` body:
```csharp
private void LoadCategories()
{
    try
    {
        _logger.LogInfo("Loading categories...");
        Categories.Clear();

        var allItems = _mediator.Send(new GetAllItemsQuery()).GetAwaiter().GetResult();
        var uniqueCategories = allItems
            .Select(i => i.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        foreach (var category in uniqueCategories)
            Categories.Add(category);

        _logger.LogInfo($"Loaded {Categories.Count} categories");
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading categories: {ex.Message}", ex);
    }
}
```

Replace `UpdateItemCount` body:
```csharp
private void UpdateItemCount()
{
    if (string.IsNullOrWhiteSpace(SelectedCategory)) { ItemCount = 0; return; }
    try
    {
        var items = _mediator.Send(new GetItemsByCategoryQuery(SelectedCategory)).GetAwaiter().GetResult();
        ItemCount = items.Count();
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error updating item count: {ex.Message}", ex);
        ItemCount = 0;
    }
}
```

Replace item check in `DeleteCategory`:
```csharp
var itemsWithCategory = _mediator.Send(new GetItemsByCategoryQuery(SelectedCategory))
    .GetAwaiter().GetResult().ToList();
```

- [ ] **Step 3: Update CategoriesNav_Click in MainWindow.xaml.cs**

Find `CategoriesNav_Click`. It currently gets `IGroceryDataService` and passes it. Change to get `IMediator`:
```csharp
private void CategoriesNav_Click(object sender, RoutedEventArgs e)
{
    try
    {
        var mediator = ((App)System.Windows.Application.Current).Services.GetRequiredService<IMediator>();
        var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILoggerService>();

        var viewModel = new CategoryViewModel(mediator, logger);
        var page = new CategoryPage(viewModel);

        DashboardContent.Visibility = Visibility.Collapsed;
        ContentFrame.Visibility = Visibility.Visible;
        ContentFrame.Navigate(page);
        UpdateNavigation("Categories");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error navigating to Categories: {ex.Message}",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

Add `using AdvGenFlow;` to MainWindow.xaml.cs if not already present.

- [ ] **Step 4: Build**
```
dotnet build AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj
```

- [ ] **Step 5: Commit**
```bash
git add AdvGenPriceComparer.WPF/ViewModels/CategoryViewModel.cs AdvGenPriceComparer.WPF/MainWindow.xaml.cs
git commit -m "feat: migrate CategoryViewModel to IMediator"
```

---

## Task 4: Migrate ItemViewModel

**Files:**
- Modify: `AdvGenPriceComparer.WPF/ViewModels/ItemViewModel.cs`
- Modify: `AdvGenPriceComparer.WPF/App.xaml.cs` (if registered with explicit factory; if registered as `services.AddTransient<ItemViewModel>()` — just change constructor signature and DI resolves it automatically)

**Operations:** `GetAllItemsQuery`, `DeleteItemCommand`

- [ ] **Step 1: Read ItemViewModel and its DI registration in App.xaml.cs**

Read `AdvGenPriceComparer.WPF/ViewModels/ItemViewModel.cs` and search App.xaml.cs for `ItemViewModel` to understand the registration style.

- [ ] **Step 2: Replace service/repository with IMediator**

Add usings:
```csharp
using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Application.Queries;
```

Replace injected `IGroceryDataService` / `IItemRepository` field with `IMediator _mediator`.

Replace calls:
- `_dataService.GetAllItems()` or `_dataService.Items.GetAll()` → `await _mediator.Send(new GetAllItemsQuery())` (or `.GetAwaiter().GetResult()` if the method is synchronous)
- `_dataService.Items.Delete(itemId)` → check result of `await _mediator.Send(new DeleteItemCommand(itemId))`

- [ ] **Step 3: Update DI registration in App.xaml.cs for ItemViewModel**

- If registered as `services.AddTransient<ItemViewModel>()` — just update the constructor; DI injects `IMediator` automatically. No lambda change needed.
- If registered with an explicit factory `provider => new ItemViewModel(provider.GetRequiredService<...>())` — update the factory to pass `provider.GetRequiredService<IMediator>()`.

- [ ] **Step 4: Build**
```
dotnet build AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj
```

- [ ] **Step 5: Commit**
```bash
git add AdvGenPriceComparer.WPF/ViewModels/ItemViewModel.cs AdvGenPriceComparer.WPF/App.xaml.cs
git commit -m "feat: migrate ItemViewModel to IMediator"
```

---

## Task 5: Migrate PlaceViewModel (+ fix AddStoreViewModel call site)

**Files:**
- Modify: `AdvGenPriceComparer.WPF/ViewModels/PlaceViewModel.cs`
- Modify: `AdvGenPriceComparer.WPF/App.xaml.cs`

**Operations:** `GetAllPlacesQuery`, `DeletePlaceCommand`

**IMPORTANT:** `PlaceViewModel.AddPlace()` likely newing up `AddStoreViewModel` directly:
```csharp
var viewModel = new AddStoreViewModel(_dataService, _dialogService);
```
After this task, `AddStoreViewModel` still has the old constructor (Task 7 changes it). So for now, if `PlaceViewModel` instantiates `AddStoreViewModel`, **leave that call site unchanged** — it will be fixed in Task 7. Just replace the data access calls in `PlaceViewModel` itself.

- [ ] **Step 1: Read PlaceViewModel completely**
- [ ] **Step 2: Replace with IMediator calls**

Add usings:
```csharp
using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Application.Queries;
```

Replace:
- `_dataService.GetAllPlaces()` → `await _mediator.Send(new GetAllPlacesQuery())` (or `.GetAwaiter().GetResult()`)
- `_dataService.Places.Delete(placeId)` → check result of `await _mediator.Send(new DeletePlaceCommand(placeId))`

- [ ] **Step 3: Update DI registration**

Same pattern as Task 4: check whether implicit or explicit factory, update accordingly.

- [ ] **Step 4: Build**
- [ ] **Step 5: Commit**
```bash
git commit -m "feat: migrate PlaceViewModel to IMediator"
```

---

## Task 6: Migrate AddItemViewModel

**Files:**
- Modify: `AdvGenPriceComparer.WPF/ViewModels/AddItemViewModel.cs`
- Modify: `AdvGenPriceComparer.WPF/App.xaml.cs`

**Operations:** `CreateItemCommand`, `UpdateItemCommand`, `GetItemByIdQuery`, `GetAllPlacesQuery`, `GetPlaceByIdQuery`, `RecordPriceCommand`, `GetPriceHistoryQuery`

**IMPORTANT — field coverage gap:** `CreateItemCommand` accepts: `Name, Brand, Category, Barcode, PackageSize, Unit, Description`. If `AddItemViewModel` sets extra fields like `SubCategory`, `Tags`, `Allergens`, `DietaryFlags`, or `ImageUrl` after calling `AddGroceryItem`, you must follow `CreateItemCommand` with `UpdateItemCommand` carrying those extra fields, OR expand `CreateItemCommand` to include them. Read the ViewModel first to determine which fields are set and choose the simpler approach.

- [ ] **Step 1: Read AddItemViewModel completely**

Pay special attention to:
- What fields are set on the item after creation (`SaveItem` method)
- `LoadPriceRecords()` — this likely uses `PriceRecords.GetByItem(ItemId)` which maps to `GetPriceHistoryQuery(itemId: ItemId)`

- [ ] **Step 2: Determine field coverage**

If extra fields beyond `CreateItemCommand`'s scope are set:
- Option A (preferred if small number of extra fields): Follow `CreateItemCommand` with an `UpdateItemCommand` that sets the extra fields
- Option B (if many extra fields): Add the missing fields to `CreateItemCommand` and its handler

- [ ] **Step 3: Replace with IMediator calls**

Add usings:
```csharp
using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Application.Queries;
```

Replace:
- `_dataService.AddGroceryItem(Name)` → `await _mediator.Send(new CreateItemCommand(Name, Brand, Category, Barcode, PackageSize, Unit, Description))`
- Follow with `UpdateItemCommand` if extra fields needed (see Step 2)
- `_dataService.GetItemById(id)` → `await _mediator.Send(new GetItemByIdQuery(id))`
- `_dataService.Items.Update(item)` → `await _mediator.Send(new UpdateItemCommand(item.Id, item.Name, item.Brand, ...))`
- `_dataService.GetAllPlaces()` → `await _mediator.Send(new GetAllPlacesQuery())`
- `_dataService.GetPlaceById(id)` → `await _mediator.Send(new GetPlaceByIdQuery(id))`
- `_dataService.RecordPrice(itemId, placeId, price, isOnSale, originalPrice, saleDesc, validFrom, validTo, source)` → `await _mediator.Send(new RecordPriceCommand(itemId, placeId, price, isOnSale, originalPrice, saleDesc, validFrom, validTo, source))`
- `_dataService.PriceRecords.GetByItem(ItemId)` → `await _mediator.Send(new GetPriceHistoryQuery(ItemId: ItemId))`

- [ ] **Step 4: Update DI registration in App.xaml.cs**
- [ ] **Step 5: Build**
- [ ] **Step 6: Commit**
```bash
git commit -m "feat: migrate AddItemViewModel to IMediator"
```

---

## Task 7: Migrate AddStoreViewModel (+ fix PlaceViewModel call site)

**Files:**
- Modify: `AdvGenPriceComparer.WPF/ViewModels/AddStoreViewModel.cs`
- Modify: `AdvGenPriceComparer.WPF/ViewModels/PlaceViewModel.cs` (update AddStoreViewModel instantiation)
- Modify: `AdvGenPriceComparer.WPF/App.xaml.cs`

**Operations:** `CreatePlaceCommand`, `UpdatePlaceCommand`

- [ ] **Step 1: Read AddStoreViewModel completely**
- [ ] **Step 2: Replace with IMediator calls**

Replace:
- Create → `await _mediator.Send(new CreatePlaceCommand(name, chain, address, suburb, state, postcode, phone))`
- Update → `await _mediator.Send(new UpdatePlaceCommand(placeId, name, chain, address, suburb, state, postcode, phone))`

- [ ] **Step 3: Fix AddStoreViewModel instantiation in PlaceViewModel**

Find the line in `PlaceViewModel` that does:
```csharp
var viewModel = new AddStoreViewModel(_dataService, _dialogService);
```

Replace with:
```csharp
var viewModel = new AddStoreViewModel(_mediator, _dialogService);
```

(`PlaceViewModel` already has `_mediator` from Task 5.)

- [ ] **Step 4: Update DI registration in App.xaml.cs**
- [ ] **Step 5: Build**
- [ ] **Step 6: Commit**
```bash
git commit -m "feat: migrate AddStoreViewModel to IMediator, fix PlaceViewModel call site"
```

---

## Task 8: Migrate PriceComparisonViewModel

**Files:**
- Modify: `AdvGenPriceComparer.WPF/ViewModels/PriceComparisonViewModel.cs`
- Modify: `AdvGenPriceComparer.WPF/App.xaml.cs`

**Operations:** `GetStoreComparisonStatsQuery`

**IMPORTANT — type change:** `GetStoreComparisonStatsQuery` returns `IEnumerable<StoreComparisonStats>` where the record has **PascalCase** properties: `StoreName`, `AveragePrice`, `ProductCount`, `DealCount`. The old `IGroceryDataService.GetStoreComparisonStats()` returned a tuple with lowercase fields (`storeName`, `avgPrice`, `productCount`). Update all projections.

- [ ] **Step 1: Read PriceComparisonViewModel**
- [ ] **Step 2: Replace service call and update property access**

Replace:
```csharp
// Old (tuple):
var stats = _dataService.GetStoreComparisonStats();
stat.storeName, stat.avgPrice, stat.productCount

// New (record):
var stats = await _mediator.Send(new GetStoreComparisonStatsQuery());
stat.StoreName, stat.AveragePrice, stat.ProductCount
```

- [ ] **Step 3: Update DI registration**
- [ ] **Step 4: Build**
- [ ] **Step 5: Commit**
```bash
git commit -m "feat: migrate PriceComparisonViewModel to IMediator"
```

---

## Task 9: Migrate MainWindowViewModel

**Files:**
- Modify: `AdvGenPriceComparer.WPF/ViewModels/MainWindowViewModel.cs`
- Modify: `AdvGenPriceComparer.WPF/App.xaml.cs`

**IMPORTANT:** `MainWindowViewModel` uses multiple data service methods — not just `GetDashboardStatsQuery`. It also instantiates child ViewModels directly. Read the file completely before editing.

**Operations:**
- `GetAllItemsQuery` — for item count
- `GetAllPlacesQuery` — for place count
- `GetRecentPriceUpdatesQuery` — for recent updates count (note: `GetDashboardStatsQuery.TotalPriceRecords` is the total count of ALL records, not recent — keep using `GetRecentPriceUpdatesQuery` for recent)
- `GetCategoryStatsQuery` — for chart data
- `GetPriceHistoryQuery` — for price history chart series

**Child ViewModel instantiations to fix:** Anywhere `MainWindowViewModel` does `new AddItemViewModel(_dataService, ...)` or `new AddStoreViewModel(_dataService, ...)`, replace `_dataService` with `_mediator`.

- [ ] **Step 1: Read MainWindowViewModel completely**

Note every use of `_dataService` and every `new XViewModel(_dataService, ...)` call.

- [ ] **Step 2: Replace data service calls with mediator sends**

Add usings:
```csharp
using AdvGenFlow;
using AdvGenPriceComparer.Application.Queries;
```

Replace each data access:
- Item count → `(await _mediator.Send(new GetAllItemsQuery())).Count()`
- Place count → `(await _mediator.Send(new GetAllPlacesQuery())).Count()`
- Recent updates → `await _mediator.Send(new GetRecentPriceUpdatesQuery(count))`
- Category stats → `await _mediator.Send(new GetCategoryStatsQuery())` (returns `IEnumerable<CategoryStats>` with `Category`, `AveragePrice`, `ItemCount`, `MinPrice`, `MaxPrice`)
- Price history → `await _mediator.Send(new GetPriceHistoryQuery(null, null, from, to))`

- [ ] **Step 3: Update child ViewModel instantiations**

Replace:
```csharp
new AddItemViewModel(_dataService, _dialogService, _categoryPredictionService)
// →
new AddItemViewModel(_mediator, _dialogService, _categoryPredictionService)

new AddStoreViewModel(_dataService, _dialogService)
// →
new AddStoreViewModel(_mediator, _dialogService)
```

- [ ] **Step 4: Update DI registration in App.xaml.cs**

`MainWindowViewModel` is registered as `services.AddTransient<MainWindowViewModel>()` (implicit). Just updating the constructor is sufficient; DI will inject `IMediator` automatically.

- [ ] **Step 5: Build**
```
dotnet build AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj
```
Expected: Zero errors.

- [ ] **Step 6: Commit**
```bash
git add AdvGenPriceComparer.WPF/ViewModels/MainWindowViewModel.cs AdvGenPriceComparer.WPF/App.xaml.cs
git commit -m "feat: migrate MainWindowViewModel to IMediator"
```

---

## Task 10: Remove Unused IGroceryDataService Injections

After migrating all ViewModels above, check for leftover `IGroceryDataService` in migrated files.

- [ ] **Step 1: Search**
```bash
grep -n "IGroceryDataService" \
  AdvGenPriceComparer.WPF/ViewModels/CategoryViewModel.cs \
  AdvGenPriceComparer.WPF/ViewModels/ItemViewModel.cs \
  AdvGenPriceComparer.WPF/ViewModels/PlaceViewModel.cs \
  AdvGenPriceComparer.WPF/ViewModels/AddItemViewModel.cs \
  AdvGenPriceComparer.WPF/ViewModels/AddStoreViewModel.cs \
  AdvGenPriceComparer.WPF/ViewModels/PriceComparisonViewModel.cs \
  AdvGenPriceComparer.WPF/ViewModels/MainWindowViewModel.cs \
  AdvGenPriceComparer.WPF/MainWindow.xaml.cs
```

- [ ] **Step 2: Remove any leftover injections and unused usings**
- [ ] **Step 3: Build**
- [ ] **Step 4: Commit**
```bash
git commit -m "chore: remove unused IGroceryDataService from mediator-migrated ViewModels"
```

---

## Verification

- [ ] `dotnet build AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj` — zero errors
- [ ] Run app in Visual Studio (x64), navigate to: Categories, Items, Stores, Add Item, Add Store, Price Comparison, Dashboard
- [ ] No runtime errors — mediator correctly resolves and dispatches all handlers
- [ ] `IGroceryDataService` no longer injected into any migrated ViewModel
