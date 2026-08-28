
## 2024-05-23 - Screen Reader Accessibility in WPF
**Learning:** The existing ToolTip attributes on icon-only buttons are insufficient for screen readers in WPF. Screen readers rely on the AutomationProperties.Name attached property to announce elements correctly.
**Action:** Explicitly define AutomationProperties.Name on all icon-only buttons in WPF XAML files to ensure they are accessible to assistive technologies.


## 2025-02-17 - WPF Icon-only Button Accessibility
**Learning:** In WPF applications using `ui:Button` and `ui:SymbolIcon`, relying solely on the `ToolTip` attribute is insufficient for screen readers. Icon-only buttons lack proper text representation without explicitly defining an ARIA label.
**Action:** Always define `AutomationProperties.Name` on icon-only buttons to ensure they are fully accessible to screen readers, just like using `aria-label` in web development.

## 2025-04-07 - Screen Reader Accessibility for Emoji Buttons
**Learning:** Buttons using emojis combined with text (e.g. `Content="📁 Select Image File"`) or emojis alone (e.g. `Content="📋"`) in their `Content` property can be problematic for screen readers, as they may read out literal emoji descriptions instead of clear actions.
**Action:** Always define a text-only `AutomationProperties.Name` on buttons that use emojis in their `Content` to ensure screen readers announce a clear, concise action.
