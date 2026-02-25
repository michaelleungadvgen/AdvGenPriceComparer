#!/usr/bin/env python3
"""
Categorize products in Woolworths and Coles JSON files based on product names.
Updates the 'category' field with matched categories.
"""

import json
import re

# Define category mapping rules based on product name patterns
CATEGORY_RULES = {
    'Snacks & Chips': [
        r'\bchips?\b', r'\bcrisps?\b', r'\bpringle', r'\bdoritos?\b', r'\bsmith\'?s\b',
        r'\bkettle\b', r'\btwisties\b', r'\bcheetos\b', r'\bpopcorn\b', r'\bcrackers?\b',
        r'\bshapes?\b', r'\bturtles? chips?\b', r'\bsnack', r'\bnuts?\b', r'\bcashews?\b',
        r'\bbar mix\b', r'\bthins\b'
    ],
    'Confectionery & Chocolate': [
        r'\bchocolate\b', r'\bcadbury\b', r'\bkit ?kat\b', r'\bmars\b', r'\bsnickers\b',
        r'\btim ?tam\b', r'\blollies\b', r'\bgummies\b', r'\btrolli\b', r'\bcandies\b',
        r'\bsweets?\b', r'\bmaltesers\b', r'\bferrero\b', r'\blindt\b', r'\btoblerone\b',
        r'\bdarrell lea\b', r'\bviolet crumble\b'
    ],
    'Beverages - Soft Drinks': [
        r'\bcoca ?cola\b', r'\bcoke\b', r'\bpepsi\b', r'\bfanta\b', r'\bsprite\b',
        r'\bsoft ?drink', r'\bsoda\b', r'\blemonade\b', r'\bsolo\b', r'\bmountain ?dew\b',
        r'\bschweppes\b', r'\bsunkist\b', r'\bpowerade\b', r'\benergy ?drink\b',
        r'\bmonster\b', r'\bred ?bull\b', r'\bsparkling ?water\b', r'\bgenki forest\b'
    ],
    'Beverages - Coffee & Tea': [
        r'\bcoffee\b', r'\bnescaf[eé]\b', r'\bmoccona\b', r'\btea\b', r'\bgrinders\b',
        r'\bcappuccino\b', r'\bespresso\b', r'\blatte\b', r'\bmocha\b'
    ],
    'Beverages - Juice & Cordial': [
        r'\bjuice\b', r'\bcordial\b', r'\bcottees?\b', r'\bgolden circle\b',
        r'\bfruit ?drink\b', r'\borange ?juice\b', r'\bapple ?juice\b'
    ],
    'Dairy & Eggs': [
        r'\bmilk\b', r'\byoghurt\b', r'\byogurt\b', r'\bcheese\b', r'\bbutter\b',
        r'\bcream\b', r'\beggs?\b', r'\bbrie\b', r'\bcamembert\b', r'\bmozzarella\b',
        r'\bcheddar\b', r'\bparmesan\b', r'\bfeta\b'
    ],
    'Bakery & Bread': [
        r'\bbread\b', r'\bloaf\b', r'\bbuns?\b', r'\brolls?\b', r'\bbagels?\b',
        r'\bmuffins?\b', r'\bcroissants?\b', r'\bpizza ?base\b', r'\bsourdough\b',
        r'\benglish ?muffin\b', r'\bhot ?cross ?bun\b', r'\bbrioche\b'
    ],
    'Fresh Produce - Fruit': [
        r'\bapples?\b', r'\bbananas?\b', r'\boranges?\b', r'\bgrapes?\b',
        r'\bstrawberr', r'\bblueberr', r'\bwatermelon\b', r'\bmango', r'\bpineapple\b',
        r'\bkiwi', r'\bpear', r'\bpeach', r'\bnectarine', r'\bpapaya\b', r'\bavocado\b'
    ],
    'Fresh Produce - Vegetables': [
        r'\btomato', r'\bpotato', r'\bcarrot', r'\blettuce\b', r'\bcucumber\b',
        r'\bonion', r'\bbroccoli\b', r'\bcauliflower\b', r'\bcapsicum\b', r'\bcelery\b',
        r'\bspinach\b', r'\bcabbage\b', r'\bpumpkin\b', r'\bsweet ?potato\b', r'\bqukes\b'
    ],
    'Meat - Chicken': [
        r'\bchicken\b', r'\bpoultry\b', r'\bturkey\b', r'\bsteggles\b', r'\bingham',
        r'\bchicken ?breast\b', r'\bchicken ?thigh\b', r'\bchicken ?drumstick\b',
        r'\bchicken ?wing\b', r'\bbbq ?chicken\b'
    ],
    'Meat - Beef': [
        r'\bbeef\b', r'\bsteak\b', r'\brump\b', r'\bporterhouse\b', r'\bmince\b',
        r'\bbrisket\b', r'\bsirloin\b', r'\brib ?eye\b', r'\bt-?bone\b', r'\bsilverside\b'
    ],
    'Meat - Pork': [
        r'\bpork\b', r'\bbacon\b', r'\bham\b', r'\bprosciutto\b', r'\bpork ?belly\b',
        r'\bpork ?chop\b', r'\bpork ?loin\b', r'\bpork ?sausage\b'
    ],
    'Meat - Lamb': [
        r'\blamb\b', r'\bmutton\b', r'\blamb ?chop\b', r'\blamb ?loin\b',
        r'\blamb ?shoulder\b', r'\blamb ?shank\b', r'\blamb ?rack\b'
    ],
    'Seafood': [
        r'\bseafood\b', r'\bfish\b', r'\bprawn', r'\bshrimp\b', r'\bsalmon\b',
        r'\btuna\b', r'\bbarramundi\b', r'\bhoki\b', r'\bsnapper\b', r'\bwhiting\b',
        r'\boyster', r'\bmussel', r'\bcrab\b', r'\blobster\b', r'\bcalamari\b'
    ],
    'Frozen Foods': [
        r'\bfrozen\b', r'\bice ?cream\b', r'\bmaxibon\b', r'\bpizza\b',
        r'\bfries?\b', r'\bwedges?\b', r'\bnuggets?\b', r'\bfinger', r'\bpie\b',
        r'\bpastry\b', r'\bgarlic ?bread\b'
    ],
    'Pantry - Rice & Pasta': [
        r'\brice\b', r'\bpasta\b', r'\bnoodles?\b', r'\bspaghetti\b', r'\bmacaroni\b',
        r'\bpenne\b', r'\bravioli\b', r'\btortellini\b', r'\bcouscous\b', r'\bquinoa\b',
        r'\bsunrice\b', r'\bramen\b', r'\budon\b', r'\binstant ?noodle\b'
    ],
    'Pantry - Cooking Oils': [
        r'\boil\b', r'\bcanola\b', r'\bolive ?oil\b', r'\bvegetable ?oil\b',
        r'\bsunflower ?oil\b', r'\bcoconut ?oil\b'
    ],
    'Pantry - Sauces & Condiments': [
        r'\bsauce\b', r'\bketchup\b', r'\bmustard\b', r'\bmayonnaise\b', r'\bmayo\b',
        r'\brelish\b', r'\bchutney\b', r'\bpesto\b', r'\baioli\b', r'\btartare\b',
        r'\bsoy ?sauce\b', r'\bsalsa\b', r'\bdolmio\b', r'\bold el paso\b', r'\btaco\b',
        r'\bdumpling ?sauce\b', r'\bplum ?sauce\b', r'\bchilli ?crisp\b'
    ],
    'Pantry - Canned Foods': [
        r'\bcanned\b', r'\btinned\b', r'\bbaked ?bean\b', r'\bspaghetti\b',
        r'\btomato ?paste\b', r'\bcorn\b', r'\bbeetroot\b'
    ],
    'Asian Foods': [
        r'\basian\b', r'\bjapanese\b', r'\bchinese\b', r'\bkorean\b', r'\bthai\b',
        r'\bnongshim\b', r'\bkewpie\b', r'\bsamyang\b', r'\bseaweed\b', r'\bmiso\b',
        r'\bsushi\b', r'\bteriyaki\b', r'\bmongolian\b', r'\bsukiyaki\b', r'\bkung ?pao\b',
        r'\bsichuan\b', r'\bmirin\b', r'\bponzu\b', r'\bjasmine ?rice\b', r'\bonigiri\b'
    ],
    'Baby Products': [
        r'\bbaby\b', r'\bnappies\b', r'\bdiapers?\b', r'\bwipes?\b', r'\bhuggies\b',
        r'\binfant\b', r'\bformula\b'
    ],
    'Personal Care & Hygiene': [
        r'\btoothpaste\b', r'\bshampoo\b', r'\bconditioner\b', r'\bsoap\b',
        r'\bdeodorant\b', r'\bshower ?gel\b', r'\bbody ?wash\b', r'\bcolgate\b',
        r'\bdove\b', r'\bnurofen\b', r'\bpanadol\b'
    ],
    'Household - Cleaning': [
        r'\bdetergent\b', r'\bbleach\b', r'\bdishwash', r'\blaundry\b',
        r'\bfinish\b', r'\bcold ?power\b', r'\bfluffy\b', r'\bfabric ?conditioner\b'
    ],
    'Household - Paper Products': [
        r'\btoilet ?tissue\b', r'\btoilet ?paper\b', r'\bpaper ?towel\b',
        r'\btissue\b', r'\bkleenex\b', r'\bsorbent\b', r'\bnapkin'
    ],
    'Household - Kitchen Supplies': [
        r'\baluminium ?foil\b', r'\bbaking ?paper\b', r'\bcling ?wrap\b',
        r'\bplastic ?bag\b', r'\bfreezer ?bag\b', r'\bstorage ?bag\b', r'\bhercules\b'
    ],
    'Pet Food': [
        r'\bpet\b', r'\bdog\b', r'\bcat\b', r'\bwhiskas\b', r'\bpedigree\b',
        r'\bschmackos\b', r'\bfelix\b', r'\bcat ?food\b', r'\bdog ?food\b',
        r'\bcat ?litter\b', r'\bmarrobones\b', r'\bbeef ?liver ?treat\b'
    ],
    'Office & Stationery': [
        r'\bbatteries\b', r'\benergizer\b', r'\bduracell\b', r'\bstationery\b'
    ]
}

