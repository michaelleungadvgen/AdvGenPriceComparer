## 2024-05-24 - WPF Accessibility
**Learning:** In WPF applications (`.xaml` files), the equivalent of an HTML `aria-label` for screen readers is `AutomationProperties.Name`.
**Action:** Always apply `AutomationProperties.Name` alongside `ToolTip` attributes on icon-only or generic UI elements (like Edit/Delete buttons) in XAML to ensure both visual and screen reader accessibility.