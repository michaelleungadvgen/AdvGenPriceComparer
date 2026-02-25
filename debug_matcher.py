import json
import re
import sys

# Configure UTF-8 encoding
sys.stdout.reconfigure(encoding='utf-8')

def extract_drakes_products(markdown_file):
    """Extract products and prices from drakes.md"""
    products = {}

    with open(markdown_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find all product lines with prices - multiple patterns
    patterns = [
        # Pattern 1: - **Product Name** - $price [unit]
        r'-\s+\*\*([^*]+)\*\*\s+-\s+\$(\d+(?:\.\d{2})?)\s*(?:ea|kg|pk|dz|bunch|/kg)?',
        # Pattern 2: - $price ea/kg Product Name
        r'-\s+\$(\d+(?:\.\d{2})?)\s+(?:ea|kg|pk|dz|bunch|/kg)\s+([^-\n]+)',
        # Pattern 3: Product Name - $price
        r'-\s+([^$-]+?)\s+-\s+\$(\d+(?:\.\d{2})?)',
    ]

    for pattern in patterns:
        matches = re.findall(pattern, content)
        for match in matches:
            if len(match) == 2:
                # Could be (name, price) or (price, name)
                if match[0].replace('.', '').isdigit():
                    price, product_name = match
                else:
                    product_name, price = match

                product_name = product_name.strip()
                # Skip if it's a "Save" amount
                if 'save' in product_name.lower():
                    continue
                products[product_name] = float(price)

    return products

# Extract and display Drakes products
drakes_products = extract_drakes_products('drakes.md')
print(f"Drakes products ({len(drakes_products)}):")
for name, price in list(drakes_products.items())[:20]:
    print(f"  - {name}: ${price}")

# Load and display JSON products
with open('price_comparison_data.json', 'r', encoding='utf-8') as f:
    price_data = json.load(f)

print(f"\nJSON products:")
if 'price_comparisons' in price_data:
    count = 0
    for category, products in price_data['price_comparisons'].items():
        for product in products[:3]:  # First 3 from each category
            print(f"  - {product.get('product', 'N/A')}")
            count += 1
            if count >= 20:
                break
        if count >= 20:
            break
