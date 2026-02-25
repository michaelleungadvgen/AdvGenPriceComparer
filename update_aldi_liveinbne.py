import re

# Read aldi.md
with open('aldi.md', 'r', encoding='utf-8') as f:
    aldi_content = f.read()

# Function to convert category name to HTML class-friendly format
def categorize(category):
    category_upper = category.upper()

    # Map categories to store badge categories
    category_map = {
        'CHOCOLATE & SWEETS': 'CONFECTIONERY',
        'PANCAKE MAKING': 'PANTRY ESSENTIALS',
        'FRESH MEAT': 'MEAT',
        'CAST IRON': 'COOKWARE & KITCHEN',
        "WOMEN'S UNDERWEAR": "WOMEN'S CLOTHING",
        "MEN'S UNDERWEAR": "MEN'S CLOTHING",
        "CHILDREN'S BEDROOM": 'BEDROOM & HOME DECOR',
        'HOME DECOR': 'BEDROOM & HOME DECOR',
        'SNACKS & CONFECTIONERY': 'SNACKS & CHIPS',
        'INSTANT MEALS': 'PANTRY ESSENTIALS',
        'DESKS & CHAIRS': 'OFFICE FURNITURE',
        'OFFICE ACCESSORIES': 'OFFICE FURNITURE',
        'AUDIO & HEADSETS': 'TECHNOLOGY',
        'TECH ACCESSORIES': 'TECHNOLOGY',
        'LUNCH CONTAINERS & ACCESSORIES': 'LUNCH STORAGE',
        'CLEANING & MAINTENANCE': 'CLEANING PRODUCTS',
        'TVS & DISPLAYS': 'TECHNOLOGY',
        'TV ACCESSORIES': 'TECHNOLOGY',
        'NETWORKING & ELECTRONICS': 'TECHNOLOGY',
        'SNACKS & TREATS': 'SNACKS & CHIPS',
        'PANTRY ESSENTIALS': 'PANTRY ESSENTIALS',
        'MEAT SNACKS': 'SNACKS & CHIPS',
        'CLEANING APPLIANCES': 'CLEANING PRODUCTS'
    }

    return category_map.get(category_upper, category_upper)

# Parse aldi.md and generate HTML
html_cards = []
current_date_section = ""
current_category = ""

lines = aldi_content.split('\n')
for line in lines:
    line = line.strip()

    # Skip empty lines and horizontal rules
    if not line or line == '---' or line.startswith('*Store:'):
        continue

    # Date section headers
    if line.startswith('## Available from'):
        current_date_section = line.replace('## ', '').strip()
        continue

    # Skip main category headers (### Food & Confectionery, etc.)
    if line.startswith('### '):
        continue

    # Sub-category headers (#### Chocolate & Sweets, etc.)
    if line.startswith('#### '):
        current_category = categorize(line.replace('#### ', '').strip())
        continue

    # Product lines
    if line.startswith('- **'):
        # Parse product line
        # Format: - **Product Name Size** - BRAND - $Price (unit price optional)
        match = re.match(r'- \*\*(.+?)\*\* - (.+?) - \$([0-9.]+)(?: \((.+?)\))?', line)
        if match:
            product_name = match.group(1).strip()
            brand = match.group(2).strip()
            price = match.group(3).strip()
            unit_price = match.group(4).strip() if match.group(4) else None

            # Create HTML card
            card = f'''                    <div class="deal-card">
                        <div class="store-badge aldi">ALDI - {current_category}</div>
                        <div class="product-name">{product_name} - {brand}</div>
                        <div class="price-info">
                            <span class="current-price">${price}</span>
                        </div>'''

            if unit_price:
                card += f'''
                        <div class="unit-price">{unit_price}</div>'''

            card += '''
                        <div class="deal-badge best-deal">SPECIAL BUY</div>
                        <div class="availability">While Stocks Last</div>
                    </div>'''

            html_cards.append(card)

# Read the original HTML file
with open('liveinbne_deal.html', 'r', encoding='utf-8') as f:
    html_lines = f.readlines()

# Find the ALDI section and replace the deals-grid content
# The section starts at line 2067 (index 2066) and the deals-grid starts at line 2076 (index 2075)
# The section ends at line 3397 (index 3396)

# Build new content
new_html = []
new_html.extend(html_lines[:2076])  # Keep everything up to and including <div class="deals-grid">

# Add all the new deal cards
for card in html_cards:
    new_html.append(card + '\n')

# Add closing divs and rest of file
new_html.extend(html_lines[3396:])  # From line 3397 onwards (the closing </div></div>)

# Update the header to reflect new dates
for i in range(len(new_html)):
    if 'ALDI Special Buys - Sat 17th, Wed 21st & Sat 24th Jan 2026' in new_html[i]:
        new_html[i] = new_html[i].replace(
            'ALDI Special Buys - Sat 17th, Wed 21st & Sat 24th Jan 2026',
            'ALDI Special Buys - Sat 24th, Wed 28th & Sat 31st Jan 2026'
        )
        break

# Write the updated HTML
with open('liveinbne_deal.html', 'w', encoding='utf-8') as f:
    f.writelines(new_html)

print(f"Updated liveinbne_deal.html with {len(html_cards)} ALDI products")
