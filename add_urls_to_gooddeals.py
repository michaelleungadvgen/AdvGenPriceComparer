#!/usr/bin/env python3
"""Add product URLs to gooddeals.md"""

# Product URLs based on retailer and product name
product_urls = {
    "Samsung Galaxy S10 Ultra Tablet (256GB)": "https://www.jbhifi.com.au/products/samsung-galaxy-tab-s10-ultra-wi-fi-256gb",
    "Sennheiser Momentum True Wireless 4": "https://www.jbhifi.com.au/products/sennheiser-momentum-true-wireless-4",
    "Lenovo Legion Pro 7 Gaming Laptop (RTX 5080)": "https://www.jbhifi.com.au/products/lenovo-legion-pro-7-16-gaming-laptop",
    'Samsung 55" QN70F Neo QLED 4K TV (2025)': "https://www.thegoodguys.com.au/samsung-55-inches-qn70f-neo-qled-4k-smart-tv-2025-qa55qn70fawxxy",
    "Ninja Creami Ice Cream Maker": "https://www.thegoodguys.com.au/ninja-creami-ice-cream-maker-nc301",
    "Dyson V7 Advanced Origin Cordless Vacuum": "https://www.thegoodguys.com.au/dyson-v7-advanced-origin-cordless-vacuum",
    "Samsung 9kg Smart Front Load Washer": "https://www.thegoodguys.com.au/samsung-9kg-front-load-washer",
    "Apple Mac Computers (Select Models)": "https://www.harveynorman.com.au/computers-tablets/computers/apple-computers.html",
    "Haier French Door Fridges": "https://www.harveynorman.com.au/haier-french-door-fridges.html",
    "Windows 11 Laptops (Select Brands)": "https://www.harveynorman.com.au/computers-tablets/computers/laptops.html"
}

# Read the current markdown
with open('gooddeals.md', 'r', encoding='utf-8') as f:
    lines = f.readlines()

# Process each line
updated_lines = []
for line in lines:
    if line.startswith('Retailer,Product,Deal Price,Savings'):
        # Header line - keep as is
        updated_lines.append(line)
    elif ',' in line:
        # Data line - add URL
        parts = line.strip().split(',', 2)  # Split only first 2 commas to get retailer and product
        if len(parts) >= 2:
            retailer = parts[0]
            # Find product name (before the price)
            product_part = parts[1]

            # Try to match product name with URL
            matched = False
            for product_name, url in product_urls.items():
                if product_name in line:
                    # Add URL column before the line break
                    updated_lines.append(line.rstrip() + f',{url}\n')
                    matched = True
                    break

            if not matched:
                # No URL found, add empty URL column
                updated_lines.append(line.rstrip() + ',\n')
        else:
            updated_lines.append(line)
    else:
        updated_lines.append(line)

# Update header to include URL column
if updated_lines and updated_lines[0].startswith('Retailer,Product,Deal Price,Savings'):
    updated_lines[0] = 'Retailer,Product,Deal Price,Savings,URL\n'

# Write the updated markdown
with open('gooddeals.md', 'w', encoding='utf-8') as f:
    f.writelines(updated_lines)

print("Successfully updated gooddeals.md with product URLs!")
print(f"\nAdded {len(product_urls)} product URLs")
print("\nUpdated file: gooddeals.md")
