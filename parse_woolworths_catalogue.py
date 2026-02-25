import re
import json
from typing import List, Dict, Any

def parse_woolworths_catalogue(input_file: str, output_file: str) -> None:
    """
    Parse Woolworths catalogue text file and convert to JSON format
    """
    products = []
    product_id_counter = 1
    
    with open(input_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Split into pages
    pages = content.split('=== PAGE')
    
    for page in pages[1:]:  # Skip the first empty split
        lines = page.split('\n')
        
        i = 0
        while i < len(lines):
            line = lines[i].strip()
            
            # Skip line numbers and empty lines
            if not line or '→' in line[:10]:
                if '→' in line:
                    actual_line = line.split('→', 1)[1] if '→' in line else line
                    line = actual_line.strip()
                else:
                    i += 1
                    continue
            
            # Look for product patterns
            if is_product_line(line) and i + 1 < len(lines):
                next_line = get_clean_line(lines[i + 1])
                
                # Check if next line contains price
                if is_price_line(next_line):
                    product_name = line
                    price_line = next_line
                    
                    # Look for savings information
                    savings = "$0.00"
                    original_price = extract_price(price_line)
                    special_type = "REGULAR"
                    
                    # Check next few lines for savings info
                    for j in range(i + 2, min(i + 5, len(lines))):
                        check_line = get_clean_line(lines[j])
                        if "Save" in check_line or "Price" in check_line:
                            savings_match = re.search(r'Save \$?([\d.]+)', check_line)
                            if savings_match:
                                savings = f"${savings_match.group(1)}"
                                # Calculate original price
                                try:
                                    current_price = float(original_price.replace('$', ''))
                                    savings_amount = float(savings.replace('$', ''))
                                    original_price = f"${current_price + savings_amount:.2f}"
                                    special_type = "SPECIAL"
                                except:
                                    pass
                            break
                    
                    # Extract brand (first word of product name usually)
                    brand = extract_brand(product_name)
                    
                    # Create product entry
                    product = {
                        "productID": f"WOL{product_id_counter:03d}",
                        "productName": product_name,
                        "category": "General",
                        "brand": brand,
                        "description": "",
                        "price": extract_price(price_line),
                        "originalPrice": original_price,
                        "savings": savings,
                        "specialType": special_type
                    }
                    
                    products.append(product)
                    product_id_counter += 1
                    
                    # Skip the processed lines
                    i += 2
                    continue
            
            i += 1
    
    # Write to JSON file
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(products, f, indent=2, ensure_ascii=False)
    
    print(f"Converted {len(products)} products to {output_file}")

def get_clean_line(line: str) -> str:
    """Extract clean line content, removing line numbers"""
    if '→' in line:
        return line.split('→', 1)[1].strip()
    return line.strip()

def is_product_line(line: str) -> bool:
    """Check if line looks like a product name"""
    if not line or len(line) < 10:
        return False
    
    # Skip common non-product lines
    skip_patterns = [
        'URL:', 'Scraped at:', 'PAGE', '===', 'Special pricing',
        'Offer valid', 'Previous', 'Next', 'Terms and Conditions',
        'powered by', 'Customer Service', 'Shop', 'Browse', 'Help',
        'My Account', 'Log in', 'Sign up', 'View cart', 'Delivery',
        'undefined', 'Choose', 'Select', 'Home', 'Categories', 'Pages'
    ]
    
    for pattern in skip_patterns:
        if pattern in line:
            return False
    
    # Look for product indicators (contains letters and reasonable length)
    if re.search(r'[A-Za-z]{3,}', line) and not line.startswith('$'):
        return True
    
    return False

def is_price_line(line: str) -> bool:
    """Check if line contains price information"""
    return bool(re.search(r'\$\d+\.\d+', line))

def extract_price(price_line: str) -> str:
    """Extract price from price line"""
    match = re.search(r'\$\d+\.\d+', price_line)
    return match.group() if match else "$0.00"

def extract_brand(product_name: str) -> str:
    """Extract brand from product name (first word)"""
    words = product_name.split()
    if words:
        # Common brand extraction logic
        brand = words[0]
        # Handle special cases
        if brand.lower() in ['australian', 'qld']:
            brand = ' '.join(words[:2]) if len(words) > 1 else brand
        return brand
    return ""

if __name__ == "__main__":
    input_file = "woolworths_catalogue_20250909_202227.txt"
    output_file = "woolworths_catalogue_20250909_202227.json"
    
    parse_woolworths_catalogue(input_file, output_file)