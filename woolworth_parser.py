import re
import json
from typing import List, Dict, Optional

def parse_woolworth_data(file_path: str) -> List[Dict]:
    """
    Parse Woolworths catalogue data from text file into structured JSON format.
    """
    products = []
    product_id_counter = 1
    
    with open(file_path, 'r', encoding='utf-8') as file:
        lines = [line.strip() for line in file.readlines()]
    
    i = 0
    while i < len(lines):
        if not lines[i] or lines[i].startswith('Special pricing') or lines[i].startswith('Offer valid'):
            i += 1
            continue
        
        # Look for product name pattern (not a price line)
        if not re.match(r'^\$\d+\.\d+', lines[i]) and not re.match(r'^\d+[¢%]', lines[i]) and lines[i]:
            product_name = lines[i]
            
            # Skip duplicate product name line if present
            if i + 1 < len(lines) and lines[i + 1] == product_name:
                i += 1
            
            # Extract size/description from product name
            size_match = re.search(r'(\d+(?:-\d+)?(?:g|ml|L|kg|Pk \d+(?:-\d+)?|\d+g))', product_name)
            description = size_match.group(1) if size_match else ""
            
            # Extract brand from product name
            brand_parts = product_name.split()
            brand = brand_parts[0] if brand_parts else ""
            
            # Look for price in next lines
            price = ""
            original_price = ""
            savings = ""
            special_type = ""
            
            j = i + 1
            while j < len(lines) and j < i + 10:  # Look ahead max 10 lines
                line = lines[j]
                
                # Price pattern
                price_match = re.match(r'^\$(\d+\.\d+)', line)
                if price_match and not price:
                    price = f"${price_match.group(1)}"
                
                # Special offer patterns
                if "Better than 1/2 Price" in line:
                    special_type = "BETTER THAN HALF PRICE"
                    save_match = re.search(r'Save \$(\d+\.\d+)', line)
                    if save_match:
                        savings = f"${save_match.group(1)}"
                        if price:
                            original_price = f"${float(price[1:]) + float(savings[1:]):.2f}"
                elif "1/2 Price" in line or "Half Price" in line:
                    special_type = "HALF PRICE"
                    save_match = re.search(r'Save \$(\d+\.\d+)', line)
                    if save_match:
                        savings = f"${save_match.group(1)}"
                        if price:
                            original_price = f"${float(price[1:]) + float(savings[1:]):.2f}"
                elif "% off" in line:
                    percent_match = re.search(r'(\d+)% off', line)
                    if percent_match:
                        special_type = f"{percent_match.group(1)}% OFF"
                        save_match = re.search(r'Save \$(\d+\.\d+)', line)
                        if save_match:
                            savings = f"${save_match.group(1)}"
                            if price:
                                original_price = f"${float(price[1:]) + float(savings[1:]):.2f}"
                
                # Stop if we hit another product or special pricing info
                if line.startswith('Special pricing') or line.startswith('Offer valid'):
                    break
                
                j += 1
            
            # Determine category based on product name
            category = determine_category(product_name)
            
            if price:  # Only add if we found a price
                product = {
                    "productID": f"WOL{product_id_counter:03d}",
                    "productName": product_name,
                    "category": category,
                    "brand": brand,
                    "description": description,
                    "price": price,
                    "originalPrice": original_price if original_price else price,
                    "savings": savings if savings else "$0.00",
                    "specialType": special_type if special_type else "REGULAR"
                }
                products.append(product)
                product_id_counter += 1
            
            i = j
        else:
            i += 1
    
    return products

def determine_category(product_name: str) -> str:
    """
    Determine product category based on product name.
    """
    name_lower = product_name.lower()
    
    if any(word in name_lower for word in ['ice cream', 'frozen', 'freezer']):
        return "Frozen"
    elif any(word in name_lower for word in ['chips', 'popcorn', 'snack']):
        return "Snacks"
    elif any(word in name_lower for word in ['pasta', 'fridge']):
        return "Fresh"
    elif any(word in name_lower for word in ['cat food', 'dog food', 'pet']):
        return "Pet Care"
    elif any(word in name_lower for word in ['soft drink', 'drink', 'beverage']):
        return "Beverages"
    elif any(word in name_lower for word in ['chocolate', 'blocks']):
        return "Confectionery"
    elif any(word in name_lower for word in ['strawberries', 'fruit', 'vegetable']):
        return "Fresh Produce"
    else:
        return "General"

def main():
    input_file = "woolworth_source.txt"
    output_file = "woolworth_products.json"
    
    try:
        products = parse_woolworth_data(input_file)
        
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(products, f, indent=2, ensure_ascii=False)
        
        print(f"Successfully parsed {len(products)} products from {input_file}")
        print(f"Output saved to {output_file}")
        
        # Display first few products as sample
        if products:
            print("\nSample products:")
            for i, product in enumerate(products[:3]):
                print(f"{i+1}. {product['productName']} - {product['price']}")
    
    except FileNotFoundError:
        print(f"Error: {input_file} not found")
    except Exception as e:
        print(f"Error parsing file: {str(e)}")

if __name__ == "__main__":
    main()