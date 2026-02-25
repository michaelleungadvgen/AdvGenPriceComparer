import re

# Read the source file
with open('data/html/price_super_market_26112025.html', 'r', encoding='utf-8') as f:
    content = f.read()

# Function to translate text content only (not in tags, not CSS)
def translate_content(text):
    translations = {
        # Navigation
        'Home': '首頁',
        'Comparison': '比較',
        'English': '英文',
        'Chinese': '中文',

        # Main content
        'Brisbane Best Deals': '布里斯班超市最佳優惠',
        'Your Weekly Guide to the Best Supermarket Specials': '每週超市特價指南',
        'Smart Shopping Starts Here': '精明購物從這裡開始',
        'Price Comparison Report': '價格比較報告',

        # Weeks
        'Week': '第',
        'Select Week:': '選擇週次：',

        # Stats
        'Total Items': '商品總數',
        'Best Deals': '最佳優惠',
        'Half Price': '半價特價',
        'Avg. Savings': '平均節省',

        # Categories
        'Premium Chocolate': '優質巧克力',
        'Soft Drinks & Beverages': '軟性飲料與飲品',
        'Meat, Seafood & Deli': '肉類、海鮮與熟食',
        'Dairy, Eggs & Meals': '乳製品、雞蛋與餐食',
        'Pantry': '食品雜貨',
        'Frozen': '冷凍食品',
        'Fresh Produce': '新鮮農產品',
        'Bakery': '烘焙食品',
        'Snacks': '零食',
        'Health & Beauty': '健康與美容',
        'Liquor': '酒類',
        'Pet Care': '寵物用品',
        'Baby': '嬰兒用品',
        'Household': '家居用品',

        # Deal types
        'HALF PRICE': '半價',
        'BETTER VALUE': '更好價值',
        'GOOD VALUE': '超值優惠',
        'BEST DEAL': '最佳優惠',
        'HOT DEAL': '熱賣優惠',
        'SPECIAL BUY': '特價',
        'CHRISTMAS SPECIAL': '聖誕特價',

        # Actions
        'Save': '節省',
        'Was': '原價',
        'Now': '現價',
        'View All': '查看全部',
        'Show More': '顯示更多',
        'Show Less': '顯示更少',

        # Common terms
        'items': '項',
        'per kg': '每公斤',
        'per litre': '每公升',
        'per 100g': '每100克',
        'per 100ml': '每100毫升',
        'each': '每個',

        # Footer
        'Support Us': '支持我們',
        'Made with': '製作',
        'All prices are subject to change': '所有價格如有更改，恕不另行通知',

        # Months
        'November': '十一月',
        'December': '十二月',
    }

    result = text
    for eng, chi in translations.items():
        # Only replace whole words in text content
        result = re.sub(r'\b' + re.escape(eng) + r'\b', chi, result)

    return result

# Split content into sections to avoid translating code
lines = content.split('\n')
translated_lines = []

in_style = False
in_script = False

for line in lines:
    # Check if we're in style or script tags
    if '<style' in line:
        in_style = True
    elif '</style>' in line:
        in_style = False
        translated_lines.append(line)
        continue
    elif '<script' in line:
        in_script = True
    elif '</script>' in line:
        in_script = False
        translated_lines.append(line)
        continue

    # Don't translate inside style or script tags
    if in_style or in_script:
        translated_lines.append(line)
        continue

    # Don't translate lines that are purely HTML tags or CSS
    if line.strip().startswith('<') and line.strip().endswith('>') and '>' not in line.strip()[1:-1]:
        translated_lines.append(line)
        continue

    # For content lines, only translate text between tags
    if '<' in line and '>' in line:
        # Split by tags and translate only text content
        parts = re.split(r'(<[^>]+>)', line)
        translated_parts = []
        for part in parts:
            if part.startswith('<') and part.endswith('>'):
                # This is a tag, don't translate
                translated_parts.append(part)
            else:
                # This is text content, translate it
                translated_parts.append(translate_content(part))
        translated_lines.append(''.join(translated_parts))
    else:
        translated_lines.append(line)

result = '\n'.join(translated_lines)

# Specific fixes
result = re.sub(r'<title>.*?</title>', '<title>價格比較報告 | Coles vs Woolworths | 2025年11月25日</title>', result)

# Fix date format in title/header
result = re.sub(r'(\d+)(st|nd|rd|th)\s+November\s+2025', r'2025年11月\1日', result)
result = re.sub(r'(\d+)(st|nd|rd|th)\s+December\s+2025', r'2025年12月\1日', result)

# Write output
with open('data/html/price_super_market_26112025_chinese.html', 'w', encoding='utf-8') as f:
    f.write(result)

print('Translation completed successfully!')
print('Output: data/html/price_super_market_26112025_chinese.html')
