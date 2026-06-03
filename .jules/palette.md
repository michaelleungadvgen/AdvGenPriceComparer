
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2024-06-03 - Replaced click-bound divs with semantic links in Blazor pages
**Learning:** In Blazor web projects, it is a common anti-pattern to use `<div>` elements with `@onclick` handlers for navigation to detail pages. This completely breaks keyboard accessibility (no focus, no Enter key trigger) and prevents standard native browser interactions like "Right-click -> Open in new tab".
**Action:** Always refactor these components to use semantic `<a href="...">` tags. In Bootstrap/standard UI layouts, you can easily preserve the visual appearance of the "card" by adding `text-decoration-none text-dark` utility classes and moving the `class` attributes directly onto the `<a>` tag, ensuring full accessibility without visual regressions.
