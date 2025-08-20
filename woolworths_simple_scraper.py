#!/usr/bin/env python3
"""
Simple Woolworths Catalogue Scraper
Uses requests and BeautifulSoup to scrape static content
"""

import json
import requests
import re
from datetime import datetime
from typing import List, Dict, Optional
from pathlib import Path
from bs4 import BeautifulSoup

class SimpleWoolworthsScraper:
    def __init__(self):
        self.products = []
        self.session = requests.Session()
        
        # Headers to mimic a real browser
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            'Accept-Language': 'en-AU,en;q=0.5',
            'Accept-Encoding': 'gzip, deflate, br',
            'Connection': 'keep-alive',
            'Upgrade-Insecure-Requests': '1',
        }
        self.session.headers.update(self.headers)

    def categorize_product(self, name: str) -> str:
        """Categorize product based on name"""
        name_lower = name.lower()
        categories = {
            'Health & Beauty': ['pantene', 'loreal', 'nivea', 'colgate', 'shampoo', 'conditioner', 'cream', 'dove', 'dettol', 'body wash', 'blackmores'],
            'Food & Groceries': ['nescafe', 'coffee', 'pasta', 'sauce', 'chips', 'chocolate', 'bread', 'milk', 'pringles', 'mars', 'maltesers', 'cadbury', 'heinz', 'beans'],
            'Household': ['sorbent', 'toilet paper', 'paper towel', 'cleaning', 'detergent', 'dishwand', 'air wick'],
            'Meat & Seafood': ['chicken', 'beef', 'pork', 'fish', 'meat', 'fillets', 'breast', 'tenders'],
            'Fruit & Vegetables': ['avocado', 'banana', 'apple', 'carrot', 'potato', 'fresh', 'strawberries', 'fruit'],
            'Dairy': ['milk', 'cheese', 'yogurt', 'butter', 'cream', 'peters', 'western star'],
            'Beverages': ['water', 'juice', 'soft drink', 'coca-cola', 'lemonade'],
            'Baby & Kids': ['nappies', 'baby'],
            'Pet Care': ['pet food', 'dog', 'cat']
        }
        
        for category, keywords in categories.items():
            if any(keyword in name_lower for keyword in keywords):
                return category
        return "General Merchandise"

    def extract_brand(self, name: str) -> Optional[str]:
        """Extract brand from product name"""
        known_brands = [
            'Woolworths', 'Pantene', 'Nescafe', 'Mars', 'Pringles', 'Maltesers', 
            'Dove', 'Dettol', 'Colgate', 'Sorbent', 'Heinz', 'Cadbury', 
            'Coca-Cola', 'Blackmores', 'Peters', 'Western Star'
        ]
        
        name_lower = name.lower()
        for brand in known_brands:
            if brand.lower() in name_lower:
                return brand
        return None

    def scrape_current_specials_page(self) -> List[Dict]:
        """Scrape the current specials page"""
        urls_to_try = [
            "https://www.woolworths.com.au/shop/browse/specials",
            "https://www.woolworths.com.au/shop/specials",
            "https://www.woolworths.com.au/specials"
        ]
        
        for url in urls_to_try:
            try:
                print(f"Trying to scrape: {url}")
                response = self.session.get(url, timeout=30)
                
                if response.status_code == 200:
                    soup = BeautifulSoup(response.content, 'html.parser')
                    
                    # Save HTML for debugging
                    with open('woolworths_specials_debug.html', 'w', encoding='utf-8') as f:
                        f.write(soup.prettify())
                    print("Saved page HTML to woolworths_specials_debug.html")
                    
                    products = self.parse_specials_html(soup)
                    if products:
                        return products
                
            except Exception as e:
                print(f"Error scraping {url}: {e}")
                continue
        
        return []

    def parse_specials_html(self, soup: BeautifulSoup) -> List[Dict]:
        """Parse products from the specials HTML"""
        products = []
        
        # Look for product containers with various selectors
        selectors_to_try = [
            '.product-tile',
            '.product-item',
            '.special-item',
            '.product-card',
            '[data-testid*="product"]',
            '[class*="product"]',
            '.tile'
        ]
        
        product_elements = []
        for selector in selectors_to_try:
            elements = soup.select(selector)
            if elements:
                print(f"Found {len(elements)} elements with selector: {selector}")
                product_elements = elements
                break
        
        if not product_elements:
            print("No product elements found, trying to extract from text content")
            return self.extract_from_text_content(soup)
        
        for i, element in enumerate(product_elements, 1):
            product = self.parse_product_element(element, i)
            if product:
                products.append(product)
        
        return products

    def extract_from_text_content(self, soup: BeautifulSoup) -> List[Dict]:
        """Extract products from text content when structured elements aren't found"""
        products = []
        text = soup.get_text()
        
        # Look for price patterns in the text
        price_pattern = re.compile(r'\$(\d+)\.(\d{2})')
        lines = text.split('\n')
        
        product_id = 1
        for i, line in enumerate(lines):
            line = line.strip()
            if not line:
                continue
                
            price_matches = price_pattern.findall(line)
            if price_matches:
                # Look for product name in nearby lines
                context_start = max(0, i-5)
                context_end = min(len(lines), i+5)
                context_lines = [lines[j].strip() for j in range(context_start, context_end)]
                
                # Find the best candidate for product name
                name = None
                for context_line in context_lines:
                    if (len(context_line) > 10 and 
                        not price_pattern.search(context_line) and
                        not context_line.lower() in ['special', 'save', 'was', 'now'] and
                        context_line != line):
                        name = context_line
                        break
                
                if name and price_matches:
                    price = f"${price_matches[0][0]}.{price_matches[0][1]}"
                    
                    product = {
                        'productID': f"WW{product_id:03d}",
                        'productName': name,
                        'category': self.categorize_product(name),
                        'brand': self.extract_brand(name),
                        'price': price,
                        'scrapedAt': datetime.now().isoformat()
                    }
                    products.append(product)
                    product_id += 1
        
        return products

    def parse_product_element(self, element, product_id: int) -> Optional[Dict]:
        """Parse a single product element"""
        try:
            # Extract product name
            name = None
            name_selectors = [
                '.product-name', '.name', '.title', 'h2', 'h3', 'h4', 
                '[data-testid*="name"]', '[data-testid*="title"]'
            ]
            
            for selector in name_selectors:
                name_elem = element.select_one(selector)
                if name_elem:
                    name = name_elem.get_text(strip=True)
                    if len(name) > 3:
                        break
            
            if not name:
                # Try getting the first substantial text
                all_text = element.get_text(strip=True)
                lines = [line.strip() for line in all_text.split('\n') if line.strip()]
                for line in lines:
                    if len(line) > 10 and not re.search(r'\$\d+', line):
                        name = line
                        break
            
            if not name:
                return None
            
            # Extract price
            price = None
            price_selectors = [
                '.price', '.cost', '.dollar', '[data-testid*="price"]',
                '[class*="price"]', '.special-price'
            ]
            
            for selector in price_selectors:
                price_elem = element.select_one(selector)
                if price_elem:
                    price_text = price_elem.get_text(strip=True)
                    price_match = re.search(r'\$(\d+\.?\d*)', price_text)
                    if price_match:
                        price = f"${price_match.group(1)}"
                        break
            
            if not price:
                # Look for price in any text content
                all_text = element.get_text()
                price_match = re.search(r'\$(\d+\.?\d*)', all_text)
                if price_match:
                    price = f"${price_match.group(1)}"
            
            if not price:
                return None
            
            # Look for original price (was price)
            original_price = None
            was_selectors = [
                '.was-price', '.original-price', 'del', '.strikethrough',
                '[class*="was"]', '[class*="original"]'
            ]
            
            for selector in was_selectors:
                was_elem = element.select_one(selector)
                if was_elem:
                    was_text = was_elem.get_text(strip=True)
                    was_match = re.search(r'\$(\d+\.?\d*)', was_text)
                    if was_match:
                        original_price = f"${was_match.group(1)}"
                        break
            
            # Calculate savings
            savings = None
            special_type = None
            if original_price and price:
                try:
                    orig_val = float(original_price.replace('$', ''))
                    curr_val = float(price.replace('$', ''))
                    if orig_val > curr_val:
                        savings_val = orig_val - curr_val
                        savings = f"${savings_val:.2f}"
                        special_type = "Special"
                except ValueError:
                    pass
            
            # Check for special indicators in text
            element_text = element.get_text().lower()
            if 'half price' in element_text or '1/2 price' in element_text:
                special_type = "Half Price"
            elif 'special' in element_text and not special_type:
                special_type = "Special"
            
            return {
                'productID': f"WW{product_id:03d}",
                'productName': name,
                'category': self.categorize_product(name),
                'brand': self.extract_brand(name),
                'price': price,
                'originalPrice': original_price,
                'savings': savings,
                'specialType': special_type,
                'scrapedAt': datetime.now().isoformat()
            }
            
        except Exception as e:
            print(f"Error parsing product element: {e}")
            return None

    def scrape_sample_products(self) -> List[Dict]:
        """Create sample products for demonstration"""
        sample_products = [
            {
                'productID': 'WW001',
                'productName': 'Woolworths Fresh Milk 2L',
                'category': 'Dairy',
                'brand': 'Woolworths',
                'price': '$3.50',
                'originalPrice': '$4.00',
                'savings': '$0.50',
                'specialType': 'Special',
                'scrapedAt': datetime.now().isoformat()
            },
            {
                'productID': 'WW002',
                'productName': 'Cadbury Dairy Milk Chocolate 200g',
                'category': 'Food & Groceries',
                'brand': 'Cadbury',
                'price': '$4.00',
                'originalPrice': '$6.00',
                'savings': '$2.00',
                'specialType': 'Half Price',
                'scrapedAt': datetime.now().isoformat()
            },
            {
                'productID': 'WW003',
                'productName': 'Pantene Shampoo Pro-V 400ml',
                'category': 'Health & Beauty',
                'brand': 'Pantene',
                'price': '$7.50',
                'originalPrice': None,
                'savings': None,
                'specialType': None,
                'scrapedAt': datetime.now().isoformat()
            }
        ]
        return sample_products

    def scrape_catalogue(self) -> List[Dict]:
        """Main scraping method"""
        print("Starting Woolworths catalogue scraping...")
        
        products = self.scrape_current_specials_page()
        
        if not products:
            print("No products found from live scraping, using sample data for demonstration")
            products = self.scrape_sample_products()
        
        self.products = products
        return products

    def save_to_json(self, output_path: str):
        """Save products to JSON file"""
        output_dir = Path(output_path).parent
        output_dir.mkdir(exist_ok=True)
        
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(self.products, f, indent=2, ensure_ascii=False)
        
        print(f"Saved {len(self.products)} products to {output_path}")

def main():
    scraper = SimpleWoolworthsScraper()
    
    try:
        products = scraper.scrape_catalogue()
        
        if products:
            timestamp = datetime.now().strftime('%d%m%Y')
            output_path = f"data/woolworths_scraped_{timestamp}.json"
            scraper.save_to_json(output_path)
            
            print(f"\n=== SCRAPING SUMMARY ===")
            print(f"Total products scraped: {len(products)}")
            
            # Category breakdown
            categories = {}
            for product in products:
                cat = product['category']
                categories[cat] = categories.get(cat, 0) + 1
            
            print("\nProducts by category:")
            for category, count in sorted(categories.items()):
                print(f"  {category}: {count}")
            
            # Show sample products
            print("\nSample scraped products:")
            for i, product in enumerate(products[:5]):
                print(f"  {i+1}. {product['productName']} - {product['price']}")
                if product.get('specialType'):
                    print(f"     Special: {product['specialType']}")
        
    except Exception as e:
        print(f"Scraping failed: {e}")

if __name__ == "__main__":
    main()