#!/usr/bin/env python3
"""Update gooddeals.html from the new gooddeals.md format"""

import re

# Read the markdown file
with open('gooddeals.md', 'r', encoding='utf-8') as f:
    md_content = f.read()

# Parse the new markdown format
products = []
current_deal = {}
lines = md_content.strip().split('\n')

for line in lines:
    # Match deal number heading
    if re.match(r'^###\s+\d+\.\s+', line):
        if current_deal:
            products.append(current_deal)
        # Extract product name
        product_name = re.sub(r'^###\s+\d+\.\s+', '', line).strip()
        current_deal = {'name': product_name}

    # Match retailer
    elif line.strip().startswith('* **Retailer:**'):
        current_deal['retailer'] = line.split('**Retailer:**')[1].strip()

    # Match price and savings
    elif line.strip().startswith('* **Price:**'):
        price_line = line.split('**Price:**')[1].strip()
        # Extract price and savings
        price_match = re.search(r'\*\*(\$[\d,]+)\*\*', price_line)
        if price_match:
            current_deal['price'] = price_match.group(1)

        savings_match = re.search(r'\(Save (\$[\d,]+)\)', price_line)
        if savings_match:
            current_deal['savings'] = f"Save {savings_match.group(1)}"
        elif 'Half Price' in price_line:
            current_deal['savings'] = "Half Price"
        else:
            current_deal['savings'] = ""

# Add the last deal
if current_deal:
    products.append(current_deal)

# Generate badges based on product type
def get_badge(product_name):
    name_lower = product_name.lower()
    if 'oled tv' in name_lower or 'qled' in name_lower:
        return 'BIG SCREEN TV!'
    elif 'tablet' in name_lower or 'galaxy tab' in name_lower:
        return 'TABLET DEAL!'
    elif 'vacuum' in name_lower or 'robo' in name_lower:
        return 'ROBOT VACUUM!'
    elif 'laptop' in name_lower or 'omnibook' in name_lower or 'vivobook' in name_lower:
        return 'LAPTOP DEAL!'
    elif 'refrigerator' in name_lower or 'fridge' in name_lower:
        return 'FRIDGE!'
    elif 'soundbar' in name_lower:
        return 'PREMIUM AUDIO!'
    elif 'dyson' in name_lower:
        return 'DYSON VACUUM!'
    else:
        return 'GREAT DEAL!'

# Generate Chinese translations for new products
def translate_product_name(name):
    translations = {
        'LG 77" C5 Evo 4K OLED TV': 'LG 77吋 C5 Evo 4K OLED電視',
        'Samsung Galaxy Tab S9 (Wi-Fi, 128GB)': 'Samsung Galaxy Tab S9 (Wi-Fi, 128GB)',
        'Roborock Q70V+ Robotic Vacuum': 'Roborock Q70V+ 掃地機器人',
        'HP OmniBook 5 AI 16" OLED Laptop': 'HP OmniBook 5 AI 16吋 OLED 筆記型電腦',
        'Dyson V8 Origin Stick Vacuum': 'Dyson V8 Origin 無線吸塵器',
        'Samsung 85" Q7F QLED 4K TV': 'Samsung 85吋 Q7F QLED 4K電視',
        'JBL Bar 1000MK2 7.1.4 Soundbar': 'JBL Bar 1000MK2 7.1.4 環繞音響',
        'Haier 508L Quad Door Refrigerator': 'Haier 508公升 四門雪櫃',
        'ASUS Vivobook 16" (Snapdragon X1)': 'ASUS Vivobook 16吋 (Snapdragon X1)',
        'Hisense 85" Q6QAU QLED TV': 'Hisense 85吋 Q6QAU QLED電視'
    }
    return translations.get(name, name)

def translate_savings(savings):
    translations = {
        'Half Price': '半價優惠',
        'Save $2,134': '節省 $2,134',
        'Save $152': '節省 $152',
        'Save $190': '節省 $190',
        'Save $616': '節省 $616',
        'Save $347': '節省 $347',
        'Save $280': '節省 $280',
        'Save $200': '節省 $200'
    }
    return translations.get(savings, savings)

# Generate HTML for English version
english_html = ""
for i, product in enumerate(products, 1):
    badge = get_badge(product['name'])
    url = f"#deal{i}"  # Placeholder URL

    english_html += f'''                <div class="deal-card">
                    <div class="deal-rank">{i}</div>
                    <div class="epic-badge">{badge}</div>
                    <div class="deal-title">{product['name']}</div>
                    <div class="deal-store">{product['retailer']}</div>
                    <div class="deal-savings">{product['savings']}</div>
                    <div class="deal-price">{product['price']}</div>
                    <a href="{url}" class="deal-link" target="_blank">View Deal</a>
                </div>

'''

# Generate HTML for Chinese version
chinese_html = ""
for i, product in enumerate(products, 1):
    badge = get_badge(product['name'])
    chinese_name = translate_product_name(product['name'])
    chinese_savings = translate_savings(product['savings'])
    url = f"#deal{i}"  # Placeholder URL

    chinese_html += f'''                <div class="deal-card">
                    <div class="deal-rank">{i}</div>
                    <div class="epic-badge">{badge}</div>
                    <div class="deal-title">{chinese_name}</div>
                    <div class="deal-store">{product['retailer']}</div>
                    <div class="deal-savings">{chinese_savings}</div>
                    <div class="deal-price">{product['price']}</div>
                    <a href="{url}" class="deal-link" target="_blank">查看優惠</a>
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

# Update the date in the header if present
import datetime
current_date = datetime.datetime.now().strftime("%B %d, %Y")
html_content = re.sub(
    r'Updated: [A-Za-z]+ \d+, \d{4}',
    f'Updated: {current_date}',
    html_content
)

# Write updated HTML
with open('gooddeals.html', 'w', encoding='utf-8') as f:
    f.write(html_content)

print("✅ Updated gooddeals.html successfully!")
print(f"   - Parsed {len(products)} products from gooddeals.md")
print(f"   - English version: {len(products)} deals")
print(f"   - Chinese version: {len(products)} deals (with translations)")
print(f"\nProducts updated:")
for i, p in enumerate(products, 1):
    print(f"   {i}. {p['name']} - {p['price']} ({p['savings']}) @ {p['retailer']}")
