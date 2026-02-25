import json
import re
from typing import List, Dict, Optional

def parse_catalog_to_json(input_file: str, output_file: str) -> None:
    """
    Convert catalog_extracted.txt to JSON format with better parsing
    """
    products = []
    product_counter = 1

    with open(input_file, 'r', encoding='utf-8') as file:
        content = file.read()

    # Split into sections and process each product area
    lines = content.split('\n')

    i = 0
    while i < len(lines):
        line = lines[i].strip()

        # Skip empty lines, page headers, and table markers
        if not line or line.startswith('---') or 'QLD-METRO' in line or '[TABLES FOUND]' in line or '[END TABLES]' in line:
            i += 1
            continue

        # Look for price patterns like $$17 or $$3225
        price_match = re.search(r'\$\$(\d{1,4})(?:\s+(\d{2}))?\s*(?:eeaa|ea)?', line)
        if price_match:
            dollars = price_match.group(1)
            cents = price_match.group(2) if price_match.group(2) else '00'

            # Parse price correctly
            if len(dollars) >= 3:
                # Format like $$3225 -> $32.25
                price = f"${dollars[:-2]}.{dollars[-2:]}"
            else:
                # Format like $$17 00 -> $17.00
                price = f"${dollars}.{cents}"

            # Look around this line for product information
            product_info = extract_product_details(lines, i)

            if product_info and product_info.get('name'):
                # Clean up the product name
                product_name = clean_product_name(product_info['name'])
                brand = extract_brand(product_name)
                category = categorize_product(product_name)

                product = {
                    "productID": f"CL{product_counter:03d}",
                    "productName": product_name,
                    "category": category,
                    "brand": brand,
                    "description": generate_description(product_name, product_info),
                    "price": price,
                    "originalPrice": product_info.get('original_price', price),
                    "savings": product_info.get('savings', '$0.00'),
                    "specialType": product_info.get('special_type', 'Regular Price')
                }

                products.append(product)
                product_counter += 1

        i += 1

    # Write to JSON file
    with open(output_file, 'w', encoding='utf-8') as file:
        json.dump(products, file, indent=2, ensure_ascii=False)

    print(f"Converted {len(products)} products to {output_file}")

def extract_product_details(lines: List[str], price_line_index: int) -> Dict:
    """Extract product details from surrounding lines"""
    details = {}

    # Look for special type indicators
    for offset in range(-3, 4):
        idx = price_line_index + offset
        if 0 <= idx < len(lines):
            line = lines[idx].strip()
            if '1/2 PRICE' in line:
                details['special_type'] = '1/2 Price'
            elif 'DOWN DOWN' in line:
                details['special_type'] = 'Down Down'

    # Look for SAVE and WAS information
    for offset in range(-5, 6):
        idx = price_line_index + offset
        if 0 <= idx < len(lines):
            line = lines[idx].strip()

            # Extract savings
            save_match = re.search(r'SAVE \$(\d+(?:\.\d+)?)', line)
            if save_match:
                details['savings'] = f"${save_match.group(1)}"
                if not details.get('special_type'):
                    details['special_type'] = 'Special'

            # Extract original price
            was_match = re.search(r'WAS \$(\d+(?:\.\d+)?)', line)
            if was_match:
                details['original_price'] = f"${was_match.group(1)}"

    # Look for product name in nearby lines
    potential_names = []
    for offset in range(-8, 5):
        idx = price_line_index + offset
        if 0 <= idx < len(lines):
            line = lines[idx].strip()

            # Skip lines with just symbols, numbers, or price info
            if not line or re.match(r'^\$\$|^\d+→|^SAVE|^WAS|^eeaa|^\d+$', line):
                continue

            # Look for lines that contain actual product names
            if is_product_name_line(line):
                potential_names.append((abs(offset), line))

    # Choose the best product name (closest to price and most descriptive)
    if potential_names:
        potential_names.sort()  # Sort by distance from price
        details['name'] = potential_names[0][1]

    return details

def is_product_name_line(line: str) -> bool:
    """Check if a line likely contains a product name"""
    line_lower = line.lower()

    # Common product/brand indicators
    product_indicators = [
        'coles', 'cadbury', 'coca-cola', 'pepsi', 'nestle', 'arnott', 'smith', 'kettle',
        'beef', 'chicken', 'pork', 'lamb', 'salmon', 'prawns', 'bacon', 'ham',
        'bread', 'milk', 'cheese', 'yogurt', 'butter', 'rice', 'pasta', 'sauce',
        'chips', 'chocolate', 'biscuit', 'cookie', 'cake', 'pie', 'crackers',
        'shampoo', 'soap', 'toothpaste', 'deodorant', 'cream', 'lotion',
        'nappies', 'wipes', 'tissue', 'toilet', 'cleaning', 'vitamins',
        'drink', 'juice', 'water', 'coffee', 'tea', 'energy', 'soft drink'
    ]

    # Must contain at least one indicator and be reasonable length
    has_indicator = any(indicator in line_lower for indicator in product_indicators)
    reasonable_length = 15 <= len(line) <= 150

    # Exclude lines that are clearly not product names
    excludes = ['per kg', 'per 100g', 'serving suggestion', 'selected stores', 'while stocks last']
    has_exclude = any(exclude in line_lower for exclude in excludes)

    return has_indicator and reasonable_length and not has_exclude

