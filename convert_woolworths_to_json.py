#!/usr/bin/env python3
"""
Convert cleaned Woolworths catalogue text to JSON format.
"""

import re
import json
import sys
from pathlib import Path

def extract_brand_from_name(product_name):
    """Extract brand from product name."""
    # Common brands to look for
    brands = [
        'Woolworths', 'Coles', 'Primo', 'Cadbury', 'Mars', 'Smith\'s', 'CC\'s',
        'Coca-Cola', 'Pepsi', 'Nestle', 'Unilever', 'Dettol', 'Tasmanian Heritage',
        'M&M\'s', 'Maltesers', 'Pods'
    ]

    for brand in brands:
        if brand.lower() in product_name.lower():
            return brand

    # Try to extract first word as potential brand
    words = product_name.split()
    if words:
        # Check if first word looks like a brand (capitalized, not common words)
        first_word = words[0]
        common_words = ['fresh', 'organic', 'natural', 'premium', 'select', 'choice']
        if first_word not in common_words and first_word[0].isupper():
            return first_word

    return "Unknown"

def categorize_product(product_name):
    """Categorize product based on name."""
    name_lower = product_name.lower()

    categories = {
        'Meat & Seafood': ['beef', 'chicken', 'pork', 'lamb', 'bacon', 'sausage', 'fish', 'salmon', 'tuna'],
        'Dairy & Eggs': ['milk', 'cheese', 'yogurt', 'butter', 'cream', 'brie', 'camembert'],
        'Bakery': ['bread', 'buns', 'rolls', 'croissant', 'muffin'],
        'Snacks': ['chips', 'corn chips', 'crackers', 'nuts'],
        'Confectionery': ['chocolate', 'candy', 'maltesers', 'm&m', 'bars', 'cadbury'],
        'Beverages': ['soft drink', 'juice', 'water', 'cola', 'pepsi', 'coca-cola'],
        'Pantry': ['pasta', 'rice', 'flour', 'sugar', 'oil', 'sauce'],
        'Personal Care': ['soap', 'shampoo', 'toothpaste', 'deodorant'],
        'Household': ['detergent', 'cleaner', 'tissue', 'paper towel']
    }

    for category, keywords in categories.items():
        for keyword in keywords:
            if keyword in name_lower:
                return category

    return "General"

def parse_savings(savings_text):
    """Parse savings information."""
    if not savings_text:
        return None, None

    # Extract savings amount
    savings_match = re.search(r'Save \$(\d+\.\d+)', savings_text)
    savings = f"${savings_match.group(1)}" if savings_match else None

    # Determine special type
    if "1/2 Price" in savings_text or "Half Price" in savings_text:
        special_type = "HALF PRICE"
    elif "Better than 1/2 Price" in savings_text:
        special_type = "BETTER THAN HALF PRICE"
    elif "% off" in savings_text:
        special_type = "PERCENTAGE OFF"
    elif "Everyday Low Price" in savings_text:
        special_type = "EVERYDAY LOW PRICE"
    else:
        special_type = "SPECIAL"

    return savings, special_type

def convert_woolworths_to_json(input_file, output_file=None):
    """Convert cleaned Woolworths catalogue to JSON."""
    input_path = Path(input_file)

    if not input_path.exists():
        print(f"Error: Input file '{input_file}' not found")
        return False

    if output_file is None:
        output_path = input_path.parent / f"{input_path.stem}.json"
    else:
        output_path = Path(output_file)

    try:
        with open(input_path, 'r', encoding='utf-8') as f:
            lines = [line.strip() for line in f.readlines() if line.strip()]

        products = []
        current_product = {}
        product_counter = 1

        i = 0
        while i < len(lines):
            line = lines[i]

            # Look for product name (usually the first line of a product block)
            # Product names typically contain letters and may have size info
            if re.search(r'[a-zA-Z]', line) and not line.startswith('$') and not re.match(r'^\d+%?\s*(off|Price)', line):
                # Check if this looks like a product name
                if not any(keyword in line.lower() for keyword in ['special pricing', 'offer valid', 'per kg', 'per litre', 'per 100g']):
                    # Start new product
                    if current_product and 'productName' in current_product:
                        products.append(current_product)

                    current_product = {
                        'productID': f"WOL{product_counter:03d}",
                        'productName': line,
                        'category': categorize_product(line),
                        'brand': extract_brand_from_name(line),
                        'description': line
                    }
                    product_counter += 1

            # Look for price information
            elif line.startswith('$') and 'each' in line:
                price_match = re.search(r'\$(\d+\.\d+)', line)
                if price_match and current_product:
                    current_product['price'] = f"${price_match.group(1)}"

            # Look for savings information
            elif any(keyword in line for keyword in ['off', 'Save', 'Price', 'Better than']):
                if current_product:
                    savings, special_type = parse_savings(line)
                    if savings:
                        current_product['savings'] = savings
                        current_product['specialType'] = special_type

                        # Calculate original price if we have current price and savings
                        if 'price' in current_product:
                            try:
                                current_price = float(current_product['price'].replace('$', ''))
                                savings_amount = float(savings.replace('$', ''))
                                original_price = current_price + savings_amount
                                current_product['originalPrice'] = f"${original_price:.2f}"
                            except ValueError:
                                pass

            i += 1

        # Add the last product
        if current_product and 'productName' in current_product:
            products.append(current_product)

        # Clean up products - ensure all have required fields
        cleaned_products = []
        for product in products:
            if 'productName' in product and 'price' in product:
                # Ensure all fields exist
                if 'originalPrice' not in product:
                    product['originalPrice'] = product['price']
                if 'savings' not in product:
                    product['savings'] = "$0.00"
                if 'specialType' not in product:
                    product['specialType'] = "REGULAR"

                cleaned_products.append(product)

        # Write to JSON file
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(cleaned_products, f, indent=2, ensure_ascii=False)

        print(f"Successfully converted catalogue to JSON:")
        print(f"  Input:  {input_path}")
        print(f"  Output: {output_path}")
        print(f"  Products extracted: {len(cleaned_products)}")

        return True

    except Exception as e:
        print(f"Error processing file: {e}")
        return False

def main():
    """Main function to handle command line usage."""
    if len(sys.argv) < 2:
        print("Usage: python convert_woolworths_to_json.py <input_file> [output_file]")
        print("Example: python convert_woolworths_to_json.py woolworths_catalogue_20250923_201005_cleaned.txt")
        return

    input_file = sys.argv[1]
    output_file = sys.argv[2] if len(sys.argv) > 2 else None

    convert_woolworths_to_json(input_file, output_file)

if __name__ == "__main__":
    main()