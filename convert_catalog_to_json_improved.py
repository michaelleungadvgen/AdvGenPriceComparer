import json
import re
from typing import List, Dict, Optional

def clean_text(text: str) -> str:
    """Clean OCR artifacts and normalize text"""
    if not text:
        return ""
    
    # Remove line numbers at start
    text = re.sub(r'^\d+→', '', text)
    
    # Remove excessive repeated characters (OCR artifacts)
    text = re.sub(r'([a-zA-Z])\1{3,}', r'\1\1', text)
    
    # Clean up spacing
    text = re.sub(r'\s+', ' ', text)
    
    # Remove obvious OCR artifacts
    text = re.sub(r'[Xx]{3,}', '', text)
    text = re.sub(r'eee[aa]+', '', text)
    
    return text.strip()

def extract_price(text: str) -> Optional[str]:
    """Extract price from text like $$21 00 or $$4 55"""
    match = re.search(r'\$\$(\d+)(?:\s+(\d+))?', text)
    if match:
        dollars = match.group(1)
        cents = match.group(2) if match.group(2) else '00'
        # Handle reasonable price ranges (under $1000)
        if int(dollars) < 1000:
            return f"${dollars}.{cents.zfill(2)}"
    return None

def extract_was_price(text: str) -> Optional[str]:
    """Extract WAS price from text"""
    match = re.search(r'WAS \$(\d+(?:\.\d+)?)', text)
    if match:
        return f"${match.group(1)}"
    return None

def extract_save_amount(text: str) -> Optional[str]:
    """Extract SAVE amount from text"""
    match = re.search(r'SAVE \$(\d+(?:\.\d+)?)', text)
    if match:
        return f"${match.group(1)}"
    return None

def categorize_product(product_name: str) -> str:
    """Categorize product based on name"""
    if not product_name:
        return "General"
    
    name_lower = product_name.lower()
    
    if any(word in name_lower for word in ['vitamin', 'supplement', 'tablet', 'magnesium', 'fish oil', 'bio', 'blackmores']):
        return "Health & Wellness"
    elif any(word in name_lower for word in ['shampoo', 'conditioner', 'deodorant', 'hand wash', 'toothbrush', 'herbal essences', 'dove', 'rexona']):
        return "Personal Care"
    elif any(word in name_lower for word in ['drink', 'cola', 'sprite', 'gatorade', 'water', 'coca-cola', 'fanta']):
        return "Beverages"
    elif any(word in name_lower for word in ['yogurt', 'burger', 'sausage', 'ice cream', 'drumstick', 'cones', 'farmers union']):
        return "Food"
    elif any(word in name_lower for word in ['pringles', 'crisps']):
        return "Snacks"
    elif any(word in name_lower for word in ['chocolate', 'lindt']):
        return "Confectionery"
    else:
        return "General"

def extract_brand(product_name: str) -> str:
    """Extract brand from product name"""
    if not product_name:
        return "Unknown"
    
    # Known brands mapping
    brands_map = {
        'farmers union': 'Farmers Union',
        'sanitarium': 'Sanitarium',
        'peters': 'Peters',
        'coca-cola': 'Coca-Cola',
        'gatorade': 'Gatorade',
        'pringles': 'Pringles',
        'lindt': 'Lindt',
        'herbal essences': 'Herbal Essences',
        'dove': 'Dove',
        'schwarzkopf': 'Schwarzkopf',
        'rexona': 'Rexona',
        'toni & guy': 'Toni & Guy',
        'blackmores': 'Blackmores',
        'palmolive': 'Palmolive',
        'glow lab': 'Glow Lab',
        'colgate': 'Colgate',
        'beach rd naturals': 'Beach Rd Naturals',
        'thankyou': 'Thankyou',
        'coles': 'Coles'
    }
    
    name_lower = product_name.lower()
    for brand_key, brand_name in brands_map.items():
        if brand_key in name_lower:
            return brand_name
    
    # Fallback: first word
    words = product_name.split()
    return words[0] if words else "Unknown"

