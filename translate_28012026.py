#!/usr/bin/env python3
"""Translate price_super_market_28012026.html to Traditional Chinese"""

import re

# Read the English HTML file
with open('data/html/price_super_market_28012026.html', 'r', encoding='utf-8') as f:
    content = f.read()

# Comprehensive translation mappings
translations = [
    # HTML attributes
    ('lang="en"', 'lang="zh-Hant"'),

    # Title
    ('Price Comparison Report | Coles vs Woolworths | 2026-01-27', '價格比較報告 | Coles vs Woolworths | 2026-01-27'),

    # Navigation
    ('>English<', '>英文版<'),
    ('>Chinese<', '>繁體中文<'),

    # Hero section
    ('Smart Price Comparison', '智能價格比較'),
    ('AI-powered analysis comparing Coles vs Woolworths weekly deals', 'AI 智能分析 Coles vs Woolworths 每週特價'),
    ('21-27 Jan 2026', '2026年1月21-27日'),
    ('28 Jan - 3 Feb 2026', '2026年1月28日 - 2月3日'),

    # Retailer section
    ('Head-to-Head Price Battle', '超市價格對決'),
    ('Woolworths wins with 34 cheaper products', 'Woolworths 獲勝！34個產品更便宜'),
    ('Woolworths wins with', 'Woolworths 獲勝！'),
    ('Coles wins with', 'Coles 獲勝！'),
    ('cheaper products', '個產品更便宜'),

    # Stats section
    ('Weekly Analysis', '每週分析'),
    ('Products Compared', '比較產品數'),
    ('Total Savings Identified', '總潛在節省'),
    ('Coles Best Deals', 'Coles 最佳優惠'),
    ('Woolworths Best Deals', 'Woolworths 最佳優惠'),

    # Product categories
    ('Cheaper at Coles', 'Coles 較便宜'),
    ('Cheaper at Woolworths', 'Woolworths 較便宜'),
    ('Same Price', '同價'),

    # Product card elements
    ('Save ', '節省 '),
    ('Coles Price:', 'Coles 價格:'),
    ('Woolworths Price:', 'Woolworths 價格:'),

    # Footer
    ('Generated on', '生成日期'),
    ('Price comparison data extracted from weekly catalogues', '價格比較數據來自每週特價目錄'),
    ('Disclaimer:', '免責聲明:'),
    ('Prices are indicative and based on catalogue information. Please check current catalogues for accurate pricing.',
     '價格僅供參考，基於特價目錄資訊。請查看當前特價目錄以獲取準確價格。'),
    ('Back to Top', '返回頂部'),

    # Common words
    (' VS ', ' VS '),
    ('COLES', 'COLES'),
    ('WOOLWORTHS', 'WOOLWORTHS'),
]

# Apply translations in order
for english, chinese in translations:
    content = content.replace(english, chinese)

# Write the Chinese version
with open('data/html/price_super_market_28012026_chinese.html', 'w', encoding='utf-8') as f:
    f.write(content)

print('Successfully created Traditional Chinese version')
print('File: data/html/price_super_market_28012026_chinese.html')
print('Week: 28 Jan - 3 Feb 2026')
