
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.

## 2025-05-11 - Adding Confirmation to Destructive Actions
**Learning:** Blazor's `@onclick` handlers naturally support upgrading from a synchronous `void` method to an asynchronous `Task` method without any syntax changes in the markup. This makes it trivial to insert `await JSRuntime.InvokeAsync<bool>("confirm", ...)` to add a native browser confirmation dialog before executing a potentially destructive operation, such as a delete.
**Action:** When implementing destructive operations in Blazor, inject `IJSRuntime` and evaluate native browser `confirm` dialogs, converting synchronous click event handlers to `async Task` to enable awaiting the result.
