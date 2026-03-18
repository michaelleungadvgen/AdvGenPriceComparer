
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2026-03-18 - Buttons with Emoji/Symbol content
**Learning:** Screen readers might poorly vocalize or entirely skip Unicode emojis and symbols used as content for buttons (e.g. `Content="🗑️ Clear"` or `Content="📋 Copy"`), which can degrade accessibility. A standard tooltip handles visual hover context but doesn't solve screen reader discovery.
**Action:** Apply `AutomationProperties.Name` explicitly on WPF elements where the primary content includes Unicode emojis/symbols serving as icons, ensuring clear screen reader vocalizations devoid of unintended character descriptions.
