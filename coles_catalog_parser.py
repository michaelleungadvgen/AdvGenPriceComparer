import json
import re
from typing import Dict, List, Optional

class ColesCatalogParser:
    def __init__(self, raw_text: str):
        self.raw_text = raw_text
        self.products = []
        self.product_id_counter = 1
        
    def clean_price(self, price_str: str) -> float:
        """Extract numeric value from price string"""
        # Remove $ and convert to float
        price_str = price_str.replace('$', '').replace(',', '')
        try:
            return float(price_str)
        except:
            return 0.0
    
    def parse_product_block(self, block: str) -> Optional[Dict]:
        """Parse a single product block from the catalog"""
        product = {}
        
        # Skip empty blocks
        if len(block.strip()) < 10:
            return None
            
        # Extract prices using regex patterns
        price_pattern = r'\$\$?(\d+(?:\.\d+)?)\s*(?:eeaa|ppkk|kkgg|bbaagg)?'
        prices = re.findall(price_pattern, block)
        
        # Extract SAVE amount
        save_pattern = r'SAVE\s+\$(\d+(?:\.\d+)?)'
        save_match = re.search(save_pattern, block)
        
        # Extract WAS price
        was_pattern = r'WAS\s+\$(\d+(?:\.\d+)?)'
        was_match = re.search(was_pattern, block)
        
        # Extract product name (usually the longest line that's not a price)
        lines = block.split('\n')
        product_name = ""
        for line in lines:
            # Skip price lines and special offer lines
            if not re.match(r'^[\$\d]|^SAVE|^WAS|^DOWN|^eeaa|^ppkk|^kkgg|^ffoorr', line.strip()):
                if len(line.strip()) > len(product_name):
                    product_name = line.strip()
        
        if not product_name or not prices:
            return None
            
        # Build product dictionary
        product['productID'] = f"CL{str(self.product_id_counter).zfill(3)}"
        product['productName'] = product_name
        
        # Determine category based on keywords
        product['category'] = self.determine_category(product_name, block)
        
        # Extract brand
        product['brand'] = self.extract_brand(product_name)
        
        # Set prices
        if prices:
            product['price'] = self.clean_price(prices[0])
        
        if was_match:
            product['originalPrice'] = self.clean_price(was_match.group(1))
        
        if save_match:
            product['savings'] = self.clean_price(save_match.group(1))
        
        # Determine special type
        if '1/2 PRICE' in block.upper():
            product['specialType'] = '1/2 Price'
        elif 'DOWN DOWN' in block:
            product['specialType'] = 'Down Down'
        elif save_match:
            product['specialType'] = 'Save'
        else:
            product['specialType'] = 'Regular Price'
        
        # Extract unit price if available
        unit_price_pattern = r'\$(\d+(?:\.\d+)?)\s+per\s+(kg|100g|100mL|litre|each)'
        unit_match = re.search(unit_price_pattern, block)
        if unit_match:
            product['unitPrice'] = f"${unit_match.group(1)}/{unit_match.group(2)}"
        
        # Extract weight/size if available
        size_pattern = r'(\d+(?:\.\d+)?)\s*(g|kg|mL|L|Litre|Pack|pack|x\d+)'
        size_match = re.search(size_pattern, product_name)
        if size_match:
            product['size'] = f"{size_match.group(1)}{size_match.group(2)}"
        
        self.product_id_counter += 1
        return product
    
    def determine_category(self, product_name: str, block: str) -> str:
        """Determine product category based on keywords"""
        name_lower = product_name.lower()
        block_lower = block.lower()
        
        categories = {
            'Meat': ['beef', 'pork', 'lamb', 'chicken', 'steak', 'mince', 'sausage', 'bacon'],
            'Fresh Produce': ['cucumber', 'potato', 'onion', 'capsicum', 'lettuce', 'corn', 'banana', 'apple', 'kiwi', 'organic'],
            'Beverages': ['drink', 'cola', 'pepsi', 'juice', 'energy', 'water', 'coffee', 'tea', 'milk'],
            'Snacks': ['chips', 'crackers', 'biscuit', 'cookie', 'chocolate', 'lollies', 'candy'],
            'Baby': ['huggies', 'nappy', 'diaper', 'baby', 'wipes'],
            'Health & Beauty': ['toothpaste', 'shampoo', 'conditioner', 'soap', 'cream', 'lotion', 'deodorant'],
            'Vitamins': ['vitamin', 'supplement', 'tablet', 'magnesium', 'iron', 'calcium'],
            'Pantry': ['rice', 'pasta', 'sauce', 'oil', 'flour', 'sugar', 'noodle'],
            'Dairy': ['cheese', 'yogurt', 'yoghurt', 'butter', 'cream'],
            'Frozen': ['frozen', 'ice cream', 'pizza'],
            'Cleaning': ['detergent', 'cleaner', 'disinfectant', 'wash'],
            'Pet': ['dog', 'cat', 'pet', 'whiskas', 'pedigree']
        }
        
        for category, keywords in categories.items():
            for keyword in keywords:
                if keyword in name_lower or keyword in block_lower:
                    return category
        
        return 'General'
    
    def extract_brand(self, product_name: str) -> str:
        """Extract brand name from product name"""
        brands = ['Coles', 'Cadbury', 'Colgate', "L'Oréal", 'Huggies', 'Kettle', 'Coca-Cola', 
                  'Pepsi', 'Arnott\'s', 'Kellogg\'s', 'Uncle Tobys', 'McCain', 'Primo', 
                  'Sunrice', 'Campbell\'s', 'Nivea', 'Dove', 'Palmolive', 'Dettol',
                  'Nature\'s Way', 'Blackmores', 'Swisse', 'Smith\'s', 'Allen\'s', 
                  'Nestlé', 'Nescafé', 'Milo', 'Peters', 'Streets', 'Gippsland']
        
        for brand in brands:
            if brand.lower() in product_name.lower():
                return brand
        
        # If no known brand found, try to extract first word
        first_word = product_name.split()[0] if product_name else ''
        if first_word and not first_word[0].isdigit():
            return first_word
        
        return 'Generic'
    
    def parse_catalog(self) -> List[Dict]:
        """Parse the entire catalog text"""
        # Split text into potential product blocks
        # Products are typically separated by price indicators
        blocks = re.split(r'\n(?=\$\$?\d)', self.raw_text)
        
        for block in blocks:
            product = self.parse_product_block(block)
            if product and product.get('productName'):
                self.products.append(product)
        
        return self.products
    
    def save_to_json(self, filename: str = 'coles_catalog.json'):
        """Save parsed products to JSON file"""
        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(self.products, f, indent=2, ensure_ascii=False)
        print(f"Saved {len(self.products)} products to {filename}")

