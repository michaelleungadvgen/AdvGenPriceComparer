import re
import html

# Read the source file
with open('data/html/price_super_market_26112025.html', 'r', encoding='utf-8') as f:
    content = f.read()

# Translation mappings
translations = {
    # Header and navigation
    'Price Comparison Report': '價格比較報告',
    'Home': '首頁',
    'Comparison': '比較',
    'English': '英文',
    'Chinese': '中文',

    # Store names
    'Coles': 'Coles',
    'Woolworths': 'Woolworths',
    'ALDI': 'ALDI',
    'Drakes': 'Drakes',
    'IGA': 'IGA',

    # Dates - will be replaced with specific pattern
    'November': '十一月',
    'December': '十二月',
    '2025': '2025年',

    # Hero section
    'Brisbane Best Deals': '布里斯班超市最佳優惠',
    'Your Weekly Guide to the Best Supermarket Specials': '每週超市特價指南',
    'Smart Shopping Starts Here': '精明購物從這裡開始',

    # Week selector
    'Select Week:': '選擇週次：',
    'Week': '第',
    'week': '週',

    # Stats
    'Total Items': '商品總數',
    'Best Deals': '最佳優惠',
    'Half Price': '半價',
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

    # Deal badges
    'HALF PRICE': '半價',
    'BETTER VALUE': '更好價值',
    'GOOD VALUE': '超值',
    'BEST DEAL': '最佳優惠',
    'HOT DEAL': '熱賣',
    'SPECIAL BUY': '特價購買',

    # Price info
    'Save': '節省',
    'Was': '原價',
    'Now': '現價',
    'per kg': '每公斤',
    'per litre': '每公升',
    'per 100g': '每100克',
    'per 100ml': '每100毫升',
    'each': '每個',

    # Buttons and actions
    'View Details': '查看詳情',
    'Add to List': '加入清單',
    'Compare Prices': '比較價格',
    'Show More': '顯示更多',
    'Show Less': '顯示更少',
    'Filter': '篩選',
    'Sort by': '排序方式',
    'Search': '搜尋',

    # Footer
    'Price Comparison': '價格比較',
    'Made with': '製作於',
    'All prices are subject to change': '所有價格如有更改，恕不另行通知',
    'Information is accurate at time of publication': '資訊在發布時準確',
    'Support Us': '支持我們',

    # Common phrases
    'items': '項商品',
    'deals': '優惠',
    'Special': '特價',
    'Specials': '特價商品',
    'New': '新品',
    'Limited Time': '限時',
    'Available': '可購買',
    'Stock': '庫存',

    # Descriptions
    'Pack': '包',
    'Pk': '包',
    'Litre': '公升',
    'Litres': '公升',
    'Kilogram': '公斤',
    'Kilograms': '公斤',
    'Gram': '克',
    'Grams': '克',
    'Millilitre': '毫升',
    'Millilitres': '毫升',
}

# Apply translations
for english, chinese in translations.items():
    # Use word boundaries for whole word replacement
    content = re.sub(r'\b' + re.escape(english) + r'\b', chinese, content, flags=re.IGNORECASE)

# Specific date translations
content = re.sub(r'(\d+)(st|nd|rd|th)\s+November', r'\1日十一月', content)
content = re.sub(r'(\d+)(st|nd|rd|th)\s+December', r'\1日十二月', content)

# Update title with correct date
content = re.sub(
    r'<title>.*?</title>',
    '<title>價格比較報告 | Coles vs Woolworths | 2025年11月25日</title>',
    content
)

# Update meta description if exists
content = re.sub(
    r'<meta name="description" content=".*?">',
    '<meta name="description" content="布里斯班超市每週價格比較 - Coles、Woolworths、ALDI、Drakes最佳優惠">',
    content
)

# Write the translated file
with open('data/html/price_super_market_26112025_chinese.html', 'w', encoding='utf-8') as f:
    f.write(content)

print('Translation completed successfully!')
print('Output file: data/html/price_super_market_26112025_chinese.html')
