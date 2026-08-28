
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2025-02-17 - Custom Clear Buttons in Search Interfaces
**Learning:** Custom UI elements acting as clear or close buttons in search interfaces (like a `Content="✕"` button) are frequently overlooked for accessibility labels because they use Unicode text instead of dedicated icon components. This renders them unintelligible to screen readers ("times" or "multiplication X" is read instead of "Clear search").
**Action:** Always ensure that custom Unicode icon buttons (e.g. "✕", "📋") in search bars or custom text fields are provided with `AutomationProperties.Name` and a corresponding `ToolTip` to accurately convey their function.
