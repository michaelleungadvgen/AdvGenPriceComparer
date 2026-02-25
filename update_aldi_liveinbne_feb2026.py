#!/usr/bin/env python3
"""
Update ALDI section in liveinbne_deal.html with data from aldi.md
Available from: Sat 31st Jan, Wed 4th Feb & Sat 7th Feb 2026
"""

def generate_aldi_deals():
    """Generate ALDI deals from aldi.md data."""
    deals = []

    # Saturday 31st January deals
    deals.extend([
        {
            'category': 'SNACKS & CONFECTIONERY',
            'name': 'Maltesers Giftbox 400g - MALTESERS',
            'price': '$7.75',
            'unit_price': '$1.94 per 100g',
            'date': 'Sat 31 Jan'
        },
        {
            'category': 'SNACKS & CONFECTIONERY',
            'name': 'Cadbury Roses Giftbox 380g - CADBURY',
            'price': '$9.99',
            'unit_price': '$2.63 per 100g',
            'date': 'Sat 31 Jan'
        },
        {
            'category': 'SNACKS & CONFECTIONERY',
            'name': 'Doritos Cheese Supreme 170g - DORITOS',
            'price': '$2.49',
            'unit_price': '$1.46 per 100g',
            'date': 'Sat 31 Jan'
        },
        {
            'category': 'SNACKS & CONFECTIONERY',
            'name': 'Big Bag Original Mix 350g - MAJANS',
            'price': '$5.49',
            'unit_price': '$1.57 per 100g',
            'date': 'Sat 31 Jan'
        },
        {
            'category': 'SNACKS & CONFECTIONERY',
            'name': 'McVitie\'s Go Ahead Forest Fruit 174g - MCVITIE\'S',
            'price': '$2.49',
            'unit_price': '$1.43 per 100g',
            'date': 'Sat 31 Jan'
        },
        {
            'category': 'SNACKS & CONFECTIONERY',
            'name': 'McVitie\'s Go Ahead Apple 174g - MCVITIE\'S',
            'price': '$2.49',
            'unit_price': '$1.43 per 100g',
            'date': 'Sat 31 Jan'
        },
        {
            'category': 'SNACKS & CONFECTIONERY',
            'name': 'McVitie\'s Hobnobs Oat Snap Cookie 255g - MCVITIE\'S',
            'price': '$1.99',
            'unit_price': '$0.78 per 100g',
            'date': 'Sat 31 Jan'
        },
        {
            'category': 'SNACKS & CONFECTIONERY',
            'name': 'Violet Crumble 150g - MENZ',
            'price': '$2.99',
            'unit_price': '$1.99 per 100g',
            'date': 'Sat 31 Jan'
        },
        {
            'category': 'INSTANT MEALS',
            'name': 'Samyang Buldak Hot Chicken Carbonara Ramen Noodles Bowl 105g - SAMYANG',
            'price': '$2.89',
            'unit_price': '',
            'date': 'Sat 31 Jan'
        },
    ])

    # Wednesday 4th February deals
    deals.extend([
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Mayonnaise 300g - KEWPIE',
            'price': '$4.28',
            'unit_price': '$1.43 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Dumpling Sauce 190ml - UMBRO',
            'price': '$2.98',
            'unit_price': '$1.57 per 100ml',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Noodle Bowls 240g Mongolian - URBAN EATS',
            'price': '$2.68',
            'unit_price': '$1.12 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Noodle Bowls 240g Teriyaki - URBAN EATS',
            'price': '$2.68',
            'unit_price': '$1.12 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Korean BBQ Noodle Bowl 240g - URBAN EATS',
            'price': '$2.68',
            'unit_price': '$1.12 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Japanese Sukiyaki Noodle Bowl 240g - URBAN EATS',
            'price': '$2.68',
            'unit_price': '$1.12 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Sweet & Sour Noodle Bowl 240g - URBAN EATS',
            'price': '$2.68',
            'unit_price': '$1.12 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Kung Pao Noodle Bowl 240g - URBAN EATS',
            'price': '$2.68',
            'unit_price': '$1.12 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Thai Hom Mali Jasmine Rice 5kg - HEAVEN\'S HARVEST',
            'price': '$12.98',
            'unit_price': '$0.26 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'ASIAN & INTERNATIONAL FOODS',
            'name': 'Sichuan Chilli Crisp 190g - BRANDED',
            'price': '$3.48',
            'unit_price': '$1.83 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'LIQUOR',
            'name': 'Sherry Cask Whisky 700ml - GLEN MARNOCH',
            'price': '$55.99',
            'unit_price': '',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'LIQUOR',
            'name': 'Gold Flaked Vodka 700ml - INVENTUM DISTILLING',
            'price': '$49.99',
            'unit_price': '',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'LIQUOR',
            'name': 'TropicALDI 6 Pack 375ml - HAWKE\'S',
            'price': '$13.99',
            'unit_price': '',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'PET',
            'name': 'Marrobones 737g - SCHMACKOS',
            'price': '$9.99',
            'unit_price': '$1.36 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'PET',
            'name': 'Recycled Paper Cat Litter 20L - BREEDERCELECT',
            'price': '$17.99',
            'unit_price': '$0.90 per L',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'PET',
            'name': 'Cat Food 48 Pack 85g - FELIX',
            'price': '$32.99',
            'unit_price': '$0.81 per 100g',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'LUNAR NEW YEAR',
            'name': 'Bamboo Steamer Basket - CROFTON',
            'price': '$14.88',
            'unit_price': '',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'LUNAR NEW YEAR',
            'name': 'Carbon Steel Wok Pan 35cm - CROFTON',
            'price': '$24.88',
            'unit_price': '',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'LUNAR NEW YEAR',
            'name': 'Digital Rice Cooker Assortment - AMBIANO',
            'price': '$58.88',
            'unit_price': '',
            'date': 'Wed 04 Feb'
        },
        {
            'category': 'HONEY',
            'name': 'Manuka Liquid Honey MGO88+ 500g - AIRBORNE',
            'price': '$13.88',
            'unit_price': '$2.78 per 100g',
            'date': 'Wed 04 Feb'
        },
    ])

    # Saturday 7th February deals
    deals.extend([
        {
            'category': 'AUTOMOTIVE',
            'name': 'IGO CAM445 Dash Cam - UNIDEN',
            'price': '$89.99',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
        {
            'category': 'AUTOMOTIVE',
            'name': 'Multifunctional Car Vacuum Cleaner - AMBIANO',
            'price': '$49.99',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
        {
            'category': 'CARAVAN & CAMPING',
            'name': '24" HD Smart TV with Built in Battery and 12V DC Charger - BAUHN',
            'price': '$189.00',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
        {
            'category': 'CARAVAN & CAMPING',
            'name': 'Flexible Solar Panel 200W - BRANDED',
            'price': '$159.00',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
        {
            'category': 'CARAVAN & CAMPING',
            'name': 'Caravan Cover, 14-16ft - ADVENTURIDGE',
            'price': '$99.99',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
        {
            'category': 'CARAVAN & CAMPING',
            'name': 'Caravan Cover, 18-20ft - ADVENTURIDGE',
            'price': '$119.00',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
        {
            'category': 'CARAVAN & CAMPING',
            'name': 'Assorted High Back Reclining Chair Mix - ADVENTURIDGE',
            'price': '$69.99',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
        {
            'category': 'OUTDOOR CLOTHING',
            'name': 'Adult\'s Waterproof Jacket - CRANE PERFORMANCE',
            'price': '$34.99',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
        {
            'category': 'OUTDOOR CLOTHING',
            'name': 'Adult Hiking Shoes - CRANE',
            'price': '$34.99',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
        {
            'category': 'OUTDOOR CLOTHING',
            'name': 'Adult\'s Convertible Hiking Pants - CRANE',
            'price': '$24.99',
            'unit_price': '',
            'date': 'Sat 07 Feb'
        },
    ])

    return deals

def generate_html_deals(deals):
    """Generate HTML for ALDI deals."""
    html_parts = []

    for deal in deals:
        unit_price_html = f'\n                        <div class="unit-price">{deal["unit_price"]}</div>' if deal['unit_price'] else ''

        html = f'''                    <div class="deal-card">
                        <div class="store-badge aldi">ALDI - {deal['category']}</div>
                        <div class="product-name">{deal['name']}</div>
                        <div class="price-info">
                            <span class="current-price">{deal['price']}</span>
                        </div>{unit_price_html}
                        <div class="deal-badge best-deal">SPECIAL BUY</div>
                        <div class="availability">On Sale {deal['date']}</div>
                    </div>'''
        html_parts.append(html)

    return '\n'.join(html_parts)

def update_liveinbne_html():
    """Update the ALDI section in liveinbne_deal.html"""
    html_file = r'C:\Users\advgen10\source\repos\AdvGenPriceComparer\liveinbne_deal.html'

    # Read the HTML file
    with open(html_file, 'r', encoding='utf-8') as f:
        html_content = f.read()

    # Generate ALDI deals
    deals = generate_aldi_deals()

    # Generate new deals HTML
    deals_html = generate_html_deals(deals)

    # Create the new ALDI section
    new_aldi_section = f'''        <!-- ALDI Special Buys -->
        <div class="category-section">
            <div class="category-header" onclick="toggleCategory('aldi')">
                <div class="category-title">
                    <span class="category-icon">🏪</span>
                    ALDI Special Buys - Sat 31 Jan, Wed 4 Feb & Sat 7 Feb 2026</div>
                <span class="expand-icon" id="aldi-icon">+</span>
            </div>
            <div class="category-content" id="aldi-content">
                <div class="deals-grid">
{deals_html}
            </div>
        </div>'''

    # Find and replace the ALDI section
    start_marker = '        <!-- ALDI Special Buys -->'
    end_marker = '\n\n    </main>'

    start_idx = html_content.find(start_marker)
    end_idx = html_content.find(end_marker)

    if start_idx == -1 or end_idx == -1:
        print("Error: Could not find ALDI section markers")
        return

    # Replace the content
    new_html = html_content[:start_idx] + new_aldi_section + html_content[end_idx:]

    # Write the updated HTML
    with open(html_file, 'w', encoding='utf-8') as f:
        f.write(new_html)

    print(f"Successfully updated ALDI section in liveinbne_deal.html")
    print(f"Valid dates: Sat 31 Jan, Wed 4 Feb & Sat 7 Feb 2026")
    print(f"Total deals: {len(deals)}")

if __name__ == '__main__':
    update_liveinbne_html()
