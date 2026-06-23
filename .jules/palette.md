
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2026-06-23 - Blazor Card Clickability Accessibility
**Learning:** In Blazor web projects, using `<div>` with `@onclick` for navigation breaks keyboard accessibility (tabbing) and native browser features (like middle-click or right-click "Open in new tab").
**Action:** Always replace them with semantic `<a>` tags. Ensure the `href` uses an absolute path (e.g., `/item/{id}`) to prevent relative path stacking issues, and apply utility classes like `text-decoration-none text-dark` to preserve visual styling without forcing `display: block`.
