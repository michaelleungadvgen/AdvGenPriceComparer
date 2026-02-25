import json
import re
import sys

# Configure UTF-8 encoding
sys.stdout.reconfigure(encoding='utf-8')

def normalize_product_name(name):
    """Normalize product name for better matching"""
    # Convert to lowercase
    name = name.lower()
    # Remove size/quantity indicators
    name = re.sub(r'\d+(?:\.\d+)?\s*(?:kg|g|ml|l|pk|pack|ea|bunch|dz)', '', name)
    # Remove extra whitespace
    name = ' '.join(name.split())
    return name

def extract_keywords(name):
    """Extract meaningful keywords from product name"""
    # Normalize first
    normalized = normalize_product_name(name)
    # Extract words longer than 2 characters
    words = [w for w in re.findall(r'\w+', normalized) if len(w) > 2]
    # Remove common filler words
    stop_words = {'and', 'the', 'with', 'for', 'per', 'from', 'assorted', 'selected'}
    keywords = [w for w in words if w not in stop_words]
    return set(keywords)

def extract_drakes_products(markdown_file):
    """Extract products and prices from drakes.md"""
    products = {}

    with open(markdown_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find all product lines with prices
    # Pattern: - **Product Name** - $price [unit]
    pattern = r'-\s+\*\*([^*]+)\*\*\s+-\s+\$(\d+(?:\.\d{2})?)\s*(?:ea|kg|pk|dz|bunch|/kg)?'

    matches = re.findall(pattern, content)

    for product_name, price in matches:
        product_name = product_name.strip()
        # Skip if it's a "Save" amount
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

        # Calculate match score
        common_keywords = json_keywords & drakes_keywords

        if len(common_keywords) == 0:
            continue

        # Score based on number of common keywords and proportion
        if len(json_keywords) == 0 or len(drakes_keywords) == 0:
            continue

        score = len(common_keywords) * (len(common_keywords) / max(len(json_keywords), len(drakes_keywords)))

        # If we have at least 1 meaningful keyword match and it's the best so far
        if score > best_score:
            best_score = score
            best_match = (drakes_name, drakes_price)

    # Return match if score is reasonable (at least 0.5)
    if best_score >= 0.5:
        return best_match

    return None

def update_price_comparison(json_file, drakes_file):
    """Update price_comparison_data.json with Drakes prices"""

    # Load existing JSON
    with open(json_file, 'r', encoding='utf-8') as f:
        price_data = json.load(f)

    # Extract Drakes products
    drakes_products = extract_drakes_products(drakes_file)
    print(f"Extracted {len(drakes_products)} products from drakes.md")

    # Track matches
    matches_found = 0

    # Update prices
    if 'price_comparisons' in price_data:
        for category, products in price_data['price_comparisons'].items():
            for product in products:
                product_name = product.get('product', '')

                # Find best match
                match = find_best_match(product_name, drakes_products)

                if match:
                    drakes_name, drakes_price = match

                    # Add Drakes price
                    if 'drakes' not in product:
                        product['drakes'] = {}
                    product['drakes']['price'] = drakes_price

                    matches_found += 1
                    print(f"Matched: '{product_name}' -> '{drakes_name}' (${drakes_price})")

    # Save updated JSON
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(price_data, f, indent=2, ensure_ascii=False)

    print(f"\nTotal matches found: {matches_found}")
    print(f"Updated {json_file}")

# Run the update
update_price_comparison(
    'price_comparison_data.json',
    'drakes.md'
)
