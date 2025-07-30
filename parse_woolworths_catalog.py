#!/usr/bin/env python3
"""
Woolworths Catalog Parser
Converts OCR-extracted catalog text to structured JSON format
"""

import re
import json
from typing import List, Dict, Optional, Tuple
from pathlib import Path

class WoolworthsProductExtractor:
    def __init__(self):
        self.products = []
        self.current_product_id = 1
        
        # Price patterns
        self.price_pattern = re.compile(r'\$(\d+)\.(\d+)')
        self.save_pattern = re.compile(r'SAVE\s*\$(\d+)\.(\d+)')
        
        # Discount patterns
        self.discount_patterns = {
            '1/2 Price': re.compile(r'1/2\s*Price', re.IGNORECASE),
            'Half Price': re.compile(r'Half\s*Price', re.IGNORECASE),
            'Buy 1 Get 1': re.compile(r'Buy\s*1.*Get\s*1', re.IGNORECASE),
            'Special': re.compile(r'Special', re.IGNORECASE)
        }
        
        # Category keywords for classification
        self.category_keywords = {
            'Health & Beauty': ['olay', 'pantene', 'loreal', 'nivea', 'colgate', 'shampoo', 'conditioner', 'serum', 'cream', 'dove', 'dettol', 'antibacterial', 'body wash', 'fish oil', 'blackmores'],
            'Food & Groceries': ['nescafe', 'coffee', 'pasta', 'sauce', 'chips', 'chocolate', 'bread', 'milk', 'leggo', 'pringles', 'mars', 'maltesers', 'cadbury', 'mccain', 'heinz', 'beans', 'moccona', 'tea', 'bertolli', 'olive oil'],
            'Household': ['sorbent', 'toilet paper', 'paper towel', 'cleaning', 'detergent', 'chux', 'dishwand', 'vileda', 'supermocio', 'earth choice', 'air wick'],
            'Meat & Seafood': ['chicken', 'beef', 'pork', 'fish', 'meat', 'fillets', 'breast', 'tenders', 'salami', 'primo'],
            'Fruit & Vegetables': ['avocado', 'banana', 'apple', 'carrot', 'potato', 'fresh', 'strawberries', 'fruit'],
            'Dairy': ['milk', 'cheese', 'yogurt', 'butter', 'cream', 'danone', 'activia', 'yoplait', 'peters', 'maxibon', 'western star', 'fetta'],
            'Beverages': ['water', 'juice', 'soft drink', 'beer', 'wine', 'coca-cola', 'lemonade', 'spring water'],
            'Baby & Kids': ['millie moon', 'nappies', 'baby'],
            'Pet Care': ['supercoat', 'pet food', 'dog', 'cat'],
            'Electronics': ['usb', 'charger', 'led', 'microwave', 'devant'],
            'Homewares': ['decor', 'thermoglass', 'bottle', 'stainless', 'jackson']
        }
    
    def categorize_product(self, product_name: str, description: str = "") -> str:
        """Categorize product based on name and description"""
        text = (product_name + " " + description).lower()
        
        for category, keywords in self.category_keywords.items():
            if any(keyword in text for keyword in keywords):
                return category
        
        return "General Merchandise"
    
    def extract_brand(self, text: str) -> Optional[str]:
        """Extract brand name from text"""
        known_brands = [
            'Olay', 'Pantene', 'Nescafe', 'Mars', 'Pringles', 'Leggo\'s', 
            'Maltesers', 'M&M\'s', 'Sorbent', 'Woolworths', 'Dove', 'Dettol',
            'Colgate', 'Earth Choice', 'Air Wick', 'Chux', 'Vileda', 'Jackson',
            'Decor', 'Primo', 'Heinz', 'Western Star', 'Danone', 'Yoplait',
            'Peters', 'Cadbury', 'Moccona', 'Coca-Cola', 'Red Rock Deli',
            'Blackmores', 'Millie Moon', 'Supercoat', 'Ramesses', 'Devant',
            'Nice & Natural', 'The Spice Tailor', 'Green\'s', 'Bertolli',
            'Allen\'s', 'Darrell Lea', 'McCain', 'Madura'
        ]
        
        text_lower = text.lower()
        for brand in known_brands:
            if brand.lower() in text_lower:
                return brand
        
        return None
    
    def find_price_context(self, lines: List[str], price_line_idx: int, window_size: int = 10) -> Dict:
        """Find product information around a price line"""
        start_idx = max(0, price_line_idx - window_size)
        end_idx = min(len(lines), price_line_idx + window_size)
        
        context_lines = []
        for i in range(start_idx, end_idx):
            line = lines[i].strip()
            if line and not line.startswith('[') and not line.startswith('---'):
                context_lines.append(line)
        
        return {
            'context': context_lines,
            'start_idx': start_idx,
            'end_idx': end_idx
        }
    
    def clean_ocr_text(self, text: str) -> str:
        """Clean up OCR artifacts and common errors"""
        # Replace common OCR misreads
        replacements = {
            'Leggo S': 'Leggo\'s',
            'M&M S': 'M&M\'s',
            'Allen S': 'Allen\'s',
            'Darrell Lea Blocks': 'Chocolate Blocks',
            'Nice & Natural': 'Nice & Natural',
            'The Spice Tallor': 'The Spice Tailor',
            'Dettol Antibacterial': 'Dettol Antibacterial',
            'Colgate Total': 'Colgate Total',
            'Earth Rescue': 'Earth Choice',
            'Alrwick': 'Air Wick',
            'Chux Dishwand': 'Chux Dishwand',
            'Vileda Supermocio': 'Vileda Supermocio',
            'Jackson': 'Jackson',
            'Decor Thermoglass': 'Decor Thermoglass',
            'Primo Danish': 'Primo Danish',
            'Heinz Baked Beans': 'Heinz Baked Beans',
            'Western Star Gold': 'Western Star Gold',
            'Danone Activia': 'Danone Activia',
            'Yoplait Yop': 'Yoplait Yop',
            'Peters Maxibon': 'Peters Maxibon',
            'Madura Organic': 'Madura Organic',
            'Green S Traditional': 'Green\'s Traditional',
            'Bertoll': 'Bertolli',
            'Cadbury Choc': 'Cadbury Chocolate',
            'Moccona Coffee': 'Moccona Coffee',
            'Coca-Cola': 'Coca-Cola',
            'Red Rock Deli': 'Red Rock Deli',
            'Blackmores Fish Oil': 'Blackmores Fish Oil',
            'Millie Moon': 'Millie Moon',
            'Supercoat': 'Supercoat',
            'Ramesses': 'Ramesses',
            'Devant': 'Devant'
        }
        
        for old, new in replacements.items():
            text = re.sub(old, new, text, flags=re.IGNORECASE)
        
        return text

    def extract_product_info(self, context_lines: List[str], price: str) -> Optional[Dict]:
        """Extract product information from context lines"""
        # Join context and clean up
        full_text = " ".join(context_lines)
        full_text = self.clean_ocr_text(full_text)
        
        # Find potential product names (longer meaningful text)
        product_candidates = []
        for line in context_lines:
            line = line.strip()
            line = self.clean_ocr_text(line)
            
            # Skip short fragments, prices, and discount indicators
            if (len(line) > 3 and 
                not self.price_pattern.search(line) and 
                not any(keyword in line.upper() for keyword in ['SAVE', 'PRICE', 'BUY', 'GET', 'IMAGE', 'DEVICERGB', 'WIDTH', 'HEIGHT', 'BPC']) and
                not line.replace(' ', '').replace('.', '').isdigit() and
                not re.match(r'^\d+\s*(per|Per)', line)):
                
                # Clean up common OCR artifacts
                line = re.sub(r'[^\w\s&\'-]', ' ', line)
                line = re.sub(r'\s+', ' ', line).strip()
                
                # Filter out meaningless fragments
                if (len(line) > 5 and 
                    not line.lower() in ['from the', 'varieties', 'special', 'range was'] and
                    not re.match(r'^[A-Z]{1,3}$', line)):  # Skip single letters or short caps
                    product_candidates.append(line)
        
        if not product_candidates:
            return None
        
        # Choose the best product name candidate
        # Prefer branded products and meaningful names
        best_candidate = None
        for candidate in product_candidates:
            # Prioritize candidates with known brands
            if self.extract_brand(candidate):
                best_candidate = candidate
                break
            # Or candidates that look like product names
            if (len(candidate.split()) >= 2 and 
                not candidate.lower().startswith('qld') and
                not re.match(r'^\d+', candidate)):
                if not best_candidate or len(candidate) > len(best_candidate):
                    best_candidate = candidate
        
        product_name = best_candidate or product_candidates[0]
        
        # Extract brand
        brand = self.extract_brand(full_text)
        
        # Build description from other candidates
        description_parts = [c for c in product_candidates if c != product_name and len(c) > 3][:2]
        description = " ".join(description_parts)
        
        # Find discount information
        discount_type = None
        original_price = None
        savings = None
        
        for discount_name, pattern in self.discount_patterns.items():
            if pattern.search(full_text):
                discount_type = discount_name
                break
        
        # Find savings amount
        save_match = self.save_pattern.search(full_text)
        if save_match:
            savings = f"${save_match.group(1)}.{save_match.group(2)}"
            
            # Try to calculate original price if we have savings
            try:
                current_price = float(price.replace('$', ''))
                savings_amount = float(savings.replace('$', ''))
                original_price = f"${current_price + savings_amount:.2f}"
            except ValueError:
                pass
        elif discount_type == "1/2 Price":
            # For half price items, calculate original price
            try:
                current_price = float(price.replace('$', ''))
                original_price = f"${current_price * 2:.2f}"
                savings = f"${current_price:.2f}"
            except ValueError:
                pass
        
        # Categorize the product
        category = self.categorize_product(product_name, description)
        
        return {
            'productName': product_name.title(),
            'category': category,
            'brand': brand,
            'description': description,
            'price': price,
            'originalPrice': original_price,
            'savings': savings,
            'specialType': discount_type
        }
    
    def parse_catalog(self, file_path: str) -> List[Dict]:
        """Parse the catalog text file and extract products"""
        print(f"Reading catalog file: {file_path}")
        
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        print(f"Total lines: {len(lines)}")
        
        # Find all price occurrences
        price_lines = []
        for i, line in enumerate(lines):
            matches = self.price_pattern.findall(line.strip())
            for match in matches:
                price = f"${match[0]}.{match[1]}"
                # Skip very low prices (likely per unit prices) and very high prices
                price_value = float(f"{match[0]}.{match[1]}")
                if 0.50 <= price_value <= 200.0:  # Reasonable product price range
                    price_lines.append((i, price))
        
        print(f"Found {len(price_lines)} potential product prices")
        
        processed_ranges = set()
        
        for line_idx, price in price_lines:
            # Skip if we've already processed this area
            if any(abs(line_idx - r) < 15 for r in processed_ranges):
                continue
            
            context = self.find_price_context(lines, line_idx)
            product_info = self.extract_product_info(context['context'], price)
            
            if product_info:
                # Add product ID
                product_info['productID'] = f"WW{self.current_product_id:03d}"
                self.current_product_id += 1
                
                self.products.append(product_info)
                processed_ranges.add(line_idx)
                
                print(f"Extracted: {product_info['productName']} - {price}")
        
        print(f"Successfully extracted {len(self.products)} products")
        return self.products
    
    def save_json(self, output_path: str):
        """Save extracted products to JSON file"""
        # Ensure data directory exists
        output_dir = Path(output_path).parent
        output_dir.mkdir(exist_ok=True)
        
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(self.products, f, indent=2, ensure_ascii=False)
        
        print(f"Saved {len(self.products)} products to {output_path}")

def main():
    # Initialize extractor
    extractor = WoolworthsProductExtractor()
    
    # Parse catalog
    catalog_path = r"C:\Users\advgen10\source\repos\AdvGenPriceComparer\catalog_extracted.txt"
    output_path = r"C:\Users\advgen10\source\repos\AdvGenPriceComparer\data\woolworths_24072025.json"
    
    products = extractor.parse_catalog(catalog_path)
    
    # Save to JSON
    extractor.save_json(output_path)
    
    # Print summary
    print("\n=== EXTRACTION SUMMARY ===")
    print(f"Total products extracted: {len(products)}")
    
    # Category breakdown
    categories = {}
    for product in products:
        cat = product['category']
        categories[cat] = categories.get(cat, 0) + 1
    
    print("\nProducts by category:")
    for category, count in sorted(categories.items()):
        print(f"  {category}: {count}")
    
    # Show sample products
    print("\nSample extracted products:")
    for i, product in enumerate(products[:5]):
        print(f"  {i+1}. {product['productName']} - {product['price']}")
        if product['specialType']:
            print(f"     Special: {product['specialType']}")

if __name__ == "__main__":
    main()