#!/usr/bin/env python3
"""
Woolworths Web Catalogue Scraper
Scrapes live product information and prices from Woolworths online catalogues
"""

import json
import time
import re
from datetime import datetime
from typing import List, Dict, Optional
from pathlib import Path
from dataclasses import dataclass

import requests
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.chrome.service import Service
from selenium.common.exceptions import TimeoutException, NoSuchElementException
from webdriver_manager.chrome import ChromeDriverManager
from bs4 import BeautifulSoup

@dataclass
class Product:
    name: str
    price: str
    original_price: Optional[str] = None
    brand: Optional[str] = None
    category: Optional[str] = None
    special_type: Optional[str] = None
    savings: Optional[str] = None
    image_url: Optional[str] = None
    product_url: Optional[str] = None

class WoolworthsWebScraper:
    def __init__(self, headless: bool = True):
        self.products = []
        self.setup_driver(headless)
        self.base_url = "https://www.woolworths.com.au/shop/catalogue/view#view=search&saleId=60903"
        
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

    def setup_driver(self, headless: bool):
        """Setup Chrome WebDriver with options"""
        chrome_options = Options()
        if headless:
            chrome_options.add_argument("--headless=new")
        
        # More stable Chrome options
        chrome_options.add_argument("--no-sandbox")
        chrome_options.add_argument("--disable-dev-shm-usage")
        chrome_options.add_argument("--disable-gpu")
        chrome_options.add_argument("--disable-web-security")
        chrome_options.add_argument("--allow-running-insecure-content")
        chrome_options.add_argument("--disable-extensions")
        chrome_options.add_argument("--disable-plugins")
        chrome_options.add_argument("--disable-images")
        chrome_options.add_argument("--disable-javascript")
        chrome_options.add_argument("--window-size=1920,1080")
        chrome_options.add_argument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")
        
        # Add prefs to disable images and other resources for speed
        prefs = {
            "profile.managed_default_content_settings.images": 2,
            "profile.default_content_settings.popups": 0,
            "profile.managed_default_content_settings.media_stream": 2,
        }
        chrome_options.add_experimental_option("prefs", prefs)
        
        try:
            service = Service(ChromeDriverManager().install())
            self.driver = webdriver.Chrome(service=service, options=chrome_options)
            self.driver.set_page_load_timeout(30)
            self.driver.implicitly_wait(10)
        except Exception as e:
            print(f"Failed to initialize Chrome driver: {e}")
            print("Trying with basic options...")
            chrome_options = Options()
            if headless:
                chrome_options.add_argument("--headless=new")
            chrome_options.add_argument("--no-sandbox")
            service = Service(ChromeDriverManager().install())
            self.driver = webdriver.Chrome(service=service, options=chrome_options)

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

    def scrape_catalogue_page(self, catalogue_url: str) -> List[Product]:
        """Scrape products from a Woolworths catalogue page"""
        print(f"Scraping catalogue: {catalogue_url}")
        
        try:
            self.driver.get(catalogue_url)
            print("Page loaded, waiting for content...")
            
            # Wait longer and try multiple approaches
            time.sleep(5)
            
            # Try to wait for any content to load
            possible_selectors = [
                ".catalogueProduct", ".catalogue-product", ".product-tile", 
                ".product-item", "[data-testid*='product']", ".special-product",
                "[class*='product']", "[class*='item']", "[data-cy*='product']"
            ]
            
            found_products = False
            for selector in possible_selectors:
                try:
                    WebDriverWait(self.driver, 10).until(
                        EC.presence_of_element_located((By.CSS_SELECTOR, selector))
                    )
                    print(f"Found products using selector: {selector}")
                    found_products = True
                    break
                except TimeoutException:
                    continue
            
            if not found_products:
                print("No products found with standard selectors, trying generic approach...")
            
            # Scroll to load more products
            self.scroll_to_load_products()
            
            # Parse products
            soup = BeautifulSoup(self.driver.page_source, 'html.parser')
            
            # Save page source for debugging
            with open('woolworths_page_debug.html', 'w', encoding='utf-8') as f:
                f.write(self.driver.page_source)
            print("Saved page source to woolworths_page_debug.html for inspection")
            
            return self.parse_products_from_html(soup)
            
        except Exception as e:
            print(f"Error scraping catalogue: {e}")
            return []

    def try_alternative_parsing(self) -> List[Product]:
        """Try alternative parsing methods if main method fails"""
        products = []
        soup = BeautifulSoup(self.driver.page_source, 'html.parser')
        
        # Try different product selectors
        selectors = [
            ".catalogue-product",
            ".product-tile",
            ".product-item",
            "[data-testid*='product']",
            ".special-product"
        ]
        
        for selector in selectors:
            elements = soup.select(selector)
            if elements:
                print(f"Found {len(elements)} products using selector: {selector}")
                products = self.parse_product_elements(elements)
                break
        
        return products

    def scroll_to_load_products(self):
        """Scroll page to trigger lazy loading of products"""
        last_height = self.driver.execute_script("return document.body.scrollHeight")
        
        for _ in range(5):  # Scroll up to 5 times
            self.driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")
            time.sleep(2)
            
            new_height = self.driver.execute_script("return document.body.scrollHeight")
            if new_height == last_height:
                break
            last_height = new_height

    def parse_products_from_html(self, soup: BeautifulSoup) -> List[Product]:
        """Parse products from HTML soup"""
        products = []
        
        # Multiple selectors to try
        product_selectors = [
            ".catalogueProduct",
            ".catalogue-product", 
            ".product-tile",
            ".special-product",
            "[class*='product']"
        ]
        
        product_elements = []
        for selector in product_selectors:
            elements = soup.select(selector)
            if elements:
                product_elements = elements
                print(f"Using selector: {selector}, found {len(elements)} products")
                break
        
        if not product_elements:
            print("No product elements found, trying to extract from any price elements")
            return self.extract_from_price_elements(soup)
        
        for element in product_elements:
            product = self.parse_single_product(element)
            if product:
                products.append(product)
        
        return products

    def extract_from_price_elements(self, soup: BeautifulSoup) -> List[Product]:
        """Extract products by finding price elements and working backwards"""
        products = []
        
        # Find all price elements
        price_selectors = [
            "[class*='price']",
            "[data-testid*='price']",
            ".dollar",
            ".cost"
        ]
        
        price_elements = []
        for selector in price_selectors:
            elements = soup.select(selector)
            if elements:
                price_elements.extend(elements)
        
        for price_elem in price_elements:
            price_text = price_elem.get_text(strip=True)
            if re.search(r'\$\d+\.?\d*', price_text):
                # Find parent container that might contain product info
                parent = price_elem.find_parent()
                for _ in range(5):  # Go up to 5 levels
                    if parent:
                        product = self.extract_product_from_container(parent, price_text)
                        if product:
                            products.append(product)
                            break
                        parent = parent.find_parent()
                    else:
                        break
        
        return products

    def extract_product_from_container(self, container, price: str) -> Optional[Product]:
        """Extract product info from a container element"""
        # Try to find product name
        name_selectors = [
            "[class*='name']",
            "[class*='title']", 
            "[class*='product']",
            "h1", "h2", "h3", "h4",
            ".brand"
        ]
        
        name = None
        for selector in name_selectors:
            name_elem = container.select_one(selector)
            if name_elem:
                potential_name = name_elem.get_text(strip=True)
                if len(potential_name) > 3 and not re.match(r'^\$?\d+\.?\d*$', potential_name):
                    name = potential_name
                    break
        
        if not name:
            # Try getting text from container directly
            container_text = container.get_text(strip=True)
            lines = [line.strip() for line in container_text.split('\n') if line.strip()]
            for line in lines:
                if (len(line) > 5 and 
                    not re.search(r'\$\d+', line) and 
                    not line.lower() in ['special', 'save', 'was']):
                    name = line
                    break
        
        if name:
            return Product(
                name=name,
                price=price,
                brand=self.extract_brand(name),
                category=self.categorize_product(name)
            )
        
        return None

    def parse_single_product(self, element) -> Optional[Product]:
        """Parse a single product element"""
        try:
            # Extract product name
            name_selectors = [
                ".product-name", ".name", ".title", 
                "[data-testid*='name']", "[data-testid*='title']",
                "h1", "h2", "h3", "h4"
            ]
            
            name = None
            for selector in name_selectors:
                name_elem = element.select_one(selector)
                if name_elem:
                    name = name_elem.get_text(strip=True)
                    break
            
            if not name:
                # Fallback: get text from element
                all_text = element.get_text(strip=True)
                lines = [line.strip() for line in all_text.split('\n') if line.strip()]
                for line in lines:
                    if len(line) > 5 and not re.search(r'\$\d+', line):
                        name = line
                        break
            
            if not name:
                return None
            
            # Extract price
            price_selectors = [
                ".price", ".cost", ".dollar", 
                "[data-testid*='price']", "[class*='price']"
            ]
            
            price = None
            original_price = None
            
            for selector in price_selectors:
                price_elem = element.select_one(selector)
                if price_elem:
                    price_text = price_elem.get_text(strip=True)
                    price_match = re.search(r'\$(\d+\.?\d*)', price_text)
                    if price_match:
                        price = f"${price_match.group(1)}"
                        break
            
            # Look for strikethrough prices (original price)
            strikethrough_elems = element.select("del, .strikethrough, [style*='line-through']")
            for elem in strikethrough_elems:
                orig_text = elem.get_text(strip=True)
                orig_match = re.search(r'\$(\d+\.?\d*)', orig_text)
                if orig_match:
                    original_price = f"${orig_match.group(1)}"
                    break
            
            # Extract image URL
            image_url = None
            img_elem = element.select_one("img")
            if img_elem:
                image_url = img_elem.get('src') or img_elem.get('data-src')
                if image_url and not image_url.startswith('http'):
                    image_url = self.base_url + image_url
            
            # Extract product URL
            product_url = None
            link_elem = element.select_one("a[href]")
            if link_elem:
                href = link_elem.get('href')
                if href and not href.startswith('http'):
                    product_url = self.base_url + href
                else:
                    product_url = href
            
            # Calculate savings if both prices available
            savings = None
            special_type = None
            if original_price and price:
                try:
                    orig_val = float(original_price.replace('$', ''))
                    curr_val = float(price.replace('$', ''))
                    savings_val = orig_val - curr_val
                    if savings_val > 0:
                        savings = f"${savings_val:.2f}"
                        special_type = "Special"
                except ValueError:
                    pass
            
            # Check for special indicators
            if not special_type:
                element_text = element.get_text().lower()
                if 'half price' in element_text or '1/2 price' in element_text:
                    special_type = "Half Price"
                elif 'special' in element_text:
                    special_type = "Special"
            
            if price:
                return Product(
                    name=name,
                    price=price,
                    original_price=original_price,
                    brand=self.extract_brand(name),
                    category=self.categorize_product(name),
                    special_type=special_type,
                    savings=savings,
                    image_url=image_url,
                    product_url=product_url
                )
        
        except Exception as e:
            print(f"Error parsing product: {e}")
        
        return None

    def scrape_catalogue(self, catalogue_url: str) -> List[Dict]:
        """Main method to scrape catalogue and return product data"""
        products = self.scrape_catalogue_page(catalogue_url)
        
        # Convert to dictionaries
        product_dicts = []
        for i, product in enumerate(products, 1):
            product_dict = {
                'productID': f"WW{i:03d}",
                'productName': product.name,
                'category': product.category,
                'brand': product.brand,
                'price': product.price,
                'originalPrice': product.original_price,
                'savings': product.savings,
                'specialType': product.special_type,
                'imageUrl': product.image_url,
                'productUrl': product.product_url,
                'scrapedAt': datetime.now().isoformat()
            }
            product_dicts.append(product_dict)
        
        self.products = product_dicts
        return product_dicts

    def save_to_json(self, output_path: str):
        """Save scraped products to JSON file"""
        output_dir = Path(output_path).parent
        output_dir.mkdir(exist_ok=True)
        
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(self.products, f, indent=2, ensure_ascii=False)
        
        print(f"Saved {len(self.products)} products to {output_path}")

    def close(self):
        """Close the webdriver"""
        if hasattr(self, 'driver'):
            self.driver.quit()

def main():
    catalogue_url = "https://www.woolworths.com.au/shop/catalogue/view#view=list&saleId=60903&areaName=QLD"
    output_path = f"data/woolworths_web_scraped_{datetime.now().strftime('%d%m%Y')}.json"
    
    scraper = WoolworthsWebScraper(headless=False)  # Set to True for headless mode
    
    try:
        print("Starting Woolworths catalogue scraping...")
        products = scraper.scrape_catalogue(catalogue_url)
        
        if products:
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
        else:
            print("No products found. The page structure may have changed.")
    
    except Exception as e:
        print(f"Scraping failed: {e}")
    
    finally:
        scraper.close()

if __name__ == "__main__":
    main()