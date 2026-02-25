import re
import json
from typing import List, Dict, Optional

def parse_catalog_to_json(input_file: str, output_file: str = "catalog_products.json") -> None:
    """Parse the catalog_extracted.txt file into structured JSON format"""
    
    with open(input_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    products = []
    product_id = 1
    
    # Split content by pages
    pages = content.split('--- Page')
    
    for page_content in pages:
        if '[OCR EXTRACTED TEXT]' not in page_content:
            continue
            
        # Extract OCR text section
        ocr_start = page_content.find('[OCR EXTRACTED TEXT]')
        ocr_end = page_content.find('[END OCR]')
        
        if ocr_start == -1:
            continue
            
        if ocr_end == -1:
            ocr_text = page_content[ocr_start + len('[OCR EXTRACTED TEXT]'):]
        else:
            ocr_text = page_content[ocr_start + len('[OCR EXTRACTED TEXT]'):ocr_end]
        
        # Parse products from OCR text
        lines = [line.strip() for line in ocr_text.split('\n') if line.strip()]
        
        i = 0
        while i < len(lines):
            line = lines[i]
            
            # Skip common header/footer text
            if any(skip_word in line.lower() for skip_word in ['woolworths', 'fresh food people', 'on sale', 'collect', 'points', 'click to browse', 'see page']):
                i += 1
                continue
            
            # Look for price patterns
            price_match = re.search(r'\$(\d+)\.?(\d{2})?', line)
            if price_match:
                current_price = f"${price_match.group(1)}.{price_match.group(2) or '00'}"
                
                # Look for savings information in nearby lines
                savings_amount = None
                original_price = None
                special_type = None
                
                # Check previous and next few lines for product info and savings
                product_name = None
                brand = None
                description = None
                category = "Unknown"
                
                # Look backwards for product name and brand
                for j in range(max(0, i-5), i):
                    prev_line = lines[j]
                    
                    # Brand detection
                    if any(brand_name in prev_line.upper() for brand_name in ['PRINGLES', 'CONNOISSEUR', 'NESTLE', 'JOHN WEST', 'FELIX', 'WICKED SISTER']):
                        brand = prev_line.title()
                    
                    # Product name detection
                    if any(product_word in prev_line.lower() for product_word in ['chips', 'ice cream', 'pasta', 'tuna', 'cat food', 'blocks']):
                        if not product_name:
                            product_name = prev_line.title()
                
                # Look forwards for savings and description
                for j in range(i+1, min(len(lines), i+5)):
                    next_line = lines[j]
                    
                    # Savings detection
                    save_match = re.search(r'SAVE \$(\d+\.?\d{0,2})', next_line.upper())
                    if save_match:
                        savings_amount = f"${save_match.group(1)}"
                        if current_price and savings_amount:
                            try:
                                original = float(current_price[1:]) + float(savings_amount[1:])
                                original_price = f"${original:.2f}"
                            except:
                                pass
                    
                    # Special type detection
                    if '1/2 price' in next_line.lower() or 'half price' in next_line.lower():
                        special_type = "HALF PRICE"
                    elif 'off' in next_line.lower() and '%' in next_line:
                        percent_match = re.search(r'(\d+)%', next_line)
                        if percent_match:
                            special_type = f"{percent_match.group(1)}% OFF"
                    
                    # Description/size detection
                    if re.search(r'\d+[gml]|\d+\-\d+[gml]|pk \d+', next_line.lower()):
                        if not description:
                            description = next_line
                
                # Category detection based on keywords
                if any(keyword in (product_name or '').lower() for keyword in ['ice cream', 'frozen']):
                    category = "Frozen"
                elif any(keyword in (product_name or '').lower() for keyword in ['chips', 'snacks']):
                    category = "Snacks"
                elif any(keyword in (product_name or '').lower() for keyword in ['pasta']):
                    category = "Pantry"
                elif any(keyword in (product_name or '').lower() for keyword in ['tuna', 'fish']):
                    category = "Pantry"
                elif any(keyword in (product_name or '').lower() for keyword in ['cat food', 'pet']):
                    category = "Pet Care"
                elif any(keyword in (product_name or '').lower() for keyword in ['chocolate', 'blocks']):
                    category = "Confectionery"
                
                # Create product entry if we have enough information
                if product_name or brand:
                    product = {
                        "productID": f"PROD{product_id:03d}",
                        "productName": product_name or brand or "Unknown Product",
                        "category": category,
                        "brand": brand or "Unknown",
                        "description": description or "",
                        "price": current_price,
                        "originalPrice": original_price,
                        "savings": savings_amount,
                        "specialType": special_type
                    }
                    
                    products.append(product)
                    product_id += 1
            
            i += 1
    
    # Write to JSON file
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(products, f, indent=2, ensure_ascii=False)
    
    print(f"Parsed {len(products)} products and saved to {output_file}")

if __name__ == "__main__":
    parse_catalog_to_json("catalog_extracted.txt")