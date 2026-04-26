
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2026-03-24 - Missing ARIA Labels on Emoji Buttons
**Learning:** Standard WPF Button components that use emojis as visual icons in their `Content` property (e.g., `<Button Content="🗑️ Clear"/>` or `<Button Content="📁 Select Image File"/>`) without providing `AutomationProperties.Name` result in screen readers reading out the literal Unicode description of the emoji, causing a confusing user experience.
**Action:** When using emojis or text-based symbols as icons inside standard WPF `Button` elements, always define a clean, text-only `AutomationProperties.Name` attribute to provide a proper ARIA-equivalent accessible name.
