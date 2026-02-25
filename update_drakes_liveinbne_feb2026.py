#!/usr/bin/env python3
"""
Update Drakes section in liveinbne_deal.html with data from drakes.md
Valid: 28 January 2026 - 3 February 2026
"""

def parse_drakes_md(md_file):
    """Parse drakes.md and return structured data."""
    with open(md_file, 'r', encoding='utf-8') as f:
        content = f.read()

    deals = []

    # Parse Featured Front Page Deals - Groceries
    deals.append({
        'name': 'Sunrice Medium Grain White Rice 5kg',
        'price': '$5',
        'unit_price': 'ea - SAVE $7.50 ($0.10 per 100g)'
    })
    deals.append({
        'name': 'Selected Kellogg\'s Cereal 460g-740g',
        'price': '$5',
        'unit_price': 'ea - SAVE $5'
    })
    deals.append({
        'name': 'Arnott\'s Tim Tams 165g-200g',
        'price': '$3',
        'unit_price': 'ea - SAVE $3 ($1.50 per 100g)'
    })
    deals.append({
        'name': 'Sorbent Silky White or Hypo-Allergenic Toilet Tissue 12 Pack',
        'price': '$5',
        'unit_price': 'ea - SAVE $1.50 ($0.25 per 100 sheets)'
    })
    deals.append({
        'name': 'Cold Power Laundry Liquid 4lt',
        'price': '$19',
        'unit_price': 'ea - SAVE $5.50 ($4.88 per litre)'
    })

    # Fresh Produce & Meat
    deals.append({
        'name': 'Australian Truss Tomatoes',
        'price': '$5',
        'unit_price': 'kg'
    })
    deals.append({
        'name': 'Australian Beef Lean Mince Min 1.2kg',
        'price': '$15.90',
        'unit_price': 'per kg'
    })

    # Snacks & Drinks
    deals.append({
        'name': 'Cadbury Block Chocolate 150g-190g',
        'price': '$3',
        'unit_price': 'ea - SAVE $1.50'
    })
    deals.append({
        'name': 'Sparkling Water',
        'price': '$2.67',
        'unit_price': 'per litre - SAVE $2.50'
    })
    deals.append({
        'name': 'Golden Circle Fruit Drink 1lt',
        'price': '$1.50',
        'unit_price': 'ea ($1.50 per litre) - SAVE $1.50'
    })
    deals.append({
        'name': 'The Good Stuff Spring Water 20x250ml',
        'price': '$5.50',
        'unit_price': 'ea ($1.10 per litre) - SAVE $2.50'
    })

    # Pantry Staples
    deals.append({
        'name': 'Nestle Milo Pro or Restore 440g',
        'price': '$6',
        'unit_price': 'ea - SAVE $6 ($1.34 per 100g)'
    })
    deals.append({
        'name': 'Wok Wizz Chicken or Beef Instant Noodles 5 Pack 340g',
        'price': '$2.50',
        'unit_price': 'ea ($0.50 per 100g)'
    })
    deals.append({
        'name': 'Primo Cabanossi or Twiggy Cheese & Crackers, Tuscan Salami & Gouda Cheese or Protein Stackers 45g-55g',
        'price': '$3',
        'unit_price': 'ea - SAVE $1'
    })

    # Frozen Foods
    deals.append({
        'name': 'McCain Pub Style Fries or Wedges 750g',
        'price': '$4',
        'unit_price': 'ea - SAVE $1.20 ($6.40 per kg)'
    })
    deals.append({
        'name': 'Rana Fresh Filled Pasta 325g',
        'price': '$6.50',
        'unit_price': 'ea ($2.00 per 100g) - SAVE $2'
    })
    deals.append({
        'name': 'Enzo\'s Meals 330g-360g',
        'price': '$8.90',
        'unit_price': 'ea ($2.69 per 100g) - SAVE $2.75'
    })
    deals.append({
        'name': 'Brubecks Meals 350g-400g',
        'price': '$9.50',
        'unit_price': 'ea - SAVE $3'
    })
    deals.append({
        'name': 'Youfoodz Fueld or High Protein Meals 330g-450g',
        'price': '$9.70',
        'unit_price': 'ea - SAVE $2.05'
    })
    deals.append({
        'name': 'Empire Food Co. Sourdough Pizza 600g',
        'price': '$8',
        'unit_price': 'ea'
    })
    deals.append({
        'name': 'Empire Food Co. Garlic Bread 92g',
        'price': '$2',
        'unit_price': 'ea'
    })
    deals.append({
        'name': 'Delecto\'s Quiche 300g',
        'price': '$7.20',
        'unit_price': 'ea ($2.40 per 100g) - SAVE $1.05'
    })

    # Baby & Personal Care
    deals.append({
        'name': 'Huggies Thick Baby Wipes 216 Pack-240 Pack',
        'price': '$11',
        'unit_price': 'ea - SAVE $1.20'
    })
    deals.append({
        'name': 'Huggies Skin Protect or Ultra Dry Nappies 60 Pack-108 Pack',
        'price': '$25',
        'unit_price': 'ea - SAVE $10'
    })
    deals.append({
        'name': 'Selected Dove Products',
        'price': '$4.50',
        'unit_price': 'ea'
    })
    deals.append({
        'name': 'Nurofen Zavance',
        'price': '$12',
        'unit_price': 'ea'
    })

    # Household Products
    deals.append({
        'name': 'Kleenex Viva Paper Towel',
        'price': '$4',
        'unit_price': 'ea'
    })
    deals.append({
        'name': 'Hercules Aluminium Foil 30cm x 20m, Baking Paper 30cm x 10cm, Cling Wrap 33cm x 70m or Ezy Slide',
        'price': '$4',
        'unit_price': 'ea'
    })
    deals.append({
        'name': 'Hercules Bags Twin Zip Storage Large 15 Pack, Freezer Guard 24 Pack, Sandwich 25 Pack or Click Zip Sandwich 40 Pack',
        'price': '$4',
        'unit_price': 'ea'
    })
    deals.append({
        'name': 'Fluffy Concentrated Fabric Conditioner 900ml-1lt',
        'price': '$4.75',
        'unit_price': 'ea ($4.88 per litre)'
    })
    deals.append({
        'name': 'Whiskas Cat Food 12x85g',
        'price': '$9.50',
        'unit_price': 'ea ($0.74 per 100g)'
    })

    # Deli & Fresh Meals
    deals.append({
        'name': 'Value Super Pops',
        'price': '$3.80',
        'unit_price': 'ea'
    })
    deals.append({
        'name': 'Value Party Mix 1kg',
        'price': '$7.80',
        'unit_price': 'ea ($0.70 per 100g)'
    })
    deals.append({
        'name': 'Value Cheese Slices',
        'price': '$4.80',
        'unit_price': 'ea'
    })
    deals.append({
        'name': 'Value Tuna Chunks in Oil 185g',
        'price': '$1.80',
        'unit_price': 'ea ($0.97 per 100g)'
    })

    # Meat & Seafood
    deals.append({
        'name': 'Australian Grain Fed Beef Rump Steak',
        'price': '$24',
        'unit_price': 'kg - SAVE $2'
    })
    deals.append({
        'name': 'Australian Grain Fed Beef for Mince 500g',
        'price': '$10',
        'unit_price': 'kg - SAVE $10'
    })
    deals.append({
        'name': 'Ingham\'s Chicken or Turkey',
        'price': '$10',
        'unit_price': 'kg'
    })
    deals.append({
        'name': 'Australian Chicken Thigh Fillets',
        'price': '$11.90',
        'unit_price': 'per kg - SAVE 50¢'
    })
    deals.append({
        'name': 'Australian Cooked Pork Belly Roast',
        'price': '$28',
        'unit_price': 'kg ($2.80 per 100g)'
    })
    deals.append({
        'name': 'Sun Valley Roast Beef',
        'price': '$28',
        'unit_price': 'kg'
    })
    deals.append({
        'name': 'Bacon Short Cut',
        'price': '$10',
        'unit_price': 'kg'
    })
    deals.append({
        'name': 'Bertocchi Mortadella Plain, Pepper, Olive or Chilli',
        'price': '$15',
        'unit_price': 'kg'
    })
    deals.append({
        'name': 'Maggie Beer Camembert or Brie 180g',
        'price': '$7',
        'unit_price': 'ea ($52.22 per kg)'
    })
    deals.append({
        'name': 'Fresh Jumbo Cooked Ocean King Prawns',
        'price': '$34',
        'unit_price': 'kg'
    })
    deals.append({
        'name': 'Extra Large Pacific Oysters Half Dozen',
        'price': '$11.50',
        'unit_price': 'half dozen ($2.00 each) - SAVE $1.50'
    })
    deals.append({
        'name': 'Hoki Fillets',
        'price': '$16.70',
        'unit_price': 'kg - SAVE $3.10'
    })
    deals.append({
        'name': 'Peri Peri Extra Large Hot BBQ Chicken',
        'price': '$17',
        'unit_price': 'ea - SAVE $2.60'
    })
    deals.append({
        'name': 'Big Crunch Chicken Sandwich 110g',
        'price': '$4.50',
        'unit_price': 'ea ($4.09 per 100g) - SAVE $1.50'
    })
    deals.append({
        'name': 'Onigiri Sushi',
        'price': '$6.50',
        'unit_price': 'ea'
    })

    return deals

