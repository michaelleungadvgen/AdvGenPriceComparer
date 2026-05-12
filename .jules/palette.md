
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.

## 2025-05-12 - Replacing div+onclick with anchor tags for card navigation
**Learning:** Using `div` with `@onclick` for navigation cards completely breaks keyboard accessibility, screen reader focus, and standard browser features (like middle-click to open in new tab).
**Action:** Always use semantic `<a>` tags with `href` for interactive cards that navigate to a new route, applying styles (`text-decoration-none text-dark display: block`) to maintain the card visual structure while restoring native accessibility.
