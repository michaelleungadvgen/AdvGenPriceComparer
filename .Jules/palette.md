
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2025-05-23 - Confirmation Prompts for Destructive Actions in WPF
**Learning:** In WPF applications, it's crucial to wrap destructive actions (like deleting an item, store, or category) with a confirmation dialog. Not doing so allows users to accidentally delete data with a single click, leading to poor user experience. The application provides `MessageBox.Show(..., MessageBoxButton.YesNo, MessageBoxImage.Question)` for straightforward interactions.
**Action:** Always add a confirmation prompt (`MessageBox.Show` with `YesNo` buttons) in ViewModel methods that execute deletion commands before performing the actual deletion.
