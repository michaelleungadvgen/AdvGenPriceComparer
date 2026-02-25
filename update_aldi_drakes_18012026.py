#!/usr/bin/env python3
"""Update liveinbne_deal.html with ALDI and Drakes content from markdown files"""

import re

# Read the markdown files
with open('drakes.md', 'r', encoding='utf-8') as f:
    drakes_content = f.read()

with open('aldi.md', 'r', encoding='utf-8') as f:
    aldi_content = f.read()

# Read the HTML file
with open('liveinbne_deal.html', 'r', encoding='utf-8') as f:
    html_content = f.read()

# Parse Drakes markdown and create HTML
drakes_html = []
drakes_html.append('                <div style="background: linear-gradient(135deg, #ff6b6b, #ee5a6f); color: white; padding: 1.5rem; border-radius: 10px; margin: 1.5rem 2rem; text-align: center;">')
drakes_html.append('                    <h3 style="margin: 0 0 0.5rem 0; font-size: 1.3rem;">🎁 Peanuts Socks Collection!</h3>')
drakes_html.append('                    <p style="margin: 0; font-size: 0.95rem;">Collect one pair with every $60 you spend! Plus enter to win a family holiday | Valid: Tue 14 Jan - Mon 20 Jan 2026</p>')
drakes_html.append('                </div>')
drakes_html.append('                <div class="deals-grid">')

# Parse drakes.md categories
current_category = ""
lines = drakes_content.split('\n')
for i, line in enumerate(lines):
    if line.startswith('## '):
        current_category = line.replace('## ', '').strip()
    elif line.startswith('### '):
        current_category = line.replace('### ', '').strip()
    elif line.startswith('- **'):
        # Extract product details
        match = re.match(r'- \*\*(.+?)\*\* - \$(\d+\.\d+)(.*?)(?:\(SAVE \$(\d+\.\d+)\))?', line)
        if match:
            product_name = match.group(1)
            price = match.group(2)
            extra_info = match.group(3).strip()
            savings = match.group(4)

            # Clean up product name
            product_name = product_name.replace(' ea', '').strip()

            drakes_html.append('                    <div class="deal-card">')
            drakes_html.append(f'                        <div class="store-badge drakes">DRAKES - {current_category.upper()}</div>')
            drakes_html.append(f'                        <div class="product-name">{product_name}</div>')
            drakes_html.append('                        <div class="price-info">')
            drakes_html.append(f'                            <span class="current-price">${price}</span>')
            drakes_html.append('                        </div>')
            if savings:
                drakes_html.append(f'                        <div class="savings">Save ${savings}</div>')
                drakes_html.append('                        <div class="deal-badge best-deal">GREAT VALUE</div>')
            drakes_html.append('                    </div>')

drakes_html.append('                ')

# Parse ALDI markdown and create HTML
aldi_html = []
aldi_html.append('                <div class="deals-grid">')

current_section = ""
current_category = ""
lines = aldi_content.split('\n')

for i, line in enumerate(lines):
    if line.startswith('## Available from '):
        current_section = line.replace('## Available from ', '').strip()
    elif line.startswith('### '):
        current_category = line.replace('### ', '').strip()
    elif line.startswith('#### '):
        current_category = line.replace('#### ', '').strip()
    elif line.startswith('- **'):
        # Extract ALDI product details
        match = re.match(r'- \*\*(.+?)\*\*(?: - (.+?))? - \$(\d+\.\d+)(.*)', line)
        if match:
            product_name = match.group(1)
            brand = match.group(2) if match.group(2) else ""
            price = match.group(3)
            extra_info = match.group(4).strip()

            # Extract unit price if present
            unit_price_match = re.search(r'\((\$[\d.]+\s+per\s+[\w\s]+)\)', extra_info)
            unit_price = unit_price_match.group(1) if unit_price_match else ""

            # Full product name with brand
            if brand:
                full_name = f"{product_name} - {brand}"
            else:
                full_name = product_name

            aldi_html.append('                    <div class="deal-card">')
            aldi_html.append(f'                        <div class="store-badge aldi">ALDI - {current_category.upper()}</div>')
            aldi_html.append(f'                        <div class="product-name">{full_name}</div>')
            aldi_html.append('                        <div class="price-info">')
            aldi_html.append(f'                            <span class="current-price">${price}</span>')
            aldi_html.append('                        </div>')
            if unit_price:
                aldi_html.append(f'                        <div class="unit-price">{unit_price}</div>')
            aldi_html.append('                        <div class="deal-badge best-deal">SPECIAL BUY</div>')
            if 'While Stocks Last' in extra_info:
                aldi_html.append('                        <div class="availability">While Stocks Last</div>')
            aldi_html.append('                    </div>')

aldi_html.append('                ')

# Replace Drakes section in HTML
drakes_pattern = r'(<!-- Drakes Supermarket Specials -->.*?<div class="category-header".*?onclick="toggleCategory\(\'drakes-special\'\)">.*?<div class="category-title">.*?<span class="category-icon">🛒</span>\s*)Drakes Supermarket - .*?(\</div>.*?<span class="expand-icon".*?</span>.*?</div>.*?<div class="category-content".*?id="drakes-special-content">)(.*?)(</div>\s*</div>\s*</div>)'
drakes_replacement = r'\1Drakes Supermarket - 14 Jan - 20 Jan 2026\2\n' + '\n'.join(drakes_html) + '\n            '
html_content = re.sub(drakes_pattern, drakes_replacement, html_content, flags=re.DOTALL)

# Replace ALDI section in HTML
aldi_pattern = r'(<!-- ALDI Special Buys -->.*?<div class="category-header".*?onclick="toggleCategory\(\'aldi\'\)">.*?<div class="category-title">.*?<span class="category-icon">🏪</span>\s*)ALDI Special Buys - .*?(\</div>.*?<span class="expand-icon".*?</span>.*?</div>.*?<div class="category-content".*?id="aldi-content">)(.*?)(</div>\s*</div>\s*</div>)'
aldi_replacement = r'\1ALDI Special Buys - Sat 17th, Wed 21st & Sat 24th Jan 2026\2\n' + '\n'.join(aldi_html) + '\n            '
html_content = re.sub(aldi_pattern, aldi_replacement, html_content, flags=re.DOTALL)

# Write the updated HTML
with open('liveinbne_deal.html', 'w', encoding='utf-8') as f:
    f.write(html_content)

print("Updated liveinbne_deal.html with content from drakes.md and aldi.md")
print(f"Drakes section: 14-20 Jan 2026")
print(f"ALDI section: Sat 17th, Wed 21st & Sat 24th Jan 2026")
