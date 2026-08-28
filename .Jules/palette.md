
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2023-10-27 - ARIA-equivalent labels for emoji buttons in WPF
**Learning:** WPF `Button` elements that use text-based icons or emojis in their `Content` property (e.g., `Content="🗑️ Clear"`, `Content="✕"`) are read literally by screen readers, which can result in confusing announcements like "wastebasket clear button". Relying solely on `ToolTip` is insufficient for accessibility.
**Action:** When standard `Button` elements in WPF use text-based icons or emojis, either alone or alongside text, explicitly define a text-only `AutomationProperties.Name` to ensure correct screen reader representation and prevent literal emoji announcements.