def categorize_product(product_name):
    """Categorize a product based on its name."""
    product_name_lower = product_name.lower()

    # Check each category's patterns
    for category, patterns in CATEGORY_RULES.items():
        for pattern in patterns:
            if re.search(pattern, product_name_lower):
                return category

    # Default category if no match
    return 'General'

def update_product_categories(json_file):
    """Update categories in a JSON file."""
    # Read the JSON file
    with open(json_file, 'r', encoding='utf-8') as f:
        products = json.load(f)

    # Update categories
    updated_count = 0
    category_stats = {}

    for product in products:
        old_category = product.get('category', 'General')
        new_category = categorize_product(product['productName'])

        if new_category != old_category:
            product['category'] = new_category
            updated_count += 1

        # Track statistics
        category_stats[new_category] = category_stats.get(new_category, 0) + 1

    # Write back to file
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(products, f, indent=2, ensure_ascii=False)

    return updated_count, category_stats, len(products)

def main():
    """Main function to update categories in both JSON files."""
    files = [
        'data/woolworths_28012026.json',
        'data/coles_28012026.json'
    ]

    for json_file in files:
        try:
            print(f"\nProcessing {json_file}...")
            updated_count, category_stats, total_products = update_product_categories(json_file)

            print(f"[OK] Updated {updated_count} out of {total_products} products")
            print(f"\nCategory Distribution:")
            print("-" * 50)

            # Sort categories by count
            sorted_categories = sorted(category_stats.items(), key=lambda x: x[1], reverse=True)
            for category, count in sorted_categories:
                percentage = (count / total_products) * 100
                print(f"  {category:.<40} {count:>4} ({percentage:>5.1f}%)")

        except FileNotFoundError:
            print(f"[ERROR] File not found - {json_file}")
        except json.JSONDecodeError:
            print(f"[ERROR] Invalid JSON format in {json_file}")
        except Exception as e:
            print(f"[ERROR] Error processing {json_file}: {e}")

    print("\n" + "="*60)
    print("Category update completed!")
    print("="*60)

if __name__ == '__main__':
    main()
