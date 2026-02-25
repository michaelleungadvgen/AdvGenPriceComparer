#!/usr/bin/env python3
"""Update liveinbne_deal.html with Coles and Woolworths deals from 7 Jan 2026"""

import json
import re

# Read JSON files
with open('data/coles_14012026.json', 'r', encoding='utf-8') as f:
    coles_data = json.load(f)

with open('data/woolworths_14012026.json', 'r', encoding='utf-8') as f:
    woolworths_data = json.load(f)

# Categorize products
categories = {
    'chocolate': [],
    'beverages': [],
    'health': [],
    'snacks': [],
    'frozen': []
}

# Categorize Coles products
for product in coles_data:
    name_lower = product['productName'].lower()
    cat = product.get('category', '').lower()

    if any(word in name_lower for word in ['chocolate', 'cadbury', 'lindt', 'ferrero', 'mars', 'snickers', 'kitkat', 'toblerone']):
        categories['chocolate'].append(('coles', product))
    elif any(word in name_lower for word in ['cola', 'pepsi', 'drink', 'juice', 'water', 'energy', 'soft drink', 'soda']):
        categories['beverages'].append(('coles', product))
    elif 'health' in cat or any(word in name_lower for word in ['vitamin', 'supplement', 'probiotic', 'fish oil']):
        categories['health'].append(('coles', product))
    elif any(word in name_lower for word in ['ice cream', 'frozen', 'drumstick', 'weis', 'cornetto']):
        categories['frozen'].append(('coles', product))
    elif any(word in name_lower for word in ['chips', 'crisp', 'pringles', 'snack']):
        categories['snacks'].append(('coles', product))

# Categorize Woolworths products
for product in woolworths_data:
    name_lower = product['productName'].lower()
    cat = product.get('category', '').lower()

    if any(word in name_lower for word in ['chocolate', 'cadbury', 'lindt', 'ferrero', 'mars', 'snickers', 'kitkat', 'toblerone']):
        categories['chocolate'].append(('woolworths', product))
    elif any(word in name_lower for word in ['cola', 'pepsi', 'drink', 'juice', 'water', 'energy', 'soft drink', 'soda']):
        categories['beverages'].append(('woolworths', product))
    elif 'health' in cat or any(word in name_lower for word in ['vitamin', 'supplement', 'probiotic', 'fish oil']):
        categories['health'].append(('woolworths', product))
    elif any(word in name_lower for word in ['ice cream', 'frozen', 'drumstick', 'weis', 'cornetto']):
        categories['frozen'].append(('woolworths', product))
    elif any(word in name_lower for word in ['chips', 'crisp', 'pringles', 'snack']):
        categories['snacks'].append(('woolworths', product))

def generate_deal_card(store, product):
    """Generate HTML for a deal card"""
    store_class = store.lower()
    store_display = store.upper()

    # Determine category display name
    name_lower = product['productName'].lower()
    if 'chocolate' in name_lower or any(x in name_lower for x in ['cadbury', 'lindt', 'ferrero']):
        cat_display = 'CHOCOLATE'
    elif 'drink' in name_lower or 'cola' in name_lower or 'juice' in name_lower:
        cat_display = 'BEVERAGES'
    elif 'vitamin' in name_lower or 'supplement' in name_lower:
        cat_display = 'HEALTH'
    elif 'ice cream' in name_lower or 'frozen' in name_lower:
        cat_display = 'FROZEN DESSERTS'
    elif 'chips' in name_lower or 'pringles' in name_lower:
        cat_display = 'SNACKS'
    else:
        cat_display = product.get('category', 'GENERAL').upper()

    savings = product.get('savings', '')
    special_type = product.get('specialType', '')

    # Build savings HTML
    savings_html = ''
    if savings and savings != 'N/A':
        savings_html = f'\n                        <div class="savings">Save {savings}</div>'

    # Build special badge HTML
    badge_html = ''
    if special_type:
        if 'Half Price' in special_type or 'HALF_PRICE' in special_type:
            badge_html = '\n                        <div class="deal-badge best-deal">HALF PRICE</div>'
        elif 'Low' in special_type:
            badge_html = '\n                        <div class="deal-badge hot-deal">PRICES DROPPED</div>'
        else:
            badge_html = f'\n                        <div class="deal-badge hot-deal">{special_type}</div>'

    # Build unit price HTML
    unit_price_html = ''
    if 'unitPrice' in product and product['unitPrice']:
        unit_price_html = f'\n                        <div class="unit-price">{product["unitPrice"]}</div>'

    card_html = f'''                    <div class="deal-card">
                        <div class="store-badge {store_class}">{store_display} - {cat_display}</div>
                        <div class="product-name">{product['productName']}</div>
                        <div class="price-info">
                            <span class="current-price">{product['price']}</span>
                        </div>{unit_price_html}{savings_html}{badge_html}
                    </div>'''

    return card_html

# Generate HTML sections
chocolate_cards = [generate_deal_card(store, prod) for store, prod in categories['chocolate'][:20]]
beverages_cards = [generate_deal_card(store, prod) for store, prod in categories['beverages'][:20]]

chocolate_html = '\n'.join(chocolate_cards)
beverages_html = '\n'.join(beverages_cards)

# Read current HTML file
with open('liveinbne_deal.html', 'r', encoding='utf-8') as f:
    html_content = f.read()

# Update Premium Chocolate section
chocolate_pattern = r'(<div class="category-header" onclick="toggleCategory\(\'chocolate\'\)">.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*</div>\s*<!-- Soft Drinks)'
chocolate_match = re.search(chocolate_pattern, html_content, re.DOTALL)

if chocolate_match:
    before = chocolate_match.group(1)
    after = chocolate_match.group(3)
    html_content = html_content.replace(
        chocolate_match.group(0),
        before + '\n' + chocolate_html + '\n                ' + after
    )
    print(f"Updated Premium Chocolate section with {len(categories['chocolate'][:20])} products")
else:
    print("Could not find Premium Chocolate section")

# Update Soft Drinks & Beverages section
beverages_pattern = r'(<div class="category-header" onclick="toggleCategory\(\'beverages\'\)">.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*</div>\s*<!-- Drakes)'
beverages_match = re.search(beverages_pattern, html_content, re.DOTALL)

if beverages_match:
    before = beverages_match.group(1)
    after = beverages_match.group(3)
    html_content = html_content.replace(
        beverages_match.group(0),
        before + '\n' + beverages_html + '\n                ' + after
    )
    print(f"Updated Soft Drinks & Beverages section with {len(categories['beverages'][:20])} products")
else:
    print("Could not find Soft Drinks & Beverages section")

# Write updated HTML
with open('liveinbne_deal.html', 'w', encoding='utf-8') as f:
    f.write(html_content)

print("\nSuccessfully updated liveinbne_deal.html with deals from 14 Jan 2026")
print(f"Total products categorized:")
print(f"  - Chocolate: {len(categories['chocolate'])}")
print(f"  - Beverages: {len(categories['beverages'])}")
print(f"  - Health: {len(categories['health'])}")
print(f"  - Frozen: {len(categories['frozen'])}")
print(f"  - Snacks: {len(categories['snacks'])}")
