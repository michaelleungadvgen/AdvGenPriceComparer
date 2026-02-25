import json
import re

# Read Coles and Woolworths JSON files
with open('data/coles_20012026.json', 'r', encoding='utf-8') as f:
    coles_products = json.load(f)

with open('data/woolworths_20012026.json', 'r', encoding='utf-8') as f:
    woolworths_products = json.load(f)

# Category mapping for HTML sections
category_mapping = {
    'chocolate': {
        'coles_categories': ['Pantry', 'Chocolate', 'Frozen'],
        'woolworths_categories': ['General', 'Frozen'],
        'keywords': ['chocolate', 'nutella', 'ferrero', 'cadbury', 'mars', 'snickers', 'maltesers', 'm&m', 'lindt', 'tim tam', 'kit kat', 'twix', 'bounty', 'milky way', 'crunchie', 'violet crumble', 'picnic', 'cherry ripe']
    },
    'beverages': {
        'coles_categories': ['Drinks', 'Pantry'],
        'woolworths_categories': ['General', 'Drinks'],
        'keywords': ['coca-cola', 'coke', 'pepsi', 'sprite', 'fanta', 'solo', 'kirks', 'schweppes', 'sunkist', 'mountain dew', 'soft drink', 'cordial', 'juice', 'lemonade', 'ginger ale', 'tonic']
    }
}

def matches_category(product, category_info, store_type):
    """Check if product matches category criteria"""
    product_name = product['productName'].lower()

    # Check category
    if store_type == 'coles':
        if product['category'] not in category_info['coles_categories']:
            # Check keywords as fallback
            if not any(keyword in product_name for keyword in category_info['keywords']):
                return False
    else:  # woolworths
        if product['category'] not in category_info['woolworths_categories']:
            # Check keywords as fallback
            if not any(keyword in product_name for keyword in category_info['keywords']):
                return False

    # Must match at least one keyword
    return any(keyword in product_name for keyword in category_info['keywords'])

def get_special_type_badge(special_type, savings):
    """Determine badge type based on special type and savings"""
    special_type_upper = str(special_type).upper()

    if 'HALF' in special_type_upper or 'HALF_PRICE' in special_type_upper:
        return 'HALF PRICE', 'best-deal'
    elif savings and savings != '$0.00':
        try:
            savings_amount = float(savings.replace('$', ''))
            if savings_amount > 0:
                return 'SPECIAL', 'hot-deal'
        except:
            pass

    return 'REGULAR', 'hot-deal'

def create_deal_card(product, store_name):
    """Create HTML deal card for a product"""
    store_badge_class = store_name.lower()
    category_display = product['category'].upper()

    # Get savings and special type
    savings = product.get('savings', '$0.00')
    special_type = product.get('specialType', 'REGULAR')
    badge_text, badge_class = get_special_type_badge(special_type, savings)

    # Build product name with description
    product_name = product['productName']
    if product.get('description') and product['description'] not in product_name:
        product_name = f"{product_name} {product['description']}"

    card = f'''                    <div class="deal-card">
                        <div class="store-badge {store_badge_class}">{store_name.upper()} - {category_display}</div>
                        <div class="product-name">{product_name}</div>
                        <div class="price-info">
                            <span class="current-price">{product['price']}</span>
                        </div>'''

    # Add savings if applicable
    if savings and savings != '$0.00':
        card += f'''
                        <div class="savings">Save {savings}</div>'''

    # Add badge
    card += f'''
                        <div class="deal-badge {badge_class}">{badge_text}</div>'''

    card += '''
                    </div>'''

    return card

# Generate HTML cards for each category
html_sections = {}

for category_name, category_info in category_mapping.items():
    cards = []

    # Add Coles products
    for product in coles_products:
        if matches_category(product, category_info, 'coles'):
            cards.append(create_deal_card(product, 'Coles'))

    # Add Woolworths products
    for product in woolworths_products:
        if matches_category(product, category_info, 'woolworths'):
            cards.append(create_deal_card(product, 'Woolworths'))

    html_sections[category_name] = cards

# Read the original HTML file
with open('liveinbne_deal.html', 'r', encoding='utf-8') as f:
    html_content = f.read()

# Function to replace section content
def replace_section(html, section_name, new_cards):
    """Replace deals-grid content in a section"""
    # Find the section by its ID
    section_id = f"{section_name}-content"

    # Pattern to match the deals-grid and its content
    pattern = rf'(<div class="category-content" id="{section_id}">[\s\S]*?<div class="deals-grid">)([\s\S]*?)(</div>\s*</div>\s*</div>)'

    # Build new cards HTML (can be empty)
    if new_cards:
        new_cards_html = '\n' + '\n'.join(new_cards) + '\n                '
    else:
        new_cards_html = '\n                '

    # Replace
    replacement = rf'\1{new_cards_html}\3'
    new_html = re.sub(pattern, replacement, html)

    return new_html

# Replace each section (even if empty)
updated_html = html_content

for section_name, cards in html_sections.items():
    print(f"Updating {section_name} section with {len(cards)} products")
    updated_html = replace_section(updated_html, section_name, cards)

# Write the updated HTML
with open('liveinbne_deal.html', 'w', encoding='utf-8') as f:
    f.write(updated_html)

print("\nSuccessfully updated liveinbne_deal.html")
print(f"- Premium Chocolate: {len(html_sections['chocolate'])} products")
print(f"- Soft Drinks & Beverages: {len(html_sections['beverages'])} products")
