# Code Review: Clean Architecture & .NET Enterprise Standards
**Project:** AdvGenPriceComparer
**Reviewer:** Jules, Senior C# & .NET Expert

---

## Executive Summary
This code review evaluates the `AdvGenPriceComparer` project against industry-standard **Clean Architecture**, Domain-Driven Design (DDD), and SOLID principles. The project is currently a well-structured multi-project solution (`Core`, `Data.LiteDB`, and `WPF`). However, it exhibits several architectural "leaks" where infrastructure concerns bleed into the domain layer, and application-specific business rules are tightly coupled to the data access layer.

By realigning the project to a strict Clean Architecture, you will dramatically improve testability, make future migrations (like transitioning from `LiteDB` to `AdvGenNoSqlServer` or SQL Server) frictionless, and elevate the overall engineering excellence to an enterprise standard.

---

## 1. Clean Architecture Violations & Dependency Flow Analysis

The fundamental rule of Clean Architecture is the **Dependency Rule**: Source code dependencies must point only inward, toward higher-level policies (the Domain).

### A. The `AdvGenPriceComparer.Core` Layer
**Current State:**
The `Core` project acts as a hybrid of Domain, Application, and Infrastructure layers.
- It contains pure domain entities like `Item`, `Place`, and `PriceRecord`.
- It also contains `NetworkManager.cs` which explicitly handles `System.Text.Json` serialization, TCP Sockets (`TcpClient`), and Network protocol messages. This is a severe violation, as Infrastructure concerns (Networking, Sockets, JSON serialization) are living in the innermost circle of the architecture.

**Recommendation:**
- Extract `NetworkManager.cs` into a dedicated Infrastructure project (e.g., `AdvGenPriceComparer.Infrastructure.Network`) or the existing Data project.
- Create an abstraction (e.g., `IP2PNetworkService`) inside the `Core` layer. The infrastructure project should implement this interface.

### B. The `AdvGenPriceComparer.Data.LiteDB` Layer
**Current State:**
This project handles database access via Repositories, which is correct. However, it also contains Application Business Rules:
- `JsonImportService.cs` and `JsonExportService.cs` contain complex business logic (file validation, domain entity mapping, JSON parsing, error handling).
- File I/O operations and format-specific parsing are Infrastructure and Application layer concerns, not purely Data layer concerns.

**Recommendation:**
- Create a new project `AdvGenPriceComparer.Application` to house Application Services, Use Cases, and Application-specific interfaces.
- Move the parsing logic, import/export orchestration, and progress tracking out of `Data.LiteDB` into the `Application` layer.
- Keep `Data.LiteDB` strictly focused on persisting and retrieving entities via `LiteDB`.

### C. The `AdvGenPriceComparer.WPF` Layer (Presentation)
**Current State:**
The UI layer is relatively clean, utilizing MVVM patterns. However, some ViewModels directly depend on concrete implementations from the Data layer rather than interfaces.
- `ImportDataViewModel.cs` directly references `JsonImportService` from `AdvGenPriceComparer.Data.LiteDB`.
- This creates a tight coupling between the UI and the specific LiteDB data implementation, violating the Dependency Inversion Principle.

**Recommendation:**
- Extract an interface for the import logic (e.g., `IImportService`) and place it in the Application or Core layer.
- Inject `IImportService` into `ImportDataViewModel` instead of the concrete `JsonImportService`.

---

## 2. Refactoring Recommendations (Roadmap)

To elevate this codebase to an enterprise standard, I recommend executing the following roadmap:

### Step 1: Establish the Application Layer
1. Create `AdvGenPriceComparer.Application` project.
2. Move all business orchestration logic here. This includes interfaces like `IImportUseCase`, `IExportUseCase`, and complex cross-repository operations.
3. This layer should reference `Core`, but *not* `Data.LiteDB` or `WPF`.

### Step 2: Purify the Core (Domain) Layer
1. Remove `System.Net.Sockets` and infrastructure-specific JSON serialization from `Core`.
2. Move `NetworkManager.cs` to an infrastructure layer.
3. Ensure Domain models (`Item`, `Place`, `PriceRecord`) are "POCOs" (Plain Old CLR Objects) devoid of database or serialization attributes. If mapping is needed, use DTOs/Entities in the outer layers.

### Step 3: Implement CQRS (Optional but highly recommended)
Given your focus on DDD and scalability, consider adopting the CQRS (Command Query Responsibility Segregation) pattern using MediatR in the new Application layer.
- **Commands:** `CreateItemCommand`, `ImportPricesCommand`
- **Queries:** `GetBestDealsQuery`, `GetPriceHistoryQuery`
This will massively simplify the overly bloated `IGroceryDataService` which currently acts as a god-object.

### Step 4: Fix Dependency Injection
Update `App.xaml.cs` to rely strictly on Interfaces from the Core/Application layers.
- Avoid injecting concrete classes where interfaces should exist.

---

## 3. Engineering Excellence & DDD Alignment

**Domain-Driven Design (DDD) Observations:**
- The current entities are somewhat anemic. They have public setters and lack rich domain behaviors. For instance, `PriceRecord` could have business logic to determine if a discount is "illusory" (e.g., `public bool IsIllusoryDiscount() { ... }`) instead of relying purely on UI or Service layers for this calculation.
- Embrace Value Objects. For example, `Price` could be a Value Object that encapsulates currency and value, preventing invalid state (e.g., negative prices).

**Testing Strategy:**
- While the project boasts 217+ tests, testing concrete UI ViewModels tightly coupled to LiteDB services is brittle.
- By adhering to Clean Architecture, you can test Application Use Cases and Domain logic entirely independently of the database or UI framework using fast-running Unit Tests with mocked interfaces (via Moq or NSubstitute).

## Conclusion
The application is functionally rich and well-organized at a macro level, but strict layer boundaries are currently blurring. Refactoring toward a rigid Clean Architecture (Core -> Application -> Infrastructure / Presentation) will unlock true scalability, easier testing, and prepare the project seamlessly for your future ML.NET, Ollama, and enterprise NoSQL integrations.