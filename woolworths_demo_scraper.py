#!/usr/bin/env python3
"""
Woolworths Demo Catalogue Scraper
Creates realistic sample data for demonstration based on typical Australian grocery prices
"""

import json
import random
from datetime import datetime
from typing import List, Dict
from pathlib import Path

class WoolworthsDemoScraper:
    def __init__(self):
        self.products = []
        
        # Realistic Australian grocery products with typical Woolworths pricing
        self.product_data = [
            # Dairy
            {'name': 'Woolworths Full Cream Milk 2L', 'category': 'Dairy', 'brand': 'Woolworths', 'base_price': 3.50, 'special_chance': 0.3},
            {'name': 'Bega Cheese Slices 250g', 'category': 'Dairy', 'brand': 'Bega', 'base_price': 5.50, 'special_chance': 0.2},
            {'name': 'Yoplait Yogurt 6 Pack', 'category': 'Dairy', 'brand': 'Yoplait', 'base_price': 6.00, 'special_chance': 0.4},
            {'name': 'Western Star Butter 500g', 'category': 'Dairy', 'brand': 'Western Star', 'base_price': 7.00, 'special_chance': 0.3},
            {'name': 'Peters Maxibon Ice Cream 4 Pack', 'category': 'Dairy', 'brand': 'Peters', 'base_price': 9.50, 'special_chance': 0.5},
            
            # Meat & Seafood
            {'name': 'RSPCA Chicken Breast Fillets 1kg', 'category': 'Meat & Seafood', 'brand': 'Woolworths', 'base_price': 12.00, 'special_chance': 0.3},
            {'name': 'Beef Mince Premium 500g', 'category': 'Meat & Seafood', 'brand': 'Woolworths', 'base_price': 8.50, 'special_chance': 0.2},
            {'name': 'Primo Ham Slices 100g', 'category': 'Meat & Seafood', 'brand': 'Primo', 'base_price': 4.50, 'special_chance': 0.4},
            
            # Food & Groceries
            {'name': 'Nescafe Instant Coffee 150g', 'category': 'Food & Groceries', 'brand': 'Nescafe', 'base_price': 8.50, 'special_chance': 0.4},
            {'name': 'Cadbury Dairy Milk Chocolate 200g', 'category': 'Food & Groceries', 'brand': 'Cadbury', 'base_price': 6.00, 'special_chance': 0.6},
            {'name': 'Mars Bars 6 Pack', 'category': 'Food & Groceries', 'brand': 'Mars', 'base_price': 7.50, 'special_chance': 0.5},
            {'name': 'Pringles Original 134g', 'category': 'Food & Groceries', 'brand': 'Pringles', 'base_price': 4.00, 'special_chance': 0.7},
            {'name': 'Maltesers Share Bag 140g', 'category': 'Food & Groceries', 'brand': 'Maltesers', 'base_price': 5.50, 'special_chance': 0.4},
            {'name': "Leggo's Pasta Sauce 500g", 'category': 'Food & Groceries', 'brand': "Leggo's", 'base_price': 3.00, 'special_chance': 0.3},
            {'name': 'Heinz Baked Beans 420g', 'category': 'Food & Groceries', 'brand': 'Heinz', 'base_price': 2.50, 'special_chance': 0.2},
            {'name': 'McCain Frozen Chips 1kg', 'category': 'Food & Groceries', 'brand': 'McCain', 'base_price': 4.50, 'special_chance': 0.3},
            {'name': 'Bertolli Olive Oil 500ml', 'category': 'Food & Groceries', 'brand': 'Bertolli', 'base_price': 9.00, 'special_chance': 0.2},
            
            # Health & Beauty
            {'name': 'Pantene Pro-V Shampoo 400ml', 'category': 'Health & Beauty', 'brand': 'Pantene', 'base_price': 8.00, 'special_chance': 0.4},
            {'name': 'Olay Total Effects Face Cream 50g', 'category': 'Health & Beauty', 'brand': 'Olay', 'base_price': 12.50, 'special_chance': 0.3},
            {'name': 'Colgate Total Toothpaste 110g', 'category': 'Health & Beauty', 'brand': 'Colgate', 'base_price': 4.50, 'special_chance': 0.3},
            {'name': 'Dove Body Wash 400ml', 'category': 'Health & Beauty', 'brand': 'Dove', 'base_price': 6.50, 'special_chance': 0.5},
            {'name': 'Dettol Antibacterial Wipes 80 Pack', 'category': 'Health & Beauty', 'brand': 'Dettol', 'base_price': 7.00, 'special_chance': 0.3},
            {'name': 'Blackmores Fish Oil 1000mg 200 Capsules', 'category': 'Health & Beauty', 'brand': 'Blackmores', 'base_price': 25.00, 'special_chance': 0.2},
            
            # Household
            {'name': 'Sorbent Toilet Paper 24 Pack', 'category': 'Household', 'brand': 'Sorbent', 'base_price': 15.00, 'special_chance': 0.4},
            {'name': 'Chux Dishwand Refills 2 Pack', 'category': 'Household', 'brand': 'Chux', 'base_price': 5.50, 'special_chance': 0.3},
            {'name': 'Air Wick Freshener Vanilla 250ml', 'category': 'Household', 'brand': 'Air Wick', 'base_price': 4.50, 'special_chance': 0.4},
            {'name': 'Earth Choice Laundry Powder 2kg', 'category': 'Household', 'brand': 'Earth Choice', 'base_price': 8.00, 'special_chance': 0.3},
            {'name': 'Vileda Supermocio Mop Refill', 'category': 'Household', 'brand': 'Vileda', 'base_price': 12.00, 'special_chance': 0.2},
            
            # Fruit & Vegetables
            {'name': 'Woolworths Bananas 1kg', 'category': 'Fruit & Vegetables', 'brand': 'Woolworths', 'base_price': 3.50, 'special_chance': 0.3},
            {'name': 'Avocados Ready to Eat 4 Pack', 'category': 'Fruit & Vegetables', 'brand': 'Woolworths', 'base_price': 6.00, 'special_chance': 0.4},
            {'name': 'Strawberries 250g Punnet', 'category': 'Fruit & Vegetables', 'brand': 'Woolworths', 'base_price': 4.50, 'special_chance': 0.5},
            {'name': 'Carrots 1kg Bag', 'category': 'Fruit & Vegetables', 'brand': 'Woolworths', 'base_price': 2.50, 'special_chance': 0.2},
            
            # Beverages
            {'name': 'Coca-Cola 24 Pack Cans', 'category': 'Beverages', 'brand': 'Coca-Cola', 'base_price': 18.00, 'special_chance': 0.6},
            {'name': 'Mount Franklin Spring Water 24 Pack', 'category': 'Beverages', 'brand': 'Mount Franklin', 'base_price': 12.00, 'special_chance': 0.4},
            {'name': 'Orange Juice 2L Woolworths', 'category': 'Beverages', 'brand': 'Woolworths', 'base_price': 4.50, 'special_chance': 0.3},
            
            # Baby & Kids
            {'name': 'Millie Moon Nappies Size 3 Jumbo', 'category': 'Baby & Kids', 'brand': 'Millie Moon', 'base_price': 35.00, 'special_chance': 0.3},
            
            # Pet Care
            {'name': 'Supercoat Adult Dog Food 8kg', 'category': 'Pet Care', 'brand': 'Supercoat', 'base_price': 28.00, 'special_chance': 0.2}
        ]
        
        # Special types and their discount patterns
        self.special_types = [
            {'type': 'Half Price', 'discount': 0.5, 'weight': 30},
            {'type': '25% Off', 'discount': 0.25, 'weight': 20},
            {'type': '$2 Off', 'discount': 2.0, 'weight': 15, 'flat': True},
            {'type': '$1 Off', 'discount': 1.0, 'weight': 10, 'flat': True},
            {'type': 'Buy 2 Get 1 Free', 'discount': 0.33, 'weight': 10},
            {'type': '30% Off', 'discount': 0.30, 'weight': 15}
        ]

    def generate_realistic_products(self, num_products: int = None) -> List[Dict]:
        """Generate realistic product data"""
        if num_products is None:
            num_products = min(len(self.product_data), 40)  # Generate up to 40 products
        
        products = []
        selected_products = random.sample(self.product_data, min(num_products, len(self.product_data)))
        
        for i, product_data in enumerate(selected_products, 1):
            # Determine if this product is on special
            is_special = random.random() < product_data['special_chance']
            
            base_price = product_data['base_price']
            current_price = base_price
            original_price = None
            savings = None
            special_type = None
            
            if is_special:
                # Choose a special type
                special_weights = [s['weight'] for s in self.special_types]
                chosen_special = random.choices(self.special_types, weights=special_weights)[0]
                
                original_price = f"${base_price:.2f}"
                special_type = chosen_special['type']
                
                if chosen_special.get('flat', False):
                    # Flat discount (e.g., $2 off)
                    discount_amount = chosen_special['discount']
                    current_price = max(0.50, base_price - discount_amount)  # Minimum 50c
                    savings = f"${discount_amount:.2f}"
                else:
                    # Percentage discount
                    discount_percent = chosen_special['discount']
                    current_price = base_price * (1 - discount_percent)
                    savings = f"${base_price - current_price:.2f}"
            
            # Add some price variation even for non-specials
            if not is_special:
                price_variation = random.uniform(0.9, 1.1)
                current_price = base_price * price_variation
            
            product = {
                'productID': f"WW{i:03d}",
                'productName': product_data['name'],
                'category': product_data['category'],
                'brand': product_data['brand'],
                'price': f"${current_price:.2f}",
                'originalPrice': original_price,
                'savings': savings,
                'specialType': special_type,
                'imageUrl': f"https://cdn0.woolworths.media/content/wowproductimages/large/product_{i:06d}.jpg",
                'productUrl': f"https://www.woolworths.com.au/shop/productdetails/product_{i:06d}",
                'scrapedAt': datetime.now().isoformat(),
                'source': 'demo_data'
            }
            
            products.append(product)
        
        return products

    def scrape_catalogue(self) -> List[Dict]:
        """Generate demo catalogue data"""
        print("Generating Woolworths catalogue demo data...")
        
        # Generate a realistic mix of products
        products = self.generate_realistic_products(35)
        
        self.products = products
        print(f"Generated {len(products)} demo products")
        
        return products

    def save_to_json(self, output_path: str):
        """Save products to JSON file"""
        output_dir = Path(output_path).parent
        output_dir.mkdir(exist_ok=True)
        
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(self.products, f, indent=2, ensure_ascii=False)
        
        print(f"Saved {len(self.products)} products to {output_path}")

