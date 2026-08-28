
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2026-05-26 - Blazor Link Accessibility
**Learning:** In Blazor web projects, using `<div>` with `@onclick` for navigation breaks keyboard accessibility and native browser features (like middle-click).
**Action:** Always replace them with semantic `<a>` tags using `href` when linking to routes, applying utility classes like `text-decoration-none text-dark` to preserve visual styling without disrupting flexbox layouts with forced `display: block`.
