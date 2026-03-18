# UI Automation Tests

This directory contains comprehensive UI automation tests for the AdvGenPriceComparer WPF application using FlaUI.

## Overview

The UI automation tests use the **FlaUI** framework to interact with the WPF application in a way that simulates real user interactions. These tests verify that the UI components work correctly from an end-user perspective.

## Framework

- **FlaUI.Core**: Core UI automation library
- **FlaUI.UIA3**: UI Automation 3 provider for Windows

## Test Structure

### Page Objects (Automation/Pages/)

The Page Object pattern is used for maintainable UI tests:

- **BasePage.cs**: Base class for all page objects with common UI interactions
- **MainWindowPage.cs**: Page object for the main application window
- **ItemsPage.cs**: Page object for the Items management page
- **AddItemDialog.cs**: Page object for the Add Item dialog
- **ImportDialog.cs**: Page object for the Import Data dialog

### Test Classes (UI/)

- **MainWindowTests.cs**: Tests for main window functionality
- **ItemsPageTests.cs**: Tests for items management
- **ImportExportTests.cs**: Tests for import/export functionality

### Utilities (Automation/)

- **ApplicationLauncher.cs**: Handles launching and managing the WPF application

## Running the Tests

### Prerequisites

1. Build the WPF application first:
   ```powershell
   dotnet build AdvGenPriceComparer.WPF/AdvGenPriceComparer.WPF.csproj
   ```

2. Ensure the application executable exists at:
   - `AdvGenPriceComparer.WPF/bin/Debug/net9.0-windows/AdvGenPriceComparer.WPF.exe`
   - Or: `AdvGenPriceComparer.WPF/bin/Release/net9.0-windows/AdvGenPriceComparer.WPF.exe`

### Run All UI Tests

```powershell
dotnet test AdvGenPriceComparer.Tests/AdvGenPriceComparer.Tests.csproj --filter "FullyQualifiedName~UI"
```

### Run Specific Test Class

```powershell
dotnet test AdvGenPriceComparer.Tests/AdvGenPriceComparer.Tests.csproj --filter "FullyQualifiedName~MainWindowTests"
```

### Run Individual Test

```powershell
dotnet test AdvGenPriceComparer.Tests/AdvGenPriceComparer.Tests.csproj --filter "MainWindowTests.Application_Launch_MainWindowDisplayed"
```

## Test Categories

The UI tests are organized by functionality:

### Main Window Tests
- Application launch
- Window title verification
- Navigation between views
- Dashboard statistics display
- Dialog opening (Add Item, Import, Export)

### Items Page Tests
- Page loading
- Items grid display
- Search functionality
- Category filtering
- Sorting
- Item selection
- Add Item workflow

### Import/Export Tests
- Dialog opening
- File path entry
- Store selection
- Workflow navigation
- Error handling
- Cancel operations

## Best Practices

1. **Page Object Pattern**: All UI interactions are encapsulated in page objects
2. **Explicit Waits**: Tests use explicit waits for elements to be ready
3. **Cleanup**: Tests properly close dialogs and dispose resources
4. **Independence**: Each test is independent and can run in isolation
5. **Error Handling**: Tests handle missing elements gracefully

## Adding New Tests

1. Create a new test class in the `UI/` folder
2. Inherit from `IDisposable` for proper cleanup
3. Use `ApplicationLauncher` to start the application
4. Use page objects for UI interactions
5. Add tests with the `[Fact]` or `[Theory]` attributes

Example:
```csharp
public class MyNewTests : IDisposable
{
    private readonly ApplicationLauncher _launcher;
    private readonly UIA3Automation _automation;

    public MyNewTests()
    {
        _launcher = new ApplicationLauncher();
        _automation = new UIA3Automation();
    }

    [Fact]
    public void MyTest()
    {
        _launcher.Launch();
        var mainWindow = _launcher.MainWindow;
        var mainPage = new MainWindowPage(mainWindow!, _automation);
        // ... test code
    }

    public void Dispose()
    {
        _launcher.Dispose();
        _automation.Dispose();
    }
}
```

## Troubleshooting

### Application Not Found
If tests fail with "Application executable not found", ensure:
- The WPF project has been built
- The path in `ApplicationLauncher.GetApplicationPath()` matches your build output

### Element Not Found
If tests fail to find UI elements:
- Check that automation IDs match between the XAML and test code
- Increase wait timeouts for slower machines
- Verify the application is actually displaying the expected UI

### Dialog Already Open
If tests fail because a dialog is already open:
- Ensure proper cleanup in previous tests
- Add explicit close operations in test cleanup

## Notes

- UI tests require a Windows environment with a display (GUI session)
- Tests may run slower than unit tests due to UI interactions
- Some tests may fail on headless environments (CI/CD without GUI)
