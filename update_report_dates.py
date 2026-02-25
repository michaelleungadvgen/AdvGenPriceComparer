import re

# Read the file
with open('data/html/price_comparison_report_20251014.html', 'r', encoding='utf-8') as f:
    content = f.read()

# Update title date: 2025-10-14 -> 2025-10-15
content = re.sub(
    r'<title>Price Comparison Report \| Coles vs Woolworths \| 2025-10-14</title>',
    '<title>Price Comparison Report | Coles vs Woolworths | 2025-10-15</title>',
    content
)

# Update Last Week link: price_comparison_report_20250924.html -> price_super_market_08102025.html
content = re.sub(
    r'<a href="price_comparison_report_20250924\.html">Last Week</a>',
    '<a href="price_super_market_08102025.html">Last Week</a>',
    content
)

# Update week selector previous week button: 24-30 Sept 2025 -> 08-14 Oct 2025
# and link: price_comparison_report_20250924.html -> price_super_market_08102025.html
content = re.sub(
    r'<a href="price_comparison_report_20250924\.html" class="week-btn">24-30 Sept 2025</a>',
    '<a href="price_super_market_08102025.html" class="week-btn">08-14 Oct 2025</a>',
    content
)

# Update current week button: 01-07 Oct 2025 -> 15-21 Oct 2025
content = re.sub(
    r'<a href="#" class="week-btn current">01-07 Oct 2025</a>',
    '<a href="#" class="week-btn current">15-21 Oct 2025</a>',
    content
)

# Update footer data sourced text: Week 1-7 Oct 2025 -> Week 15-21 Oct 2025
content = re.sub(
    r'Data sourced from official Coles and Woolworths catalogues - Week 1-7 Oct 2025',
    'Data sourced from official Coles and Woolworths catalogues - Week 15-21 Oct 2025',
    content
)

# Save the updated file
with open('data/html/price_comparison_report_20251014.html', 'w', encoding='utf-8') as f:
    f.write(content)

print("Updated price_comparison_report_20251014.html with corrected dates:")
print("  - Title date: 2025-10-15")
print("  - Last Week link: price_super_market_08102025.html")
print("  - Previous week: 08-14 Oct 2025")
print("  - Current week: 15-21 Oct 2025")
print("  - Footer: Week 15-21 Oct 2025")
