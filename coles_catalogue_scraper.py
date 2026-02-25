#!/usr/bin/env python3
"""
Coles Catalogue Scraper
Automatically loads all products from a Coles catalogue page and extracts product data to JSON.
"""

import json
import time
import re
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.chrome.service import Service
from selenium.common.exceptions import TimeoutException, NoSuchElementException
from webdriver_manager.chrome import ChromeDriverManager
from bs4 import BeautifulSoup
from datetime import datetime

class ColesCalculagogueScraper:
    def __init__(self, headless=False):
        self.chrome_options = Options()
        if headless:
            self.chrome_options.add_argument("--headless")
        self.chrome_options.add_argument("--no-sandbox")
        self.chrome_options.add_argument("--disable-dev-shm-usage")
        self.chrome_options.add_argument("--disable-gpu")
        self.chrome_options.add_argument("--disable-web-security")
        self.chrome_options.add_argument("--disable-features=VizDisplayCompositor")
        self.chrome_options.add_argument("--window-size=1920,1080")
        self.chrome_options.add_argument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")
        self.chrome_options.binary_location = r"C:\Program Files\Google\Chrome\Application\chrome.exe"

        self.driver = None
        self.products = []

    def start_driver(self):
        """Initialize the Chrome WebDriver"""
        try:
            # Use webdriver-manager to automatically handle Chrome driver
            service = Service(ChromeDriverManager().install())
            self.driver = webdriver.Chrome(service=service, options=self.chrome_options)
            print("Chrome WebDriver initialized successfully")
        except Exception as e:
            print(f"Error initializing Chrome WebDriver: {e}")
            raise

    def load_catalogue_page(self, url):
        """Load the Coles catalogue page"""
        print(f"Loading catalogue page: {url}")
        self.driver.get(url)

        # Wait for the page to load
        WebDriverWait(self.driver, 10).until(
            EC.presence_of_element_located((By.CLASS_NAME, "sf-item"))
        )
        print("Catalogue page loaded successfully")

    def click_load_more_until_end(self):
        """Keep clicking 'Load more' button until no more products to load"""
        load_more_clicks = 0

        while True:
            try:
                # Wait for load more button to be clickable
                load_more_button = WebDriverWait(self.driver, 5).until(
                    EC.element_to_be_clickable((By.ID, "show-more"))
                )

                # Scroll to the button to ensure it's visible
                self.driver.execute_script("arguments[0].scrollIntoView(true);", load_more_button)
                time.sleep(1)

                # Click the button
                load_more_button.click()
                load_more_clicks += 1
                print(f"Clicked 'Load more' button {load_more_clicks} times")

                # Wait 2 seconds before next click as requested
                time.sleep(2)

                # Wait for new content to load
                time.sleep(1)

            except TimeoutException:
                print("No more 'Load more' button found - all products loaded")
                break
            except NoSuchElementException:
                print("Load more button not found - all products loaded")
                break
            except Exception as e:
                print(f"Error clicking load more button: {e}")
                break

        print(f"Total 'Load more' clicks: {load_more_clicks}")

    def parse_product_element(self, product_element):
        """Parse a single product element and extract product data"""
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

    def extract_all_products(self):
        """Extract all product data from the loaded page"""
        print("Extracting product data...")

        # Get page source and parse with BeautifulSoup
        page_source = self.driver.page_source
        soup = BeautifulSoup(page_source, 'html.parser')

        # Find all product elements
        product_elements = soup.find_all("a", class_="sf-item")
        print(f"Found {len(product_elements)} product elements")

        products = []
        for i, product_element in enumerate(product_elements, 1):
            product_data = self.parse_product_element(product_element)
            if product_data:
                products.append(product_data)
                if i % 50 == 0:  # Progress indicator
                    print(f"Processed {i}/{len(product_elements)} products")

        print(f"Successfully extracted {len(products)} products")
        self.products = products
        return products

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

    def close_driver(self):
        """Close the WebDriver"""
        if self.driver:
            self.driver.quit()
            print("WebDriver closed")

    def scrape_catalogue(self, url, output_file=None):
        """Main method to scrape the entire catalogue"""
        try:
            self.start_driver()
            self.load_catalogue_page(url)
            self.click_load_more_until_end()
            self.extract_all_products()
            filename = self.save_to_json(output_file)
            return filename

        except Exception as e:
            print(f"Error during scraping: {e}")
            raise
        finally:
            self.close_driver()

def main():
    """Main function to run the scraper"""
    # The URL from the user's request
    catalogue_url = "https://www.coles.com.au/catalogues/view#view=list&saleId=61391&areaName=c-qld-met"

    print("Starting Coles Catalogue Scraper...")
    print(f"Target URL: {catalogue_url}")

    scraper = ColesCalculagogueScraper(headless=False)  # Set to True for headless mode

    try:
        output_file = scraper.scrape_catalogue(catalogue_url)
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

    except Exception as e:
        print(f"Scraping failed: {e}")

if __name__ == "__main__":
    main()