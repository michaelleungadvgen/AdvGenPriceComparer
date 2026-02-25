#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Translate price_super_market_03122025.html to Traditional Chinese
"""

from bs4 import BeautifulSoup
import re

# Translation dictionary for common supermarket terms
translations = {
    # Headers and titles
    "Brisbane Best Grocery Deals": "布里斯本最佳超市優惠",
    "Best Grocery Deals in Brisbane": "布里斯本最佳超市優惠",
    "Week of": "本週",
    "Valid": "有效期",

    # Store names
    "Coles": "Coles",
    "Woolworths": "Woolworths",
    "Drakes": "Drakes",
    "Aldi": "Aldi",
    "IGA": "IGA",

    # Product categories
    "Dairy & Eggs": "乳製品與雞蛋",
    "Meat & Seafood": "肉類與海鮮",
    "Fruits & Vegetables": "水果與蔬菜",
    "Bakery": "烘焙食品",
    "Pantry": "食品儲藏",
    "Beverages": "飲料",
    "Snacks": "零食",
    "Frozen": "冷凍食品",
    "Health & Beauty": "健康與美容",
    "Household": "家居用品",

    # Common terms
    "each": "個",
    "kg": "公斤",
    "g": "克",
    "ml": "毫升",
    "L": "升",
    "pack": "包",
    "box": "盒",
    "bottle": "瓶",
    "can": "罐",
    "Price": "價格",
    "Save": "節省",
    "Special": "特價",
    "Was": "原價",
    "Now": "現價",
    "per": "每",
    "Compare Prices": "價格比較",
    "Best Deals": "最佳優惠",
    "This Week": "本週",
    "Last Week": "上週",
    "Previous": "上一週",
    "Next": "下一週",
    "Update": "更新",
    "Updated": "已更新",
    "Loading": "載入中",
    "Search": "搜尋",
    "Filter": "篩選",
    "Sort": "排序",
    "Category": "類別",
    "Store": "商店",
    "All Stores": "所有商店",
    "All Categories": "所有類別",
    "Show More": "顯示更多",
    "Show Less": "顯示較少",
}

def translate_text(text):
    """Translate English text to Traditional Chinese"""
    if not text or not isinstance(text, str):
        return text

    result = text
    for eng, chi in translations.items():
        # Case-insensitive replacement
        result = re.sub(re.escape(eng), chi, result, flags=re.IGNORECASE)

    return result

def translate_html(input_file, output_file):
    """Translate HTML file from English to Traditional Chinese"""
    print(f"Reading {input_file}...")

    with open(input_file, 'r', encoding='utf-8') as f:
        html_content = f.read()

    print("Parsing HTML...")
    soup = BeautifulSoup(html_content, 'html.parser')

    # Update page title
    if soup.title:
        soup.title.string = translate_text(soup.title.string)

    # Translate all text nodes
    print("Translating content...")
    for element in soup.find_all(text=True):
        if element.parent.name not in ['script', 'style']:
            translated = translate_text(element.string)
            if translated != element.string:
                element.replace_with(translated)

    # Translate specific attributes
    for tag in soup.find_all(attrs={'title': True}):
        tag['title'] = translate_text(tag['title'])

    for tag in soup.find_all(attrs={'alt': True}):
        tag['alt'] = translate_text(tag['alt'])

    for tag in soup.find_all(attrs={'placeholder': True}):
        tag['placeholder'] = translate_text(tag['placeholder'])

    print(f"Writing to {output_file}...")
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(str(soup))

    print("Translation complete!")

if __name__ == '__main__':
    input_file = r'C:\Users\advgen10\source\repos\AdvGenPriceComparer\data\html\price_super_market_03122025.html'
    output_file = r'C:\Users\advgen10\source\repos\AdvGenPriceComparer\data\html\price_super_market_03122025_chinese.html'

    translate_html(input_file, output_file)