def clean_product_name(name: str) -> str:
    """Clean up product name"""
    # Remove line numbers and arrows
    name = re.sub(r'^\d+→', '', name)
    # Remove extra whitespace
    name = re.sub(r'\s+', ' ', name)
    # Remove price per unit info
    name = re.sub(r'\$[\d.]+\s*per\s*\w+', '', name)
    # Clean up common OCR artifacts
    name = re.sub(r'\s*eeaa\s*', '', name)
    name = name.strip()
    return name[:100]  # Limit length

def extract_brand(product_name: str) -> str:
    """Extract brand from product name"""
    known_brands = [
        'Coles', 'Cadbury', 'Coca-Cola', 'Pepsi', 'Nestle', 'Arnott', 'Smith',
        'Huggies', 'Colgate', 'L\'Oreal', 'Nivea', 'Dettol', 'Pantene',
        'Nature\'s Way', 'Blackmores', 'Swisse', 'Berocca', 'Fantastic',
        'Kettle', 'V Energy', 'Kirks', 'Quilton', 'Nice & Natural',
        'Betty Crocker', 'Moccona', 'Lipton', 'Oreo', 'Life Savers',
        'Lindt', 'Ferrero', 'Sunrice', 'John West', 'SPC', 'MasterFoods',
        'McCain', 'Four\'N Twenty', 'Golden Circle', 'Uncle Tobys',
        'Kellogg\'s', 'Bonne Maman', 'Vegemite', 'Campbell\'s'
    ]

    product_lower = product_name.lower()
    for brand in known_brands:
        if brand.lower() in product_lower:
            return brand

    # Try first word if it looks like a brand
    words = product_name.split()
    if words and len(words[0]) > 2 and words[0][0].isupper():
        return words[0]

    return 'Generic'

def categorize_product(product_name: str) -> str:
    """Categorize product based on name"""
    name_lower = product_name.lower()

    categories = {
        'Meat & Seafood': ['beef', 'chicken', 'pork', 'lamb', 'salmon', 'prawns', 'fish', 'bacon', 'ham', 'steak', 'mince'],
        'Dairy': ['milk', 'cheese', 'yogurt', 'yoghurt', 'butter', 'cream'],
        'Pantry': ['rice', 'pasta', 'sauce', 'oil', 'stock', 'gravy', 'beans', 'tuna', 'flour', 'sugar'],
        'Bakery': ['bread', 'roll', 'bagel', 'muffin', 'cake', 'pie', 'pastry', 'biscuit', 'cookie'],
        'Fresh Produce': ['apple', 'banana', 'potato', 'onion', 'carrot', 'lettuce', 'tomato', 'capsicum', 'cucumber'],
        'Snacks': ['chips', 'crackers', 'nuts', 'popcorn', 'pretzels'],
        'Confectionery': ['chocolate', 'lollies', 'candy'],
        'Beverages': ['soft drink', 'juice', 'water', 'coffee', 'tea', 'cola', 'pepsi', 'energy drink', 'drink'],
        'Frozen': ['ice cream', 'frozen', 'pizza'],
        'Health & Beauty': ['shampoo', 'soap', 'toothpaste', 'deodorant', 'cream', 'lotion'],
        'Baby & Child': ['nappies', 'baby', 'wipes', 'formula'],
        'Household': ['toilet paper', 'tissues', 'cleaning', 'detergent', 'batteries'],
        'Vitamins': ['vitamin', 'magnesium', 'calcium', 'supplement', 'tablets', 'capsules']
    }

    for category, keywords in categories.items():
        if any(keyword in name_lower for keyword in keywords):
            return category

    return 'General'

def generate_description(product_name: str, product_info: Dict) -> str:
    """Generate description for product"""
    # Extract size info if present
    size_match = re.search(r'(\d+(?:\.\d+)?(?:kg|g|ml|l|pack|litre))', product_name.lower())
    size = f" {size_match.group(1)}" if size_match else ""

    base_name = product_name.lower().replace(size, "") if size else product_name.lower()

    if 'organic' in base_name or 'premium' in base_name:
        return f"Premium {base_name.strip()}{size}"
    elif 'family' in base_name or 'bulk' in base_name:
        return f"Family size {base_name.strip()}{size}"
    else:
        return f"Quality {base_name.strip()}{size}"

if __name__ == "__main__":
    input_file = "catalog_extracted.txt"
    output_file = "catalog_products_improved.json"
    parse_catalog_to_json(input_file, output_file)