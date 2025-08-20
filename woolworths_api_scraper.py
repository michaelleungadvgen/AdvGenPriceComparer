#!/usr/bin/env python3
"""
Woolworths API Catalogue Scraper
Attempts to fetch product data directly from Woolworths API endpoints
"""

import json
import requests
import time
from datetime import datetime
from typing import List, Dict, Optional
from pathlib import Path

class WoolworthsAPIScraper:
    def __init__(self):
        self.products = []
        self.session = requests.Session()
        
        # Headers to mimic a real browser
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
            'Accept': 'application/json, text/plain, */*',
            'Accept-Language': 'en-AU,en;q=0.9',
            'Accept-Encoding': 'gzip, deflate, br',
            'Connection': 'keep-alive',
            'Referer': 'https://www.woolworths.com.au/',
        }
        self.session.headers.update(self.headers)
        
        # Category mapping
        self.category_keywords = {
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

    def categorize_product(self, name: str) -> str:
        """Categorize product based on name"""
        name_lower = name.lower()
        for category, keywords in self.category_keywords.items():
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

    def get_catalogue_api_data(self, sale_id: str = "60903", area_name: str = "QLD") -> List[Dict]:
        """Try to fetch catalogue data from potential API endpoints"""
        
        # Known Woolworths API patterns
        api_endpoints = [
            f"https://www.woolworths.com.au/api/v1/catalogue/specials/{sale_id}",
            f"https://www.woolworths.com.au/api/catalogue/specials?saleId={sale_id}&areaName={area_name}",
            f"https://www.woolworths.com.au/api/v1/catalogue/{sale_id}/products",
            f"https://www.woolworths.com.au/shop/api/catalogue/specials/{sale_id}",
            f"https://www.woolworths.com.au/shop/api/v1/catalogue?saleId={sale_id}",
            "https://www.woolworths.com.au/apis/ui/catalogue/specials",
            "https://www.woolworths.com.au/shop/api/ui/catalogue/specials"
        ]
        
        for endpoint in api_endpoints:
            try:
                print(f"Trying API endpoint: {endpoint}")
                
                # Add specific params for some endpoints
                params = {}
                if "specials" in endpoint and sale_id:
                    params = {
                        'saleId': sale_id,
                        'areaName': area_name,
                        'limit': 100
                    }
                
                response = self.session.get(endpoint, params=params, timeout=10)
                
                if response.status_code == 200:
                    try:
                        data = response.json()
                        print(f"Success! Got data from: {endpoint}")
                        print(f"Response keys: {list(data.keys()) if isinstance(data, dict) else 'List response'}")
                        
                        # Save raw response for analysis
                        with open(f'woolworths_api_response_{sale_id}.json', 'w') as f:
                            json.dump(data, f, indent=2)
                        
                        return self.parse_api_response(data)
                    except json.JSONDecodeError:
                        print(f"Non-JSON response from {endpoint}")
                        continue
                else:
                    print(f"HTTP {response.status_code} from {endpoint}")
                    
            except requests.RequestException as e:
                print(f"Request failed for {endpoint}: {e}")
                continue
            
            time.sleep(1)  # Rate limiting
        
        return []

    def parse_api_response(self, data: Dict) -> List[Dict]:
        """Parse API response to extract product information"""
        products = []
        
        # Handle different response structures
        items_data = None
        
        if isinstance(data, list):
            items_data = data
        elif isinstance(data, dict):
            # Try common keys for product data
            for key in ['products', 'items', 'specials', 'data', 'results', 'content']:
                if key in data:
                    items_data = data[key]
                    break
        
        if not items_data:
            print("Could not find product data in API response")
            return []
        
        if isinstance(items_data, list):
            for i, item in enumerate(items_data):
                product = self.parse_api_product(item, i + 1)
                if product:
                    products.append(product)
        
        return products

    def parse_api_product(self, item: Dict, product_id: int) -> Optional[Dict]:
        """Parse individual product from API response"""
        try:
            # Extract name (try multiple possible keys)
            name = None
            for name_key in ['name', 'displayName', 'title', 'productName', 'description']:
                if name_key in item and item[name_key]:
                    name = str(item[name_key]).strip()
                    break
            
            if not name:
                return None
            
            # Extract price
            price = None
            original_price = None
            
            # Try different price structures
            price_keys = ['price', 'currentPrice', 'salePrice', 'specialPrice']
            for price_key in price_keys:
                if price_key in item:
                    price_obj = item[price_key]
                    if isinstance(price_obj, dict):
                        price = price_obj.get('value') or price_obj.get('amount')
                    elif isinstance(price_obj, (int, float, str)):
                        price = price_obj
                    
                    if price:
                        if isinstance(price, str) and not price.startswith('$'):
                            price = f"${price}"
                        elif isinstance(price, (int, float)):
                            price = f"${price:.2f}"
                        break
            
            # Extract original price
            original_price_keys = ['wasPrice', 'originalPrice', 'listPrice', 'regularPrice']
            for orig_key in original_price_keys:
                if orig_key in item:
                    orig_obj = item[orig_key]
                    if isinstance(orig_obj, dict):
                        original_price = orig_obj.get('value') or orig_obj.get('amount')
                    elif isinstance(orig_obj, (int, float, str)):
                        original_price = orig_obj
                    
                    if original_price:
                        if isinstance(original_price, str) and not original_price.startswith('$'):
                            original_price = f"${original_price}"
                        elif isinstance(original_price, (int, float)):
                            original_price = f"${original_price:.2f}"
                        break
            
            if not price:
                return None
            
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
            
            # Extract special type
            if not special_type:
                special_keys = ['specialType', 'promoType', 'offerType', 'saleType']
                for special_key in special_keys:
                    if special_key in item and item[special_key]:
                        special_type = str(item[special_key])
                        break
            
            # Extract other metadata
            brand = self.extract_brand(name)
            if not brand and 'brand' in item:
                brand = item['brand']
            
            category = self.categorize_product(name)
            if 'category' in item:
                category = item['category']
            
            # Extract image URL
            image_url = None
            image_keys = ['imageUrl', 'image', 'thumbnail', 'mediumImageFile']
            for img_key in image_keys:
                if img_key in item and item[img_key]:
                    image_url = item[img_key]
                    if isinstance(image_url, dict):
                        image_url = image_url.get('url') or image_url.get('src')
                    break
            
            return {
                'productID': f"WW{product_id:03d}",
                'productName': name,
                'category': category,
                'brand': brand,
                'price': price,
                'originalPrice': original_price,
                'savings': savings,
                'specialType': special_type,
                'imageUrl': image_url,
                'scrapedAt': datetime.now().isoformat(),
                'rawData': item  # Keep original for reference
            }
            
        except Exception as e:
            print(f"Error parsing product: {e}")
            return None

    def scrape_catalogue_by_search(self, search_terms: List[str] = None) -> List[Dict]:
        """Try to get products by searching for common items"""
        if not search_terms:
            search_terms = [
                'milk', 'bread', 'cheese', 'chicken', 'beef', 'apple', 'banana',
                'chocolate', 'coffee', 'tea', 'pasta', 'rice', 'yogurt'
            ]
        
        all_products = []
        
        search_endpoints = [
            "https://www.woolworths.com.au/api/v1/ui/search/products",
            "https://www.woolworths.com.au/shop/search/products",
            "https://www.woolworths.com.au/apis/ui/search/products"
        ]
        
        for term in search_terms[:5]:  # Limit to first 5 terms
            for endpoint in search_endpoints:
                try:
                    params = {
                        'searchTerm': term,
                        'pageNumber': 1,
                        'pageSize': 20,
                        'sortType': 'TraderRelevance'
                    }
                    
                    print(f"Searching for '{term}' at {endpoint}")
                    response = self.session.get(endpoint, params=params, timeout=10)
                    
                    if response.status_code == 200:
                        try:
                            data = response.json()
                            products = self.parse_api_response(data)
                            if products:
                                print(f"Found {len(products)} products for '{term}'")
                                all_products.extend(products)
                                break
                        except json.JSONDecodeError:
                            continue
                    
                except requests.RequestException:
                    continue
                
                time.sleep(0.5)  # Rate limiting
        
        # Remove duplicates by product name
        unique_products = {}
        for product in all_products:
            name = product['productName']
            if name not in unique_products:
                unique_products[name] = product
        
        return list(unique_products.values())

    def scrape_catalogue(self, sale_id: str = "60903", area_name: str = "QLD") -> List[Dict]:
        """Main method to scrape catalogue data"""
        print("Attempting to scrape Woolworths catalogue via API...")
        
        # Try direct API access first
        products = self.get_catalogue_api_data(sale_id, area_name)
        
        if not products:
            print("Direct API access failed, trying search-based approach...")
            products = self.scrape_catalogue_by_search()
        
        if products:
            self.products = products
            print(f"Successfully scraped {len(products)} products")
        else:
            print("No products found via API methods")
        
        return products

    def save_to_json(self, output_path: str):
        """Save scraped products to JSON file"""
        output_dir = Path(output_path).parent
        output_dir.mkdir(exist_ok=True)
        
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(self.products, f, indent=2, ensure_ascii=False)
        
        print(f"Saved {len(self.products)} products to {output_path}")

def main():
    scraper = WoolworthsAPIScraper()
    
    try:
        products = scraper.scrape_catalogue()
        
        if products:
            timestamp = datetime.now().strftime('%d%m%Y')
            output_path = f"data/woolworths_api_scraped_{timestamp}.json"
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
                if product['specialType']:
                    print(f"     Special: {product['specialType']}")
        
    except Exception as e:
        print(f"Scraping failed: {e}")

if __name__ == "__main__":
    main()