import re

# Read drakes.md
with open('drakes.md', 'r', encoding='utf-8') as f:
    drakes_content = f.read()

# Function to convert category name to HTML class-friendly format
def categorize(category):
    category_upper = category.upper()

    # Map categories to store badge categories
    category_map = {
        'FRONT PAGE DEALS': 'FEATURED DEALS',
        'PANTRY STAPLES': 'PANTRY',
        'SNACKS': 'CHIPS & SNACKS',
        'CONFECTIONERY': 'CONFECTIONERY',
        'BREAKFAST': 'BREAKFAST & CEREAL',
        'COFFEE & TEA': 'BEVERAGES',
        'DELI MEATS': 'DELI',
        'FRESH CHICKEN': 'MEAT & POULTRY',
        'FRESH BEEF': 'MEAT & POULTRY',
        'PORK & LAMB': 'MEAT & POULTRY',
        'SAUSAGES & PROCESSED MEATS': 'MEAT & POULTRY',
        'FRESH SEAFOOD': 'SEAFOOD',
        'FRUITS': 'FRESH PRODUCE',
        'VEGETABLES': 'FRESH PRODUCE',
        'DAIRY & EGGS': 'DAIRY & CHILLED',
        'CHILLED PRODUCTS': 'DAIRY & CHILLED',
        'FROZEN MEALS': 'FROZEN FOODS',
        'ICE CREAM & DESSERTS': 'FROZEN FOODS',
        'FROZEN SEAFOOD & VEGETABLES': 'FROZEN FOODS',
        'CLEANING PRODUCTS': 'HOUSEHOLD & CLEANING',
        'HOUSEHOLD ITEMS': 'HOUSEHOLD & CLEANING',
        'LAUNDRY': 'HOUSEHOLD & CLEANING',
        'PET FOOD': 'BIRD & PET FOOD',
        'PERSONAL CARE': 'HEALTH & BEAUTY',
        'STATIONERY': 'STATIONERY & OFFICE'
    }

    return category_map.get(category_upper, category_upper)

# Parse drakes.md and generate HTML
html_cards = []
current_category = ""
valid_dates = ""

lines = drakes_content.split('\n')
for line in lines:
    line = line.strip()

    # Skip empty lines and horizontal rules
    if not line or line == '---':
        continue

    # Extract valid dates
    if line.startswith('**Valid:'):
        valid_dates = line.replace('**Valid:', '').replace('**', '').strip()
        continue

    # Skip the promotion header and title
    if line.startswith('#') or line.startswith('**Special Promotion'):
        continue

    # Main category headers (## Groceries, ## Fresh Meat & Seafood, etc.)
    if line.startswith('## '):
        continue

    # Sub-category headers (### Snacks, ### Deli Meats, etc.)
    if line.startswith('### '):
        current_category = categorize(line.replace('### ', '').strip())
        continue

    # Product lines
    if line.startswith('- **'):
        # Parse product line
        # Format: - **Product Name Size** - $Price ea/kg/etc.
        match = re.match(r'- \*\*(.+?)\*\* (?:(.+?) )?- \$([0-9.]+) (.+)', line)
        if match:
            product_name = match.group(1).strip()
            size_or_extra = match.group(2).strip() if match.group(2) else ""
            price = match.group(3).strip()
            unit = match.group(4).strip()

            # Combine product name with size if available
            full_product_name = f"{product_name} {size_or_extra}".strip()

            # Create HTML card
            card = f'''                    <div class="deal-card">
                        <div class="store-badge drakes">DRAKES - {current_category}</div>
                        <div class="product-name">{full_product_name}</div>
                        <div class="price-info">
                            <span class="current-price">${price}</span>
                        </div>'''

            # Add unit if it's not just "ea"
            if unit and unit != "ea":
                card += f'''
                        <div class="unit-price">{unit}</div>'''

            card += '''
                    </div>'''

            html_cards.append(card)

# Read the original HTML file
with open('liveinbne_deal.html', 'r', encoding='utf-8') as f:
    html_lines = f.readlines()

# Build new content
# Drakes section starts at line 655 (index 654)
# The deals-grid starts at line 668 (index 667)
# The section ends at line 2064 (index 2063)

new_html = []
new_html.extend(html_lines[:668])  # Keep everything up to and including <div class="deals-grid">

# Add all the new deal cards
for card in html_cards:
    new_html.append(card + '\n')

# Add closing divs and rest of file
new_html.extend(html_lines[2063:])  # From line 2064 onwards (the closing </div></div>)

# Update the header to reflect new dates
if valid_dates:
    # Extract start and end dates from valid_dates
    # Format: "21 January 2026 - 27 January 2026"
    date_match = re.match(r'(\d+) (\w+) (\d+) - (\d+) (\w+) (\d+)', valid_dates)
    if date_match:
        start_day = date_match.group(1)
        start_month = date_match.group(2)[:3]  # Get first 3 letters
        end_day = date_match.group(4)
        end_month = date_match.group(5)[:3]
        year = date_match.group(6)

        header_date = f"{start_day} {start_month} - {end_day} {end_month} {year}"

        for i in range(len(new_html)):
            if 'Drakes Supermarket - ' in new_html[i] and 'category-title' in new_html[i-1]:
                new_html[i] = new_html[i].replace(
                    re.search(r'Drakes Supermarket - [^<]+', new_html[i]).group(0),
                    f'Drakes Supermarket - {header_date}'
                )
                break

# Update the Peanuts Socks promotion text
for i in range(len(new_html)):
    if 'Peanuts Socks Collection' in new_html[i]:
        # Update the next line with the correct dates
        if i+1 < len(new_html) and valid_dates:
            new_html[i+1] = f'                    <p style="margin: 0; font-size: 0.95rem;">Collect one pair with every $60 you spend! | Valid: {valid_dates}</p>\n'
        break

# Write the updated HTML
with open('liveinbne_deal.html', 'w', encoding='utf-8') as f:
    f.writelines(new_html)

print(f"Updated liveinbne_deal.html with {len(html_cards)} Drakes products")
print(f"Valid dates: {valid_dates}")
