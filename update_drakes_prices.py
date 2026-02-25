"""
Update price_comparison_data.json with Drakes prices from drakes.md
"""

import json
import re
from datetime import datetime

# Drakes price data extracted from drakes.md
drakes_prices = {
    # Ice Cream & Frozen Treats
    "Streets Magnum Ice Cream 4 Pack": {"price": 6.00, "unit": "per litre/kg", "size": "360ml-428ml", "category": "Ice Cream & Frozen Treats", "save": 6.00},
    "Connoisseur Ice Cream": {"price": 0.90, "unit": "per 100ml", "category": "Ice Cream & Frozen Treats", "save": 3.00},
    "Peters Maxibon Cookie Sandwich 4 Pack": {"price": None, "size": "360ml-560ml", "category": "Ice Cream & Frozen Treats", "save": 5.75},

    # Soft Drinks & Beverages
    "Coca-Cola or Flavours 12x300ml": {"price": 2.78, "unit": "per litre", "category": "Soft Drinks & Beverages", "save": 10.00},
    "Coca-Cola 24x375ml": {"price": 2.75, "unit": "per litre", "category": "Soft Drinks & Beverages", "save": 17.15},
    "Pepsi or Schweppes 10x375ml": {"price": 2.53, "unit": "per litre", "category": "Soft Drinks & Beverages"},
    "Pepsi, Solo or Sunkist 1.25lt": {"price": 1.60, "unit": "per litre", "category": "Soft Drinks & Beverages", "save": 1.50},
    "Powerade 1lt": {"price": 2.80, "unit": "per litre", "category": "Soft Drinks & Beverages", "save": 3.50},
    "Gatorade or G Active 600ml": {"price": None, "category": "Soft Drinks & Beverages", "save": 2.10},

    # Dairy
    "Western Star Butter 250g": {"price": 1.40, "unit": "per 100g", "category": "Dairy"},
    "Bega Cheese Slices": {"price": 1.28, "unit": "per 100g", "category": "Dairy"},
    "Bega Tasty Block Cheese 1kg": {"price": 19.00, "unit": "per kg", "category": "Dairy"},
    "Meadow Lea Margarine 1kg": {"price": 0.59, "unit": "per 100g", "category": "Dairy", "save": 0.75},
    "Dare Flavoured Milk 500ml": {"price": 4.80, "unit": "per litre", "category": "Dairy"},
    "Pauls Vanilla Custard 1kg": {"price": 0.39, "unit": "per 100g", "category": "Dairy"},

    # Meat & Deli
    "Australian Economy Beef Fillet (Deli Dept)": {"price": 22.00, "unit": "per kg", "category": "Meat"},
    "Bacon Middle Rashers": {"price": 12.00, "unit": "per kg", "category": "Meat"},
    "Australian Lamb BBQ Chops": {"price": 20.00, "unit": "per kg", "category": "Meat", "save": 3.00, "min_size": "1.2kg"},
    "Australian Baby Back Beef Ribs": {"price": 16.00, "unit": "per kg", "category": "Meat", "save": 4.00},
    "Australian Pork Leg Steaks Bulk tray": {"price": 14.00, "unit": "per kg", "category": "Meat"},
    "Don Footy Franks Skin On 750g": {"price": None, "category": "Meat", "save": 1.90},

    # Snacks
    "Smith's Crinkle Cut or Double Crunch Chips 150g-170g": {"price": None, "category": "Snacks", "save": 2.50},
    "Kettle Chips 150g-165g": {"price": None, "category": "Snacks", "save": 2.50},
    "Ritz Crackers 227g": {"price": 0.77, "unit": "per 100g", "category": "Snacks", "save": 1.75},
    "Quest Tortilla Style Protein Chips 32g": {"price": 14.38, "unit": "per 100g", "category": "Snacks", "save": 2.30},
    "Arnott's Shapes In A Biskit 145g-160g": {"price": None, "category": "Snacks", "save": "1-2"},

    # Confectionery
    "M&M's, Pods or Maltesers 120g-180g": {"price": None, "category": "Confectionery", "save": 2.80},
    "Old Gold Chocolate 176g-180g": {"price": None, "category": "Confectionery"},
    "Cadbury Medium Bars 30g-60g": {"price": 2.33, "unit": "per 100g", "category": "Confectionery", "save": 1.80},
    "Cadbury Share Packs 144g-180g": {"price": None, "category": "Confectionery", "save": 2.10},

    # Pantry
    "Heinz Baked Beans or Spaghetti 535g-555g": {"price": None, "category": "Pantry", "save": 1.30},
    "Bega Peanut Butter 470g": {"price": 1.02, "unit": "per 100g", "category": "Pantry", "save": 2.10},
    "Old El Paso Tortillas 10 Pack 400g": {"price": None, "category": "Pantry", "save": 1.60},

    # Household
    "Cold Power Triple Laundry Capsules 29 Pack": {"price": 0.45, "unit": "each", "category": "Household", "save": 13.00},
    "Comfort Fabric Conditioner 800ml": {"price": 0.56, "unit": "per 100ml", "category": "Household", "save": 4.50},
    "Palmolive Ultra Dishwashing Liquid 950ml": {"price": 0.42, "unit": "per 100ml", "category": "Household", "save": 2.60},
    "Sorbent Soft & Strong Toilet Tissue 48 Pack": {"price": 0.23, "unit": "per 100 sheets", "category": "Household", "save": 3.00},
    "Finish Powerball Ultimate Plus Dishwasher Tablets 45 Pack": {"price": 0.43, "unit": "each", "category": "Household"},

    # Produce
    "Australian Blueberries 125g": {"price": 3.90, "unit": "each", "category": "Produce", "price_per_kg": 31.20},
    "Mandarins": {"price": 3.90, "unit": "per kg", "category": "Produce"},
    "Queensland Lemons": {"price": 4.90, "unit": "per kg", "category": "Produce"},
    "Australian Lebanese Cucumbers": {"price": 3.90, "unit": "per kg", "category": "Produce"},
    "Queensland Mini Roma Tomatoes 250g": {"price": 3.50, "unit": "each", "category": "Produce", "price_per_kg": 14.00},
    "Australian Button or Sliced Mushrooms 500g": {"price": 12.00, "unit": "per kg", "category": "Produce"},

    # Frozen Foods
    "McCain Pub Style Fries or Wedges 750g": {"price": None, "category": "Frozen Foods"},
    "McCain Rustica Thin & Crispy or Sourdough Pizza 335g-450g": {"price": None, "category": "Frozen Foods", "save": 4.40},
    "Nanna's Pies or Salted Caramel Tarts 450g-400g": {"price": None, "category": "Frozen Foods", "save": 3.25},
    "Chiko Rolls 4 Pack 650g": {"price": 1.47, "unit": "per 100g", "category": "Frozen Foods"},

    # Pet Food
    "Whiskas Cat Food 800g": {"price": 0.56, "unit": "per 100g", "category": "Pet Food", "save": 2.50},
    "Whiskas Cat Food 12x85g": {"price": 0.74, "unit": "per 100g", "category": "Pet Food", "save": 5.50},
    "Dine Cat Food 7x85g": {"price": 1.18, "unit": "per 100g", "category": "Pet Food"},
}

