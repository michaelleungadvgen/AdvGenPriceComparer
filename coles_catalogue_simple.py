#!/usr/bin/env python3
"""
Coles Catalogue Simple Scraper
Uses requests + BeautifulSoup approach for more stable scraping
"""

import json
import requests
import re
from bs4 import BeautifulSoup
from datetime import datetime
import time

class ColesSimpleScraper:
    def __init__(self):
        self.session = requests.Session()
        self.session.headers.update({
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            'Accept-Language': 'en-US,en;q=0.5',
            'Accept-Encoding': 'gzip, deflate',
            'Connection': 'keep-alive',
        })
        self.products = []

    def get_catalogue_data(self, sale_id="61391", area_name="c-qld-met"):
        """Get catalogue data using API endpoint"""
        print(f"Fetching catalogue data for sale_id={sale_id}, area_name={area_name}")

        # Try the API endpoint that powers the catalogue view
        api_url = f"https://www.coles.com.au/api/catalogues/{sale_id}/items"

        params = {
            'areaName': area_name,
            'page': 0,
            'size': 1000  # Try to get many products at once
        }

        try:
            response = self.session.get(api_url, params=params)
            print(f"API Response status: {response.status_code}")

            if response.status_code == 200:
                data = response.json()
                return data
            else:
                print(f"API request failed with status {response.status_code}")
                return None

        except Exception as e:
            print(f"Error calling API: {e}")
            return None

    def scrape_web_page(self, url):
        """Fallback: scrape the web page directly"""
        print(f"Scraping web page: {url}")

        try:
            response = self.session.get(url)
            print(f"Web page response status: {response.status_code}")

            if response.status_code == 200:
                soup = BeautifulSoup(response.content, 'html.parser')

                # Look for product containers
                product_links = soup.find_all("a", class_="sf-item")
                print(f"Found {len(product_links)} product elements on page")

                products = []
                for product_element in product_links:
                    product_data = self.parse_product_element(product_element)
                    if product_data:
                        products.append(product_data)

                return products
            else:
                print(f"Web page request failed with status {response.status_code}")
                return []

        except Exception as e:
            print(f"Error scraping web page: {e}")
            return []

    def parse_product_element(self, product_element):
        """Parse a single product element from HTML"""
        try:
            product_data = {}

            # Extract product title
            title_element = product_element.find("h4", class_="sf-item-heading")
            product_data["title"] = title_element.get_text(strip=True) if title_element else "N/A"

            # Extract product ID from data-itemid attribute
            product_data["item_id"] = product_element.get("data-itemid", "N/A")

            # Extract image URL
            img_element = product_element.find("img")
            product_data["image_url"] = img_element.get("src", "") if img_element else ""

            # Extract pricing information
            pricing_div = product_element.find("div", class_="sf-price-options")
            if pricing_div:
                # Was price and savings
                was_price_span = pricing_div.find("span", class_="sf-regprice")
                save_desc_span = pricing_div.find("span", class_="sf-regoptiondesc")

                if was_price_span:
                    was_price_text = was_price_span.get_text(strip=True)
                    product_data["was_price"] = was_price_text

                if save_desc_span:
                    save_text = save_desc_span.get_text(strip=True)
                    # Extract savings amount using regex
                    save_match = re.search(r'Save\s*\$?([\d.]+)', save_text)
                    product_data["savings"] = f"${save_match.group(1)}" if save_match else "N/A"

                # Current/sale price
                sale_price_span = pricing_div.find("span", class_="sf-pricedisplay")
                if sale_price_span:
                    product_data["current_price"] = sale_price_span.get_text(strip=True)

                # Price suffix (each, per kg, etc.)
                suffix_span = pricing_div.find("span", class_="sf-optionsuffix")
                if suffix_span:
                    product_data["price_unit"] = suffix_span.get_text(strip=True)

                # Comparative text (price per unit)
                comparative_p = pricing_div.find("p", class_="sf-comparativeText")
                if comparative_p:
                    product_data["price_per_unit"] = comparative_p.get_text(strip=True)

            # Extract special message (availability dates)
            special_msg_div = product_element.find("div", class_="sf-special-message")
            if special_msg_div:
                product_data["availability"] = special_msg_div.get_text(strip=True)

            # Clean up empty fields
            product_data = {k: v for k, v in product_data.items() if v and v != "N/A"}

            return product_data

        except Exception as e:
            print(f"Error parsing product element: {e}")
            return None

    def save_to_json(self, filename=None):
        """Save extracted products to JSON file"""
        if not filename:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"coles_catalogue_{timestamp}.json"

        with open(filename, 'w', encoding='utf-8') as f:
            json.dump({
                "extraction_date": datetime.now().isoformat(),
                "total_products": len(self.products),
                "products": self.products
            }, f, indent=2, ensure_ascii=False)

        print(f"Products saved to {filename}")
        return filename

    def scrape_catalogue(self, sale_id="61391", area_name="c-qld-met"):
        """Main method to scrape the catalogue"""
        print("Starting Coles Catalogue Simple Scraper...")

        # Try API approach first
        api_data = self.get_catalogue_data(sale_id, area_name)
        if api_data and isinstance(api_data, dict):
            if 'items' in api_data:
                self.products = api_data['items']
                print(f"Successfully extracted {len(self.products)} products from API")
                return self.save_to_json()
            elif 'products' in api_data:
                self.products = api_data['products']
                print(f"Successfully extracted {len(self.products)} products from API")
                return self.save_to_json()

        # Fallback to web scraping
        print("API approach failed, trying web scraping...")
        url = f"https://www.coles.com.au/catalogues/view#view=list&saleId={sale_id}&areaName={area_name}"
        self.products = self.scrape_web_page(url)

        if self.products:
            print(f"Successfully extracted {len(self.products)} products from web scraping")
            return self.save_to_json()
        else:
            print("No products found")
            return None

def main():
    """Main function to run the scraper"""
    scraper = ColesSimpleScraper()

    try:
        output_file = scraper.scrape_catalogue()

        if output_file:
            print(f"\nScraping completed successfully!")
            print(f"Output file: {output_file}")
            print(f"Total products extracted: {len(scraper.products)}")

            # Print sample of first few products
            if scraper.products:
                print("\nSample products:")
                for i, product in enumerate(scraper.products[:3], 1):
                    print(f"\nProduct {i}:")
                    for key, value in product.items():
                        print(f"  {key}: {value}")
        else:
            print("No products extracted")

    except Exception as e:
        print(f"Scraping failed: {e}")

if __name__ == "__main__":
    main()