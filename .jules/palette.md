
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2024-05-24 - Confirm Dialogs for Destructive Actions in Blazor
**Learning:** In Blazor, replacing immediate destructive actions with a native browser confirmation dialog by using `IJSRuntime` (`await JSRuntime.InvokeAsync<bool>("confirm", "...")`) makes the application safer by preventing accidental data loss. Because `@onclick` supports async operations seamlessly, this can be added with no markup changes other than updating the method signature.
**Action:** Always add a native JS confirmation dialog via `IJSRuntime` to buttons performing destructive actions like deletions.
