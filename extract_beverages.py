import json

# Read the JSON files
with open('data/coles_19112025.json', 'r', encoding='utf-8') as f:
    coles_data = json.load(f)

with open('data/woolworths_19112025.json', 'r', encoding='utf-8') as f:
    woolworths_data = json.load(f)

# Get beverages
beverage_deals = []
beverage_keywords = ['coffee', 'tea', 'drink', 'cola', 'pepsi', 'water', 'juice', 'milk', 'powerade', 'lipton', 'schweppes', 'kirks', 'nescafe', 'moccona', 'sprite', 'fanta', 'solo', 'sunkist', 'gatorade', 'v energy', 'red bull', 'monster']

for item in coles_data:
    name_lower = item['productName'].lower()
    if any(keyword in name_lower for keyword in beverage_keywords):
        # Exclude items that are clearly not beverages
        if 'steak' not in name_lower and 'beef' not in name_lower and 'pork' not in name_lower and 'lamb' not in name_lower:
            beverage_deals.append({
                'store': 'COLES',
                'name': item['productName'],
                'price': item['price'].replace('$', ''),
                'originalPrice': item.get('originalPrice', '').replace('$', '') if item.get('originalPrice') else None,
                'savings': item.get('savings', '').replace('$', '') if item.get('savings') else None,
                'specialType': item.get('specialType', 'REGULAR')
            })

for item in woolworths_data:
    name_lower = item['productName'].lower()
    if any(keyword in name_lower for keyword in beverage_keywords):
        # Exclude items that are clearly not beverages
        if 'steak' not in name_lower and 'beef' not in name_lower and 'pork' not in name_lower and 'lamb' not in name_lower:
            beverage_deals.append({
                'store': 'WOOLWORTHS',
                'name': item['productName'],
                'price': item['price'].replace('$', ''),
                'originalPrice': item.get('originalPrice', '').replace('$', '') if item.get('originalPrice') else None,
                'savings': item.get('savings', '').replace('$', '') if item.get('savings') else None,
                'specialType': item.get('specialType', 'REGULAR')
            })

# Print top 20 beverage deals
print("=== BEVERAGES (Top 20) ===")
for deal in beverage_deals[:20]:
    savings_text = f"Save ${deal['savings']}" if deal['savings'] and deal['savings'] != '0.00' else ""
    original_price = f"${deal['originalPrice']}" if deal['originalPrice'] and deal['originalPrice'] != deal['price'] else ""

    badge = "HALF PRICE" if "HALF" in deal['specialType'] else "GOOD VALUE"

    print(f"""                    <div class="deal-card">
                        <div class="store-badge {'coles' if deal['store'] == 'COLES' else 'woolworths'}">{deal['store']}</div>
                        <div class="product-name">{deal['name']}</div>
                        <div class="price-info">
                            <span class="current-price">${deal['price']}</span>""")
    if original_price:
        print(f"""                            <span class="original-price">{original_price}</span>""")
    if savings_text:
        print(f"""                            <span class="savings">{savings_text}</span>""")
    print(f"""                        </div>
                        <div class="deal-badge {'half-price' if 'HALF' in deal['specialType'] else 'best-deal'}">{badge}</div>
                    </div>""")

print(f"\n\nTotal beverage deals found: {len(beverage_deals)}")
