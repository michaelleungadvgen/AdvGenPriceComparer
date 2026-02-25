#!/usr/bin/env python3
"""
Update liveinbne_deal.html with latest price comparison data
"""

import json
import re
from datetime import datetime

def load_comparison_data():
    """Load the price comparison JSON data"""
    with open('price_comparison_data.json', 'r', encoding='utf-8') as f:
        return json.load(f)

def create_deal_card_html(product, show_comparison=False):
    """Create HTML for a single deal card"""
    store = product['store']
    name = product['name']
    description = product.get('description', '')
    pricing = product['pricing']

    # Combine name and description
    full_name = f"{name} {description}".strip() if description else name

    # Determine store badge class
    store_class = store.lower()

    # Determine deal badge
    deal_badge = ""
    deal_badge_class = ""

    if pricing['discount_percentage'] >= 50:
        deal_badge = "HALF PRICE" if pricing['discount_percentage'] == 50 else f"{int(pricing['discount_percentage'])}% OFF"
        deal_badge_class = "half-price"
    elif pricing['discount_percentage'] >= 40:
        deal_badge = f"{int(pricing['discount_percentage'])}% OFF"
        deal_badge_class = "best-deal"
    elif pricing['discount_percentage'] > 0:
        deal_badge = "GOOD VALUE"
        deal_badge_class = "better-value"
    else:
        deal_badge = "REGULAR PRICE"
        deal_badge_class = "better-value"

    # Build HTML
    html = f"""                    <div class="deal-card">
                        <div class="store-badge {store_class}">{store.upper()}</div>
                        <div class="product-name">{full_name}</div>
                        <div class="price-info">
                            <span class="current-price">{pricing['current_price_formatted']}</span>"""

    if pricing['original_price'] and pricing['original_price'] > 0:
        html += f"""
                            <span class="original-price">{pricing['original_price_formatted']}</span>"""

    if pricing['savings_text']:
        html += f"""
                            <span class="savings">Save {pricing['savings_text']}</span>"""

    html += f"""
                        </div>
                        <div class="deal-badge {deal_badge_class}">{deal_badge}</div>
                    </div>
"""

    return html

def update_html_category(html_content, category_name, category_id, products_data):
    """Update a specific category section in the HTML"""

    # Get products from both stores
    coles_products = products_data.get('coles', [])
    woolworths_products = products_data.get('woolworths', [])

    # Create deal cards HTML
    deals_html = ""

    # Add products alternating between stores for better comparison
    max_len = max(len(coles_products), len(woolworths_products))

    for i in range(max_len):
        if i < len(coles_products):
            deals_html += create_deal_card_html(coles_products[i])
        if i < len(woolworths_products):
            deals_html += create_deal_card_html(woolworths_products[i])

    # Find and replace the category content
    pattern = rf'(<div class="category-header" onclick="toggleCategory\(\'{category_id}\'\)">.*?</div>\s*<div class="category-content" id="{category_id}-content">\s*<div class="deals-grid">)(.*?)(</div>\s*</div>\s*</div>)'

    replacement = rf'\1\n{deals_html}                \3'

    updated_html = re.sub(pattern, replacement, html_content, flags=re.DOTALL)

    return updated_html

def main():
    """Main function to update the HTML file"""
    print("Loading price comparison data...")
    data = load_comparison_data()

    print("Reading HTML file...")
    with open('liveinbne_deal.html', 'r', encoding='utf-8') as f:
        html_content = f.read()

    # Update date in header
    html_content = re.sub(
        r'<div class="date-badge">.*?</div>',
        '<div class="date-badge">05 November - 12 November 2025</div>',
        html_content
    )

    # Categories to update
    categories_to_update = {
        'Premium Chocolate': 'chocolate',
        'Soft Drinks & Beverages': 'beverages',
    }

    print("Updating categories...")
    for category_name, category_id in categories_to_update.items():
        if category_name in data['categories']:
            print(f"  - Updating {category_name}...")
            category_data = data['categories'][category_name]
            html_content = update_html_category(
                html_content,
                category_name,
                category_id,
                category_data['products']
            )

    print("Writing updated HTML file...")
    with open('liveinbne_deal.html', 'w', encoding='utf-8') as f:
        f.write(html_content)

    print("Done! HTML file has been updated.")

if __name__ == '__main__':
    main()
