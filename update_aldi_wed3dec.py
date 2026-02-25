import re

# Selected products from ALDI Wed 3rd December catalogue (best deals)
aldi_products = [
    # Coffee & Beverages
    {"name": "Moccona Classic Instant Coffee Medium Roast 400g", "price": "$25.99", "category": "COFFEE & BEVERAGES", "details": "400g, $6.50 per 100g"},
    {"name": "Moccona Classic Instant Coffee Dark Roast 400g", "price": "$25.99", "category": "COFFEE & BEVERAGES", "details": "400g, $6.50 per 100g"},
    {"name": "Nescafé Espresso Concentrate 500ml", "price": "$5.49", "category": "COFFEE & BEVERAGES", "details": "500ml, $1.10 per 100ml"},
    {"name": "Red Bull 8 Pack 250ml", "price": "$13.99", "category": "COFFEE & BEVERAGES", "details": "2L, $7.00 per L"},

    # Confectionery
    {"name": "Toffifee 375g", "price": "$10.99", "category": "CONFECTIONERY", "details": "375g, $2.93 per 100g"},
    {"name": "Jelly Belly Bean Boozled Spinner Giftbox 100g", "price": "$9.99", "category": "CONFECTIONERY", "details": "100g, $9.99 per 100g"},
    {"name": "Toblerone Pralines 180g", "price": "$9.99", "category": "CONFECTIONERY", "details": "180g, $5.55 per 100g"},
    {"name": "Cadbury Favourites 265g", "price": "$6.99", "category": "CONFECTIONERY", "details": "265g, $2.64 per 100g"},

    # Budget Gifting
    {"name": "Crofton Mug, Teapot or Tea for One Set", "price": "$19.99", "category": "BUDGET GIFTING", "details": ""},
    {"name": "Crofton Double-Walled Stainless Steel Wine or Cocktail Cups", "price": "$19.99", "category": "BUDGET GIFTING", "details": ""},

    # Beauty
    {"name": "Retinol Gift Set 3 Piece", "price": "$29.99", "category": "BEAUTY & SELF CARE", "details": ""},
    {"name": "Luminosity Face Care Duo", "price": "$14.99", "category": "BEAUTY & SELF CARE", "details": ""},
]

# Generate ALDI HTML
aldi_html = ''
for product in aldi_products:
    card = f'''                    <div class="deal-card">
                        <div class="store-badge aldi">ALDI - {product['category']}</div>
                        <div class="product-name">{product['name']}</div>
                        <div class="price-info">
                            <span class="current-price">{product['price']}</span>
                        </div>'''

    if product['details']:
        card += f'''
                        <div class="unit-price">{product['details']}</div>'''

    card += f'''
                        <div class="deal-badge best-deal">SPECIAL BUY</div>
                    </div>
'''
    aldi_html += card

# Read liveinbne_deal.html
with open('C:/Users/advgen10/source/repos/AdvGenPriceComparer/liveinbne_deal.html', 'r', encoding='utf-8') as f:
    content = f.read()

# Update ALDI section header
content = re.sub(
    r'ALDI Special Buys - Available from \w+ \d+\w+ \w+',
    'ALDI Special Buys - Available from Wed 3rd December',
    content
)

# Update ALDI deals grid
aldi_pattern = r'(<div class="category-header" onclick="toggleCategory\(\'aldi\'\)">.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*<!-- Good Deals)'
content = re.sub(aldi_pattern, r'\1\n' + aldi_html + '                \3', content, flags=re.DOTALL)

# Write updated content
with open('C:/Users/advgen10/source/repos/AdvGenPriceComparer/liveinbne_deal.html', 'w', encoding='utf-8') as f:
    f.write(content)

print(f'Updated ALDI section with {len(aldi_products)} products from Wed 3rd December catalogue')
