
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2024-04-18 - WPF Emoji Button Accessibility
**Learning:** Using emojis as button Content in WPF XAML does not automatically provide accessible names to screen readers, unlike standard text. Relying solely on `ToolTip` is insufficient.
**Action:** Always explicitly define `AutomationProperties.Name` on icon-only and emoji buttons in WPF applications to provide clear, textual ARIA-equivalent labels.
