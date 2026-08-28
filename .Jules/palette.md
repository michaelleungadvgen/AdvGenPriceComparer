
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2025-03-30 - WPF Visual Status Badge Accessibility
**Learning:** Visual status indicators like `ui:Badge` used to show numerical counts (e.g., active alerts or unread notifications) require both a visual `ToolTip` and an explicitly defined `AutomationProperties.Name` attribute to be announced correctly by screen readers in WPF applications.
**Action:** Always define `AutomationProperties.Name` and `ToolTip` on all visual status badges to ensure they provide necessary context to all users, including those using assistive technologies.
