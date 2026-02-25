import re

def parse_drakes_md(filepath):
    """Parse drakes.md and extract products by category"""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Extract date range - format: **Valid:** Wednesday, 22 October 2025 – Tuesday, 28 October 2025
    date_match = re.search(r'\*\*Valid:\*\*\s+\w+,\s+(\d{1,2}\s+\w+\s+\d{4})\s+[–-]\s+\w+,\s+(\d{1,2}\s+\w+\s+\d{4})', content)
    date_range = f"{date_match.group(1)} to {date_match.group(2)}" if date_match else "This Week"

    products = []

    # Pattern to match product lines: - **Product Name** – $price [unit info]
    # Using – (en dash) not - (hyphen)
    pattern = r'-\s+\*\*([^*]+)\*\*\s+[–-]\s+\$(\d+(?:\.\d{2})?)\s*(?:each|ea)?\s*(?:\(([^)]+)\))?'

    matches = re.findall(pattern, content)

    for match in matches:
        product_name = match[0].strip()
        price = match[1].strip()
        unit_info = match[2].strip() if match[2] else ''

        if price:
            products.append({
                'name': product_name,
                'size': '',
                'price': price,
                'savings': '',
                'unit_info': unit_info
            })

    return products, date_range

def parse_aldi_md(filepath):
    """Parse aldi.md and extract products from the most recent week"""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    products = []

    # Find all "Available from" sections
    all_sections = re.findall(r'## Available from ([^(]+)\((\d+) Products\)', content)

    if all_sections:
        # Get the most recent section (last one in the list)
        latest_section_name = all_sections[-1][0].strip()

        # Find that section content
        section_pattern = rf'## Available from {re.escape(latest_section_name)}.*?(?=## Available from|\Z)'
        section_match = re.search(section_pattern, content, re.DOTALL)

        if section_match:
            section_content = section_match.group(0)

            # Pattern: - **Product Name** (Brand) - $price (unit info)
            # More flexible pattern to catch different formats
            pattern = r'-\s+\*\*([^*]+?)\*\*\s+(?:\(([^)]+)\))?\s+[–-]\s+\$(\d+(?:\.\d{2})?)\s*(?:\(([^)]+)\))?'

            matches = re.findall(pattern, section_content)

            for match in matches:
                product_name = match[0].strip()
                brand = match[1].strip() if match[1] else ''
                price = match[2].strip()
                unit_info = match[3].strip() if match[3] else ''

                products.append({
                    'name': product_name,
                    'size': '',
                    'brand': brand,
                    'price': price,
                    'unit_info': unit_info
                })

    return products

def generate_drakes_html(products, date_range):
    """Generate HTML cards for Drakes products"""
    cards = []

    for product in products[:50]:  # Limit to 50 products
        name = product['name']
        size = product['size']
        price = product['price']
        savings = product['savings']

        display_name = f"{name} {size}" if size else name

        card = f'''                    <div class="deal-card">
                        <div class="store-badge drakes">DRAKES</div>
                        <div class="product-name">{display_name}</div>
                        <div class="price-info">
                            <span class="current-price">${price}</span>'''

        if savings:
            card += f'''
                            <span class="savings">Save ${savings}</span>'''

        card += '''
                        </div>'''

        # Add badge based on savings
        if savings:
            try:
                savings_val = float(savings)
                price_val = float(price)
                if savings_val >= price_val:
                    badge = 'half-price">½ PRICE'
                elif savings_val > price_val * 0.3:
                    badge = 'best-deal">GREAT DEAL'
                else:
                    badge = 'better-value">SPECIAL'
            except:
                badge = 'better-value">SPECIAL'
        else:
            badge = 'better-value">WEEKLY SPECIAL'

        card += f'''
                        <div class="deal-badge {badge}</div>
                    </div>'''

        cards.append(card)

    return '\n'.join(cards)

def generate_aldi_html(products):
    """Generate HTML cards for ALDI products"""
    cards = []

    for product in products[:40]:  # Limit to 40 products
        name = product['name']
        size = product['size']
        brand = product['brand']
        price = product['price']
        unit_info = product['unit_info']

        card = f'''                    <div class="deal-card">
                        <div class="store-badge aldi">ALDI</div>
                        <div class="product-name">{name} {size}</div>
                        <div class="price-info">
                            <span class="current-price">${price}</span>
                        </div>'''

        if unit_info:
            card += f'''
                        <div class="unit-price">{unit_info}</div>'''

        card += '''
                        <div class="deal-badge best-deal">SPECIAL BUY</div>
                    </div>'''

        cards.append(card)

    return '\n'.join(cards)

def update_html(html_file, drakes_html, aldi_html, date_range):
    """Update the HTML file with new Drakes and ALDI sections"""
    with open(html_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Update Drakes section
    drakes_pattern = r'(<div class="category-content" id="drakes-special-content">\s*<div class="deals-grid">)(.*?)(</div>\s*</div>\s*</div>\s*<!-- ALDI)'

    def drakes_replacer(match):
        return match.group(1) + '\n' + drakes_html + '\n                ' + match.group(3)

    content = re.sub(drakes_pattern, drakes_replacer, content, flags=re.DOTALL)

    # Update Drakes header with date
    drakes_header_pattern = r'Drakes Supermarket - [^<]+'
    content = re.sub(drakes_header_pattern, f'Drakes Supermarket - {date_range}', content)

    # Update ALDI section
    aldi_pattern = r'(<div class="category-content" id="aldi-content">\s*<div class="deals-grid">)(.*?)(</div>\s*</div>\s*</div>\s*</main>)'

    def aldi_replacer(match):
        return match.group(1) + '\n' + aldi_html + '\n                    ' + match.group(3)

    content = re.sub(aldi_pattern, aldi_replacer, content, flags=re.DOTALL)

    # Save updated HTML
    with open(html_file, 'w', encoding='utf-8') as f:
        f.write(content)

    print(f"Updated {html_file}")

def main():
    print("Parsing drakes.md...")
    drakes_products, date_range = parse_drakes_md('drakes.md')
    print(f"Found {len(drakes_products)} Drakes products")
    print(f"Date range: {date_range}")

    print("\nParsing aldi.md...")
    aldi_products = parse_aldi_md('aldi.md')
    print(f"Found {len(aldi_products)} ALDI products")

    print("\nGenerating HTML...")
    drakes_html = generate_drakes_html(drakes_products, date_range)
    aldi_html = generate_aldi_html(aldi_products)

    print("\nUpdating liveinbne_deal.html...")
    update_html('liveinbne_deal.html', drakes_html, aldi_html, date_range)

    print("\nDone!")

if __name__ == "__main__":
    main()
