import json
from collections import defaultdict

# Read the JSON files
with open('data/coles_19112025.json', 'r', encoding='utf-8') as f:
    coles_data = json.load(f)

with open('data/woolworths_19112025.json', 'r', encoding='utf-8') as f:
    woolworths_data = json.load(f)

# Organize by category
def organize_by_category(data, store_name):
    categories = defaultdict(list)
    for item in data:
        category = item.get('category', 'General')
        categories[category].append({
            'store': store_name,
            'name': item['productName'],
            'price': item['price'].replace('$', ''),
            'originalPrice': item.get('originalPrice', '').replace('$', '') if item.get('originalPrice') else None,
            'savings': item.get('savings', '').replace('$', '') if item.get('savings') else None,
            'specialType': item.get('specialType', 'REGULAR'),
            'description': item.get('description', '')
        })
    return categories

coles_categories = organize_by_category(coles_data, 'COLES')
woolworths_categories = organize_by_category(woolworths_data, 'WOOLWORTHS')

# Merge categories
all_categories = set(list(coles_categories.keys()) + list(woolworths_categories.keys()))

# Get top deals for Premium Chocolate category
chocolate_deals = []

# Add Coles chocolate deals
if 'Health & Beauty' in coles_categories:
    for item in coles_categories['Health & Beauty']:
        if 'chocolate' in item['name'].lower() or 'cadbury' in item['name'].lower() or 'ferrero' in item['name'].lower() or 'lindt' in item['name'].lower():
            chocolate_deals.append(item)

# Add Woolworths chocolate deals
if 'General' in woolworths_categories:
    for item in woolworths_categories['General']:
        if 'chocolate' in item['name'].lower() or 'cadbury' in item['name'].lower() or 'ferrero' in item['name'].lower() or 'lindt' in item['name'].lower():
            chocolate_deals.append(item)

# Get beverages
beverage_deals = []
beverage_keywords = ['coffee', 'tea', 'drink', 'cola', 'pepsi', 'water', 'juice', 'milk']

for categories_dict in [coles_categories, woolworths_categories]:
    for category, items in categories_dict.items():
        for item in items:
            if any(keyword in item['name'].lower() for keyword in beverage_keywords):
                if item not in beverage_deals:
                    beverage_deals.append(item)

# Print top 20 chocolate deals
print("=== PREMIUM CHOCOLATE (Top 20) ===")
for i, deal in enumerate(chocolate_deals[:20], 1):
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

print("\n\n=== SOFT DRINKS & BEVERAGES (Top 20) ===")
for i, deal in enumerate(beverage_deals[:20], 1):
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

print(f"\n\nTotal Coles deals: {len(coles_data)}")
print(f"Total Woolworths deals: {len(woolworths_data)}")
print(f"Total chocolate deals found: {len(chocolate_deals)}")
print(f"Total beverage deals found: {len(beverage_deals)}")