def main():
    # Read the raw catalog text from file
    # You would replace this with actual file reading
    raw_catalog_text = """

--- Page 1 ---
QLD Wed 24 Sep to Tue 30 Sep 2025 Contactless Contactless Shop
See page 30 for details collect delivery in store
Serving suggestion only
Footy
fever
22
AAnnyy ffoorr
$$1100
SAVE $3
Coles Classic Burgers or Sausages 400g-550g
Product sold uncooked
Introducing Free with glassware credits**
**See pg 18
European Glassware when you spend $20 in one transaction for T&Cs
and scan your Flybuys
$$22 $$22
$$7722 55 $$8844 00
eeaa eeaa eeaa ppkk
SAVE $2 SAVE $7.25 SAVE $2 SAVE $8.40
WAS $4 WAS $14.50 WAS $4 WAS $16.80
Coca-Cola, Fanta Ben & Jerry's Ice Cream Arnott's Shapes Primo Rindless Short
or Sprite Soft Drink Tub 458mL Crackers 130g-190g Cut Bacon 750g
1.25 Litre $1.60 per litre $1.58 per 100mL $11.20 per kg
FP-QLD-METRO-2409-25-488150
[TABLES FOUND]
 | 
 | 
[END TABLES]

--- Page 2 ---
55 $$55
ffoorr
$$44
ppkk
Green Kiwifruit Australian Raspberries
Single sell $0.90 each. $0.80 each when you buy 5 125g Punnet $40.00 per kg
$$55
$$3355 00
kkgg ppkk
Australian Packham Pears Australian Eureka Blueberries
$3.50 per kg 200g Punnet $25.00 per kg
QLD-METRO-2409-25-488150 2
--- Page 3 ---
100% Aussie Asparagus
1111
$$$$ 7777 0000
bbeeaaaagg
Coles Australian Green Asparagus Bunch
$1.70 per each
$$3399 00 $$3355 00 $$4488 00
SUPPORTING
ppkk kkgg bbaagg
STATE GROWN
Coles Australian Red Perino Queensland Gold Sweet Potatoes Coles Australian Organic Carrots
Tomatoes 200g Pack $19.50 per kg $3.50 per kg 1kg Bag $4.80 per kg
$$55
$$1122 $$1144
eeaa
ppkk ppkk
DOWN DOWN
WAS $5.80 SAVE $2 SAVE $4
AUG 2025 WAS $14 WAS $18
Coles Kitchen Australian Classic Coles Dry Roasted or Roasted Coles Dry Roasted or Roasted
Coleslaw, Pasta or Potato Salad & Salted Mixed Nuts 400g Pack & Salted Cashews 750g Pack
400g Pack $12.50 per kg $30.00 per kg $18.67 per kg
33
$$ 99 00
kkgg
Australian Gourmet Tomatoes
$3.90 per kg
QLD-METRO-2409-25-488150 3
[TABLES FOUND]
$$ | 
 | 
 | 
SAVE $2
WAS $14 | 
 | 
SAVE $4
WAS $18 | 
[END TABLES]

--- Page 4 ---
1/2 PRICE
$$33
$$1166 55 $$3355 00
eeaa eeaa eeaa
SAVE $1.65 SAVE $3 SAVE $3.50
WAS $3.30 WAS $6 WAS $7
John West Salmon 95g Red Rock Deli Potato Chips Mars M&M's, Maltesers
$17.37 per kg 150g-165g or Pods 120g-180g
$$1122 $$2255 00 $$1155
eeaa eeaa eeaa
SAVE $12 SAVE $2.50 SAVE $15
WAS $24 WAS $5 WAS $30
I&J Raw Peeled Prawns U By Kotex Ultra Thin Pads Omo Laundry Liquid 2 Litre
Tail On or Tail Off 500g with Wings Regular 14 Pack or Powder 2kg
$24.00 per kg or Super 12 Pack
11
$$ 77 00
bbaagg
Coles Australian Carrots 1kg Bag
$1.70 per kg
QLD-METRO-2409-25-488150 4
[TABLES FOUND]
 | 
 | 
[END TABLES]

--- Page 5 ---
1/2 PRICE
$$1177 55 $$2277 55 $$3344 55
eeaa eeaa eeaa
SAVE $1.75 SAVE $2.75 SAVE $3.45
WAS $3.50 WAS $5.50 WAS $6.90
Ritz Crackers 227g Kellogg's K-Time Baked Twists Fibre One Bars or Squares
$0.77 per 100g 185g $1.49 per 100g 105g-120g or Nature Valley
Crunchy Bars 252g
$$44
$$5544 00 $$443300
eeaa eeaa eeaa
SAVE $5.40 SAVE $4 SAVE $4.30
WAS $10.80 WAS $8 WAS $8.60
Kellogg's Nutri Grain 765g Nescafé Coffee Sachets Moccona Liquid Espresso
or Just Right Cereal 740g 8 Pack-10 Pack Coffee Sachets 8 Pack
*See page 19 for T&Cs $0.54 per each
BETTER
1T /HA 2N
PRICE
$$88
$$223300 $$1133
eeaa eeaa eeaa
SAVE $2.30 SAVE $8 SAVE $17
WAS $4.60 WAS $16 WAS $30
Leggo's Chunky Pasta Sauce La Española Olive Oil 500mL Gold Sunset Vegetable
490g-500g $1.60 per 100mL or Canola Oil 4 Litre
$0.33 per 100mL
QLD-METRO-2409-25-488150 5
[TABLES FOUND]
 | 
 | 
 | 
 | 
SAVE $5.40
WAS $10.80 | 0
[END TABLES]

--- Page 6 ---
1/2 PRICE
$$442255
$$229955 $$337755
eeaa
eeaa eeaa
SAVE $4.25
SAVE $2.95 SAVE $3.75 WAS $8.50
WAS $5.90 WAS $7.50
Brut 48hr Endurance
U By Kotex Sports Tampons Palmolive Liquid Hand Wash Antiperspirant Aerosol
Regular 16 Pack Refill 1 Litre $0.38 per 100mL. Deodorant 212mL
$0.18 per each *See page 19 for T&Cs $2.00 per 100mL
$$55
$$442255
$$11005500
eeaa eeaa eeaa
SAVE $4.25 SAVE $5 SAVE $10.50
WAS $8.50 WAS $10 WAS $21
Rexona 48hr Aerosol Lynx Fine Fragrance Head & Shoulders Shampoo
Deodorant 250mL Deodorant Body Spray 150mL or Conditioner 660mL
$1.70 per 100mL $3.33 per 100mL $1.59 per 100mL
$$1188
$$11115500
$$11442255 eeaa $$22005500
eeaa
eeaa SAVE $18 eeaa
SAVE $11.50 WAS $36
WAS $23 SAVE $14.25 SAVE $20.50
WAS $28.50 Nature's Own WAS $41
Ogx Argan Oil of Glucosamine Sulfate
Morocco Shampoo or Cenovis Magnesium with Chondroitin Ostelin Calcium DK2
Conditioner 385mL Tablets 200 Pack^ Tablets 200 Pack^ Tablets 60 Pack^
$2.99 per 100mL $7.13 per 100 each $9.00 per 100 each $34.17 per 100 each
^This medicine may not be right for you. Read the label before purchase. Follow the directions for use. If symptoms
persist, talk to your healthcare professional. Vitamin and mineral supplements should not replace a balanced diet.
QLD-METRO-2409-25-488150 6
[TABLES FOUND]
337755
eeaa | 
 | 
 |  | 
SAVE $4.25
WAS $8.50 |  | 
005500
eeaa |  | 
[END TABLES]

--- Page 7 ---
1/2 PRICE
$$11445500 $$6655 00 $$5555 00
eeaa eeaa eeaa
SAVE $14.50 SAVE $6.50 SAVE $5.50
WAS $29 WAS $13 WAS $11
Dynamo Professional Laundry Downy Concentrate Fabric Comfort Fabric Conditioner
Liquid 2 Litre or Capsules Conditioner 800mL-900mL Fragrance Collection 900mL
29 Pack $6.11 per litre. 2nd week on sale
SCENTORY
Excludes
clearance items
$$55
$$11775500 $$2255 00
eeaa eeaa eeaa
SAVE $5 SAVE $17.50 SAVE $2.50
WAS $10 WAS $35 WAS $5
Scentory Room Spray 250mL Botanica by Air Wick Wax Palmolive Ultra Dishwashing
$2.00 per 100mL Blended Candle 1 Each $17.50 Liquid 500mL $0.50 per 100mL.
per each. *See page 19 for T&Cs *See page 19 for T&Cs
$$3311 00 $$5555 00 $$3300
eeaa eeaa eeaa
SAVE $3.10 SAVE $5.50 SAVE $30
WAS $6.20 WAS $11 WAS $60
White King Power Toilet Gel Bref In The Bowl Toilet Cleaner Fairy Platinum Plus Stain
700mL $0.44 per 100mL 2 Pack 100g Remover Dishwashing Tablets
$5.50 per 100g 68 Pack $0.44 per each
QLD-METRO-2409-25-488150 7
[TABLES FOUND]
 |  | 
 | 
SAVE $5
WAS $10 | 
11 00 | 
 | 
[END TABLES]

--- Page 8 ---
1/2 PRICE
$$33
$$113355 $$2255 00
eeaa eeaa eeaa
SAVE $1.35 SAVE $2.50 SAVE $3
WAS $2.70 WAS $5 WAS $6
Twisties, Burger Rings Doritos or XXL Corn Chips Kettle Ridge Cut Potato Chips
or Cheetos 65g-90g 150g-170g 165g $1.82 per 100g.
*See page 19 for T&Cs
$$1155 00 $$116622 $$2277 55
eeaa eeaa eeaa
SAVE $1.50 SAVE $1.63 SAVE $2.75
WAS $3 WAS $3.25 WAS $5.50
Schweppes Mixers or Soft Drink Mt Franklin Lightly Sparkling Raw C Coconut Water 1 Litre
1.1 Litre $1.36 per litre Water 1.25 Litre $1.30 per litre $2.75 per litre
$$2277 55 $$1100 $$1100
eeaa eeaa eeaa
SAVE $2.75 SAVE $10 SAVE $10
WAS $5.50 WAS $20 WAS $20
Golden Circle Zero Sugar Coca-Cola, Fanta or Sprite Mt Franklin Lightly Sparkling
Cordial 2 Litre $1.38 per litre Soft Drink 10x375mL Water 10x375mL
$2.67 per litre $2.67 per litre
QLD-METRO-2409-25-488150 8
[TABLES FOUND]
2277 55
eeaa | 
[END TABLES]

--- Page 9 ---
1/2 PRICE
$$22
$$1122 55 $$1155 00
eeaa eeaa eeaa
SAVE $1.25 SAVE $1.50 SAVE $2
WAS $2.50 WAS $3 WAS $4
Mars Chocolate Bar 30g-56g Nestlé Chocolate Bar 35g-50g Haribo 140g-150g
$$2255 00 $$2255 00 $$2277 55
eeaa eeaa eeaa
SAVE $2.50 SAVE $2.50 SAVE $2.75
WAS $5 WAS $5 WAS $5.50
Oreo Mini Creme Cookies The Natural Confectionery Co. Cadbury Milk Chocolate
10 Pack 204g $1.23 per 100g 130g-230g or Sour Patch 190g Fingers 114g $2.41 per 100g
$$44
$$1155 $$1177
eeaa eeaa eeaa
SAVE $4 SAVE $15 SAVE $17
WAS $8 WAS $30 WAS $34
Cadbury Dairy Milk Block Cadbury Favourites Lindt Cornet 327g-333g
Chocolate 150g-190g 680g-700g
QLD-METRO-2409-25-488150 9
--- Page 10 ---
Footy fever
Serving suggestion only
$$88
$$9955 00
$$3377 00
ppkk ppkk
eeaa SAVE $1 SAVE $1.50
WAS $9 WAS $11
SAVE $1
WAS $4.70
Coles Finest Sausages Coles Finest Beef Brisket Burgers
450g-500g 600g $15.83 per kg
Tip Top White Rolls 6 Pack
450g-520g *See page 19 for T&Cs
22
AAnnyy ffoorr
$$1155
SAVE $2
Coles Entertaining Chicken Tenderloin Skewers 300g,
Beef Sliders 500g or Pork Kransky Bites 400g
QLD-METRO-2409-25-488150 10
[TABLES FOUND]
88 | 
 | 
 | 
 | 
[END TABLES]

--- Page 11 ---
Footy fever
Serving suggestion only
$$4422 00
$$33 $$44
eeaa
eeaa eeaa DOWN DOWN
SAVE $2 SAVE $2.50 WA S $5
WAS $5 WAS $6.50 AUG 2025
Olina's Artisan Crackers La Famiglia Garlic Bread Black Swan Crafted Dip
90g-100g Slices 270g $1.48 per 100g 150g-170g
$15
$10
ea
$$44 22
ffoorr
$$665500 Atiru Marlborough
Sauvignon Blanc
eeaa 750mL
SAVE $1
See page 44 for T&Cs
Coles Cream Cheese 190g Coles Pre-Packed Antipasto
$21.05 per kg 110g-135g
Selected stores only#
#For locations of Deli counter stores visit coles.com.au/range-availability
QLD-METRO-2409-25-488150 11
[TABLES FOUND]
 | 
 | 
[END TABLES]

--- Page 12 ---
Footy fever
Serving suggestion only
$$88
$$3355 00 $$5577 55
ppkk ppkk eeaa
SAVE 65¢ SAVE $1 SAVE $1.95
WAS $4.15 WAS $9 WAS $7.70
Coles Bakery Pizza Rolls Coles RSPCA Approved Chicken Bundaberg Brewed Soft Drink
2 Pack $1.75 per each. Southern Thigh Fillet Burgers 4x375mL $3.83 per litre
Selected stores only 400g $20.00 per kg
$$4488 00 $$6699 00 $$1155
eeaa
eeaa eeaa
SAVE $1.20 SAVE $2.60
WAS $6 WAS $9.50
Birds Eye Golden Crunch Chips Four'N Twenty Traditional Pies Dimmies & Tinnies Dim Sims
900g $5.33 per kg 4 Pack 700g $0.99 per 100g 1.2kg $1.25 per 100g
QLD-METRO-2409-25-488150 12
[TABLES FOUND]
77 55 | 
 | 
[END TABLES]

--- Page 13 ---
Footy fever
$$4455 00 $$4455 00 $$5599 55
eeaa eeaa eeaa
SAVE $1.50 SAVE $1.50 SAVE $2.55
WAS $6 WAS $6 WAS $8.50
Arnott's Tim Tam Biscuits Darrell Lea Bullets or Choc Twists Lindt Excellence Block
165g-200g 150g-204g Chocolate 80g-100g
22 ffoorr 22 ffoorr $$99
$$66 $$66
eeaa
SAVE $2 SAVE $4 SAVE $4
WAS $13
Arnott's Creams Biscuits or Smith's Crinkle Cut Potato Chips Cadbury Chocolate Coated Fruit
Jatz Crackers 200g-250g or Double Crunch 150g-170g or Nut Jar 270g-340g
$$559900
$$1155 $$3300
eeaa
eeaa eeaa
DOWN DOWN
WAS $8.25 SAVE $20
AUG 2025 WAS $50
Remedy Sodaly 4x250mL Coca-Cola Soft Drink 15x250mL Coca-Cola Soft Drink 30x375mL
$5.90 per litre $4.00 per litre $2.67 per litre
QLD-METRO-2409-25-488150 13
[TABLES FOUND]
Footy fever
$$4455 00 $$4455 00 $$5599 55
eeaa eeaa eeaa
SAVE $1.50 SAVE $1.50 SAVE $2.55
WAS $6 WAS $6 WAS $8.50
Arnott's Tim Tam Biscuits Darrell Lea Bullets or Choc Twists Lindt Excellence Block
165g-200g 150g-204g Chocolate 80g-100g
22 ffoorr 22 ffoorr $$99
$$66 $$66
eeaa
SAVE $2 SAVE $4 SAVE $4
WAS $13
Arnott's Creams Biscuits or Smith's Crinkle Cut Potato Chips Cadbury Chocolate Coated Fruit
Jatz Crackers 200g-250g or Double Crunch 150g-170g or Nut Jar 270g-340g
$$559900
$$1155 $$3300
eeaa
eeaa eeaa
DOWN DOWN
WAS $8.25 SAVE $20
AUG 2025 WAS $50
Remedy Sodaly 4x250mL Coca-Cola Soft Drink 15x250mL Coca-Cola Soft Drink 30x375mL
$5.90 per litre $4.00 per litre $2.67 per litre
 | 
 | 
[END TABLES]

--- Page 14 ---
EVERY DAY
Footy Footy
fever fever
$$22
$$1111 55 $$1199 00
eeaa eeaa eeaa
Coles Soft Drink 1.25 Litre Coles Wafer Thin Crackers Coles Tomato or Barbecue
$0.92 per litre 100g $1.90 per 100g Sauce 500mL $0.40 per 100mL
$$2222 00 $$2255 00 $$2266 00
eeaa eeaa eeaa
Coles Corn Chips 175g Coles Sesame Bread Sticks Coles Dip 200g
$1.26 per 100g 125g $2.00 per 100g $1.30 per 100g
$$55 $$55 $$99
eeaa eeaa eeaa
Coles Finest Triple Cream Coles Chocolate & Strawberry Coles Frozen Party Sausage
Brie 200g $25.00 per kg Vanilla Milkshake Pops 8 Pack Rolls 24 Pack 900g
440mL $1.14 per 100mL $1.00 per 100g
QLD-METRO-2409-25-488150 14
[TABLES FOUND]
Footy
fever
Footy
fever
1111 55
eeaa |  | 



[END TABLES]

--- Page 15 ---
40% OFF
$$2255 00 $$2255 00
eeaa eeaa
Footy
SAVE $1.70 SAVE $1.70
WAS $4.20 WAS $4.20
Gatorade Sports Drink 600mL G Active Water 600mL
$4.17 per litre fever $4.17 per litre
94¢
PER
CAN
$$7755 00 $$22225500
eeaa eeaa
SAVE $5 SAVE $15
WAS $12.50 WAS $37.50
Red Bull Energy Drink 4x250mL Pepsi Max or Solo Zero Sugar Soft Drink
$7.50 per litre 24x375mL $2.50 per litre
$$3300
eeaa
SAVE $20
WAS $50
Coca-Cola Zero Sugar Soft Drink
30x375mL $2.67 per litre
$$1155 $$22
eeaa
eeaa
SAVE $2
WAS $4
Coca-Cola Zero Sugar Coca-Cola
Soft Drink 15x250mL Zero Sugar
$4.00 per litre Soft Drink
1.25 Litre
$1.60 per litre
$$1100
eeaa
SAVE $10
Coca-Cola Zero Sugar Soft Drink
*See page 19 for T&Cs WAS $20 10x375mL $2.67 per litre
ADVERTISER PROMOTION
QLD-METRO-2409-25-488150 15
[TABLES FOUND]
 | 
 | 
 |  | 
 |  | 
 |  | SAVE $1.70
WAS $4.20
fever | 
 | 
 |  |  | 
 | $$3300
eeaa
SAVE $20
WAS $50
Coca-Cola Zero Sugar Soft Drink
30x375mL $2.67 per litre
$$1155 $$22
eeaa
eeaa
SAVE $2
WAS $4
Coca-Cola Zero Sugar Coca-Cola
Soft Drink 15x250mL Zero Sugar
$4.00 per litre Soft Drink
1.25 Litre
$1.60 per litre
$$1100
eeaa
SAVE $10
Coca-Cola Zero Sugar Soft Drink
*See page 19 for T&Cs WAS $20 10x375mL $2.67 per litre |  | 
 | *See page 19 for T&Cs |  | 
 |  |  | 
ADVERTISER PROMOTION | ADVERTISER PROMOTION |  | 
 |  |  | 


 | 
 | 
 | 
SAVE $10
WAS $20 | 
[END TABLES]

--- Page 16 ---
Footy 1/2 PRICE Footy
fever fever
$$2299 00 $$4422 55 $$4455 00
eeaa eeaa eeaa
SAVE $2.90 SAVE $4.25 SAVE $4.50
WAS $5.80 WAS $8.50 WAS $9
McCain Airfryer Chips 750g Proud & Punch Frozen Sara Lee Frozen Cheesecake
$3.87 per kg. Dessert Sticks 6 Pack 360g-425g
*See page 19 for T&Cs 420mL-450mL
$$66
$$55
eeaa
eeaa
DOWN DOWN
WAS $7.50
AUG 2025
Coles Frozen Sweet Potato Chips 750g Edgell Chiko Rolls 4 Pack 650g
$6.67 per kg $0.92 per 100g
$$77
$$6699 00 $$11008800
eeaa eeaa eeaa
SAVE $3.10 SAVE $7.20 SAVE $2.50
WAS $10 WAS $18 WAS $9.50
Herbert Adams Slow Cooked Patties Party Pack 30 Pack Dr. Oetker Ristorante Pizza
Beef Pies 2 Pack 400g-420g 1.25kg $0.86 per 100g 310g-390g
QLD-METRO-2409-25-488150 16
[TABLES FOUND]
Footy
fever | 1/2 PRICE | Footy
fever
 |  | 
 | 
 | 
 | 
 | 
 | 
 | 
[END TABLES]

--- Page 17 ---
Footy fever
22
AAnnyy ffoorr
77
$$
SAVE $2
Black Swan Favourites Dip 180g-200g, Yumi's Dip 200g
or Red Rock Deli Dip 130g-135g
$$44
$$5599 00 $$6655 00
eeaa eeaa ppkk
SAVE $1.50 SAVE $6.05 SAVE $1.30
WAS $5.50 WAS $11.95 WAS $7.80
Obela Garnished Hommus Dip Tasmanian Heritage Double Don Footy Franks Skinless 600g
220g $1.82 per 100g. Cream Brie or Camembert 200g $10.83 per kg
On sale for 2 weeks $29.50 per kg
$$555500
$$4455 00
$$6699 00 $$1100
eeaa
eeaa
eeaa eeaa
DOWN DOWN SAVE $1.20
WAS $6.50 WAS $5.70 SAVE $3 SAVE $1
AUG 2025 WAS $9.90 WAS $11
Philadelphia Cream
Yumi's Falafel Balls Cheese Block 250g Mersey Valley Cheese Bega Cheese Slices 500g
225g $2.44 per 100g $18.00 per kg. 235g $29.36 per kg. $20.00 per kg
On sale for 2 weeks On sale for 2 weeks
QLD-METRO-2409-25-488150 17
[TABLES FOUND]
 | 
 | 
 | 
 | 
 | 
 | 
 | 
 | 
 | 
 | 
[END TABLES]

--- Page 18 ---
Free
European Glassware
with glassware credits**
Every
$20 spent
= 1 credit
when you scan
your Flybuys
FREE
with 36 credits**
or
18 credits
FREE FREE
+ $18
with 40 credits** with 48 credits**
or or
20 credits 24 credits
+ $20 + $24
FREE
with 36 credits**
or
18 credits
+ $18 FREE
with 32 credits**
or
16 credits
+ $16
FREE
with 22 credits**
or
11 credits FREE
+ $11
with 26 credits**
or
13 credits
+ $13
**Spend $20 in one transaction in-store or online at Coles or Liquorland (after savings and discounts have been applied) and scan your Flybuys to receive
one glassware credit. Earn credits until 6/1/26 and redeem credits (excludes Liquorland, Liquorland Online & Coles App) by 13/1/26 or while stocks last.
$20 spend excludes the purchase of Coles Insurance products, Coles Express, DoorDash, Menulog, UberEats, iTunes cards, gift cards, Mobile and Tech
Accessories (inc. plans and recharge), Opal top up, calling cards, charity products, tobacco, tobacco related product purchases, subscriptions, delivery
fees and liquor purchases in WA North and NT. For full terms & conditions visit coles.com.au/glassware-terms Schott Zwiesel®
QLD-METRO-2409-25-488150 18
[TABLES FOUND]
 | 
[END TABLES]

--- Page 19 ---
Bonus
glassware
credit*
1 Bonus
when you purchase a participating
glassware product and spend $20 in one
transaction & scan your Flybuys.
credit
Scan to check out all the
participating products
*Spend $20 in one transaction, scan your Flybuys and receive one bonus glassware credit when you also purchase a selected product from a participating
brand or product range. Limit of one glassware credit applies per brand, per transaction. $20 spend excludes the purchase of Coles Insurance products,
Coles Express, DoorDash, Menulog, UberEats, iTunes cards, gift cards, Mobile and Tech Accessories (inc. plans and recharge), Opal top up, calling cards,
charity products, tobacco, tobacco related product purchases, subscriptions, delivery fees and liquor purchases in WA North and NT. For full terms &
conditions visit coles.com.au/glassware-terms Schott Zwiesel®
QLD-METRO-2409-25-488150 19
--- Page 20 ---
DOWN
DOWN
$$55 $$99
$$11335500
kkgg
ppkk ppkk
DOWN DOWN DOWN DOWN DOWN DOWN
WA S $6 WA S $10 WAS $15kg
AUG 2025 AUG 2025 AUG 2025
Coles Australian Pork Mince Coles Australian Pork Coles Free Range RSPCA
3 Star Regular 500g Thin Cut Loin Steaks 500g Approved Chicken Breast
$10.00 per kg $18.00 per kg Large Pack $13.50 per kg
$$1177
$$1166 $$1155
ppkk
kkgg kkgg
SAVE $3
WAS $20 SAVE $2 SAVE $2
kg kg
WAS $18kg WAS $17kg
Coles Australian No Added
Hormones GRAZE Grass Fed Beef Coles Australian No Added Coles Australian No Added
New York Strip Steak 380g Hormones Beef Slow Cook Hormones Beef Slow Cook Brisket
$44.74 per kg Short Ribs $16.00 per kg $15.00 per kg
$$1111 $$11115500 $$2266
ppkk ppkk kkgg
SAVE $3 SAVE $1.50 SAVE $7
kg
WAS $14 WAS $13 WAS $33kg
Coles Made Easy Slow Cooked Coles RSPCA Approved Chicken Coles Australian Lamb Loin Chops
Honey BBQ Pork Belly Bites 500g Boneless Herb & Garlic 1kg $26.00 per kg. On sale for 2 weeks
$22.00 per kg $11.50 per kg
Serving suggestion only
$$1111
ppkk
SAVE $4
Coles Australian No Added Hormones Beef
WAS $15 Rump Steak 2 Pack 500g $22.00 per kg.
Product sold uncooked
QLD-METRO-2409-25-488150 20
[TABLES FOUND]
 | gg
SAVE $2
kg
WAS $18kg | 
 | 
SAVE $2
kg
WAS $17kg | 
 | 
SAVE $1.50
WAS $13 | 

Coles Australian No Added Hormones Beef
Rump Steak 2 Pack 500g $22.00 per kg.
Product sold uncooked
[END TABLES]

--- Page 21 ---
$$2288
kkgg
SAVE $11
kg
WAS $39kg
Coles Queensland Thawed Cooked Extra
Large Black Tiger Prawns $28.00 per kg.
Selected stores only#. On sale for 2 weeks Serving suggestion only
$$1100 $$1111
ppkk ppkk
SAVE $5 SAVE $5
WAS $15 WAS $16
Tassal Smoked Salmon 150g Hans Twiggy Sticks 500g
$66.67 per kg $22.00 per kg. Selected stores only#
$$1199 $$2288
kkgg kkgg
SAVE $3 SAVE $8
kg kg
WAS $22kg WAS $36kg
Karumba Australian MSC Certified Thawed Raw Coles Australian Fresh Barramundi
Banana Prawns $19.00 per kg. Portions Skin On $28.00 per kg. Selected stores only#
Selected stores only#. 2nd week on sale
$$11775500
ppkk
DOWN DOWN
WAS $19.50
AUG 2025
Coles Tasmanian Salmon Portions Skin On
4 Pack 460g $38.04 per kg.
From the Meat department Serving suggestion only
#For locations of Deli counter stores visit coles.com.au/range-availability
QLD-METRO-2409-25-488150 21
[TABLES FOUND]
 | 
 | 
SAVE $5
WAS $15 | 
 | 
 | 
SAVE $5
WAS $16 | 
 | 00
kk
WN
50
DOWN DOWN
WAS $19.50
AUG 2025 | WN
50
[END TABLES]

--- Page 22 ---
EVERY DAY
$$33
$$3344 00 $$11225500
eeaa eeaa eeaa
Coles Kitchen Tomato Pizza Coles Kitchen Garlic Baguette Coles Beef Lasagne 1.8kg
Base 300g $1.00 per 100g Twin Pack 450g $0.76 per 100g $0.69 per 100g
$$55 $$77
$$5544 00
eeaa eeaa eeaa
SAVE $1.40 SAVE $2.50 SAVE $4
WAS $6.80 WAS $7.50 WAS $11
Dari’s Soup 550g On the Menu Pasta Sauce 425g On The Menu Filled Pasta 600g
$0.98 per 100g $1.18 per 100g $1.17 per 100g
$$558800
$$4433 00 $$9955 00 $$11335500
eeaa
eeaa
eeaa eeaa
DOWN DOWN
SAVE $1.10 WAS $6.50 SAVE $1
WAS $5.40 AUG 2025 WAS $10.50
Dare Flavoured Milk Perfect Italiano Bega Cheese Block or Devondale Shredded
750mL $5.73 per litre Grated Cheese 250g Grated 500g Cheese 750g
$23.20 per kg $19.00 per kg $18.00 per kg
QLD-METRO-2409-25-488150 22
[TABLES FOUND]
 | 
 | 
 | 
 | 
[END TABLES]

--- Page 23 ---
SPECIALS
OONN PPRROOTTEEIINN FFAAVVEESS
$$2244 00 $$222200
eeaa eeaa
SAVE 60¢ SAVE $1.50
WAS $3 WAS $3.70
Danone YoPro Protein Yoghurt Pouch Danone YoPro Perform Protein Yoghurt
150g $1.60 per 100g 175g $1.26 per 100g
$$339900
$$2299 00
eeaa
eeaa
DOWN DOWN
SAVE $1.50 WAS $4.40
WAS $4.40 SEP 2025
Pauls Plus+ Protein Low Fat Milk 400mL Oak Plus Protein Milk 500mL
$7.25 per litre $7.80 per litre
$$66
$$2233 00
eeaa eeaa
SAVE $1.50
WAS $7.50
Coles Nature's Kitchen Plant Based Firm Tofu Vegie Delights Plant Based Sausages 300g
300g $0.77 per 100g $2.00 per 100g
QLD-METRO-2409-25-488150 23
[TABLES FOUND]
 |  | 
SAVE $1.50
WAS $4.40 |  | 
 | 
 | 
[END TABLES]

--- Page 24 ---
1/2 PRICE
$$33
$$2233 00
eeaa eeaa
SAVE $3 SAVE $2.30
WAS $6 WAS $4.60
Mr Kipling Cakes or Slices Golden Crumpet Rounds 6 Pack 300g
155g-165g $0.77 per 100g
$$447700
$$3355 00 $$445500
eeaa
eeaa eeaa
DOWN DOWN
SAVE $1 WAS $5.50
WAS $4.50 AUG 2025
Mission Souvlaki Bread 320g Wonder White Bread Simson's Pantry Low Carb High
$1.09 per 100g 680g-700g Protein White Wraps 5 Pack 225g
$2.09 per 100g
$$225500 $$445500
$$55
eeaa eeaa
eeaa
DOWN DOWN DOWN DOWN
WAS $3.25 WAS $4.90 SAVE $1.50
AUG 2025 JUL 2025 WAS $6.50
Coles Bakery Stone Baked by Coles Muffins 4 Pack 420g or Coles Bakery Cookies 24 Pack
Laurent Mini Pane di Casa 9 Pack 315g $0.21 per each. Selected stores only
$2.50 per each. Selected stores only
$$33
EVERY DAY $$2299 00
ppkk eeaa
Coles Bakery Rolls 6 Pack Coles Bake at Home Dinner
$0.50 per each. Rolls 6 Pack 220g-260g
Selected stores only
QLD-METRO-2409-25-488150 24
[TABLES FOUND]
 | 
 | 
 |  | 
 |  | 
00 | 
 | 
 | 5500
eeaa
OWN
DOWN DOWN
WAS $3.25
AUG 2025 | OWN
[END TABLES]

--- Page 25 ---
1/2 PRICE
$$1144 00 $$2255 00
eeaa eeaa
SAVE $1.40 SAVE $2.50
WAS $2.80 WAS $5
Continental Cup a Soup Heinz Ketchup 500mL
2 Serves 50g-75g $0.50 per 100mL. *See page 19 for T&Cs
$$22
$$3355 00
$$22
eeaa eeaa $$3377 00
eeaa SAVE 90¢ SAVE $1 eeaa
WAS $2.90 WAS $4.50
SAVE 75¢ SAVE $1
WAS $2.75 Heinz Baked Beanz Campbell's Country WAS $4.70
or Spaghetti 300g Ladle or Chunky Soup
Campbell's Real Stock $0.67 per 100g. 495g-505g Sirena Tuna 185g
500mL $0.40 per 100mL *See page 19 for T&Cs On sale for 2 weeks $20.00 per kg
$$44
$$3366 00 $$1155 $$2200
eeaa eeaa eeaa eeaa
SAVE 90¢ SAVE $1.50 SAVE $8 SAVE $5
WAS $4.50 WAS $5.50 WAS $23 WAS $25
Sunrice Microwave Rice Patak's Simmer Sauce Bertolli Olive Oil 750mL Red Island Extra Virgin
Pouch 450g 450g $0.89 per 100g $2.00 per 100mL Olive Oil 1 Litre
$0.80 per 100g $2.00 per 100mL
QLD-METRO-2409-25-488150 25
--- Page 26 ---
40% OFF
$$3399 00 $$4422 00
eeaa eeaa
SAVE $2.60 SAVE $2.80
WAS $6.50 WAS $7
Uncle Tobys Plus Antioxidants 435g Nestlé Milo Cereal 340g-350g
$0.90 per 100g
$$44 $$88
eeaa eeaa
SAVE $2.80 SAVE $5.50
WAS $6.80 WAS $13.50
Spam Canned Ham 340g Twinings Tea Bags
$11.76 per kg 80 Pack-100 Pack
$$665500
$$2266 00 $$4455 00
eeaa
eeaa eeaa
DOWN DOWN
SAVE 80¢ SAVE $1.30 WAS $7.90
WAS $3.40 WAS $5.80 SEP 2025
Pureharvest Unsweetened Organic Bega Smooth or Crunchy Peanut Mayver's Pure Goodness Everything
Almond Milk 1 Litre $2.60 per litre Butter 375g $1.20 per 100g Spread 280g $2.32 per 100g
50¢
PER
BOTTLE
$$44
$$3355 00 $$1122
eeaa
eeaa eeaa
SAVE $1.30 SAVE $1.50
WAS $4.80 WAS $5.50
Golden Circle Apple or Mango V8 Juice 1.25 Litre Frantelle Natural Still Water
Nectar Juice 2 Litre $1.75 per litre $3.20 per litre 24x600mL $0.83 per litre
QLD-METRO-2409-25-488150 26
[TABLES FOUND]
00 | 
 | 
 | 
 | 
[END TABLES]

--- Page 27 ---
DOWN
DOWN
$$55
$$663300 $$3355
eeaa eeaa eeaa
DOWN DOWN DOWN DOWN DOWN DOWN
WAS $5.80 WAS $7.90 WAS $50
AUG 2025 AUG 2025 JUL 2025
Wellness Road Black Chia Heritage Mill Muesli or Clusters Jed's Coffee Beans 1kg
Seeds 300g $1.67 per 100g 750g $0.84 per 100g $3.50 per 100g
$$22 $$44
$$6688 00
eeaa eeaa eeaa
SAVE $1.30 SAVE $2.30 SAVE $1.80
WAS $3.30 WAS $6.30 WAS $8.60
Vitasoy Unsweetened Almond Mayver's Peanut Butter 375g Capilano 100% Pure
Milk 1 Litre $2.00 per litre $1.07 per 100g Australian Honey Squeeze 500g
$1.36 per 100g
$$6655 00 $$1166 $$2266
eeaa eeaa eeaa
SAVE $3 SAVE $7 SAVE $11.50
WAS $9.50 WAS $23 WAS $37.50
Vittoria Nespresso Compatible Vittoria Freeze Dried Instant Moccona Freeze Dried Instant
Coffee Capsules 10 Pack Coffee 200g $8.00 per 100g Coffee 400g $6.50 per 100g
$0.65 per each. On sale for 2 weeks
QLD-METRO-2409-25-488150 27
--- Page 28 ---
Bringing home
the bacon
22
ffoorr
88
$$
SAVE $3
Coles Bacon 200g
$20.00 per kg. Product sold uncooked
Serving suggestion only
QLD-METRO-2409-25-488150 28
--- Page 29 ---
11
$$ 33 55
eeaa $$6622 00
SAVE $1.35 eeaa
WAS $2.70 SAVE $1.80
WAS $8
Chobani Greek Yogurt Pouch 140g Tamar Valley Dairy Greek Style
$0.96 per 100g Yoghurt 1kg $0.62 per 100g
$$445500
$$1166 55 $$1188 55
eeaa
eeaa ee aa
DOWN DOWN
SAVE 85¢ SAVE 85¢ WAS $5.75
WAS $2.50 WAS $2.70 AUG 2025
Farmers Union Greek Style or No Chobani Greek or No Sugar Danone Activia Probiotics
Added Sugar Kids Yogurt Pouch Added Greek Yogurt 150g-160g No Added Sugar Yoghurt 4x125g
130g $1.27 per 100g $0.90 per 100g
$$667700
$$2255 00 $$4488 00 $$6633 00
eeaa
eeaa eeaa eeaa
DOWN DOWN
SAVE $1 SAVE $1.40 SAVE 90¢ WAS $7.50
WAS $3.50 WAS $6.20 WAS $7.20 SEP 2025
The Juice Lab Wellness Boost Juice 1 Litre Tamar Valley Dairy The Western Star
Shot 60mL $41.67 per litre $4.80 per litre Creamery Yoghurt 700g Spreadable Blend 500g
$0.90 per 100g $1.34 per 100g
QLD-METRO-2409-25-488150 29
[TABLES FOUND]
 | 
 | 
 | 
 | 
 | 
 | 
 | 
 | 
[END TABLES]

--- Page 30 ---
$$44 $$55
eeaa eeaa
Bonus
SAVE $1.80 SAVE $2.50
WAS $5.80 WAS $7.50
McCain Beer Batter Chips Pickers Snacks 230g-350g
750g $5.33 per kg
glassware
credit*
22 22
ffoorr ffoorr
$$88 $$1155
SAVE $3.20 SAVE $4
McCain Red Box Tray Meal McCain Pub Size Meal
375g-400g 480g-500g
*See page 19 for T&Cs
$$99
$$7755 00 $$11226600
eeaa
eeaa eeaa
SAVE $2 SAVE $3 SAVE $5.40
WAS $9.50 WAS $12 WAS $18
Core Powerfoods Frozen Meal Birds Eye Oven Bake Fish Fillets KB's Gyoza 750g
350g $2.14 per 100g 425g $21.18 per kg $1.68 per 100g
Catalogue on sale Wed 24th Sep until Tue 30th Sep 2025 or until Tue 7th Oct where indicated. Selected
specials commenced Wed 17th Sep. Promotional prices commence 7am Wednesday and may commence or
extend beyond the advertised date. Promotional products and prices are not available on DoorDash, UberEats or at Coles
Express, and may not be available at Coles online, Coles Local, Coles Brisbane CBD, Express Myer, Tweed City, Tweed Heads,
Banora Central and Banora Point. Deli promotions are not available at Coles Ormeau, Nerang Fair, Upper MT Gravatt,
Murrumba Downs, Helensvale, Everton Park, Strathpine Westfield, Benowa, Waterford, Mt Warren Park, Caneland (Mackay),
Mudgeeraba, Upper Coomera, Pelican Waters, Aspley, Edmonton, Ferny Grove, Jindalee & Springfield. Savings, single sell prices
and unit prices shown are off Queensland regular selling prices. Some advertised products may already be priced below the
Queensland selling price therefore savings and unit prices may vary. Multi Save price only available when purchased in the
multiples specified. We reserve the right to limit sale quantities. Some product varieties, colours or styles may not be available
at all stores and only available while stocks last.
QLD-METRO-2409-25-488150 30
[TABLES FOUND]
6600 | 
 | 
[END TABLES]

--- Page 31 ---
$$66
$$6655 00 $$7722 00
eeaa
eeaa eeaa
DOWN DOWN
WA S $7 SAVE $3 SAVE $4.80
JUL 2025 WAS $9.50 WAS $12
Toscano Petite Liege Style Waffles Weis Frozen Dessert Bars Connoisseur Ice Cream Tub
6 Pack 270g $2.22 per 100g 4 Pack-6 Pack 264mL-280mL 1 Litre $0.72 per 100mL
$$88
$$8844 00 $$8877 00
eeaa eeaa eeaa
SAVE $2 SAVE $3.60 SAVE $2.20
WAS $10 WAS $12 WAS $10.90
Weis Frozen Dessert Tub 1 Litre Streets Magnum Sticks Peters Original Ice Cream Tub
$0.80 per 100mL 4 Pack-6 Pack 360mL-428mL 4 Litre $0.22 per 100mL
$$5555 00
eeaa
Bulla Creamy Classics Ice Cream
Tub 2 Litre $0.28 per 100mL
SAVE $5.50
WAS $11
ADVERTISER PROMOTION
QLD-METRO-2409-25-488150 31
[TABLES FOUND]
 | 
 | 
 | 
 | 


 | 
 | 
[END TABLES]

--- Page 32 ---
$$66
$$665500
eeaa eeaa
Coles Frozen Mango Sorbet Sticks 6 Pack Streets Blue Ribbon Frozen Dessert Tub 2 Litre
360mL $1.67 per 100mL $0.33 per 100mL.*See page 19 for T&Cs
$$1111
$$6699 00 $$8855 00
eeaa
eeaa eeaa
DOWN DOWN
SAVE $2.60 SAVE $4 WAS $12.50
WAS $9.50 WAS $12.50 SEP 2025
Peters Drumstick Cones Norco Cape Byron Ice Cream Destination Italy Gelato Tub
4 Pack-6 Pack 475mL-490mL Sticks 4 Pack 380mL 500mL $2.20 per 100mL
$2.24 per 100mL
$$7722 55
eeaa
SAVE $7.25
WAS $14.50
Ben & Jerry's Ice Cream Tub
458mL $1.58 per 100mL
ADVERTISER PROMOTION
QLD-METRO-2409-25-488150 32
[TABLES FOUND]
 | 
 | 
[END TABLES]

--- Page 33 ---
1/2 PRICE
SUNSCREEN
Exclusions apply†
$$77 $$88 $$99
eeaa eeaa eeaa
SAVE $7 SAVE $8 SAVE $9
WAS $14 WAS $16 WAS $18
Hamilton Active Family Olay Complete Defence Sensitive Surf Life Saving Sport Sunscreen
Sunscreen SPF50+ 110mL^ Daily UV Moisturising Lotion Lotion SPF50+ 200mL^
$6.36 per 100mL SPF30 75mL $10.67 per 100mL $4.50 per 100mL
$$11007755 $$1111 $$11335500
eeaa eeaa eeaa
SAVE $10.75 SAVE $11 SAVE $13.50
WAS $21.50 WAS $22 WAS $27
Nivea Sun Protect & Moisture Banana Boat Sport Neutrogena Hydro Boost Face
Sunscreen SPF50+ 200mL^ Sunscreen SPF50+ 200g^ Sunscreen SPF50 85mL^
$5.38 per 100mL $5.50 per 100g $15.88 per 100mL
†Excludes Every Day, Down Down & clearance items
^This product may not be right for you. Read the warnings before purchase. Avoid prolonged exposure
to the sun. Wear protective clothing, hats and eyewear when exposed to the sun.
Frequent reapplication is required for effective sun protection.
tt
$$1133 $$2222 $$6655 00
eeaa eeaa eeaa
SAVE $13 SAVE $15 SAVE $4.50
WAS $26 WAS $37 WAS $11
Garnier Good Hair Colourant Garnier Vitamin C* Garnier Pure Active
1 Pack $13.00 per each Brightening Serum 30mL Pimple Patches
$73.33 per 100mL 22 Pack
$0.30 per each
tt
ADVERTISER PROMOTION
QLD-METRO-2409-25-488150 33
[TABLES FOUND]
 |  | 
 |  | tt
 |  | 



[END TABLES]

--- Page 34 ---
40% OFF
$$33
$$1122
eeaa eeaa
SAVE $2 SAVE $8
WAS $5 WAS $20
Rascals Premium Baby Wipes BabyLove Bulk Nappy Pants
72 Pack–80 Pack 22 Pack-28 Pack
$$22 $$8855 00 $$4488 00
$$3355 00
eeaa eeaa eeaa
eeaa
SAVE $1.70 SAVE $5.90 SAVE $3.20
SAVE $2.40 WAS $3.70 WAS $14.40 WAS $8
WAS $5.90
Poise Bladder Depend Real Fit Listerine Freshburst
Poise Continence Protection Light Underwear for or Freshburst Zero
Ultra Long Liners Continence Liners Women XL 8 Pack Mouthwash 500mL
20 Pack $0.18 per each 18 Pack $0.11 per each $1.06 per each $0.96 per 100mL
$$4477 55 $$99 $$1122
$$1100
eeaa eeaa
eeaa eeaa
SAVE $4.75 SAVE $8
WAS $9.50 SAVE $6 SAVE $7 WAS $20
WAS $15 WAS $17
Nivea Antiperspirant Neutrogena
Aerosol Deodorant Garnier Body Garnier Micellar Hydro Boost Water
250mL $1.90 per 100mL. Superfood 380mL Cleansing Water 400mL Gel Cleanser 145mL
2nd week on sale $2.37 per 100mL $2.50 per 100mL $8.28 per 100mL
QLD-METRO-2409-25-488150 34
[TABLES FOUND]
 | 
[END TABLES]

--- Page 35 ---
$$22
$$1166 $$2200
eeaa eeaa eeaa
SAVE 30¢ SAVE $4 SAVE $5
WAS $2.30 WAS $20 WAS $25
Rafferty's Garden 4+ Months, Alula Gentle Stage 3 Toddler Nestlé NAN Optipro Stage
6+ Months or 8+ Months Baby Milk Drink 900g $1.78 per 100g 3 Toddler Milk Drink 800g
Food Pouch 120g $1.67 per 100g $2.50 per 100g
$$995500
$$22005500 $$22115500
eeaa
eeaa eeaa
DOWN DOWN
WA S $17 SAVE $9.40 SAVE $9.50
SEP 2025 WAS $29.90 WAS $31
Nivea Body Wash 1 Litre Metamucil Daily Fibre Rascals Premium Jumbo Nappy
$0.95 per 100mL Supplement Orange Smooth Pants 42 Pack-56 Pack
72 Doses 425g^ $4.82 per 100g
^This medicine may not be right for you. Read the label before purchase. Follow the directions for use.
If symptoms persist, talk to your healthcare professional. Incorrect use could be harmful.
$$2255 00 $$2255 00
eeaa eeaa
SAVE $2.50 $$2288 00 SAVE $2.50
WAS $5 WAS $5
eeaa
U By Kotex Ultra Thin Pads U By Kotex Zero Pads U by Kotex Overnight
Regular with Wings 14 Pack SAVE $2.80 Super with Wings Extra Pads with Wings
$0.18 per each WAS $5.60 10 Pack $0.28 per each 10 Pack $0.25 per each
ADVERTISER PROMOTION
QLD-METRO-2409-25-488150 35
[TABLES FOUND]
00
WN | 
 | 
[END TABLES]

--- Page 36 ---
$$33
$$4455 00 $$5555 00
eeaa eeaa eeaa
SAVE $1.50 SAVE $2.50 SAVE $2.50
WAS $4.50 WAS $7 WAS $8
Colgate Plax Freshmint Colgate Total Active Prevention Colgate Max Fresh Watermelon
Mouthwash 250mL Deep Clean or Whitening Toothpaste 100g
$1.20 per 100mL Toothpaste 115g $3.91 per 100g $5.50 per 100g
*See page 19 for T&Cs
$$33 $$55
$$2277 55
eeaa eeaa eeaa
SAVE $2.75 SAVE $3 SAVE $5
WAS $5.50 WAS $6 WAS $10
Oral B 3D White 2 In 1 Whitening Oral B Pro Health Advanced Oral B Sensitivity & Gum Extra
Long Lasting or Enamel Toothpaste Deep Clean or Whitening Fresh or All Day Protection
110g $2.50 per 100g Toothpaste 110g $2.73 per 100g Toothpaste 90g $5.56 per 100g
++
& FIGHTS CAVITIES^
EVEN IN BACK MOLARS
$$1100
$$8800
eeaa
eeaa
SAVE $10
SAVE $80 WAS $20
WAS $160
Oral B 3D White Freshness
Oral B iO2 Electric Blast Intensive Whitening
Regular Oral-B iO
Toothbrush Black Toothpaste 94g
Manual Toothbrush Electric Toothbrush
1 Pack $80.00 per each $10.64 per 100g
^ ++
ADVERTISER PROMOTION
QLD-METRO-2409-25-488150 36
[TABLES FOUND]
 | 
[END TABLES]

--- Page 37 ---
88
$$ 55 00
eeaa $$1111 00
SAVE $8.50 eeaa
WAS $17 SAVE 50¢
WAS $1.60
Schmackos Dog Treats Dine Classic Collections Cat Food
450g-500g 85g $1.29 per 100g
$$88
$$1199 00 $$1100
eeaa eeaa eeaa
SAVE 85¢ SAVE $2 SAVE $4.50
WAS $2.75 WAS $10 WAS $14.50
Fussy Cat Cat Food 400g Temptations Cat Treats 180g Felix Cat Food 12x85g
$0.48 per 100g $4.44 per 100g $0.98 per 100g
$$1111 $$1155 $$3366
eeaa eeaa eeaa
SAVE $5 SAVE $5 SAVE $9
WAS $16 WAS $20 WAS $45
Dine Classic Collections Cat Food Purina One Adult Dry Cat Food Optimum Dry Dog Food
Pouches 12x85g $1.08 per 100g 1.4kg-1.5kg 6.2kg-7kg
$$99
$$33228800
DOWN
eeaa
eeaa
DOWN
DOWN DOWN DOWN DOWN
WA S $11 WAS $ 36.50
JUL 2025 AUG 2025
Woofin' Good Oven Baked Greenies Original Dog Treats
Dog Treat Biscuits 800g Regular 12 Pack $2.73 per each
$1.13 per 100g
QLD-METRO-2409-25-488150 37
[TABLES FOUND]
 | 
 | 
 | 
DOWN DOWN
WA S $11
JUL 2025 | 
[END TABLES]

--- Page 38 ---
EVERY DAY
$$66
$$1100 55 $$2288 00
eeaa eeaa eeaa
Coles Ultra Multipurpose Coles Ultra Rinse Aid 500mL Coles Ultra Advanced
Domestic Cleaning Wipes $0.56 per 100mL Dishwasher Tablets 40 Pack
10 Pack $0.11 per each $0.15 per each
$$44
$$77
eeaa
eeaa
DOWN DOWN
WA S $7 SAVE $5
SEP 2025 WAS $12
Dettol Multipurpose Cleaner 750mL Dettol Multipurpose Disinfectant Wipes
$0.53 per 100mL. *See page 19 for T&Cs 110 Pack $0.06 per each. *See page 19 for T&Cs
$$11335500
$$44
$$552255
eeaa
eeaa eeaa
DOWN DOWN
SAVE $2 SAVE $5.25 WA S $15
WAS $6 WAS $10.50 SEP 2025
Handee Ultra Paper Towel Cuddly Concentrate Fabric Quilton 4-Ply Softness Gold Toilet
4 Pack $1.67 per 100 sheets Conditioner 850mL $6.18 per litre Tissue 20 Pack $0.56 per 100 sheets
QLD-METRO-2409-25-488150 38
[TABLES FOUND]
 | aa
WN |  | 
DOWN DO
WA S $7
SEP 2025 | WN |  | 
 | 5
SAVE $5.25
WAS $10.50 | 5
[END TABLES]

--- Page 39 ---
$$22 1100 $$335500 $$339900
eeaa eeaa eeaa
Coles Ultra Pre Wash Coles Ultra Fabric Coles Ultra Oxy Action
Stain Remover 750mL Conditioner 2 Litre Laundry Stain Remover
$2.80 per litre $1.75 per litre Powder 1kg $3.90 per kg
$$44 $$44
$$11005500
eeaa
eeaa eeaa
Coles Ultra Fabric Conditioner Coles Ultra Fabric Coles Ultra Concentrate
Tropical Oasis 1 Litre Conditioner Island Breeze Laundry Powder 2kg
$4.00 per litre 1 Litre $4.00 per litre $5.25 per kg
QLD-METRO-2409-25-488150 39
[TABLES FOUND]
 | 
 | 
 | 
 | 
 | 
 | 
 | 
 | 
[END TABLES]

--- Page 40 ---
1/2 PRICE
BBOONNDDSS AAPPPPAARREELL
Exclusions apply†
$$88
$$$$00116600 00 $$1133
eeaaeeaa eeaa eeaa
SASVAEV $E0 $01.600 SAVE $13 SAVE $8
WWAAS S$ $03.020 WAS $26 WAS $16
Bonds Men's Hipster Briefs Bonds Women's Seamless Bonds Women's Socks 1/4 Crew
5 Pack Full Brief 2 Pack Logo 3 Pack On sale for 2 weeks
†Excludes Babywear, Period and Continence Underwear & clearance items
$$77
$$8855 00 $$11005500
eeaa eeaa eeaa
SAVE $8.50 SAVE $7 SAVE $4.50
WAS $17 WAS $14 WAS $15
Décor Microsafe Easy Heat Brunnings Fast Action Path Joseph Joseph Duo 3 In 1
Containers 900mL 3 Pack Weeder 1 Litre Avocado Tool
$2.83 per each
$$8877 55 $$5555 00 $$555500
eeaa eeaa eeaa
SAVE $3.75 SAVE $4.50
WAS $12.50 WAS $10
Heat Beads BBQ Briquettes 4kg Bic Mega Lighter 1 Each Coles BBQ Trays 4 Pack
$0.22 per 100g $5.50 per each $1.38 per each
$100 and $250
Visa Gift Cards
$100 + $5.95 Activation Fee,
$250 + $7.50 Activation Fee.
Excludes Birthday and Thank you
Visa cards. In-store only.
Limit 5 Gift Cards per Flybuys account. Offer valid from 24/9/25 to 30/9/25 and only available in store at Coles Supermarkets (excludes
Coles Online, Coles Express and purchases via giftcards.com.au), subject to store availability. Excludes $25, $50, and $400 Visa Gift Cards.
Excludes Birthday and Thank you Visa Gift Cards. To qualify for this offer you must present your Flybuys card at the time of purchase. While
stocks last, no rainchecks. BONUS POINTS can be awarded more than once for each Gift Card type including different denominations
of the same Gift Card but can only be collected up to 5 times per Flybuys account. Gift Cards can't be used to purchase other Gift
Cards at Coles. Standard Flybuys terms and conditions apply and are available at Flybuys.com.au. Visa gift cards are issued by Heritage
and People's Choice Limited trading as Heritage Bank ABN 11 087 651 125 AFSL 244310 pursuant to a license from Visa Worldwide Pte
Limited.
QLD-METRO-2409-25-488150 40
[TABLES FOUND]
 | 
SASVAEV $E0 $01.600
WWAAS S$ $03.020 | 
SAVE $8
WAS $16 | 
 | 
SAVE $4.50
WAS $15 | 
 | 
SAVE $7
WAS $14 | 
[END TABLES]

--- Page 41 ---
BETTER
1T /HA 2N
$$1133 PRICE
eeaa $$5599
SAVE $26 eeaa
WAS $39
SAVE $20
WAS $79
Telstra T-Essential 4G
• 5” FWVGA+ display
• Rear camera
(5MP with flash)
B 1TE /HT AT 2E NR B 1TE /HT AT 2E NR
$$1155 PRICE $$1133 PRICE
eeaa eeaa
SAVE $20 SAVE $26
WAS $35 WAS $39
Belong $35 Telstra $39 SIM Kit
Mobile SIM • 45GB on first 3
• Includes SIM card, recharges, 15GB
80GB + Unlimited from 4th
calls & texts • 28 days expiry
in Aus.
ADVERTISER PROMOTION
BETTER BETTER
1T /HA 2N 1T /HA 2N
PRICE PRICE
$$1155
$$2255
eeaa $$7799 $$5599
eeaa
SAVE $25 eeaa eeaa
WAS $40 SAVE $34
WAS $59 SAVE $20 SAVE $20
amaysim $40 WAS $99 WAS $79
Starter Pack Optus $59 Prepaid SIM
• 80GB data, • 45GB + 40GB bonus Optus X Plus Optus X Start 4 4G
28 day expiry data first 3 recharges, • 6.52” 90Hz display • 5” display
• Unlimited data banking 28 day expiry • 8MP AI camera • 5MP camera with flash
General Devices: Excluding iPhones, all devices are Network Locked. Unlocking fees may apply. All included starter pack
offers are included in device packaging. General SIMs: All value SIMs include first recharge applied on activation. All for
personal use in AU only. New services only, not for recharge purposes. All recharges revert to standard plan price. Speed
caps apply. 5G coverage in selected areas where specified. Terms & Fair Go policies apply, see individual packs. Limit 5
per customer. Telstra & Boost SIMs: For bonus data inclusions, activate by 6/10/25, with remaining 2 recharges by 5/12/25.
amaysim $40 SIM: Promotion price ends 30/9/25. Ongoing $40/80GB/28 day expiry. Optus $59 SIM: Unlimited data every
weekend available with AutoRecharge activated only.
QLD-METRO-2409-25-488150 41
[TABLES FOUND]
 | BETTER
1T /HA 2N
$$1133 PRICE
eeaa $$5599
SAVE $26 eeaa
WAS $39
SAVE $20
WAS $79
Telstra T-Essential 4G
• 5” FWVGA+ display
• Rear camera
(5MP with flash)
B 1TE /HT AT 2E NR B 1TE /HT AT 2E NR
$$1155 PRICE $$1133 PRICE
eeaa eeaa
SAVE $20 SAVE $26
WAS $35 WAS $39
Belong $35 Telstra $39 SIM Kit
Mobile SIM • 45GB on first 3
• Includes SIM card, recharges, 15GB
80GB + Unlimited from 4th
calls & texts • 28 days expiry
in Aus.
ADVERTISER PROMOTION
BETTER BETTER
1T /HA 2N 1T /HA 2N
PRICE PRICE
 | SAVE $20
WAS $79
Telstra T-Essen
• 5” FWVGA+ di
• Rear camera
(5MP with flas
BETTER
1T /HA 2N
$$1155 PRICE $$1133
eeaa eeaa
SAVE $20 SAVE $26
WAS $35 WAS $39
Belong $35 Telstra $39 SIM
Mobile SIM • 45GB on first 3
• Includes SIM card, recharges, 15
80GB + Unlimited from 4th
calls & texts • 28 days expir
in Aus.
 | ADVERTISER PROMOTION
BETTER BETTER
1T /HA 2N 1T /HA 2N
PRICE PRICE
[END TABLES]

--- Page 42 ---
$$4499 $$$111222999 $$$111333999
eeaa eaeeaa eee aaa
SAVE $10 SSAAVVEE $ 2$200 SSAAVVEE $ $3300
WAS $59 WWAASS $ 1$41949 WWAASS $$116699
Optus X Lite 4 Optus X Tap 3 Optus X Pro 5G
• 2.4" display • 6.6" HD+ • 4G + 5G
• Rear camera display network
• 4G network • 50MP main access
access camera • 50MP main
• 28GB camera
storage • Google
Wallet via NFC
BETTER
1T /HA 2N
PRICE
$$1144 $$110099 $$221199
eeaa eeaa eeaa
SAVE $25 SAVE $71 SAVE $131
WAS $39 WAS $180 WAS $350
Optus Optus Optus
$39 Prepaid $180 Prepaid $350 Prepaid
SIM Starter Kit SIM Starter Kit SIM Starter Kit
• Get 45GB first 3 recharges only • Get 140GB first 3 recharges only • G et 300GB first 3 recharges only
• Unlimited Data. Every Weekend. • Unlimited std talk & text within • U nlimited std talk & text within
When you stay connected with Australia Australia
AutoRecharge. • Roll over up to 200GB • Roll over up to 200GB
• Unlimited std talk & text in • 186 day expiry • 365 day expiry
Australia
• Roll over up to 200GB
• 28 day expiry
All for use in Australia. Fair Go Policy applies. Activate within 30 days of purchase or by date advertised
on the promotion, whichever is earlier, to receive advertised inclusions. Optus X Lite 4, Optus X Tap 3, Optus
$180 SIM Starter Kit and Optus $350 SIM Starter Kit offers end 30/9/25. Optus X Pro 5G offer ends 7/10/25. $39 SIM
Starter Kit ends 14/10/25. Free Prepaid SIM: New services only. Activate SIM to apply included recharge to an
available plan of same value. See optus.com.au/prepaidplans for full plan details. $39 SIM Starter Kit: Includes
$39 recharge. New services only. Data applies for first 3 recharges within 90 days of activation only, reverts to
25GB after 3rd recharge (or as otherwise notified). Unlimited Data Weekends: from 12.01am Sat to 11.59pm Sun
(local time) when you activate/recharge with AutoRecharge on $39 Optus Flex Plus (excl. expired or manual
recharges). Not accessible on modems. $180 SIM Starter Kit: Includes $180 recharge. New services only. First 3
recharges must be within 3 years of activation. Reverts to 90GB after 3rd recharge (or as otherwise notified). $350
SIM Starter Kit: Includes $350 recharge. New services only. First 3 recharges must be within 3 years of activation.
Reverts to 220GB after 3rd recharge (or as otherwise notified). Speed capped at 250Mbps (actual speeds will
vary and may be slower). Data Rollover: Requires recharge before expiry. 5G: 5G available in selected areas
(excl. NT) with a compatible device and plan. General: Coverage varies, see optus.com.au/coverage For SIM
offers, ongoing recharges revert to standard price. Devices are network locked. For unlocking fees see optus.com.
au/unlock To use SIM, device must be compatible with the Optus Network, see optus.com.au/compatibility
ADVERTISER PROMOTION
QLD-METRO-2409-25-488150 42
[TABLES FOUND]
 | 
 |  | 
SAVE $71
WAS $180 |  | 
 |  | 
[END TABLES]

--- Page 43 ---
    """
    
    # If reading from file:
    # with open('coles_catalog_raw.txt', 'r', encoding='utf-8') as f:
    #     raw_catalog_text = f.read()
    
    # Parse and save
    parser = ColesCatalogParser(raw_catalog_text)
    parser.parse_catalog()
    parser.save_to_json('coles_catalog.json')
    
    # Print summary
    print(f"\nParsing complete!")
    print(f"Total products parsed: {len(parser.products)}")
    
    # Print sample of parsed products
    if parser.products:
        print("\nSample of parsed products:")
        for product in parser.products[:3]:
            print(f"\n{product['productID']}: {product['productName']}")
            print(f"  Price: ${product.get('price', 'N/A')}")
            print(f"  Category: {product.get('category', 'N/A')}")
            print(f"  Special: {product.get('specialType', 'N/A')}")

if __name__ == "__main__":
    main()