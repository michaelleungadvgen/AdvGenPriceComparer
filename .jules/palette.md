
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2024-05-24 - Improve Browse page accessibility and card navigation
**Learning:** Using `div` elements with `@onclick` in Blazor prevents native web behaviours like right-clicking, middle-clicking to open in a new tab, and keyboard navigation.
**Action:** Always replace `div` based navigation with semantic `a` tags (styled with `text-decoration-none text-reset`) to preserve native accessibility and web behaviors.
