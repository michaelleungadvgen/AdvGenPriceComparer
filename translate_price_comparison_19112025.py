import re

# Read the English HTML file
with open('data/html/price_super_market_19112025.html', 'r', encoding='utf-8') as f:
    html_content = f.read()

# Translation dictionary
translations = {
    # Title and headers
    'Price Comparison Report': '價格比較報告',
    'Coles vs Woolworths': 'Coles vs Woolworths',
    'AI-powered analysis comparing Coles vs Woolworths weekly deals': 'AI 智能分析 Coles vs Woolworths 每週特價',

    # Navigation
    'Home': '首頁',
    'Products': '商品',
    'Winners': '最優惠',
    'Analysis': '分析',

    # Winner section
    'Coles wins with': 'Coles 獲勝',
    'cheaper products': '個更便宜的商品',
    'Woolworths wins with': 'Woolworths 獲勝',

    # Stats
    'Coles Best Deals': 'Coles 最佳優惠',
    'Woolworths Best Deals': 'Woolworths 最佳優惠',
    'Total Comparisons': '總比較數',
    'Average Savings': '平均節省',

    # Product comparison section
    'Product Comparisons': '商品價格比較',
    'Comparing': '比較',
    'products across both stores': '個商品跨兩家超市',
    'Filter by winner:': '按優勝者篩選：',
    'All': '全部',
    'Search products...': '搜尋商品...',

    # Product details
    'General': '一般',
    'Similarity': '相似度',
    'Better Deal': '更優惠',
    'Same Price': '同價',
    'More Expensive': '更貴',
    'Not Available': '無庫存',

    # Store names
    'Woolworths': 'Woolworths',
    'Coles': 'Coles',

    # Common terms
    'Save': '節省',
    'Was': '原價',
    'Now': '現價',
    'per': '每',
    'each': '個',
    'kg': '公斤',

    # Winners section
    'Top Deals from Each Store': '各超市最佳優惠',
    'Best': '最佳',
    'Deals': '優惠',

    # Analysis section
    'Price Analysis': '價格分析',
    'Category Breakdown': '分類細分',
    'Price Distribution': '價格分佈',
    'Best Value Categories': '最超值分類',

    # Footer
    'Generated on': '生成於',
    'Data accuracy disclaimer': '數據準確性聲明',
    'Prices and availability may vary by location': '價格和供應可能因地點而異',

    # Months
    'January': '一月',
    'February': '二月',
    'March': '三月',
    'April': '四月',
    'May': '五月',
    'June': '六月',
    'July': '七月',
    'August': '八月',
    'September': '九月',
    'October': '十月',
    'November': '十一月',
    'December': '十二月',
}

# Apply translations
for english, chinese in translations.items():
    html_content = html_content.replace(english, chinese)

# Additional regex replacements for dynamic content
# Replace "X cheaper products" pattern
html_content = re.sub(r'(\d+)\s+個更便宜的商品', r'\1個更便宜的商品', html_content)

# Replace date format "2025-11-18" with Chinese format
html_content = re.sub(r'2025-11-18', '2025年11月18日', html_content)
html_content = re.sub(r'2025-11-19', '2025年11月19日', html_content)

# Save the translated file
with open('data/html/price_super_market_19112025_chinese.html', 'w', encoding='utf-8') as f:
    f.write(html_content)

print("Translation completed!")
print("Saved to: data/html/price_super_market_19112025_chinese.html")