def is_valid_product_name(name: str) -> bool:
    """Check if the product name looks valid (not OCR garbage)"""
    if not name or len(name) < 5:
        return False
    
    # Check for excessive repeated characters or obvious OCR artifacts
    if re.search(r'([a-zA-Z])\1{4,}', name):
        return False
    
    # Check for reasonable character distribution
    alpha_chars = sum(c.isalpha() for c in name)
    total_chars = len(name.replace(' ', ''))
    
    if total_chars > 0 and alpha_chars / total_chars < 0.5:
        return False
    
    return True

def parse_catalog_to_json(input_file: str, output_file: str) -> None:
    """
    Convert catalog_extracted.txt to JSON format with improved parsing
    """
    products = []
    product_counter = 1
    
    with open(input_file, 'r', encoding='utf-8') as file:
        content = file.read()
    
    # Split content by pages and process each page
    pages = content.split('--- Page')
    
    for page_idx, page in enumerate(pages):
        if not page.strip():
            continue
            
        lines = [line for line in page.split('\n') if line.strip()]
        
        i = 0
        while i < len(lines):
            line = lines[i].strip()
            
            # Skip headers, tables, and empty lines
            if (not line or line.startswith('---') or 
                'TABLES FOUND' in line or 'END TABLES' in line or
                line.isdigit() or len(line) < 3):
                i += 1
                continue
            
            # Look for price pattern
            current_price = extract_price(line)
            if current_price:
                # Look for product information in surrounding lines
                product_info_lines = []
                save_info_lines = []
                
                # Collect context lines (before and after)
                start_idx = max(0, i - 10)
                end_idx = min(len(lines), i + 10)
                
                for j in range(start_idx, end_idx):
                    context_line = clean_text(lines[j])
                    if context_line and not extract_price(context_line):
                        if 'SAVE' in context_line or 'WAS' in context_line or '1/2 PRICE' in context_line:
                            save_info_lines.append(context_line)
                        elif len(context_line) > 10 and is_valid_product_name(context_line):
                            product_info_lines.append(context_line)
                
                # Find the best product name candidate
                product_name = ""
                description = ""
                
                for info_line in product_info_lines:
                    if (not product_name and 
                        len(info_line) > 15 and 
                        any(char.isalpha() for char in info_line)):
                        product_name = info_line
                    elif product_name and not description and info_line != product_name:
                        description = info_line
                        break
                
                # Extract pricing information
                was_price = current_price
                save_amount = "$0.00"
                special_type = "Regular Price"
                
                for save_line in save_info_lines:
                    if '1/2 PRICE' in save_line:
                        special_type = "1/2 PRICE"
                    
                    was_match = extract_was_price(save_line)
                    if was_match:
                        was_price = was_match
                    
                    save_match = extract_save_amount(save_line)
                    if save_match:
                        save_amount = save_match
                
                # Create product entry if we have valid information
                if product_name and is_valid_product_name(product_name):
                    product = {
                        "productID": f"COL{product_counter:03d}",
                        "productName": product_name[:100],
                        "category": categorize_product(product_name),
                        "brand": extract_brand(product_name),
                        "description": (description[:200] if description else product_name[:200]),
                        "price": current_price,
                        "originalPrice": was_price,
                        "savings": save_amount,
                        "specialType": special_type
                    }
                    
                    products.append(product)
                    product_counter += 1
                    
                    # Skip ahead to avoid duplicates
                    i += 8
                    continue
            
            i += 1
    
    # Write to JSON file
    with open(output_file, 'w', encoding='utf-8') as file:
        json.dump(products, file, indent=2, ensure_ascii=False)
    
    print(f"Converted {len(products)} products to {output_file}")
    
    # Show sample products
    if products:
        print("\nSample products:")
        for product in products[:3]:
            print(f"- {product['productName']} - {product['price']} (Brand: {product['brand']}, Category: {product['category']})")

if __name__ == "__main__":
    input_file = "catalog_extracted.txt"
    output_file = "catalog_products_improved.json"
    parse_catalog_to_json(input_file, output_file)