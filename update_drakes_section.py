"""
Update Drakes section in liveinbne_deal.html with data from drakes.md
"""

import re

def load_file(filepath):
    """Load file content"""
    with open(filepath, 'r', encoding='utf-8') as f:
        return f.read()

def save_file(filepath, content):
    """Save file content"""
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

def parse_drakes_markdown(md_content):
    """Parse Drakes markdown and extract products"""
    products = []

    # Split into sections by ##
    sections = re.split(r'^##\s+', md_content, flags=re.MULTILINE)

    current_category = ""

    for section in sections:
        if not section.strip():
            continue

        lines = section.split('\n')
        category_line = lines[0].strip()

        # Skip metadata sections
        if 'Below 1/2 Price' in category_line or 'Weekly Specials' in category_line:
            current_category = "WEEKLY SPECIALS"
        elif 'Deli' in category_line:
            current_category = "DELI"
        elif 'Meat' in category_line or 'Poultry' in category_line:
            current_category = "MEAT"
        elif 'Produce' in category_line:
            current_category = "PRODUCE"
        elif 'Bakery' in category_line or 'Easy Meals' in category_line:
            current_category = "BAKERY"
        elif 'Dairy' in category_line:
            current_category = "DAIRY"
        elif 'Freezer' in category_line or 'Frozen' in category_line:
            current_category = "FROZEN"
        elif 'Refreshments' in category_line or 'Confectionery' in category_line:
            current_category = "DRINKS & SNACKS"
        elif 'Snacking' in category_line:
            current_category = "SNACKS"
        elif 'Pantry' in category_line:
            current_category = "PANTRY"
        elif 'Household' in category_line:
            current_category = "HOUSEHOLD"
        elif 'Health' in category_line or 'Beauty' in category_line:
            current_category = "HEALTH & BEAUTY"
        elif 'Pet Food' in category_line:
            current_category = "PET FOOD"
        else:
            continue

        # Parse products in this section
        for line in lines[1:]:
            # Match pattern: - **Product Name** - **$Price** ...
            product_match = re.match(r'-\s+\*\*(.+?)\*\*.*?-\s+\*\*\$([0-9.]+)\*\*', line)

            if product_match:
                product_name = product_match.group(1).strip()
                price = product_match.group(2).strip()

                # Extract savings if present
                savings_match = re.search(r'Save \$([0-9.]+)', line)
                savings = savings_match.group(1) if savings_match else None

                # Extract unit price if present
                unit_match = re.search(r'\(([^)]*per[^)]*)\)', line)
                unit_price = unit_match.group(1) if unit_match else ""

                # Extract size/details
                size_match = re.search(r'\*\*\s+([0-9]+[a-zA-Z]+(?:-[0-9]+[a-zA-Z]+)?)', line)
                size = size_match.group(1).strip() if size_match else ""

                products.append({
                    'name': product_name,
                    'price': price,
                    'savings': savings,
                    'unit_price': unit_price,
                    'size': size,
                    'category': current_category
                })

    return products

def generate_drakes_deal_cards(products):
    """Generate HTML deal cards for Drakes products"""
    cards = []

    # Group by category and limit
    category_counts = {}

    for product in products:
        category = product['category']

        # Limit products per category
        if category_counts.get(category, 0) >= 5:
            continue

        category_counts[category] = category_counts.get(category, 0) + 1

        # Determine deal badge
        deal_class = "better-value"
        deal_text = "SPECIAL"

        if product['savings']:
            try:
                savings_val = float(product['savings'])
                if savings_val >= 10:
                    deal_class = "best-deal"
                    deal_text = "HUGE SAVINGS"
                elif savings_val >= 5:
                    deal_class = "best-deal"
                    deal_text = "GREAT SAVINGS"
                else:
                    deal_text = "GOOD VALUE"
            except:
                deal_text = "GOOD VALUE"

        card_html = f'''                    <div class="deal-card">
                        <div class="store-badge drakes">DRAKES - {product['category']}</div>
                        <div class="product-name">{product['name']}</div>
                        <div class="price-info">
                            <span class="current-price">${product['price']}</span>'''

        if product['savings']:
            card_html += f'''
                            <span class="savings">Save ${product['savings']}</span>'''

        card_html += '''
                        </div>'''

        if product['unit_price']:
            card_html += f'''
                        <div class="unit-price">{product['unit_price']}</div>'''

        card_html += f'''
                        <div class="deal-badge {deal_class}">{deal_text}</div>
                    </div>'''

        cards.append(card_html)

        # Stop at 30 products total
        if len(cards) >= 30:
            break

    return '\n'.join(cards)

def update_drakes_section(html_content, drakes_cards):
    """Update Drakes section in HTML"""

    # Find the Drakes section
    drakes_start = html_content.find('<!-- Drakes Supermarket Specials -->')
    if drakes_start == -1:
        drakes_start = html_content.find('Drakes Supermarket -')

    if drakes_start == -1:
        print("Could not find Drakes section")
        return html_content

    # Update the title to reflect current date
    title_pattern = r'Drakes Supermarket - \d+-\d+ September 2025'
    html_content = re.sub(
        title_pattern,
        'Drakes Supermarket - 1-7 October 2025',
        html_content
    )

    # Find the deals-grid section for Drakes
    drakes_grid_start = html_content.find('<div class="deals-grid">', drakes_start)
    if drakes_grid_start == -1:
        print("Could not find Drakes deals-grid")
        return html_content

    # Find the end of Drakes section (before ALDI section)
    aldi_section = html_content.find('<!-- ALDI Special Buys -->', drakes_grid_start)
    if aldi_section == -1:
        print("Could not find end of Drakes section")
        return html_content

    # Find the closing tags before ALDI
    drakes_section_end = html_content.rfind('</div>\n            </div>\n        </div>', drakes_grid_start, aldi_section)

    if drakes_section_end == -1:
        print("Could not find Drakes section end tags")
        return html_content

    # Replace the Drakes cards
    before = html_content[:drakes_grid_start + len('<div class="deals-grid">')]
    after = html_content[drakes_section_end:]

    new_content = before + '\n' + drakes_cards + '\n                ' + after

    return new_content

# Main execution
if __name__ == "__main__":
    drakes_md_file = "drakes.md"
    html_file = "liveinbne_deal.html"

    print(f"Loading Drakes data from {drakes_md_file}...")
    drakes_md = load_file(drakes_md_file)

    print("Parsing Drakes products...")
    products = parse_drakes_markdown(drakes_md)
    print(f"Found {len(products)} Drakes products")

    if not products:
        print("No products found! Exiting...")
        exit(1)

    print("Generating Drakes deal cards...")
    drakes_cards = generate_drakes_deal_cards(products)

    print(f"Loading HTML from {html_file}...")
    html_content = load_file(html_file)

    print("Updating Drakes section...")
    updated_html = update_drakes_section(html_content, drakes_cards)

    print(f"Saving updated HTML to {html_file}...")
    save_file(html_file, updated_html)

    print("Done!")
    print(f"Updated Drakes section with up to 30 products")
