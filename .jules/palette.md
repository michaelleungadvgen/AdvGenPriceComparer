
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2026-07-01 - Keyboard Accessibility in Blazor
**Learning:** In Blazor web projects using Bootstrap, converting a `div` element with `@onclick` to an `<a>` anchor tag is crucial for keyboard accessibility. However, it's important to ensure the `href` uses an absolute path (e.g., `/item/{id}`) to prevent relative path stacking issues, and apply utility classes like `text-decoration-none text-dark` to preserve visual styling.
**Action:** Always replace `<div @onclick="...">` with semantic `<a href="...">` tags for navigation elements in Blazor to ensure full keyboard navigation support and native browser features.
