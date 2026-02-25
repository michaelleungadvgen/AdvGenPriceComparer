#!/usr/bin/env python3
"""
Convert Coles HTML catalogue to JSON format with structured product data.
"""

import json
import re
from bs4 import BeautifulSoup
import argparse
import sys
from pathlib import Path

def parse_price(price_text):
    """Extract numeric price from price text."""
    if not price_text:
        return None

    # Remove currency symbols and extract number
    price_match = re.search(r'\$?(\d+(?:\.\d{2})?)', price_text.replace(',', ''))
    return f"${price_match.group(1)}" if price_match else None

def calculate_savings(original_price, current_price):
    """Calculate savings amount."""
    try:
        orig = float(original_price.replace('$', ''))
        curr = float(current_price.replace('$', ''))
        savings = orig - curr
        return f"${savings:.2f}" if savings > 0 else None
    except:
        return None

def extract_product_data(html_content):
    """Extract product data from Coles HTML catalogue."""
    soup = BeautifulSoup(html_content, 'html.parser')
    products = []
    product_id_counter = 1

    # Split text into lines and process
    lines = soup.get_text().split('\n')
    lines = [line.strip() for line in lines if line.strip()]

    # Navigation/UI elements to skip
    skip_elements = {
        'Skip to main content', 'Search Products', 'Search products & recipes',
        'More from Coles', 'Log in / Sign up', 'Lists', 'Shop products',
        'Specials & catalogues', 'Bought before', 'Recipes & inspiration',
        'Ways to shop', 'Help', 'Click & Collect', 'Sunnybank Hills', 'When',
        'Home', 'Catalogues', 'Next week\'s catalogue', 'Change location',
        'Sign Up', 'Back to catalogue', 'Search a product', 'Email', 'Download PDF',
        'Shop special', 'You can purchase this product starting Wednesday 24th September'
    }

    i = 0
    while i < len(lines):
        line = lines[i]

        # Skip navigation and UI elements
        if line in skip_elements or line.startswith('$0.') or line.startswith('All') or line.startswith('Half Price'):
            i += 1
            continue

        # Look for product patterns - products usually have a descriptive name
        # followed by price information
        if (len(line) > 15 and
            not line.startswith('Was ') and
            not line.startswith('Save ') and
            not re.search(r'\$\d+', line) and
            not line.endswith(' each') and
            not line.endswith(' pk') and
            not line.endswith(' per kg') and
            not line.endswith(' per litre') and
            not line.endswith(' per 100mL') and
            not line.isdigit()):

            # This looks like a product name
            product_name = line

            # Look ahead for duplicate name (common in HTML structure)
            if i + 1 < len(lines) and lines[i + 1] == product_name:
                i += 1  # Skip the duplicate

            # Look for price information in the next few lines
            price = None
            original_price = None
            savings = None
            special_type = None

            j = i + 1
            while j < min(i + 8, len(lines)):  # Look ahead up to 8 lines
                next_line = lines[j]

                # Current price
                if re.search(r'\$\d+\.\d{2} each', next_line):
                    price = parse_price(next_line)
                elif 'for $' in next_line and re.search(r'\$\d+\.\d{2}', next_line):
                    price = parse_price(next_line)
                    special_type = 'Multi-buy'

                # Original price and savings
                elif next_line.startswith('Was ') and '$' in next_line:
                    original_price = parse_price(next_line.replace('Was ', '').split(',')[0])
                    if 'Save ' in next_line:
                        savings = parse_price(next_line.split('Save ')[1])
                    special_type = 'Special'

                # Half price detection
                elif 'Save $' in next_line:
                    savings = parse_price(next_line.replace('Save ', ''))

                j += 1

            # After collecting all price info, check for half price
            if original_price and price:
                try:
                    orig = float(original_price.replace('$', ''))
                    curr = float(price.replace('$', ''))
                    if abs(orig - curr * 2) < 0.01:  # Check if it's half price
                        special_type = '1/2 Price'
                except:
                    pass

            # Only add if we found a price
            if price:
                product = {
                    'productID': f'CL{product_id_counter:03d}',
                    'productName': product_name,
                    'category': categorize_product(product_name),
                    'brand': extract_brand(product_name),
                    'description': product_name,
                    'price': price,
                    'originalPrice': original_price,
                    'savings': savings,
                    'specialType': special_type
                }
                products.append(product)
                product_id_counter += 1

        i += 1

    return products

def categorize_product(product_name):
    """Categorize product based on name."""
    name_lower = product_name.lower()

    if any(word in name_lower for word in ['burger', 'sausage', 'bacon', 'chicken', 'beef', 'pork', 'lamb']):
        return 'Meat, Seafood & Deli'
    elif any(word in name_lower for word in ['bread', 'bakery', 'cake', 'muffin']):
        return 'Bread & Bakery'
    elif any(word in name_lower for word in ['kiwi', 'apple', 'banana', 'raspberry', 'fruit', 'vegetable']):
        return 'Fruit & Vegetables'
    elif any(word in name_lower for word in ['coca-cola', 'fanta', 'sprite', 'drink', 'juice', 'water']):
        return 'Drinks'
    elif any(word in name_lower for word in ['ice cream', 'frozen', 'ben & jerry']):
        return 'Frozen'
    elif any(word in name_lower for word in ['rice', 'pasta', 'crackers', 'shapes']):
        return 'Pantry'
    elif any(word in name_lower for word in ['milk', 'cheese', 'yogurt', 'dairy']):
        return 'Dairy, Eggs & Meals'
    else:
        return 'General'

def extract_brand(product_name):
    """Extract brand from product name."""
    # Common brand patterns
    brands = [
        'Coles', 'Coca-Cola', 'Ben & Jerry\'s', 'Arnott\'s', 'Primo',
        'Sunrice', 'Woolworths', 'Black & Gold'
    ]

    for brand in brands:
        if brand.lower() in product_name.lower():
            return brand

    # Extract first word as potential brand
    words = product_name.split()
    if len(words) > 1:
        return words[0]

    return 'Generic'

def main():
    parser = argparse.ArgumentParser(description='Convert Coles HTML catalogue to JSON')
    parser.add_argument('input_file', help='Input HTML file path')
    parser.add_argument('output_file', nargs='?', help='Output JSON file path')

    args = parser.parse_args()

    input_path = Path(args.input_file)
    if not input_path.exists():
        print(f"Error: Input file '{input_path}' does not exist.")
        sys.exit(1)

    # Set default output file if not provided
    if args.output_file:
        output_path = Path(args.output_file)
    else:
        output_path = input_path.with_suffix('.json')

    try:
        # Read HTML file
        with open(input_path, 'r', encoding='utf-8') as f:
            html_content = f.read()

        # Extract product data
        products = extract_product_data(html_content)

        # Write JSON file
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(products, f, indent=2, ensure_ascii=False)

        print(f"Successfully converted {len(products)} products to {output_path}")

    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()