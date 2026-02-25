import re

# Selected products from ALDI 6th December catalogue
aldi_products = [
    {"name": "Smashed Burger Kit", "price": "$29.99", "category": "BBQ", "brand": "CROFTON"},
    {"name": "BBQ Caddy", "price": "$19.99", "category": "BBQ", "brand": "COOLABAH"},
    {"name": "Grill Tool Set 4 Piece", "price": "$19.99", "category": "BBQ", "brand": "COOLABAH"},
    {"name": "Caviar Collagen Pro Luxury Anti Wrinkle Gift Set", "price": "$34.99", "category": "BEAUTY", "brand": "LACURA SKIN SCIENCE"},
    {"name": "Heated Hair Roller Set", "price": "$39.99", "category": "BEAUTY", "brand": "VISAGE"},
    {"name": "7 Piece Makeup Brush Set & Silicone Holder", "price": "$12.99", "category": "BEAUTY", "brand": "LACURA BEAUTY"},
    {"name": "Premium Wet and Dry Shaver", "price": "$39.99", "category": "MEN'S GROOMING", "brand": "VISAGE"},
    {"name": "Premium Cordless Hair Clippers", "price": "$29.99", "category": "MEN'S GROOMING", "brand": "VISAGE"},
    {"name": "Interactive Pet Talking Buttons", "price": "$29.99", "category": "PET", "brand": "PETPLAY"},
    {"name": "Dog Toy Gift Set", "price": "$12.99", "category": "PET", "brand": "PETPLAY"},
    {"name": "Portable Vlogging Kit", "price": "$34.99", "category": "TECHNOLOGY", "brand": "BAUHN"},
    {"name": "Speaker with LED Mirror and Wireless Charging", "price": "$39.99", "category": "TECHNOLOGY", "brand": "BAUHN"},
]

# Generate ALDI HTML
aldi_html = ''
for product in aldi_products:
    aldi_html += f'''                    <div class="deal-card">
                        <div class="store-badge aldi">ALDI - {product['category']}</div>
                        <div class="product-name">{product['name']}</div>
                        <div class="price-info">
                            <span class="current-price">{product['price']}</span>
                        </div>
                        <div class="deal-badge best-deal">SPECIAL BUY</div>
                    </div>
'''

# Read liveinbne_deal.html
with open('C:/Users/advgen10/source/repos/AdvGenPriceComparer/liveinbne_deal.html', 'r', encoding='utf-8') as f:
    content = f.read()

# Update ALDI section header
content = re.sub(
    r'ALDI Special Buys - Available from \w+ \d+\w+ \w+',
    'ALDI Special Buys - Available from Sat 6th December',
    content
)

# Update ALDI deals grid
aldi_pattern = r'(<div class="category-header" onclick="toggleCategory\(\'aldi\'\)">.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*<!-- Good Deals)'
content = re.sub(aldi_pattern, r'\1\n' + aldi_html + '                \3', content, flags=re.DOTALL)

# Write updated content
with open('C:/Users/advgen10/source/repos/AdvGenPriceComparer/liveinbne_deal.html', 'w', encoding='utf-8') as f:
    f.write(content)

print(f'Updated ALDI section with {len(aldi_products)} products from 6th December catalogue')
