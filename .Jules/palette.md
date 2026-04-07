
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2025-03-05 - WPF Text-based Icon Button Accessibility
**Learning:** In WPF applications, standard text-emoji based icon buttons (e.g. `Content="🗑️"` or `Content="➤"`) are still considered icon-only buttons for accessibility purposes. Like shape-based icons, using `ToolTip` is not enough to make them accessible to screen readers.
**Action:** Always add an explicit `AutomationProperties.Name` to any text or emoji button that serves as an icon to ensure full screen reader accessibility.
