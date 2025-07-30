#!/usr/bin/env python3
"""
Script to trim and preprocess Woolworths catalogue text for easier parsing.
Removes OCR artifacts, page markers, and consolidates product information.
"""

import re

def clean_catalog_text(input_file, output_file, max_lines=1000):
    """
    Clean and trim catalogue text to make it more parseable.
    
    Args:
        input_file: Path to the original catalogue text file
        output_file: Path to save the cleaned text
        max_lines: Maximum number of lines to process (for size control)
    """
    
    with open(input_file, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    cleaned_lines = []
    current_product = []
    
    for i, line in enumerate(lines[:max_lines]):
        line = line.strip()
        
        # Skip empty lines and page markers
        if not line or line.startswith('---') or '[OCR EXTRACTED TEXT]' in line:
            continue
            
        # Skip image references
        if '<image:' in line or 'DeviceRGB' in line:
            continue
            
        # Skip line numbers (format: number→)
        if re.match(r'^\s*\d+→', line):
            continue
            
        # Clean up common OCR artifacts
        line = re.sub(r'[→←↑↓]', ' ', line)  # Remove arrows
        line = re.sub(r'\s+', ' ', line)     # Normalize whitespace
        line = re.sub(r'[^\w\s$.,&%-]', '', line)  # Keep alphanumeric, prices, basic punctuation
        
        if len(line) < 2:  # Skip very short fragments
            continue
            
        # Look for price patterns
        price_pattern = r'\$\d+\.?\d*'
        if re.search(price_pattern, line):
            current_product.append(f"PRICE: {line}")
        # Look for discount indicators
        elif any(word in line.upper() for word in ['SAVE', 'PRICE', 'BUY', 'GET', 'BONUS', 'OFF']):
            current_product.append(f"DISCOUNT: {line}")
        # Look for product names (longer text fragments)
        elif len(line) > 10 and not line.isdigit():
            current_product.append(f"PRODUCT: {line}")
        else:
            current_product.append(line)
            
        # Group products together (every 10-15 lines)
        if len(current_product) >= 10:
            if current_product:
                cleaned_lines.append("=" * 50)
                cleaned_lines.extend(current_product)
                cleaned_lines.append("")
                current_product = []
    
    # Add any remaining product info
    if current_product:
        cleaned_lines.append("=" * 50)
        cleaned_lines.extend(current_product)
    
    # Write cleaned text
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write('\n'.join(cleaned_lines))
    
    print(f"Processed {len(lines)} original lines")
    print(f"Generated {len(cleaned_lines)} cleaned lines")
    print(f"Saved to: {output_file}")

if __name__ == "__main__":
    input_file = "catalog_extracted.txt"
    output_file = "catalog_trimmed.txt"
    
    # Process in chunks to keep file manageable
    clean_catalog_text(input_file, output_file, max_lines=2000)
    
    print("\nTo process more lines, increase max_lines parameter")
    print("You can now parse the trimmed file more easily with Claude")