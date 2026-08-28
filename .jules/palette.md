
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2024-05-20 - Semantic Anchor Tags in Blazor
**Learning:** In Blazor applications, using a `<div>` wrapper with an `@onclick` handler for navigation is an accessibility anti-pattern. Screen readers do not recognize these interactive `div` elements as links or buttons, and keyboard users cannot navigate to them via `Tab` or activate them using the `Enter` key. Furthermore, this approach breaks standard browser capabilities, such as middle-clicking to open in a new tab.
**Action:** Replace clickable `div` blocks intended for routing with semantic `<a>` tags. Apply utility classes like `text-decoration-none d-block` (and `text-dark` if needed) to ensure the anchor tag matches the original visual styling while providing native accessibility and browser features.
