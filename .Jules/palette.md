
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.


## 2025-02-17 - WPF Text-Based Icon Buttons
**Learning:** Even when WPF buttons use text or emoji (like "✕", "🗑️", or "➤") as their `Content` instead of an explicit icon control (like `ui:SymbolIcon`), they are still functionally icon-only buttons. The screen reader will read the raw character (which can be confusing or skipped). `ToolTip` is not enough for accessibility.
**Action:** Always provide an explicit `AutomationProperties.Name` for text-based icon-only buttons (like emojis or symbols) to ensure screen readers announce their true function (e.g., "Close window", "Clear chat", "Send message") rather than reading the raw character.
