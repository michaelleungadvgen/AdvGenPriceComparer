#!/usr/bin/env python3
"""Update gooddeals.html with real product URLs"""

import re

# Product URLs found from research
product_urls = {
    1: "https://www.jbhifi.com.au/products/lg-77-oled-evo-c5-ai-4k-uhd-smart-tv-2025",
    2: "https://www.jbhifi.com.au/products/samsung-galaxy-tab-s9-wi-fi-128gb-black-renewed-as-new",
    3: "https://www.jbhifi.com.au/collections/home-appliances/roborock",  # Collection page - specific model not found
    4: "https://www.harveynorman.com.au/hp-omnibook-5-14-inch-oled-snapdragon-x-x1-26-100-16gb-512gb-ssd-next-gen-ai-copilot-pc-laptop.html",  # 14" model
    5: "https://www.jbhifi.com.au/products/dyson-v8-origin-stick-vacuum",
    6: "https://www.jbhifi.com.au/products/samsung-85-q7f-qled-4k-smart-tv-2025",
    7: "https://www.harveynorman.com.au/jbl-bar-1000mk2-7-1-4-channel-dolby-atmos-soundbar-with-detachable-speakers-black.html",
    8: "https://www.thegoodguys.com.au/haier-508l-quad-door-refrigerator-hrf580yhc",  # Chose HRF580YHC model
    9: "https://www.jbhifi.co.nz/products/asus-vivobook-16-16-wuxga-laptop-snapdragon-x1512gb",  # JB Hi-Fi NZ link
    10: "https://www.thegoodguys.com.au/hisense-85-inches-q6qau-qled-4k-smart-tv-2025-85q6qau"
}

# Read the current HTML
with open('gooddeals.html', 'r', encoding='utf-8') as f:
    html_content = f.read()

# Replace placeholder URLs with real URLs
for deal_num, url in product_urls.items():
    # Replace both English and Chinese versions
    placeholder = f'href="#deal{deal_num}"'
    replacement = f'href="{url}"'
    html_content = html_content.replace(placeholder, replacement)

# Write the updated HTML
with open('gooddeals.html', 'w', encoding='utf-8') as f:
    f.write(html_content)

print("Successfully updated gooddeals.html with product URLs!")
print(f"\nUpdated {len(product_urls)} product links:")
for deal_num, url in product_urls.items():
    print(f"  Deal #{deal_num}: {url}")

print("\nNotes:")
print("  - Deal #2: Renewed/As New product (not brand new)")
print("  - Deal #3: Roborock collection page (specific Q70V+ model not found)")
print("  - Deal #4: 14-inch model (16-inch not found at Harvey Norman)")
print("  - Deal #9: JB Hi-Fi NZ link (AU version not found)")
