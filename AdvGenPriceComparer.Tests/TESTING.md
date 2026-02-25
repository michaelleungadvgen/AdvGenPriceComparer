# AdvGenPriceComparer Testing Strategy

**Last Updated:** 2026-02-26  
**Test Count:** 217+ tests  
**Coverage:** 27.67% line coverage (and growing)

---

## Table of Contents

1. [Overview](#overview)
2. [Test Architecture](#test-architecture)
3. [Test Categories](#test-categories)
4. [Running Tests](#running-tests)
5. [Test Data Management](#test-data-management)
6. [CI/CD Integration](#cicd-integration)
7. [Code Coverage](#code-coverage)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

---

## Overview

The AdvGenPriceComparer test suite is built on **xUnit** with supporting libraries for mocking and assertions. The goal is to ensure reliable import/export functionality, data integrity, and UI behavior across the WPF application.

### Testing Philosophy

- **Unit Tests:** Test individual components in isolation
- **Integration Tests:** Test component interactions and workflows
- **Repository Tests:** Test database operations with in-memory databases
- **ViewModel Tests:** Test UI logic without requiring the actual UI

### Test Statistics

| Category | Count | Status |
|----------|-------|--------|
| Service Tests | 54+ | ✅ Passing |
| Repository Tests | 98 | ✅ Passing |
| ViewModel Tests | 44 | ✅ Passing |
| Integration Tests | 7+ | ✅ Passing |
| **Total** | **217+** | ✅ **All Passing** |

---

## Test Architecture

### Project Structure

```
AdvGenPriceComparer.Tests/
├── Services/
│   ├── JsonImportServiceTests.cs      # 24 tests - JSON import, parsing, errors
│   ├── ServerConfigServiceTests.cs    # 30 tests - config management
│   └── DuplicateDetectionTests.cs     # Tests for duplicate handling
├── Repositories/
│   ├── ItemRepositoryTests.cs         # Item CRUD operations
│   ├── PlaceRepositoryTests.cs        # Place/store operations
│   └── PriceRecordRepositoryTests.cs  # Price history operations
├── ViewModels/
│   ├── MainWindowViewModelTests.cs    # Main window logic
│   ├── ItemViewModelTests.cs          # Item management UI
│   └── ImportDataViewModelTests.cs    # Import workflow UI
├── Integration/
│   └── ImportExportIntegrationTests.cs # End-to-end workflows
├── TestData/
│   ├── sample_coles.json              # Test data samples
│   ├── sample_woolworths.json
│   └── sample_drakes.md
└── TESTING.md                         # This file
```

### Dependencies

```xml
<PackageReference Include="xUnit" Version="2.6.6" />
<PackageReference Include="xUnit.runner.visualstudio" Version="2.5.6" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
```

---

## Test Categories

### 1. Service Layer Tests

**Location:** `Services/`

#### JsonImportServiceTests (24 tests)

Tests the core JSON import functionality:

| Test Method | Purpose |
|-------------|---------|
| `PreviewImportAsync_ValidJsonFile_ReturnsItems` | Preview without saving |
| `ImportFromFile_ColesJson_ImportsSuccessfully` | Full import workflow |
| `ImportColesProducts_MapsProductsToItems` | Data mapping validation |
| `ImportFromFile_ReportsProgress` | Progress tracking |
| `ParsePrice_VariousFormats_ParsesCorrectly` | Price string parsing |
| `ImportFromFile_InvalidJson_ReturnsErrors` | Error handling |

**Key Testing Patterns:**
```csharp
[Fact]
public async Task ImportFromFile_ColesJson_ImportsSuccessfully()
{
    // Arrange
    var testFile = TestDataHelper.CreateTestFile("sample_coles.json");
    
    // Act
    var result = await _service.ImportFromFile(testFile, "coles");
    
    // Assert
    result.Success.Should().BeTrue();
    result.ItemsImported.Should().BeGreaterThan(0);
}
```

#### ServerConfigServiceTests (30 tests)

Tests server configuration management:

| Test Category | Count |
|---------------|-------|
| Load/Save Config | 8 |
| Server CRUD | 10 |
| Connection Testing | 6 |
| Default Server | 6 |

### 2. Repository Layer Tests

**Location:** `Repositories/`

Tests database operations using in-memory LiteDB instances:

#### ItemRepositoryTests

```csharp
[Fact]
public void Add_ValidItem_AddsToDatabase()
{
    // Arrange
    var item = TestDataGenerator.CreateMockItem();
    
    // Act
    _repository.Add(item);
    
    // Assert
    var retrieved = _repository.GetById(item.Id);
    retrieved.Should().NotBeNull();
    retrieved.Name.Should().Be(item.Name);
}
```

**Test Coverage:**
- CRUD operations
- Search by name
- Filter by category
- Price range queries

#### PlaceRepositoryTests

Tests store/location management:
- Add/Update/Delete places
- Search by suburb/state
- Chain filtering

#### PriceRecordRepositoryTests

Tests price history tracking:
- Record price entries
- Get price history
- Latest price retrieval
- Sale price tracking

### 3. ViewModel Tests

**Location:** `ViewModels/`

Tests MVVM logic without UI dependencies:

#### MainWindowViewModelTests

| Test | Purpose |
|------|---------|
| `LoadDashboardStats_WithData_UpdatesCorrectly` | Dashboard stats |
| `NavigateToItems_UpdatesCurrentView` | Navigation |
| `RefreshData_UpdatesAllStats` | Data refresh |

#### ImportDataViewModelTests

| Test | Purpose |
|------|---------|
| `ImportData_ValidJson_CallsImportService` | Import workflow |
| `PreviewImport_ValidFile_LoadsPreview` | Preview functionality |
| `SetSelectedFiles_ValidFiles_UpdatesFileList` | File selection |

### 4. Integration Tests

**Location:** `Integration/`

End-to-end workflow testing:

| Test | Purpose |
|------|---------|
| `ImportThenExport_DataIntegrity_Maintained` | Round-trip data integrity |
| `ImportMultipleFormats_AllStoresInDatabase` | Multi-format import |
| `ExportAndReimport_NoDataLoss` | Export/import cycle |
| `ImportWithDateFiltering_FiltersCorrectly` | Date filtering |
| `ImportWithStoreFiltering_FiltersCorrectly` | Store filtering |
| `ExportWithCompression_CreatesValidGzFile` | Compression |
| `ImportExport_WithDuplicateData_HandlesCorrectly` | Duplicate handling |

---

## Running Tests

### Command Line

```powershell
# Build the solution
dotnet build AdvGenPriceComparer.sln

# Run all tests
dotnet test AdvGenPriceComparer.Tests/AdvGenPriceComparer.Tests.csproj

# Run with verbosity
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~JsonImportServiceTests"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### In Visual Studio

1. Open Test Explorer: `Test > Test Explorer`
2. Click `Run All` or select specific tests
3. View results and output

### Filter Examples

```powershell
# Run by trait
dotnet test --filter "Category=Integration"

# Run by display name
dotnet test --filter "DisplayName~Import"

# Run skipped tests only
dotnet test --filter "Skip!=true"
```

---

## Test Data Management

### TestDatabaseHelper

Creates isolated in-memory databases for each test:

```csharp
public class TestDatabaseHelper
{
    public static DatabaseService CreateInMemoryDatabase()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        return new DatabaseService(tempPath);
    }
    
    public static void CleanupDatabase(DatabaseService db)
    {
        var path = db.DatabasePath;
        db.Dispose();
        if (File.Exists(path))
            File.Delete(path);
    }
}
```

### MockDataGenerator

Generates consistent test data:

```csharp
public static Item CreateMockItem(string name = "Test Product")
{
    return new Item
    {
        Id = Guid.NewGuid().ToString(),
        Name = name,
        Price = 10.99m,
        Brand = "Test Brand",
        Category = "Test Category",
        CreatedAt = DateTime.Now
    };
}
```

### Test Data Files

Located in `TestData/`:
- `sample_coles.json` - Coles format sample
- `sample_woolworths.json` - Woolworths format sample
- `sample_drakes.md` - Drakes markdown sample

---

## CI/CD Integration

### GitHub Actions Workflow

```yaml
name: Build and Test

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET 9
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

### Test Results

- **Current Status:** All 217+ tests passing
- **Build Status:** ✅ Green
- **Average Runtime:** ~30 seconds

---

## Code Coverage

### Current Coverage (27.67%)

| Layer | Coverage | Target |
|-------|----------|--------|
| Services | 45% | 90% |
| Repositories | 85% | 95% |
| ViewModels | 35% | 80% |
| Models | 20% | 70% |
| **Overall** | **27.67%** | **85%** |

### Running Coverage Locally

```powershell
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# Generate with coverlet
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# View HTML report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport
coveragereport\index.html
```

### Coverage Configuration (coverlet.runsettings)

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>cobertura,json</Format>
          <TargetDir>TestResults</TargetDir>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

---

## Best Practices

### 1. Test Naming

Use descriptive names following the pattern:
```
MethodName_StateUnderTest_ExpectedBehavior
```

Examples:
- `ImportFromFile_ValidJson_ReturnsSuccessResult`
- `Add_DuplicateItem_ThrowsException`
- `ParsePrice_StringWithDollarSign_ReturnsDecimal`

### 2. Arrange-Act-Assert

Structure tests clearly:

```csharp
[Fact]
public void ExampleTest()
{
    // Arrange - setup test data and mocks
    var input = "test data";
    
    // Act - execute the method under test
    var result = _service.Process(input);
    
    // Assert - verify the outcome
    result.Should().Be("expected output");
}
```

### 3. Use FluentAssertions

Prefer fluent assertions for readability:

```csharp
// Good
result.Should().BeTrue();
items.Should().HaveCount(3);
action.Should().Throw<ArgumentException>();

// Avoid
Assert.True(result);
Assert.Equal(3, items.Count);
Assert.Throws<ArgumentException>(() => action());
```

### 4. Isolation

Each test should be independent:

```csharp
public class TestClass : IDisposable
{
    private readonly DatabaseService _db;
    
    public TestClass()
    {
        _db = TestDatabaseHelper.CreateInMemoryDatabase();
    }
    
    public void Dispose()
    {
        TestDatabaseHelper.CleanupDatabase(_db);
    }
}
```

### 5. Mock External Dependencies

Use Moq for external services:

```csharp
var mockLogger = new Mock<ILoggerService>();
var service = new JsonImportService(mockLogger.Object);

mockLogger.Verify(x => x.LogInfo(It.Is<string>(s => s.Contains("success"))), Times.Once);
```

---

## Troubleshooting

### Common Issues

#### 1. Tests Failing with File Lock

**Problem:** LiteDB file locked by another test  
**Solution:** Ensure proper disposal in `Dispose()` method

```csharp
public void Dispose()
{
    _db?.Dispose();
    GC.Collect();
    GC.WaitForPendingFinalizers();
}
```

#### 2. Async Test Deadlocks

**Problem:** Tests hang indefinitely  
**Solution:** Use `async/await` properly

```csharp
// Good
[Fact]
public async Task TestAsync()
{
    await _service.MethodAsync();
}

// Avoid
[Fact]
public void TestAsync()
{
    _service.MethodAsync().Wait(); // Can deadlock
}
```

#### 3. Test Data Conflicts

**Problem:** Tests interfere with each other  
**Solution:** Use unique IDs or in-memory databases

```csharp
var uniqueId = Guid.NewGuid().ToString();
```

### Debugging Tests

1. **In Visual Studio:**
   - Set breakpoint in test
   - Right-click test > Debug

2. **With Logging:**
   ```csharp
   _outputHelper.WriteLine($"Debug: {variable}");
   ```

3. **Test Output:**
   ```csharp
   public class TestClass
   {
       private readonly ITestOutputHelper _output;
       
       public TestClass(ITestOutputHelper output)
       {
           _output = output;
       }
   }
   ```

---

## Future Improvements

### Planned Test Additions

1. **UI Automation Tests** - Test WPF UI with FlaUI or Appium
2. **Performance Tests** - Benchmark import/export with large datasets
3. **Property-Based Tests** - Use FsCheck for randomized testing
4. **Contract Tests** - Verify API compatibility with AdvGenNoSqlServer

### Coverage Goals

- **Q1 2026:** 50% overall coverage
- **Q2 2026:** 70% overall coverage
- **Q3 2026:** 85% overall coverage (target)

---

## Contributing

When adding new tests:

1. Follow the existing folder structure
2. Use the naming conventions described above
3. Ensure tests run in isolation
4. Add test data to `TestData/` folder if needed
5. Update this documentation

---

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/Moq/moq4/wiki/Quickstart)
- [FluentAssertions Docs](https://fluentassertions.com/)
- [LiteDB Testing Patterns](https://www.litedb.org/)

---

**Maintained by:** Agent-020  
**Last Review:** 2026-02-26
