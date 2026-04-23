
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2025-02-18 - Emoji Icon Button Accessibility in WPF
**Learning:** When using emojis in the `Content` of WPF buttons (e.g., `Content="🔥 Best Prices"`), screen readers will announce the literal emoji description alongside the text (e.g., "Fire Best Prices"), which sounds unprofessional and confusing.
**Action:** Always assign a clean, text-only `AutomationProperties.Name` (e.g., `AutomationProperties.Name="Best Prices"`) to buttons using emojis in their content to ensure clean screen reader announcements.
