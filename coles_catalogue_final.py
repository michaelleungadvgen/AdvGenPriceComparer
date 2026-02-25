#!/usr/bin/env python3
"""
Coles Catalogue Final Scraper
Enhanced browser automation to handle anti-bot protection
"""

import json
import time
import re
from datetime import datetime
import random
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.action_chains import ActionChains
from selenium.common.exceptions import TimeoutException, NoSuchElementException
from bs4 import BeautifulSoup

class ColesAdvancedScraper:
    def __init__(self, headless=False):
        self.chrome_options = Options()

        # Stealth mode options
        if headless:
            self.chrome_options.add_argument("--headless")

        # Standard options
        self.chrome_options.add_argument("--no-sandbox")
        self.chrome_options.add_argument("--disable-dev-shm-usage")
        self.chrome_options.add_argument("--disable-blink-features=AutomationControlled")
        self.chrome_options.add_experimental_option("excludeSwitches", ["enable-automation"])
        self.chrome_options.add_experimental_option('useAutomationExtension', False)
        self.chrome_options.add_argument("--window-size=1920,1080")

        # Mimic real browser
        self.chrome_options.add_argument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")
        self.chrome_options.add_argument("--accept-language=en-AU,en;q=0.9")
        self.chrome_options.add_argument("--accept-encoding=gzip, deflate, br")

        self.driver = None
        self.products = []

    def start_driver(self):
        """Start Chrome with stealth configuration"""
        try:
            self.driver = webdriver.Chrome(options=self.chrome_options)

            # Execute stealth scripts
            self.driver.execute_script("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})")
            self.driver.execute_script("Object.defineProperty(navigator, 'plugins', {get: () => [1, 2, 3, 4, 5]})")
            self.driver.execute_script("Object.defineProperty(navigator, 'languages', {get: () => ['en-AU', 'en']})")

            print("Chrome WebDriver started in stealth mode")
            return True

        except Exception as e:
            print(f"Failed to start Chrome: {e}")
            return False

    def human_like_delay(self, min_sec=1, max_sec=3):
        """Add human-like random delays"""
        delay = random.uniform(min_sec, max_sec)
        time.sleep(delay)

    def load_page_with_retries(self, url, max_retries=3):
        """Load page with retry logic and bot detection handling"""
        for attempt in range(max_retries):
            try:
                print(f"Loading page (attempt {attempt + 1}/{max_retries}): {url}")

                self.driver.get(url)
                self.human_like_delay(3, 5)

                # Check if we're blocked
                page_source = self.driver.page_source.lower()
                if any(blocked_text in page_source for blocked_text in ["incapsula", "cloudflare", "access denied", "blocked"]):
                    print(f"Detected bot protection on attempt {attempt + 1}")
                    if attempt < max_retries - 1:
                        print("Waiting before retry...")
                        time.sleep(10 + attempt * 5)
                        continue
                    else:
                        print("All attempts failed due to bot protection")
                        return False

                # Wait for page to load
                try:
                    WebDriverWait(self.driver, 20).until(
                        lambda driver: driver.execute_script("return document.readyState") == "complete"
                    )
                    print("Page loaded successfully")
                    return True

                except TimeoutException:
                    print(f"Page load timeout on attempt {attempt + 1}")

            except Exception as e:
                print(f"Error loading page on attempt {attempt + 1}: {e}")

        return False

    def wait_for_products(self, timeout=30):
        """Wait for products to appear on page"""
        print("Waiting for products to load...")

        wait = WebDriverWait(self.driver, timeout)

        # Try multiple selectors that might contain products
        product_selectors = [
            "a[data-itemid]",
            ".sf-item",
            "[class*='item']",
            "[class*='product']"
        ]

        for selector in product_selectors:
            try:
                elements = wait.until(EC.presence_of_all_elements_located((By.CSS_SELECTOR, selector)))
                if elements:
                    print(f"Found {len(elements)} elements with selector '{selector}'")
                    return True
            except TimeoutException:
                continue

        print("No products found with any selector")
        return False

    def smart_click_load_more(self, max_clicks=50):
        """Intelligently find and click load more buttons"""
        clicks = 0

        # Common text patterns for load more buttons
        load_more_texts = ["load more", "show more", "more products", "see more", "more items"]

        print("Looking for 'Load More' buttons...")

        while clicks < max_clicks:
            try:
                # Look for buttons with load more text
                load_more_button = None

                # Try different button selectors
                button_selectors = [
                    "button",
                    "a[role='button']",
                    ".button",
                    "[class*='button']",
                    "[id*='more']",
                    "[class*='more']"
                ]

                for selector in button_selectors:
                    try:
                        buttons = self.driver.find_elements(By.CSS_SELECTOR, selector)
                        for button in buttons:
                            if button.is_displayed() and button.is_enabled():
                                button_text = button.text.lower()
                                if any(text in button_text for text in load_more_texts):
                                    load_more_button = button
                                    break
                        if load_more_button:
                            break
                    except:
                        continue

                if not load_more_button:
                    print(f"No more 'Load More' button found after {clicks} clicks")
                    break

                # Scroll to button with human-like movement
                ActionChains(self.driver).move_to_element(load_more_button).perform()
                self.human_like_delay(1, 2)

                # Click the button
                load_more_button.click()
                clicks += 1
                print(f"Clicked 'Load More' button {clicks} times")

                # Wait for new content to load (2 seconds as requested)
                self.human_like_delay(2, 3)

            except Exception as e:
                print(f"Error clicking load more after {clicks} clicks: {e}")
                break

        print(f"Total load more clicks: {clicks}")
        return clicks

    def extract_products_advanced(self):
        """Advanced product extraction with multiple fallback methods"""
        print("Starting advanced product extraction...")

        # Get page source
        page_source = self.driver.page_source
        soup = BeautifulSoup(page_source, 'html.parser')

        # Save page source for debugging
        with open("debug_page_source.html", "w", encoding="utf-8") as f:
            f.write(page_source)
        print("Page source saved to debug_page_source.html")

        # Try multiple approaches to find products
        products = []

        # Method 1: Look for elements with data-itemid
        data_item_elements = soup.find_all(attrs={"data-itemid": True})
        if data_item_elements:
            print(f"Method 1: Found {len(data_item_elements)} elements with data-itemid")
            products = data_item_elements

        # Method 2: Look for sf-item class
        if not products:
            sf_items = soup.find_all(class_="sf-item")
            if sf_items:
                print(f"Method 2: Found {len(sf_items)} sf-item elements")
                products = sf_items

        # Method 3: Look for any links that might be products
        if not products:
            all_links = soup.find_all("a")
            product_links = []
            for link in all_links:
                link_text = link.get_text(strip=True).lower()
                link_class = " ".join(link.get("class", []))
                # Filter for likely product links
                if (len(link_text) > 10 and
                    any(word in link_text for word in ["pack", "g", "ml", "kg", "$"]) or
                    any(word in link_class for word in ["item", "product"])):
                    product_links.append(link)

            if product_links:
                print(f"Method 3: Found {len(product_links)} potential product links")
                products = product_links

        # Method 4: Extract from JSON-LD or script tags
        if not products:
            script_tags = soup.find_all("script", {"type": "application/ld+json"})
            for script in script_tags:
                try:
                    data = json.loads(script.string)
                    if isinstance(data, dict) and "offers" in str(data).lower():
                        print("Method 4: Found JSON-LD data with offers")
                        # Process JSON data here
                except:
                    continue

        if not products:
            print("No products found with any method")
            return []

        # Parse the found products
        parsed_products = []
        for i, product_element in enumerate(products):
            try:
                product_data = self.parse_product_comprehensive(product_element)
                if product_data and product_data.get("title"):
                    parsed_products.append(product_data)

                if (i + 1) % 20 == 0:
                    print(f"Processed {i + 1}/{len(products)} products")

            except Exception as e:
                continue

        print(f"Successfully extracted {len(parsed_products)} products")
        self.products = parsed_products
        return parsed_products

    def parse_product_comprehensive(self, element):
        """Comprehensive product parsing with multiple extraction methods"""
        try:
            product_data = {}

            # Get all text
            all_text = element.get_text(" ", strip=True) if hasattr(element, 'get_text') else str(element)

            # Extract title - try multiple methods
            title = ""

            # Method 1: Look for specific heading tags
            for tag in ["h1", "h2", "h3", "h4", "h5", "h6"]:
                heading = element.find(tag)
                if heading:
                    title = heading.get_text(strip=True)
                    break

            # Method 2: Look for title attribute
            if not title and hasattr(element, 'get'):
                title = element.get('title', '') or element.get('alt', '')

            # Method 3: Extract from text content
            if not title:
                # Look for product-like text patterns
                lines = [line.strip() for line in all_text.split('\n') if line.strip()]
                for line in lines:
                    # Skip common UI text
                    if not any(skip in line.lower() for skip in ["was", "save", "special", "shop", "add to"]):
                        if len(line) > 5 and len(line) < 100:
                            title = line
                            break

            if not title:
                title = "Unknown Product"

            product_data["title"] = title

            # Extract item ID
            if hasattr(element, 'get'):
                item_id = element.get('data-itemid', '') or element.get('data-id', '') or element.get('id', '')
                product_data["item_id"] = item_id

            # Extract image URL
            img = element.find('img')
            if img:
                img_src = img.get('src', '') or img.get('data-src', '') or img.get('data-lazy-src', '')
                if img_src:
                    product_data["image_url"] = img_src

            # Extract prices using regex
            price_pattern = r'\$(\d+(?:\.\d{2})?)'
            prices = re.findall(price_pattern, all_text)

            if prices:
                # Convert to proper format
                price_values = [f"${p}" for p in prices]
                product_data["prices_found"] = price_values

                # Assign was/current prices if multiple found
                if len(price_values) >= 2:
                    product_data["was_price"] = price_values[0]
                    product_data["current_price"] = price_values[1]
                elif len(price_values) == 1:
                    product_data["current_price"] = price_values[0]

            # Extract savings information
            save_patterns = [
                r'save\s*\$?(\d+(?:\.\d{2})?)',
                r'was\s*\$?\d+(?:\.\d{2})?,?\s*save\s*\$?(\d+(?:\.\d{2})?)',
            ]

            for pattern in save_patterns:
                save_match = re.search(pattern, all_text, re.IGNORECASE)
                if save_match:
                    product_data["savings"] = f"${save_match.group(1)}"
                    break

            # Extract unit information
            unit_patterns = [
                r'(\$[\d.]+)\s*per\s*(\w+)',
                r'(\d+(?:\.\d+)?)\s*(g|kg|ml|l|pack)\b',
                r'\b(each|per\s+kg|per\s+100g|per\s+litre)\b'
            ]

            for pattern in unit_patterns:
                unit_match = re.search(pattern, all_text, re.IGNORECASE)
                if unit_match:
                    product_data["unit_info"] = unit_match.group(0)
                    break

            # Store raw text for debugging (truncated)
            product_data["raw_text"] = all_text[:150] + "..." if len(all_text) > 150 else all_text

            return product_data

        except Exception as e:
            return None

    def save_to_json(self, filename=None):
        """Save products to JSON file"""
        if not filename:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"coles_catalogue_{timestamp}.json"

        data = {
            "extraction_date": datetime.now().isoformat(),
            "total_products": len(self.products),
            "url": "https://www.coles.com.au/catalogues/view#view=list&saleId=61391&areaName=c-qld-met",
            "products": self.products
        }

        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)

        print(f"Results saved to {filename}")
        return filename

    def close_driver(self):
        """Close the browser"""
        if self.driver:
            self.driver.quit()

    def scrape_catalogue(self, url):
        """Main scraping orchestrator"""
        try:
            if not self.start_driver():
                raise Exception("Failed to start browser")

            if not self.load_page_with_retries(url):
                raise Exception("Failed to load catalogue page")

            if not self.wait_for_products():
                print("No products detected, but continuing to try extraction...")

            self.smart_click_load_more()

            self.extract_products_advanced()

            if self.products:
                return self.save_to_json()
            else:
                print("No products extracted")
                return None

        except Exception as e:
            print(f"Scraping failed: {e}")
            raise
        finally:
            self.close_driver()

def main():
    """Main execution function"""
    url = "https://www.coles.com.au/catalogues/view#view=list&saleId=61391&areaName=c-qld-met"

    print("Starting Coles Advanced Catalogue Scraper")
    print(f"Target URL: {url}")
    print("=" * 60)

    scraper = ColesAdvancedScraper(headless=False)  # Set to True for headless mode

    try:
        output_file = scraper.scrape_catalogue(url)

        if output_file:
            print("\n" + "=" * 60)
            print("SCRAPING COMPLETED SUCCESSFULLY!")
            print(f"Output file: {output_file}")
            print(f"Total products extracted: {len(scraper.products)}")

            if scraper.products:
                print("\nSample products:")
                for i, product in enumerate(scraper.products[:3], 1):
                    print(f"\n--- Product {i} ---")
                    for key, value in product.items():
                        if key != "raw_text":  # Skip raw text in sample
                            print(f"{key}: {value}")
        else:
            print("\nNo products were extracted")

    except Exception as e:
        print(f"\nScraping failed: {e}")

if __name__ == "__main__":
    main()