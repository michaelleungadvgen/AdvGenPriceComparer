import json
import re
from typing import List, Dict, Optional

def parse_catalog_to_json(input_file: str, output_file: str) -> None:
    """
    Convert catalog_extracted.txt to JSON format matching the specified structure
    """
    products = []
    product_counter = 1
    
    with open(input_file, 'r', encoding='utf-8') as file:
        content = file.read()
    
    # Split content by pages
    pages = content.split('--- Page')
    
    for page in pages:
        if not page.strip():
            continue
            
        lines = page.split('\n')
        current_product = {}
        collecting_product = False
        price_info = {}
        
        i = 0
        while i < len(lines):
            line = lines[i].strip()
            
            # Skip empty lines and page headers
            if not line or line.startswith('---') or 'TABLES FOUND' in line or 'END TABLES' in line:
                i += 1
                continue
            
            # Look for price patterns like $$2211 00 or $$42 55
            price_match = re.search(r'\$\$(\d+)(?:\s+(\d+))?\s*(?:eeaa)?', line)
            if price_match:
                # Extract price
                dollars = price_match.group(1)
                cents = price_match.group(2) if price_match.group(2) else '00'
                current_price = f"${dollars}.{cents.zfill(2)}"
                
                # Look for SAVE and WAS information in subsequent lines
                save_amount = None
                was_price = None
                special_type = None
                
                # Check next few lines for SAVE/WAS information
                for j in range(i+1, min(i+10, len(lines))):
                    next_line = lines[j].strip()
                    
                    # Look for 1/2 PRICE or similar special types
                    if '1/2 PRICE' in next_line:
                        special_type = "1/2 PRICE"
                    elif 'SAVE' in next_line and 'WAS' in next_line:
                        # Extract save and was amounts
                        save_match = re.search(r'SAVE \$(\d+(?:\.\d+)?)', next_line)
                        was_match = re.search(r'WAS \$(\d+(?:\.\d+)?)', next_line)
                        if save_match:
                            save_amount = f"${save_match.group(1)}"
                        if was_match:
                            was_price = f"${was_match.group(1)}"
                    elif 'SAVE' in next_line:
                        save_match = re.search(r'SAVE \$(\d+(?:\.\d+)?)', next_line)
                        if save_match:
                            save_amount = f"${save_match.group(1)}"
                    elif 'WAS' in next_line:
                        was_match = re.search(r'WAS \$(\d+(?:\.\d+)?)', next_line)
                        if was_match:
                            was_price = f"${was_match.group(1)}"
                
                # Look for product description in previous lines
                product_name = ""
                description = ""
                brand = ""
                category = "General"
                
                # Look backwards for product information
                for j in range(max(0, i-10), i):
                    prev_line = lines[j].strip()
                    if prev_line and not re.match(r'^\$\$|\d+→|SAVE|WAS|eeaa|[TABLES FOUND]', prev_line):
                        # This might be product information
                        if not product_name and len(prev_line) > 10:  # Likely product name
                            product_name = prev_line
                        elif product_name and not description:
                            description = prev_line
                
                # Clean up product name and description
                product_name = re.sub(r'^\d+→', '', product_name).strip()
                description = re.sub(r'^\d+→', '', description).strip()
                
                # Extract brand from product name (first word usually)
                if product_name:
                    words = product_name.split()
                    if words:
                        brand = words[0]
                        
                    # Categorize based on keywords
                    product_lower = product_name.lower()
                    if any(word in product_lower for word in ['vitamin', 'supplement', 'tablet', 'magnesium', 'fish oil']):
                        category = "Health & Wellness"
                    elif any(word in product_lower for word in ['shampoo', 'conditioner', 'deodorant', 'hand wash', 'toothbrush']):
                        category = "Personal Care"
                    elif any(word in product_lower for word in ['drink', 'cola', 'sprite', 'gatorade', 'water']):
                        category = "Beverages"
                    elif any(word in product_lower for word in ['yogurt', 'burger', 'sausage', 'ice cream']):
                        category = "Food"
                
                # Create product entry if we have enough information
                if product_name and current_price:
                    product = {
                        "productID": f"CL{product_counter:03d}",
                        "productName": product_name[:100],  # Limit length
                        "category": category,
                        "brand": brand,
                        "description": (description[:200] if description else f"Quality {product_name[:180]}"),
                        "price": current_price,
                        "originalPrice": was_price if was_price else current_price,
                        "savings": save_amount if save_amount else "$0.00",
                        "specialType": special_type if special_type else "Regular Price"
                    }
                    
                    products.append(product)
                    product_counter += 1
                    
                    # Skip ahead to avoid duplicates
                    i += 5
                    continue
            
            i += 1
    
    # Write to JSON file
    with open(output_file, 'w', encoding='utf-8') as file:
        json.dump(products, file, indent=2, ensure_ascii=False)
    
    print(f"Converted {len(products)} products to {output_file}")

if __name__ == "__main__":
    input_file = "catalog_extracted.txt"
    output_file = "catalog_products.json"
    parse_catalog_to_json(input_file, output_file)