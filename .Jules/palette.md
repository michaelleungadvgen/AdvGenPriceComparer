## 2025-05-13 - Blazor Confirmation Dialogs

**Learning:** The project relies on standard `confirm()` dialogs via JS interop rather than custom UI components for destructive action confirmations. This pattern is simpler but requires converting synchronous event handlers to `async Task` methods in Blazor.
**Action:** Use `await JSRuntime.InvokeAsync<bool>("confirm", ...)` when adding confirmations to `@onclick` handlers in `.razor` files, ensuring `IJSRuntime` is injected.
