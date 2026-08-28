
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2025-05-04 - Adding AutomationProperties.Name to Buttons with Symbols
**Learning:** Screen readers may read out the literal name of emojis or text symbols used in button content (like '✕' or '🗑️'). This creates a confusing experience.
**Action:** Always add an explicit `AutomationProperties.Name` containing a clear description (e.g., "Clear search", "Delete alert") to buttons that use emoji/text symbols as content to provide an accessible ARIA-equivalent text label.
