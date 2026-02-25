#!/usr/bin/env python3
"""
Clean up Woolworths catalogue text file by removing navigation elements,
headers, footers and extracting only product information.
"""

import re
import sys
from pathlib import Path

def clean_woolworths_catalogue(input_file, output_file=None):
    """
    Clean up Woolworths catalogue text file.

    Args:
        input_file (str): Path to input catalogue text file
        output_file (str, optional): Path to output cleaned file.
                                   If None, creates new file with _cleaned suffix
    """
    input_path = Path(input_file)

    if not input_path.exists():
        print(f"Error: Input file '{input_file}' not found")
        return False

    # Generate output filename if not provided
    if output_file is None:
        output_path = input_path.parent / f"{input_path.stem}_cleaned{input_path.suffix}"
    else:
        output_path = Path(output_file)

    try:
        with open(input_path, 'r', encoding='utf-8') as f:
            content = f.read()

        lines = content.split('\n')
        cleaned_lines = []

        # Navigation and UI elements to remove
        skip_patterns = [
            r'^=== PAGE \d+ ===$',
            r'^URL:',
            r'^Scraped at:',
            r'^Everyday & Other Services$',
            r'^Lists &$',
            r'^Buy again$',
            r'^Log in or Sign up$',
            r'^My Account$',
            r'^View cart button$',
            r'^\$0\.00$',
            r'^Browse products$',
            r'^Specials & catalogue$',
            r'^undefined$',
            r'^Recipes & ideas$',
            r'^Get more value$',
            r'^Ways to shop$',
            r'^Help$',
            r'^Shop for business$',
            r'^Stores$',
            r'^Delivery to:$',
            r'^Set your Delivery address$',
            r'^Choose$',
            r'^Select a time:$',
            r'^View available times$',
            r'^Choose Time of Delivery\.',
            r'^Home\s*/\s*Catalogues',
            r'^Home$',
            r'^Pages$',
            r'^Product List$',
            r'^Categories$',
            r'^Bookmarks$',
            r'^Previous$',
            r'^Next$',
            r'^Back to top$',
            r'^Customer Service$',
            r'^Help & Support$',
            r'^Contact Us$',
            r'^Feedback$',
            r'^Product Safety$',
            r'^Product Recalls$',
            r'^Return Policy$',
            r'^Scam Warning$',
            r'^Shop Groceries Online$',
            r'^View My Account$',
            r'^Pick up$',
            r'^Delivery$',
            r'^Delivery Now$',
            r'^New to Online Shopping\?$',
            r'^Shop for your Business$',
            r'^Useful Links$',
            r'^Store Locations & Trading Hours$',
            r'^Everyday Rewards$',
            r'^Recipes & Easy Dinner Ideas$',
            r'^Woolworths Catalogue$',
            r'^Our pricing$',
            r'^Meal Planner$',
            r'^Woolworths Fresh Magazine$',
            r'^Woolworths Apps$',
            r'^About Woolworths$',
            r'^Careers$',
            r'^Why Pick Woolies\?$',
            r'^Our Brands$',
            r'^powered by Salefinder\.com\.au$',
            r'^» Terms and Conditions$',
            r'^\d+$',  # Single numbers (pagination)
            r'^\s*worth\s*$',
            r'^\s*in total\s*$',
            r'^\s*$',  # Empty lines
        ]

        for line in lines:
            # Remove line numbers if present (format: "   123→")
            clean_line = re.sub(r'^\s*\d+→', '', line).strip()

            # Skip if line matches any skip pattern
            should_skip = False
            for pattern in skip_patterns:
                if re.match(pattern, clean_line, re.IGNORECASE):
                    should_skip = True
                    break

            if should_skip:
                continue

            # Keep lines that look like product information
            if clean_line and (
                # Price patterns
                re.search(r'\$\d+\.\d+', clean_line) or
                # Product names (contain letters and common product words)
                re.search(r'\b(beef|chicken|pork|bread|milk|cheese|apple|banana|orange|pasta|rice|flour|sugar|oil|butter|yogurt|juice|water|pack|pk|kg|g|ml|l|litre|gram)\b', clean_line, re.IGNORECASE) or
                # Discount/offer patterns
                re.search(r'\b(off|save|price|special|offer|valid|everyday|low)\b', clean_line, re.IGNORECASE) or
                # Product description patterns
                re.search(r'\b\d+\s*(g|kg|ml|l|litre|pack|pk)\b', clean_line, re.IGNORECASE)
            ):
                cleaned_lines.append(clean_line)

        # Write cleaned content
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(cleaned_lines))

        print(f"Successfully cleaned catalogue file:")
        print(f"  Input:  {input_path}")
        print(f"  Output: {output_path}")
        print(f"  Lines reduced from {len(lines)} to {len(cleaned_lines)}")

        return True

    except Exception as e:
        print(f"Error processing file: {e}")
        return False

def main():
    """Main function to handle command line usage."""
    if len(sys.argv) < 2:
        print("Usage: python clean_woolworths_catalogue.py <input_file> [output_file]")
        print("Example: python clean_woolworths_catalogue.py woolworths_catalogue_20250923_201005.txt")
        return

    input_file = sys.argv[1]
    output_file = sys.argv[2] if len(sys.argv) > 2 else None

    clean_woolworths_catalogue(input_file, output_file)

if __name__ == "__main__":
    main()