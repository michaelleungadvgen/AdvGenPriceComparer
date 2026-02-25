#!/usr/bin/env python3
"""
Price Matcher Script
Extracts prices from Coles and Woolworths JSON files and matches them with categories in the HTML file.
"""

import json
import re
from typing import Dict, List, Tuple
from pathlib import Path

class PriceMatcher:
    def __init__(self):
        self.coles_file = "data/colse_05112025.json"
        self.woolworths_file = "data/woolworths_05112025.json"
        self.html_file = "liveinbne_deal.html"

        # Category mapping from JSON categories to HTML categories
        self.category_mapping = {
            # Coles categories to HTML categories
            "Confectionery": "Premium Chocolate",
            "Pantry": "Premium Biscuits & Crackers",
            "Drinks": "Soft Drinks & Beverages",
            "Frozen": "Premium Ice Cream",
            "Meat, Seafood & Deli": "Fresh Meat & Seafood",
            "Snacks": "Potato Chips & Snacks",
            "Health & Beauty": "Personal Care & Health",
            "Dairy & Eggs": "Fresh Meat & Seafood",

            # Woolworths categories to HTML categories
            "Meat & Seafood": "Fresh Meat & Seafood",
            "Snacks": "Potato Chips & Snacks",
            "Dairy & Eggs": "Fresh Meat & Seafood",
            "Beverages": "Soft Drinks & Beverages",
            "Frozen Foods": "Premium Ice Cream",
            "Bakery": "Premium Biscuits & Crackers",
            "Pantry Items": "Premium Biscuits & Crackers",
            "Personal Care": "Personal Care & Health",
        }

        # Product keyword mapping for better matching
        self.product_keywords = {
            "Premium Chocolate": ["chocolate", "mars", "maltesers", "m&m", "cadbury", "lindt", "toblerone"],
            "Premium Biscuits & Crackers": ["biscuits", "crackers", "shapes", "arnott", "kettle", "chips"],
            "Canned Tuna 95g": ["tuna", "fish", "salmon"],
            "Breakfast Cereals & Drinks": ["cereal", "kellogg", "sanitarium", "weet-bix", "up&go"],
            "Potato Chips & Snacks": ["chips", "snacks", "doritos", "pringles", "smith", "popcorn"],
            "Soft Drinks & Beverages": ["coca-cola", "pepsi", "sprite", "fanta", "gatorade", "coconut water"],
            "Personal Care & Health": ["shampoo", "deodorant", "toothpaste", "sunscreen", "fish oil"],
            "Fresh Meat & Seafood": ["beef", "lamb", "chicken", "pork", "prawns", "salmon", "bacon"],
            "Premium Ice Cream": ["ice cream", "magnum", "connoisseur", "ben & jerry", "gelato"],
            "Instant Coffee": ["coffee", "nescafe", "moccona"],
            "Rice 5kg Bags": ["rice", "jasmine", "basmati", "5kg"]
        }

    def load_json_data(self, file_path: str) -> List[Dict]:
        """Load JSON data from file."""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                return json.load(f)
        except FileNotFoundError:
            print(f"Warning: {file_path} not found")
            return []
        except json.JSONDecodeError as e:
            print(f"Error parsing JSON from {file_path}: {e}")
            return []

    def clean_price(self, price_str: str) -> float:
        """Extract numeric price from price string."""
        if not price_str:
            return 0.0

        # Remove currency symbols and extract number
        price_clean = re.sub(r'[^\d.]', '', str(price_str))
        try:
            return float(price_clean) if price_clean else 0.0
        except ValueError:
            return 0.0

    def calculate_discount_percentage(self, original_price: float, current_price: float) -> float:
        """Calculate discount percentage."""
        if original_price <= 0 or current_price <= 0:
            return 0.0
        return round(((original_price - current_price) / original_price) * 100, 1)

    def categorize_product(self, product: Dict) -> str:
        """Determine the HTML category for a product."""
        # First try direct category mapping
        json_category = product.get('category', '')
        if json_category in self.category_mapping:
            return self.category_mapping[json_category]

        # Then try keyword matching on product name
        product_name = product.get('productName', '').lower()

        for html_category, keywords in self.product_keywords.items():
            for keyword in keywords:
                if keyword.lower() in product_name:
                    return html_category

        return "Other"

    def extract_products_by_category(self) -> Dict[str, List[Dict]]:
        """Extract and categorize products from both stores."""
        categorized_products = {}

        # Load Coles data
        coles_data = self.load_json_data(self.coles_file)
        print(f"Loaded {len(coles_data)} Coles products")

        # Load Woolworths data
        woolworths_data = self.load_json_data(self.woolworths_file)
        print(f"Loaded {len(woolworths_data)} Woolworths products")

        # Process Coles products
        for product in coles_data:
            category = self.categorize_product(product)
            if category not in categorized_products:
                categorized_products[category] = []

            # Add store identifier and clean data
            current_price = self.clean_price(product.get('price', '0'))
            original_price = self.clean_price(product.get('originalPrice', '0'))
            savings_amount = self.clean_price(product.get('savings', '0')) if product.get('savings') else 0.0

            product_info = {
                'productID': product.get('productID', ''),
                'store': 'Coles',
                'name': product.get('productName', ''),
                'description': product.get('description', ''),
                'brand': product.get('brand', ''),
                'pricing': {
                    'current_price': current_price,
                    'current_price_formatted': product.get('price', '$0.00'),
                    'original_price': original_price,
                    'original_price_formatted': product.get('originalPrice', '') if product.get('originalPrice') else None,
                    'savings_text': product.get('savings', ''),
                    'savings_amount': savings_amount,
                    'discount_percentage': self.calculate_discount_percentage(original_price, current_price),
                    'is_on_special': bool(product.get('specialType', '') and product.get('specialType', '') != 'REGULAR'),
                    'special_type': product.get('specialType', '')
                },
                'categories': {
                    'original_json_category': product.get('category', ''),
                    'matched_html_category': category
                }
            }
            categorized_products[category].append(product_info)

        # Process Woolworths products
        for product in woolworths_data:
            category = self.categorize_product(product)
            if category not in categorized_products:
                categorized_products[category] = []

            # Add store identifier and clean data
            current_price = self.clean_price(product.get('price', '0'))
            original_price = self.clean_price(product.get('originalPrice', '0'))
            savings_amount = self.clean_price(product.get('savings', '0')) if product.get('savings') else 0.0

            product_info = {
                'productID': product.get('productID', ''),
                'store': 'Woolworths',
                'name': product.get('productName', ''),
                'description': product.get('description', ''),
                'brand': product.get('brand', ''),
                'pricing': {
                    'current_price': current_price,
                    'current_price_formatted': product.get('price', '$0.00'),
                    'original_price': original_price,
                    'original_price_formatted': product.get('originalPrice', '') if product.get('originalPrice') else None,
                    'savings_text': product.get('savings', ''),
                    'savings_amount': savings_amount,
                    'discount_percentage': self.calculate_discount_percentage(original_price, current_price),
                    'is_on_special': bool(product.get('specialType', '') and product.get('specialType', '') != 'REGULAR'),
                    'special_type': product.get('specialType', '')
                },
                'categories': {
                    'original_json_category': product.get('category', ''),
                    'matched_html_category': category
                }
            }
            categorized_products[category].append(product_info)

        return categorized_products

    def find_price_comparisons(self, categorized_products: Dict[str, List[Dict]]) -> Dict[str, List[Dict]]:
        """Find comparable products between stores."""
        comparisons = {}

        for category, products in categorized_products.items():
            if category == "Other":
                continue

            coles_products = [p for p in products if p['store'] == 'Coles']
            woolworths_products = [p for p in products if p['store'] == 'Woolworths']

            category_comparisons = []

            # Find similar products
            for coles_product in coles_products:
                for woolworths_product in woolworths_products:
                    similarity_score = self.calculate_similarity(
                        coles_product['name'],
                        woolworths_product['name']
                    )

                    if similarity_score > 0.5:  # 50% similarity threshold
                        coles_price = coles_product['pricing']['current_price']
                        woolworths_price = woolworths_product['pricing']['current_price']

                        comparison = {
                            'coles': coles_product,
                            'woolworths': woolworths_product,
                            'similarity': similarity_score,
                            'price_difference': abs(coles_price - woolworths_price),
                            'cheaper_store': 'Coles' if coles_price < woolworths_price else 'Woolworths' if coles_price > woolworths_price else 'Same Price'
                        }
                        category_comparisons.append(comparison)

            if category_comparisons:
                # Sort by similarity score
                category_comparisons.sort(key=lambda x: x['similarity'], reverse=True)
                comparisons[category] = category_comparisons

        return comparisons

    def calculate_similarity(self, name1: str, name2: str) -> float:
        """Calculate similarity between two product names."""
        name1_words = set(name1.lower().split())
        name2_words = set(name2.lower().split())

        # Common words
        common_words = name1_words.intersection(name2_words)
        total_words = name1_words.union(name2_words)

        if not total_words:
            return 0.0

        return len(common_words) / len(total_words)

    def generate_report(self, categorized_products: Dict[str, List[Dict]],
                       comparisons: Dict[str, List[Dict]]) -> str:
        """Generate a comprehensive price comparison report."""
        report = []
        report.append("# Grocery Price Comparison Report")
        report.append(f"Generated on: {Path().absolute()}")
        report.append("\n## Summary by Category\n")

        for category, products in categorized_products.items():
            if category == "Other":
                continue

            coles_count = len([p for p in products if p['store'] == 'Coles'])
            woolworths_count = len([p for p in products if p['store'] == 'Woolworths'])

            report.append(f"### {category}")
            report.append(f"- Coles: {coles_count} products")
            report.append(f"- Woolworths: {woolworths_count} products")

            if category in comparisons:
                report.append(f"- Direct comparisons found: {len(comparisons[category])}")

            report.append("")

        report.append("\n## Direct Price Comparisons\n")

        for category, category_comparisons in comparisons.items():
            report.append(f"### {category}\n")

            for comparison in category_comparisons[:5]:  # Top 5 comparisons per category
                coles = comparison['coles']
                woolworths = comparison['woolworths']

                report.append(f"**Product Match (Similarity: {comparison['similarity']:.1%})**")
                report.append(f"- **Coles**: {coles['name']} - {coles['pricing']['current_price_formatted']}")
                if coles['pricing']['savings_text']:
                    report.append(f"  - Savings: {coles['pricing']['savings_text']}")
                if coles['pricing']['discount_percentage'] > 0:
                    report.append(f"  - Discount: {coles['pricing']['discount_percentage']}%")

                report.append(f"- **Woolworths**: {woolworths['name']} - {woolworths['pricing']['current_price_formatted']}")
                if woolworths['pricing']['savings_text']:
                    report.append(f"  - Savings: {woolworths['pricing']['savings_text']}")
                if woolworths['pricing']['discount_percentage'] > 0:
                    report.append(f"  - Discount: {woolworths['pricing']['discount_percentage']}%")

                price_diff = comparison['price_difference']
                if price_diff > 0:
                    report.append(f"- **Price Difference**: ${price_diff:.2f} ({comparison['cheaper_store']} cheaper)")
                else:
                    report.append("- **Price**: Same price at both stores")

                report.append("")

        return "\n".join(report)

    def generate_csv_output(self, categorized_products: Dict[str, List[Dict]]) -> str:
        """Generate CSV format output for easy analysis."""
        csv_lines = []
        csv_lines.append("ProductID,Store,Category,Product Name,Brand,Current Price,Original Price,Savings Amount,Discount %,Is On Special,Special Type,JSON Category")

        for category, products in categorized_products.items():
            for product in products:
                csv_line = f'"{product["productID"]}","{product["store"]}","{category}","{product["name"]}","{product["brand"]}",{product["pricing"]["current_price"]:.2f},{product["pricing"]["original_price"]:.2f},{product["pricing"]["savings_amount"]:.2f},{product["pricing"]["discount_percentage"]:.1f},{product["pricing"]["is_on_special"]},"{product["pricing"]["special_type"]}","{product["categories"]["original_json_category"]}"'
                csv_lines.append(csv_line)

        return "\n".join(csv_lines)

    def generate_json_output(self, categorized_products: Dict[str, List[Dict]],
                           comparisons: Dict[str, List[Dict]]) -> Dict:
        """Generate JSON format output for easy analysis."""
        json_output = {
            "metadata": {
                "generated_date": str(Path().absolute()),
                "total_products": sum(len(products) for products in categorized_products.values()),
                "total_categories": len(categorized_products),
                "stores": ["Coles", "Woolworths"]
            },
            "categories": {},
            "price_comparisons": {},
            "summary": {}
        }

        # Add categorized products
        for category, products in categorized_products.items():
            coles_products = [p for p in products if p['store'] == 'Coles']
            woolworths_products = [p for p in products if p['store'] == 'Woolworths']

            json_output["categories"][category] = {
                "total_products": len(products),
                "coles_count": len(coles_products),
                "woolworths_count": len(woolworths_products),
                "products": {
                    "coles": coles_products,
                    "woolworths": woolworths_products
                }
            }

            # Add to summary
            json_output["summary"][category] = {
                "coles_count": len(coles_products),
                "woolworths_count": len(woolworths_products),
                "total": len(products),
                "has_comparisons": category in comparisons
            }

        # Add price comparisons
        for category, category_comparisons in comparisons.items():
            json_output["price_comparisons"][category] = []

            for comparison in category_comparisons:
                comparison_data = {
                    "similarity_score": round(comparison['similarity'], 3),
                    "price_difference": round(comparison['price_difference'], 2),
                    "cheaper_store": comparison['cheaper_store'],
                    "coles_product": comparison['coles'],
                    "woolworths_product": comparison['woolworths']
                }
                json_output["price_comparisons"][category].append(comparison_data)

        return json_output

    def run_analysis(self):
        """Run the complete price matching analysis."""
        print("Starting price analysis...")

        # Extract and categorize products
        categorized_products = self.extract_products_by_category()

        print(f"Categorized products into {len(categorized_products)} categories")

        # Find price comparisons
        comparisons = self.find_price_comparisons(categorized_products)

        print(f"Found comparisons in {len(comparisons)} categories")

        # Generate reports
        report = self.generate_report(categorized_products, comparisons)
        csv_output = self.generate_csv_output(categorized_products)
        json_output = self.generate_json_output(categorized_products, comparisons)

        # Save reports
        with open("price_comparison_report.md", "w", encoding="utf-8") as f:
            f.write(report)

        with open("price_comparison_data.csv", "w", encoding="utf-8") as f:
            f.write(csv_output)

        with open("price_comparison_data.json", "w", encoding="utf-8") as f:
            json.dump(json_output, f, indent=2, ensure_ascii=False)

        print("Analysis complete!")
        print("- Report saved to: price_comparison_report.md")
        print("- Data saved to: price_comparison_data.csv")
        print("- JSON data saved to: price_comparison_data.json")

        # Print summary
        print("\n=== SUMMARY ===")
        for category, products in categorized_products.items():
            if category != "Other":
                print(f"{category}: {len(products)} products")

if __name__ == "__main__":
    matcher = PriceMatcher()
    matcher.run_analysis()