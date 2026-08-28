
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2026-06-27 - Semantic navigation in Blazor
**Learning:** Using `<div>` elements with `@onclick` for navigation in Blazor components prevents native browser behaviors (open in new tab, copy link) and breaks keyboard accessibility, as they aren't natively focusable or operable with the Enter key.
**Action:** Always replace interactive navigation `<div>`s with semantic `<a>` anchor tags. Apply utility classes like `text-decoration-none text-dark` to preserve visual styling. Ensure the `href` uses an absolute path (e.g., `/item/{id}`) to avoid relative path stacking issues.
