
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2025-05-23 - Destructive Action Confirmations
**Learning:** Destructive actions, such as deleting a category, were performing instantly upon button click in the UI without a confirmation dialog. This can easily lead to accidental data loss or require annoying recovery steps.
**Action:** Always wrap destructive operations (like deleting items or categories) with a simple user confirmation prompt (e.g., `MessageBox.Show` with `YesNo`) before proceeding with the operation.
