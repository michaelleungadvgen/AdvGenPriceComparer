import json
import re

# Read Coles catalogue
with open('C:/Users/advgen10/source/repos/AdvGenPriceComparer/data/cloes_26112025.json', 'r', encoding='utf-8') as f:
    coles_data = json.load(f)

# Read Woolworths catalogue
with open('C:/Users/advgen10/source/repos/AdvGenPriceComparer/data/woolworths_26112025.json', 'r', encoding='utf-8') as f:
    woolworths_data = json.load(f)

# Extract top 6 Coles deals (half price or big savings)
coles_deals = []
for product in coles_data:
    if 'savings' in product and product.get('savings') and product.get('originalPrice'):
        try:
            savings_value = float(product['savings'].replace('$', '').replace(',', ''))
            if savings_value >= 4.00:  # Minimum $4 savings
                coles_deals.append(product)
        except:
            pass

coles_deals = sorted(coles_deals, key=lambda x: float(x.get('savings', '$0').replace('$', '').replace(',', '')), reverse=True)[:6]

# Extract top 6 Woolworths deals
woolworths_deals = []
for product in woolworths_data:
    if 'savings' in product and product.get('savings') and product.get('originalPrice'):
        try:
            savings_value = float(product['savings'].replace('$', '').replace(',', ''))
            if savings_value >= 4.00:  # Minimum $4 savings
                woolworths_deals.append(product)
        except:
            pass

woolworths_deals = sorted(woolworths_deals, key=lambda x: float(x.get('savings', '$0').replace('$', '').replace(',', '')), reverse=True)[:6]

# Generate Coles HTML
coles_html = ''
for deal in coles_deals:
    try:
        price_val = float(deal['price'].replace('$', ''))
        orig_val = float(deal['originalPrice'].replace('$', ''))
        badge = 'HALF PRICE' if abs(price_val - orig_val/2) < 0.01 else 'GREAT VALUE'
    except:
        badge = 'GREAT VALUE'

    coles_html += f'''                    <div class="deal-card">
                        <div class="store-badge coles">COLES - {deal['category'].upper()}</div>
                        <div class="product-name">{deal['productName']}</div>
                        <div class="price-info">
                            <span class="current-price">{deal['price']}</span>
                            <span class="original-price">{deal['originalPrice']}</span>
                        </div>
                        <div class="savings">Save {deal['savings']}</div>
                        <div class="unit-price">{deal.get('description', 'each')}</div>
                        <div class="deal-badge half-price">{badge}</div>
                    </div>
'''

# Generate Woolworths HTML
woolworths_html = ''
for deal in woolworths_deals:
    try:
        price_val = float(deal['price'].replace('$', ''))
        orig_val = float(deal['originalPrice'].replace('$', ''))
        badge = 'HALF PRICE' if abs(price_val - orig_val/2) < 0.01 else 'GREAT VALUE'
    except:
        badge = 'GREAT VALUE'

    woolworths_html += f'''                    <div class="deal-card">
                        <div class="store-badge woolworths">WOOLWORTHS - {deal['category'].upper()}</div>
                        <div class="product-name">{deal['productName']}</div>
                        <div class="price-info">
                            <span class="current-price">{deal['price']}</span>
                            <span class="original-price">{deal['originalPrice']}</span>
                        </div>
                        <div class="savings">Save {deal['savings']}</div>
                        <div class="unit-price">{deal.get('description', 'each')}</div>
                        <div class="deal-badge half-price">{badge}</div>
                    </div>
'''

# Drakes HTML (using the single Milo product)
drakes_html = '''                    <div class="deal-card">
                        <div class="store-badge drakes">DRAKES - BEVERAGES</div>
                        <div class="product-name">Milo Restore 440g</div>
                        <div class="price-info">
                            <span class="current-price">$8.40</span>
                        </div>
                        <div class="savings">Save $3.60</div>
                        <div class="deal-badge best-deal">GREAT VALUE</div>
                    </div>
'''

# Read liveinbne_deal.html
with open('C:/Users/advgen10/source/repos/AdvGenPriceComparer/liveinbne_deal.html', 'r', encoding='utf-8') as f:
    content = f.read()

# Update Coles section
coles_pattern = r'(<div class="category-header" onclick="toggleCategory\(\'coles-special\'\)">.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*<!-- Woolworths)'
content = re.sub(coles_pattern, r'\1\n' + coles_html + '                \3', content, flags=re.DOTALL)

# Update Woolworths section
woolworths_pattern = r'(<div class="category-header" onclick="toggleCategory\(\'woolworths-special\'\)">.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*<!-- Drakes)'
content = re.sub(woolworths_pattern, r'\1\n' + woolworths_html + '                \3', content, flags=re.DOTALL)

# Update Drakes section
drakes_pattern = r'(<div class="category-header" onclick="toggleCategory\(\'drakes-special\'\)">.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*</div>)'
content = re.sub(drakes_pattern, r'\1\n' + drakes_html + '                \3', content, flags=re.DOTALL)

# Update dates in headers
content = re.sub(r'Coles - \d+ \w+ - \d+ \w+ \d{4}', 'Coles - 26 Nov - 2 Dec 2025', content)
content = re.sub(r'Woolworths - \d+ \w+ - \d+ \w+ \d{4}', 'Woolworths - 26 Nov - 2 Dec 2025', content)
content = re.sub(r'Drakes Supermarket - \d+ \w+ - \d+ \w+ \d{4}', 'Drakes Supermarket - 26 Nov - 2 Dec 2025', content)

# Update date badge
content = re.sub(r'\d+ \w+ - \d+ \w+ \d{4}', '26 November - 2 December 2025', content, count=1)

# Write updated content
with open('C:/Users/advgen10/source/repos/AdvGenPriceComparer/liveinbne_deal.html', 'w', encoding='utf-8') as f:
    f.write(content)

print('Updated liveinbne_deal.html successfully')
print(f'Coles: {len(coles_deals)} deals')
print(f'Woolworths: {len(woolworths_deals)} deals')
print('Drakes: 1 deal')
