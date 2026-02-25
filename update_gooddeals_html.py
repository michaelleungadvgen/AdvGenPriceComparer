import re

# Read the markdown file
with open('gooddeals.md', 'r', encoding='utf-8') as f:
    md_content = f.read()

# Parse markdown table
products = []
lines = md_content.strip().split('\n')
for line in lines[3:]:  # Skip header rows
    if line.strip() and '|' in line:
        parts = [p.strip() for p in line.split('|')[1:-1]]  # Remove empty first and last
        if len(parts) == 4 and parts[0].startswith('**'):
            retailer = parts[0].replace('**', '')
            product_name = parts[1].replace('**', '')

            # Extract savings from product name
            savings = ""
            if '(Save' in product_name:
                match = re.search(r'\(Save (\$\d+)\)', product_name)
                if match:
                    savings = f"Save {match.group(1)}"
                product_name = re.sub(r'\s*\(Save.*?\)', '', product_name)
            elif '(60% Off)' in product_name or 'Off)' in product_name:
                match = re.search(r'\((\d+% Off)\)', product_name)
                if match:
                    savings = match.group(1)
                product_name = re.sub(r'\s*\(\d+% Off\)', '', product_name)
            elif '(Hot Buy!)' in product_name or '(Hot Deal)' in product_name:
                savings = "Hot Deal"
                product_name = re.sub(r'\s*\(\*\*Hot Buy!\*\*\)|\s*\(Hot Deal\)', '', product_name)

            price = parts[2].replace('**', '')

            # Extract URL from markdown link
            url_match = re.search(r'\[(.*?)\]\((.*?)\)', parts[3])
            url = url_match.group(2) if url_match else ""

            products.append({
                'retailer': retailer,
                'name': product_name,
                'price': price,
                'savings': savings,
                'url': url
            })

# Generate badges based on product type
def get_badge(product_name):
    name_lower = product_name.lower()
    if 'galaxy s' in name_lower or 'pixel' in name_lower:
        return 'FLAGSHIP PHONE!'
    elif 'watch' in name_lower:
        return 'SMARTWATCH!'
    elif 'laptop' in name_lower:
        return 'LAPTOP DEAL!'
    elif 'refrigerator' in name_lower or 'fridge' in name_lower:
        return 'FRIDGE!'
    elif 'headphone' in name_lower:
        return 'PREMIUM AUDIO!'
    elif 'vacuum' in name_lower or 'robo' in name_lower:
        return 'ROBOT VACUUM!'
    elif 'washer' in name_lower:
        return 'WASHER!'
    elif 'air fryer' in name_lower:
        return 'AIR FRYER!'
    elif 'tv' in name_lower or 'television' in name_lower:
        return 'BIG SCREEN TV!'
    else:
        return 'GREAT DEAL!'

# Generate Chinese translations
chinese_translations = {
    'Samsung Galaxy S25 Ultra 256GB': 'Samsung Galaxy S25 Ultra 256GB',
    'Samsung Galaxy Watch6 Classic 47mm (Black)': 'Samsung Galaxy Watch6 Classic 47mm (黑色)',
    'Dell 15 DC15255 15.6" FHD 120Hz Laptop (Ryzen 5)': 'Dell 15 DC15255 15.6吋 FHD 120Hz 筆記型電腦 (Ryzen 5)',
    'LG 315L Top Mount Refrigerator': 'LG 315公升 上置冷凍式雪櫃',
    'Bose QuietComfort Ultra Headphones 2nd Gen': 'Bose QuietComfort Ultra 降噪耳機 第2代',
    'Roborock QRevo Edge C Robotic Vacuum': 'Roborock QRevo Edge C 掃地機器人',
    'LG 8kg Front Load Washer': 'LG 8公斤 前置式洗衣機',
    'Sunbeam 4L Alinea Manual Diamond Force Air Fryer': 'Sunbeam 4公升 Alinea 手動鑽石力量氣炸鍋',
    'TCL 75" C6K QD Mini LED Google TV 2025': 'TCL 75吋 C6K QD Mini LED Google TV 2025',
    'LG A9-ACE CordZero Stick Vacuum': 'LG A9-ACE CordZero 無線吸塵器'
}

savings_translations = {
    'Save $300': '節省 $300',
    '60% Off': '60% 折扣',
    'Save $211': '節省 $211',
    'Save $102': '節省 $102',
    'Hot Deal': '熱賣商品',
    'Save $104': '節省 $104',
    'Save $54': '節省 $54'
}

retailer_translations = {
    'JB Hi-Fi': 'JB Hi-Fi',
    'The Good Guys': 'The Good Guys',
    'Harvey Norman': 'Harvey Norman'
}

# Generate HTML for English version
english_html = ""
for i, product in enumerate(products, 1):
    badge = get_badge(product['name'])
    english_html += f'''                <div class="deal-card">
                    <div class="deal-rank">{i}</div>
                    <div class="epic-badge">{badge}</div>
                    <div class="deal-title">{product['name']}</div>
                    <div class="deal-store">{product['retailer']}</div>
                    <div class="deal-savings">{product['savings']}</div>
                    <div class="deal-price">{product['price']}</div>
                    <a href="{product['url']}" class="deal-link" target="_blank">View Deal</a>
                </div>

'''

# Generate HTML for Chinese version
chinese_html = ""
for i, product in enumerate(products, 1):
    badge = get_badge(product['name'])
    chinese_name = chinese_translations.get(product['name'], product['name'])
    chinese_savings = savings_translations.get(product['savings'], product['savings'])

    chinese_html += f'''                <div class="deal-card">
                    <div class="deal-rank">{i}</div>
                    <div class="epic-badge">{badge}</div>
                    <div class="deal-title">{chinese_name}</div>
                    <div class="deal-store">{product['retailer']}</div>
                    <div class="deal-savings">{chinese_savings}</div>
                    <div class="deal-price">{product['price']}</div>
                    <a href="{product['url']}" class="deal-link" target="_blank">查看優惠</a>
                </div>

'''

# Read current HTML
with open('gooddeals.html', 'r', encoding='utf-8') as f:
    html_content = f.read()

# Replace English section
english_pattern = r'(<!-- English Version -->.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*<!-- Chinese Version -->)'
html_content = re.sub(english_pattern, f'\\1\n{english_html}            \\3', html_content, flags=re.DOTALL)

# Replace Chinese section
chinese_pattern = r'(<!-- Chinese Version -->.*?<div class="deals-grid">)(.*?)(</div>\s*</div>\s*</div>)'
html_content = re.sub(chinese_pattern, f'\\1\n{chinese_html}            \\3', html_content, flags=re.DOTALL)

# Write updated HTML
with open('gooddeals.html', 'w', encoding='utf-8') as f:
    f.write(html_content)

print("Updated gooddeals.html with 10 products from gooddeals.md")
print(f"   - English version: {len(products)} products")
print(f"   - Chinese version: {len(products)} products (with translations)")