def generate_html_deals(deals):
    """Generate HTML for deals."""
    html_parts = []

    for deal in deals:
        unit_price = deal.get('unit_price', '')
        unit_price_html = f'\n                        <div class="unit-price">{unit_price}</div>' if unit_price else ''

        html = f'''                    <div class="deal-card">
                        <div class="store-badge drakes">DRAKES - </div>
                        <div class="product-name">{deal['name']}</div>
                        <div class="price-info">
                            <span class="current-price">{deal['price']}</span>
                        </div>{unit_price_html}
                    </div>'''
        html_parts.append(html)

    return '\n'.join(html_parts)

def update_liveinbne_html():
    """Update the Drakes section in liveinbne_deal.html"""
    html_file = r'C:\Users\advgen10\source\repos\AdvGenPriceComparer\liveinbne_deal.html'
    md_file = r'C:\Users\advgen10\source\repos\AdvGenPriceComparer\drakes.md'

    # Read the HTML file
    with open(html_file, 'r', encoding='utf-8') as f:
        html_content = f.read()

    # Parse drakes.md
    deals = parse_drakes_md(md_file)

    # Generate new deals HTML
    deals_html = generate_html_deals(deals)

    # Create the new Drakes section
    new_drakes_section = f'''        <!-- Drakes Supermarket Specials -->
        <div class="category-section">
            <div class="category-header" onclick="toggleCategory('drakes-special')">
                <div class="category-title">
                    <span class="category-icon">🛒</span>
                    Drakes Supermarket - 28 Jan - 3 Feb 2026</div>
                <span class="expand-icon" id="drakes-special-icon">+</span>
            </div>
            <div class="category-content" id="drakes-special-content">
                <div style="background: linear-gradient(135deg, #009900, #00cc00); color: white; padding: 1.5rem; border-radius: 10px; margin: 1.5rem 2rem; text-align: center;">
                    <h3 style="margin: 0 0 0.5rem 0; font-size: 1.3rem;">🎁 Collect Free Socks!</h3>
                    <p style="margin: 0; font-size: 0.95rem;">Collect one pair of socks with every $60 you spend! While stocks last.</p>
                </div>
                <div style="background: linear-gradient(135deg, #ff6b6b, #ee5a6f); color: white; padding: 1.5rem; border-radius: 10px; margin: 1.5rem 2rem; text-align: center;">
                    <h3 style="margin: 0 0 0.5rem 0; font-size: 1.3rem;">🏆 Win a Family Holiday Valued at $10,000!</h3>
                    <p style="margin: 0; font-size: 0.95rem;">Spend $40 and purchase any participating product to enter. Competition running from 7/1/2026 - 17/2/2026</p>
                </div>
                <div class="deals-grid">
{deals_html}
            </div>
        </div>'''

    # Find and replace the Drakes section
    start_marker = '        <!-- Drakes Supermarket Specials -->'
    end_marker = '        <!-- ALDI Special Buys -->'

    start_idx = html_content.find(start_marker)
    end_idx = html_content.find(end_marker)

    if start_idx == -1 or end_idx == -1:
        print("Error: Could not find Drakes section markers")
        return

    # Replace the content
    new_html = html_content[:start_idx] + new_drakes_section + '\n\n' + html_content[end_idx:]

    # Write the updated HTML
    with open(html_file, 'w', encoding='utf-8') as f:
        f.write(new_html)

    print(f"Successfully updated Drakes section in liveinbne_deal.html")
    print(f"Valid dates: 28 January 2026 - 3 February 2026")
    print(f"Total deals: {len(deals)}")

if __name__ == '__main__':
    update_liveinbne_html()
