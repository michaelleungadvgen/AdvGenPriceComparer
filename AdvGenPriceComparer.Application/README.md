# AdvGenPriceComparer.Application

This project contains the **Application Layer** for the AdvGenPriceComparer application, following Clean Architecture principles.

## Purpose

The Application Layer acts as the orchestration layer between the UI (Presentation Layer) and the Domain/Core layer. It contains:

- **Use Cases**: Application-specific business rules and workflows
- **Interfaces**: Abstractions that define what the application can do
- **DTOs**: Data Transfer Objects for communication between layers

## Clean Architecture Dependency Rule

**The Application Layer only references the Core Layer.**

```
UI (WPF) → Application → Core
                ↓
           Infrastructure (Data.LiteDB, etc.)
```

This means:
- ✅ Application can use Core.Models, Core.Interfaces
- ✅ Application defines interfaces that Infrastructure implements
- ❌ Application does NOT reference Data.LiteDB, WPF, or external frameworks

## Structure

```
AdvGenPriceComparer.Application/
├── Interfaces/
│   ├── IImportUseCase.cs      # Interface for data import operations
│   └── IExportUseCase.cs      # Interface for data export operations
├── DTOs/
│   ├── ImportDtos.cs          # Import result and progress DTOs
│   ├── ExportDtos.cs          # Export result and progress DTOs
│   ├── ImportRequestDto.cs    # Import request and options DTOs
│   ├── ExportRequestDto.cs    # Export request and filter DTOs
│   └── ...                    # Other DTOs
└── README.md                  # This file
```

## Key Interfaces

### IImportUseCase

Defines operations for importing grocery data from various sources:
- `ImportFromJsonAsync` - Import from Coles/Woolworths JSON format
- `ImportFromMarkdownAsync` - Import from Drakes markdown format
- `PreviewImportAsync` - Preview before committing import
- `BulkImportAsync` - Import multiple files

### IExportUseCase

Defines operations for exporting grocery data:
- `ExportToJsonAsync` - Export to standardized JSON format
- `ExportToJsonGzAsync` - Export to compressed JSON
- `IncrementalExportAsync` - Export only changed items
- `ExportForP2PAsync` - Export for P2P sharing (static data format)

## DTOs

Data Transfer Objects are used to pass data between layers without exposing domain entities directly:

### Import DTOs
- **ImportRequestDto** - Request data for import operations
- **ImportOptionsDto** - Import configuration options
- **ImportResultDto** - Result of import operations
- **ImportPreviewDto** - Preview of items to be imported
- **ImportProgressDto** - Progress during import
- **ImportErrorDto** - Error information

### Export DTOs
- **ExportRequestDto** - Request data for export operations
- **ExportFilterDto** - Filter options for exports
- **ExportOptionsDto** - Export configuration options
- **ExportResultDto** - Result of export operations
- **IncrementalExportRequestDto** - Request for incremental exports
- **P2PExportRequestDto** / **P2PExportResultDto** - P2P-specific export data

## Usage

The Application layer interfaces are implemented by the Infrastructure layer (WPF project). Register in DI container:

```csharp
// In App.xaml.cs
services.AddTransient<IImportUseCase, ImportUseCase>();
services.AddTransient<IExportUseCase, ExportUseCase>();
```

ViewModels depend on the interfaces:

```csharp
public class ImportDataViewModel
{
    private readonly IImportUseCase _importUseCase;
    
    public ImportDataViewModel(IImportUseCase importUseCase)
    {
        _importUseCase = importUseCase;
    }
}
```

## Benefits

1. **Separation of Concerns**: Business logic is separated from UI and infrastructure
2. **Testability**: Interfaces can be mocked for unit testing
3. **Flexibility**: Easy to swap implementations without changing UI code
4. **Dependency Inversion**: UI depends on abstractions, not concrete implementations
5. **Clean Architecture**: Clear dependency direction (UI → Application → Core)

## Notes

This project was created as part of Phase 14: Clean Architecture Refactoring. The actual implementations of `IImportUseCase` and `IExportUseCase` reside in the WPF project (Infrastructure layer) for now. Future refactoring may move these implementations to a dedicated Infrastructure project.
