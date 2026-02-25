#!/usr/bin/env python3
"""Update gooddeals.html with products from gooddeals.md"""

import csv
from datetime import datetime

# Read products from gooddeals.md
products = []
with open('gooddeals.md', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        products.append(row)

# Read the HTML template
with open('gooddeals.html', 'r', encoding='utf-8') as f:
    html = f.read()

# Generate English deals cards
english_cards = []
chinese_cards = []

for idx, product in enumerate(products, 1):
    retailer = product['Retailer']
    name = product['Product']
    price = product['Deal Price']
    savings = product['Savings']
    url = product['URL'].strip() if product['URL'].strip() else '#'

    # Determine badge based on retailer or product type
    badge = "EPIC DEAL!"
    if "TV" in name or "OLED" in name or "QLED" in name:
        badge = "BIG SCREEN TV!"
    elif "Tablet" in name or "iPad" in name:
        badge = "TABLET DEAL!"
    elif "Laptop" in name:
        badge = "LAPTOP DEAL!"
    elif "Vacuum" in name or "Washer" in name or "Fridge" in name:
        badge = "HOME APPLIANCE!"
    elif "Earbuds" in name or "Headphones" in name or "Wireless" in name:
        badge = "AUDIO DEAL!"
    elif "Mac" in name or "Apple" in name:
        badge = "APPLE DEAL!"

    # English card
    link_class = 'deal-link' if url != '#' else 'deal-link no-link'
    english_card = f'''                <div class="deal-card">
                    <div class="deal-rank">{idx}</div>
                    <div class="epic-badge">{badge}</div>
                    <div class="deal-title">{name}</div>
                    <div class="deal-store">{retailer}</div>
                    <div class="deal-savings">{savings}</div>
                    <div class="deal-price">{price}</div>
                    <a href="{url}" class="{link_class}" target="_blank">{'View Deal' if url != '#' else 'Coming Soon'}</a>
                </div>
'''
    english_cards.append(english_card)

    # Chinese translations
    name_zh = name
    retailer_zh = retailer
    savings_zh = savings

    # Simple translations
    if "Samsung Galaxy S10 Ultra Tablet" in name:
        name_zh = "Samsung Galaxy S10 Ultra 平板電腦 (256GB)"
    elif "Sennheiser Momentum True Wireless 4" in name:
        name_zh = "Sennheiser Momentum True Wireless 4 真無線耳機"
    elif "Lenovo Legion Pro 7" in name:
        name_zh = "Lenovo Legion Pro 7 遊戲筆電 (RTX 5080)"
    elif 'Samsung 55" QN70F' in name:
        name_zh = "Samsung 55吋 QN70F Neo QLED 4K 電視 (2025)"
    elif "Ninja Creami Ice Cream Maker" in name:
        name_zh = "Ninja Creami 雪糕製作機"
    elif "Dyson V7 Advanced Origin" in name:
        name_zh = "Dyson V7 Advanced Origin 無線吸塵器"
    elif "Samsung 9kg Smart Front Load Washer" in name:
        name_zh = "Samsung 9公斤智能前置式洗衣機"
    elif "Apple Mac Computers" in name:
        name_zh = "Apple Mac 電腦 (精選型號)"
    elif "Haier French Door Fridges" in name:
        name_zh = "Haier 法式門雪櫃"
    elif "Windows 11 Laptops" in name:
        name_zh = "Windows 11 筆記本電腦 (精選品牌)"

    # Translate savings
    if "OFF" in savings:
        savings_zh = savings.replace("OFF", "折扣")
    elif "Monster Deal" in savings:
        savings_zh = "超值優惠"
    elif "Limited Time" in savings:
        savings_zh = "限時優惠"
    elif "Back to School" in savings:
        savings_zh = "開學優惠"
    elif "Cashback" in savings:
        savings_zh = savings.replace("Cashback", "現金回贈")

    # Translate badge
    badge_zh = badge
    if badge == "BIG SCREEN TV!":
        badge_zh = "大螢幕電視！"
    elif badge == "TABLET DEAL!":
        badge_zh = "平板優惠！"
    elif badge == "LAPTOP DEAL!":
        badge_zh = "筆電優惠！"
    elif badge == "HOME APPLIANCE!":
        badge_zh = "家電優惠！"
    elif badge == "AUDIO DEAL!":
        badge_zh = "音響優惠！"
    elif badge == "APPLE DEAL!":
        badge_zh = "Apple 優惠！"
    else:
        badge_zh = "超值優惠！"

    # Chinese card
    chinese_card = f'''                <div class="deal-card">
                    <div class="deal-rank">{idx}</div>
                    <div class="epic-badge">{badge_zh}</div>
                    <div class="deal-title">{name_zh}</div>
                    <div class="deal-store">{retailer_zh}</div>
                    <div class="deal-savings">{savings_zh}</div>
                    <div class="deal-price">{price}</div>
                    <a href="{url}" class="{link_class}" target="_blank">{'查看優惠' if url != '#' else '即將推出'}</a>
                </div>
'''
    chinese_cards.append(chinese_card)

# Find and replace English deals section
english_start = html.find('<!-- English Version -->')
english_end = html.find('<!-- Chinese Version -->')

if english_start != -1 and english_end != -1:
    english_section = f'''<!-- English Version -->
        <div class="content english active">
            <div class="deals-grid">
{chr(10).join(english_cards)}
            </div>
        </div>


        '''
    html = html[:english_start] + english_section + html[english_end:]

# Find and replace Chinese deals section
chinese_start = html.find('<!-- Chinese Version -->')
chinese_end = html.find('</div>', chinese_start + 100)  # Find closing div after Chinese section start

# Find the proper end (after deals-grid closure)
temp_pos = chinese_start
div_count = 0
proper_end = -1
while temp_pos < len(html):
    if html[temp_pos:temp_pos+12] == '<div class="':
        div_count += 1
    elif html[temp_pos:temp_pos+6] == '</div>' and div_count > 0:
        div_count -= 1
        if div_count == 0:
            proper_end = temp_pos + 6
            break
    temp_pos += 1

if chinese_start != -1 and proper_end != -1:
    chinese_section = f'''<!-- Chinese Version -->
       <div class="content chinese">
        <div class="deals-grid">
{chr(10).join(chinese_cards)}
            </div>
        </div>'''
    html = html[:chinese_start] + chinese_section + html[proper_end:]

# Update the timestamp
current_date = datetime.now().strftime('%Y-%m-%d')
html = html.replace('Generated on 2025-01-10', f'Generated on {current_date}')

# Write updated HTML
with open('gooddeals.html', 'w', encoding='utf-8') as f:
    f.write(html)

print(f"✅ Successfully updated gooddeals.html")
print(f"📦 Updated {len(products)} products")
print(f"🔗 {sum(1 for p in products if p['URL'].strip())} products have URLs")
print(f"📅 Date: {current_date}")
