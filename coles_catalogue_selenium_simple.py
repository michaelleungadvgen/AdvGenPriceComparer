#!/usr/bin/env python3
"""
Coles Catalogue Selenium Simple Scraper
Simplified Selenium approach with better error handling
"""

import json
import time
import re
from datetime import datetime
import os
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.firefox.options import Options as FirefoxOptions
from selenium.webdriver.firefox.service import Service as FirefoxService
from selenium.common.exceptions import TimeoutException, NoSuchElementException
from bs4 import BeautifulSoup

class ColesSeleniumSimpleScraper:
    def __init__(self, headless=True):
        self.firefox_options = FirefoxOptions()
        if headless:
            self.firefox_options.add_argument("--headless")
        self.firefox_options.add_argument("--no-sandbox")
        self.firefox_options.add_argument("--disable-dev-shm-usage")
        self.firefox_options.add_argument("--window-size=1920,1080")

        self.driver = None
        self.products = []

    def start_driver(self):
        """Initialize Firefox WebDriver"""
        try:
            # Try to use system Firefox
            self.driver = webdriver.Firefox(options=self.firefox_options)
            print("Firefox WebDriver initialized successfully")
            return True
        except Exception as e:
            print(f"Firefox failed: {e}")

            # Try Chrome as fallback without webdriver-manager
            try:
                from selenium.webdriver.chrome.options import Options as ChromeOptions
                from selenium.webdriver.chrome.service import Service as ChromeService

                chrome_options = ChromeOptions()
                chrome_options.add_argument("--headless")
                chrome_options.add_argument("--no-sandbox")
                chrome_options.add_argument("--disable-dev-shm-usage")
                chrome_options.add_argument("--disable-gpu")
                chrome_options.add_argument("--window-size=1920,1080")

                # Try with system Chrome driver
                self.driver = webdriver.Chrome(options=chrome_options)
                print("Chrome WebDriver initialized successfully")
                return True
            except Exception as e2:
                print(f"Chrome also failed: {e2}")
                return False

    def load_page_and_wait(self, url, max_wait=30):
        """Load page and wait for content"""
        print(f"Loading page: {url}")
        self.driver.get(url)

        # Wait for the page to load some content
        wait = WebDriverWait(self.driver, max_wait)

        try:
            # Wait for either products to load or error message
            wait.until(
                lambda driver: (
                    len(driver.find_elements(By.CSS_SELECTOR, "a.sf-item, .sf-item, [class*='item'], [class*='product']")) > 0 or
                    "error" in driver.page_source.lower() or
                    "not found" in driver.page_source.lower()
                )
            )
            print("Page content loaded")
            return True
        except TimeoutException:
            print("Timeout waiting for page content")
            return False

    def click_load_more(self, max_clicks=50):
        """Click load more button until no more"""
        clicks = 0

        while clicks < max_clicks:
            try:
                # Look for various possible "load more" button selectors
                load_more_selectors = [
                    "#show-more",
                    "button#show-more",
                    ".sf-button-primary",
                    "[class*='load-more']",
                    "[class*='show-more']",
                    "button[class*='primary']"
                ]

                load_more_button = None
                for selector in load_more_selectors:
                    try:
                        elements = self.driver.find_elements(By.CSS_SELECTOR, selector)
                        for element in elements:
                            if "load" in element.text.lower() or "more" in element.text.lower():
                                load_more_button = element
                                break
                        if load_more_button:
                            break
                    except:
                        continue

                if not load_more_button or not load_more_button.is_displayed():
                    print(f"No more 'Load More' button found after {clicks} clicks")
                    break

                # Scroll to button and click
                self.driver.execute_script("arguments[0].scrollIntoView(true);", load_more_button)
                time.sleep(1)
                load_more_button.click()
                clicks += 1
                print(f"Clicked 'Load More' {clicks} times")

                # Wait 2 seconds as requested
                time.sleep(2)

            except Exception as e:
                print(f"Error clicking load more: {e}")
                break

        print(f"Total load more clicks: {clicks}")
        return clicks

    def extract_products(self):
        """Extract products from the loaded page"""
        print("Extracting products from page...")

        # Get page source and parse with BeautifulSoup
        page_source = self.driver.page_source
        soup = BeautifulSoup(page_source, 'html.parser')

        # Try multiple selectors to find products
        product_selectors = [
            "a.sf-item",
            ".sf-item",
            "[class*='item']",
            "[class*='product']",
            "[data-itemid]"
        ]

        all_products = []
        for selector in product_selectors:
            products = soup.select(selector)
            if products:
                print(f"Found {len(products)} elements with selector '{selector}'")
                all_products = products
                break

        if not all_products:
            print("No products found with any selector")
            # Save the page source for debugging
            with open("debug_page_source.html", "w", encoding="utf-8") as f:
                f.write(page_source)
            print("Page source saved to debug_page_source.html for inspection")
            return []

        # Parse each product
        parsed_products = []
        for i, product_element in enumerate(all_products):
            try:
                product_data = self.parse_product_element(product_element)
                if product_data:
                    parsed_products.append(product_data)

                if (i + 1) % 50 == 0:
                    print(f"Processed {i + 1}/{len(all_products)} products")

            except Exception as e:
                print(f"Error parsing product {i}: {e}")
                continue

        print(f"Successfully parsed {len(parsed_products)} products")
        self.products = parsed_products
        return parsed_products

    def parse_product_element(self, product_element):
        """Parse a single product element"""
        try:
            product_data = {}

            # Get all text content for debugging
            all_text = product_element.get_text(strip=True) if hasattr(product_element, 'get_text') else str(product_element)

            # Try to extract title
            title_selectors = ["h4", ".sf-item-heading", "[class*='heading']", "[class*='title']"]
            title = ""
            for selector in title_selectors:
                title_elem = product_element.find(selector) if hasattr(product_element, 'find') else None
                if title_elem:
                    title = title_elem.get_text(strip=True)
                    break

            if not title:
                # Try to extract title from data attributes or text content
                if hasattr(product_element, 'get'):
                    title = product_element.get('title', '') or product_element.get('alt', '')
                if not title and all_text:
                    # Use first meaningful line as title
                    lines = [line.strip() for line in all_text.split('\n') if line.strip()]
                    title = lines[0] if lines else "Unknown Product"

            product_data["title"] = title

            # Extract item ID
            item_id = ""
            if hasattr(product_element, 'get'):
                item_id = product_element.get('data-itemid', '') or product_element.get('data-id', '')
            product_data["item_id"] = item_id

            # Extract image
            img_elem = product_element.find('img') if hasattr(product_element, 'find') else None
            if img_elem:
                product_data["image_url"] = img_elem.get('src', '') or img_elem.get('data-src', '')

            # Extract prices using regex from all text
            price_patterns = [
                r'\$(\d+\.\d{2})',  # $X.XX format
                r'\$(\d+)',         # $X format
            ]

            prices = []
            for pattern in price_patterns:
                matches = re.findall(pattern, all_text)
                prices.extend([f"${match}" for match in matches])

            if prices:
                product_data["prices_found"] = prices
                if len(prices) >= 2:
                    product_data["was_price"] = prices[0]
                    product_data["current_price"] = prices[1]
                else:
                    product_data["current_price"] = prices[0]

            # Look for savings info
            save_match = re.search(r'Save\s*\$?([\d.]+)', all_text, re.IGNORECASE)
            if save_match:
                product_data["savings"] = f"${save_match.group(1)}"

            # Look for unit info
            unit_patterns = [
                r'(\$[\d.]+)\s+per\s+(\w+)',
                r'(\d+g|\d+ml|\d+kg|\w+\s+pack)',
                r'\b(each|per kg|per 100g)\b'
            ]

            for pattern in unit_patterns:
                match = re.search(pattern, all_text, re.IGNORECASE)
                if match:
                    product_data["unit_info"] = match.group(0)
                    break

            # Store all text for debugging
            product_data["all_text"] = all_text[:200] + "..." if len(all_text) > 200 else all_text

            return product_data

        except Exception as e:
            print(f"Error parsing individual product: {e}")
            return None

    def save_to_json(self, filename=None):
        """Save products to JSON file"""
        if not filename:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"coles_catalogue_{timestamp}.json"

        data = {
            "extraction_date": datetime.now().isoformat(),
            "total_products": len(self.products),
            "products": self.products
        }

        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)

        print(f"Products saved to {filename}")
        return filename

    def close_driver(self):
        """Close the browser"""
        if self.driver:
            self.driver.quit()
            print("Browser closed")

    def scrape_catalogue(self, url):
        """Main scraping method"""
        try:
            if not self.start_driver():
                raise Exception("Failed to start browser driver")

            if not self.load_page_and_wait(url):
                raise Exception("Failed to load page content")

            self.click_load_more()
            self.extract_products()

            if self.products:
                return self.save_to_json()
            else:
                print("No products extracted")
                return None

        except Exception as e:
            print(f"Error during scraping: {e}")
            raise
        finally:
            self.close_driver()

def main():
    """Main function"""
    url = "https://www.coles.com.au/catalogues/view#view=list&saleId=61391&areaName=c-qld-met"

    print("Starting Coles Catalogue Selenium Simple Scraper...")

    scraper = ColesSeleniumSimpleScraper(headless=False)  # Set to True for headless

    try:
        output_file = scraper.scrape_catalogue(url)

        if output_file:
            print(f"\nScraping completed!")
            print(f"Output file: {output_file}")
            print(f"Products found: {len(scraper.products)}")

            # Show sample products
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