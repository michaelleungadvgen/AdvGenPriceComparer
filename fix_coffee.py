# Fix the remaining "Buy me a Coffee" text

with open('data/html/price_super_market_15102025_chinese.html', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('Buy me a Coffee', '請我喝咖啡')

with open('data/html/price_super_market_15102025_chinese.html', 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed: Buy me a Coffee translation")