def load_json(filepath):
    """Load JSON file"""
    with open(filepath, 'r', encoding='utf-8') as f:
        return json.load(f)

def save_json(filepath, data):
    """Save JSON file"""
    with open(filepath, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def normalize_product_name(name):
    """Normalize product name for matching"""
    # Remove extra spaces, convert to lowercase
    name = re.sub(r'\s+', ' ', name.strip().lower())
    # Remove common variations
    name = re.sub(r'\d+\s*x\s*\d+ml', '', name)
    name = re.sub(r'\d+ml', '', name)
    name = re.sub(r'\d+g', '', name)
    name = re.sub(r'\d+kg', '', name)
    name = re.sub(r'\d+lt?', '', name)
    name = re.sub(r'\d+\s*pack', '', name)
    return name.strip()

def find_matching_product(product_name, drakes_data):
    """Find matching product in Drakes data"""
    normalized_name = normalize_product_name(product_name)

    for drakes_name, drakes_info in drakes_data.items():
        normalized_drakes = normalize_product_name(drakes_name)

        # Check for exact match or partial match
        if normalized_drakes in normalized_name or normalized_name in normalized_drakes:
            return drakes_info

        # Check for key word matches
        drakes_words = set(normalized_drakes.split())
        product_words = set(normalized_name.split())
        common_words = drakes_words & product_words

        # If more than 60% words match, consider it a match
        if len(common_words) / max(len(drakes_words), len(product_words)) > 0.6:
            return drakes_info

    return None

def update_with_drakes_prices(data):
    """Update JSON data with Drakes prices"""

    # Add Drakes to stores if not already there
    if "Drakes" not in data["metadata"]["stores"]:
        data["metadata"]["stores"].append("Drakes")

    matches_found = 0

    # Iterate through all categories
    for category_name, category_data in data["categories"].items():

        # Add drakes_count if not exists
        if "drakes_count" not in category_data:
            category_data["drakes_count"] = 0

        # Create drakes products list if not exists
        if "drakes" not in category_data["products"]:
            category_data["products"]["drakes"] = []

        # Check Coles products for matches
        for product in category_data["products"].get("coles", []):
            drakes_match = find_matching_product(product["name"], drakes_prices)

            if drakes_match:
                # Create Drakes product entry
                drakes_product = {
                    "productID": "DR" + product["productID"][2:],
                    "store": "Drakes",
                    "name": product["name"],
                    "description": product.get("description", ""),
                    "brand": product.get("brand", ""),
                    "pricing": {
                        "current_price": drakes_match.get("price", 0),
                        "current_price_formatted": f"${drakes_match.get('price', 0):.2f}" if drakes_match.get("price") else "TBA",
                        "unit_price": drakes_match.get("unit", ""),
                        "is_on_special": drakes_match.get("save") is not None,
                        "savings_amount": drakes_match.get("save", 0),
                        "special_type": "Weekly Special"
                    },
                    "categories": {
                        "matched_category": category_name
                    }
                }

                # Check if this product already exists in drakes list
                exists = False
                for existing in category_data["products"]["drakes"]:
                    if normalize_product_name(existing["name"]) == normalize_product_name(product["name"]):
                        exists = True
                        break

                if not exists:
                    category_data["products"]["drakes"].append(drakes_product)
                    category_data["drakes_count"] += 1
                    matches_found += 1

    # Update metadata
    data["metadata"]["last_updated"] = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    return data, matches_found

# Main execution
if __name__ == "__main__":
    input_file = "price_comparison_data.json"
    output_file = "price_comparison_data.json"

    print("Loading price comparison data...")
    data = load_json(input_file)

    print("Updating with Drakes prices...")
    updated_data, matches = update_with_drakes_prices(data)

    print(f"Found {matches} matching products")

    print(f"Saving updated data to {output_file}...")
    save_json(output_file, updated_data)

    print("Done!")
