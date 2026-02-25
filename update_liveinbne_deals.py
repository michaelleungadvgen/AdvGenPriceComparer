import json
import re

# Read the JSON files
with open('data/coles_12112025.json', 'r', encoding='utf-8') as f:
    coles_data = json.load(f)

with open('data/woolworths_12112025.json', 'r', encoding='utf-8') as f:
    woolworths_data = json.load(f)

# Read the current HTML file
with open('liveinbne_deal.html', 'r', encoding='utf-8') as f:
    html_content = f.read()

# Function to generate deal card HTML
def generate_deal_card(store, category, product):
    store_class = store.lower()
    product_name = product.get('productName', '')
    current_price = (product.get('price') or '').replace('$', '')
    was_price = (product.get('originalPrice') or '').replace('$', '')
    save_price = (product.get('savings') or '').replace('$', '')

    # Calculate savings
    savings_html = ''
    deal_badge = 'better-value">REGULAR PRICE'

    if was_price and save_price:
        savings_html = f'<span class="original-price">${was_price}</span>\n                            <span class="savings">Save ${save_price}</span>'

        # Determine deal type
        try:
            was_val = float(was_price)
            current_val = float(current_price)
            if current_val <= was_val / 2:
                deal_badge = 'half-price">HALF PRICE'
            else:
                deal_badge = 'best-deal">GOOD VALUE'
        except:
            deal_badge = 'best-deal">GOOD VALUE'

    card = f'''                    <div class="deal-card">
                        <div class="store-badge {store_class}">{store.upper()}</div>
                        <div class="product-name">{product_name}</div>
                        <div class="price-info">
                            <span class="current-price">${current_price}</span>
                            {savings_html}
                        </div>
                        <div class="deal-badge {deal_badge}</div>
                    </div>'''

    return card

# Find and extract "Premium Chocolate" section products from both stores
print("Processing Premium Chocolate section...")
chocolate_cards = []

# Get chocolate products from Coles
for product in coles_data:
    name_lower = product.get('productName', '').lower()
    if any(keyword in name_lower for keyword in ['chocolate', 'cadbury', 'lindt', 'toblerone', 'ferrero', 'kit kat', 'mars', 'snickers']):
        chocolate_cards.append(generate_deal_card('COLES', 'Premium Chocolate', product))
        if len(chocolate_cards) >= 20:
            break

# Get chocolate products from Woolworths
for product in woolworths_data:
    name_lower = product.get('productName', '').lower()
    if any(keyword in name_lower for keyword in ['chocolate', 'cadbury', 'lindt', 'toblerone', 'ferrero', 'kit kat', 'mars', 'snickers']):
        chocolate_cards.append(generate_deal_card('WOOLWORTHS', 'Premium Chocolate', product))
        if len(chocolate_cards) >= 40:
            break

# Find and extract "Soft Drinks & Beverages" section products
print("Processing Soft Drinks & Beverages section...")
beverage_cards = []

# Get beverage products from Coles
for product in coles_data:
    name_lower = product.get('productName', '').lower()
    if any(keyword in name_lower for keyword in ['coca', 'pepsi', 'sprite', 'fanta', 'drink', 'water', 'juice', 'coffee', 'tea', 'milk', 'energy']):
        beverage_cards.append(generate_deal_card('COLES', 'Beverages', product))
        if len(beverage_cards) >= 20:
            break

# Get beverage products from Woolworths
for product in woolworths_data:
    name_lower = product.get('productName', '').lower()
    if any(keyword in name_lower for keyword in ['coca', 'pepsi', 'sprite', 'fanta', 'drink', 'water', 'juice', 'coffee', 'tea', 'milk', 'energy']):
        beverage_cards.append(generate_deal_card('WOOLWORTHS', 'Beverages', product))
        if len(beverage_cards) >= 40:
            break

# Generate the combined HTML for chocolate section
chocolate_html = '\n'.join(chocolate_cards[:40])

# Generate the combined HTML for beverages section
beverages_html = '\n'.join(beverage_cards[:40])

print(f"\nGenerated {len(chocolate_cards[:40])} chocolate deals")
print(f"Generated {len(beverage_cards[:40])} beverage deals")

# Find and replace the chocolate section
chocolate_pattern = r'(<div class="category-content" id="chocolate-content">.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*</div>\s*</div>\s*</div>.*?<!-- Soft Drinks)'
match = re.search(chocolate_pattern, html_content, re.DOTALL)

if match:
    new_chocolate_section = match.group(1) + '\n' + chocolate_html + '\n                ' + match.group(3)
    html_content = html_content[:match.start()] + new_chocolate_section + html_content[match.end():]
    print("Updated Premium Chocolate section")
else:
    print("Could not find Premium Chocolate section pattern")

# Find and replace the beverages section
beverages_pattern = r'(<div class="category-content" id="beverages-content">.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*</div>\s*</div>\s*</div>.*?<!-- Drakes)'
match = re.search(beverages_pattern, html_content, re.DOTALL)

if match:
    new_beverages_section = match.group(1) + '\n' + beverages_html + '\n                ' + match.group(3)
    html_content = html_content[:match.start()] + new_beverages_section + html_content[match.end():]
    print("Updated Soft Drinks & Beverages section")
else:
    print("Could not find Soft Drinks & Beverages section pattern")

# Write the updated HTML
with open('liveinbne_deal.html', 'w', encoding='utf-8') as f:
    f.write(html_content)

print("\nSuccessfully updated liveinbne_deal.html")
