import json
import re
import sys

sys.stdout.reconfigure(encoding='utf-8')

def normalize_product_name(name):
    """Normalize product name for better matching"""
    name = name.lower()
    # Remove size/quantity indicators
    name = re.sub(r'\d+(?:\.\d+)?\s*(?:kg|g|ml|l|pk|pack|ea|bunch|dz|x)', '', name)
    # Remove extra whitespace
    name = ' '.join(name.split())
    return name

def extract_keywords(name):
    """Extract meaningful keywords from product name"""
    normalized = normalize_product_name(name)
    words = [w for w in re.findall(r'\w+', normalized) if len(w) > 2]
    stop_words = {'and', 'the', 'with', 'for', 'per', 'from', 'assorted', 'selected', 'various', 'pack'}
    keywords = [w for w in words if w not in stop_words]
    return set(keywords)

def extract_drakes_products(markdown_file):
    """Extract products and prices from drakes.md"""
    products = {}

    with open(markdown_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find all product lines with prices
    patterns = [
        r'-\s+\*\*([^*]+)\*\*\s+-\s+\$(\d+(?:\.\d{2})?)\s*(?:ea|kg|pk|dz|bunch|/kg)?',
        r'-\s+\$(\d+(?:\.\d{2})?)\s+(?:ea|kg|pk|dz|bunch|/kg)\s+([^-\n]+)',
        r'-\s+([^$-]+?)\s+-\s+\$(\d+(?:\.\d{2})?)',
    ]

    for pattern in patterns:
        matches = re.findall(pattern, content)
        for match in matches:
            if len(match) == 2:
                if match[0].replace('.', '').isdigit():
                    price, product_name = match
                else:
                    product_name, price = match

                product_name = product_name.strip()
                if 'save' in product_name.lower():
                    continue
                products[product_name] = float(price)

    return products

def find_best_match(json_product_name, drakes_products):
    """Find the best matching Drakes product"""
    json_keywords = extract_keywords(json_product_name)

    best_match = None
    best_score = 0

    for drakes_name, drakes_price in drakes_products.items():
        drakes_keywords = extract_keywords(drakes_name)

        common_keywords = json_keywords & drakes_keywords

        if len(common_keywords) == 0:
            continue

        if len(json_keywords) == 0 or len(drakes_keywords) == 0:
            continue

        # Calculate match score based on common keywords
        score = len(common_keywords) * (len(common_keywords) / max(len(json_keywords), len(drakes_keywords)))

        if score > best_score:
            best_score = score
            best_match = (drakes_name, drakes_price, score)

    # Lower threshold to catch more matches
    if best_score >= 0.25:
        return best_match

    return None

def update_price_comparison(json_file, drakes_file):
    """Update price_comparison_data.json with Drakes prices"""

    with open(json_file, 'r', encoding='utf-8') as f:
        price_data = json.load(f)

    drakes_products = extract_drakes_products(drakes_file)
    print(f"Extracted {len(drakes_products)} products from drakes.md\n")

    matches_found = 0
    total_products = 0

    # Navigate the correct JSON structure
    if 'categories' in price_data:
        for category_name, category_data in price_data['categories'].items():
            if 'products' in category_data:
                # Check all store types (coles, woolworths, etc.)
                for store_type, products in category_data['products'].items():
                    for product in products:
                        total_products += 1
                        product_name = product.get('name', '')

                        match = find_best_match(product_name, drakes_products)

                        if match:
                            drakes_name, drakes_price, score = match

                            # Add Drakes pricing info
                            if 'drakes_pricing' not in product:
                                product['drakes_pricing'] = {}

                            product['drakes_pricing'] = {
                                'price': drakes_price,
                                'matched_product': drakes_name,
                                'match_confidence': round(score, 2)
                            }

                            matches_found += 1
                            print(f"[{score:.2f}] {product_name} -> {drakes_name} (${drakes_price})")

    # Update metadata
    if 'Drakes' not in price_data['metadata']['stores']:
        price_data['metadata']['stores'].append('Drakes')

    # Save updated JSON
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(price_data, f, indent=2, ensure_ascii=False)

    print(f"\nTotal products scanned: {total_products}")
    print(f"Total matches found: {matches_found}")
    print(f"Match rate: {(matches_found/total_products*100):.1f}%")
    print(f"Updated {json_file}")

# Run the update
update_price_comparison(
    'price_comparison_data.json',
    'drakes.md'
)