def main():
    scraper = WoolworthsDemoScraper()
    
    try:
        products = scraper.scrape_catalogue()
        
        timestamp = datetime.now().strftime('%d%m%Y')
        output_path = f"data/woolworths_demo_{timestamp}.json"
        scraper.save_to_json(output_path)
        
        print(f"\n=== DEMO CATALOGUE SUMMARY ===")
        print(f"Total products: {len(products)}")
        
        # Category breakdown
        categories = {}
        specials_count = 0
        total_savings = 0
        
        for product in products:
            cat = product['category']
            categories[cat] = categories.get(cat, 0) + 1
            
            if product['specialType']:
                specials_count += 1
                if product['savings']:
                    savings_val = float(product['savings'].replace('$', ''))
                    total_savings += savings_val
        
        print(f"Products on special: {specials_count}")
        print(f"Total potential savings: ${total_savings:.2f}")
        
        print("\nProducts by category:")
        for category, count in sorted(categories.items()):
            print(f"  {category}: {count}")
        
        # Show special products
        print("\nSpecial offers:")
        special_products = [p for p in products if p['specialType']]
        for product in special_products[:8]:  # Show first 8 specials
            print(f"  • {product['productName']}")
            print(f"    {product['specialType']}: {product['price']} (was {product['originalPrice']}) - Save {product['savings']}")
        
        print(f"\n✅ Demo data saved to: {output_path}")
        print("This demonstrates the structure and format for Woolworths catalogue data.")
        
    except Exception as e:
        print(f"Demo generation failed: {e}")

if __name__ == "__main__":
    main()