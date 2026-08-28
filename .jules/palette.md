
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2025-02-18 - Semantic HTML in Blazor Navigation
**Learning:** In Blazor, attaching `@onclick` navigation events to `<div>` elements breaks native browser functionality (like "Open in new tab", "Copy link address") and impairs keyboard navigation because `<div>` elements are not inherently focusable or actionable.
**Action:** Always replace `<div>` navigation wrappers with semantic `<a>` tags. Ensure the `href` uses absolute paths (e.g., `/item/{id}`) to prevent route stacking issues, and use utility classes like `text-decoration-none text-dark` to preserve visual card styling.
