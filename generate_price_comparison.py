import json
import re
from typing import List, Dict, Optional
from datetime import datetime
from difflib import SequenceMatcher

def load_json_data(file_path: str) -> List[Dict]:
    """Load JSON data from file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            return json.load(file)
    except Exception as e:
        print(f"Error loading {file_path}: {e}")
        return []

def normalize_product_name(name: str) -> str:
    """Normalize product name for comparison"""
    # Remove extra spaces, convert to lowercase
    normalized = re.sub(r'\s+', ' ', name.lower().strip())
    # Remove size indicators and pack information for better matching
    normalized = re.sub(r'\b\d+g\b|\b\d+ml\b|\b\d+l\b|\bpack\b|\btray\b', '', normalized)
    normalized = re.sub(r'\s+', ' ', normalized).strip()
    return normalized

def extract_brand_from_name(name: str) -> str:
    """Extract brand from product name"""
    brands = ['cadbury', 'mars', 'smiths', 'smith\'s', 'bulla', 'woolworths', 'coles', 
              'peters', 'nestle', 'unilever', 'palmolive', 'dove', 'rexona', 'blackmores',
              'farmers union', 'sanitarium', 'coca-cola', 'pepsi', 'gatorade', 'pringles',
              'lindt', 'herbal essences', 'toni & guy', 'glow lab', 'colgate']
    
    name_lower = name.lower()
    for brand in brands:
        if brand in name_lower:
            return brand.title()
    
    # Fallback: first word
    words = name.split()
    return words[0] if words else "Unknown"

def calculate_similarity(name1: str, name2: str) -> float:
    """Calculate similarity between two product names"""
    norm1 = normalize_product_name(name1)
    norm2 = normalize_product_name(name2)
    
    # Use SequenceMatcher for similarity
    similarity = SequenceMatcher(None, norm1, norm2).ratio()
    
    # Boost similarity if brands match
    brand1 = extract_brand_from_name(name1)
    brand2 = extract_brand_from_name(name2)
    if brand1.lower() == brand2.lower() and brand1.lower() != "unknown":
        similarity += 0.2  # Boost for matching brands
    
    return min(similarity, 1.0)  # Cap at 1.0

def find_matching_products(coles_data: List[Dict], woolworths_data: List[Dict], 
                          similarity_threshold: float = 0.6) -> List[Dict]:
    """Find matching products between Coles and Woolworths"""
    matches = []
    used_woolworths = set()
    
    for coles_product in coles_data:
        best_match = None
        best_similarity = 0
        best_index = -1
        
        for i, woolworths_product in enumerate(woolworths_data):
            if i in used_woolworths:
                continue
                
            similarity = calculate_similarity(
                coles_product['productName'], 
                woolworths_product['productName']
            )
            
            if similarity > best_similarity and similarity >= similarity_threshold:
                best_similarity = similarity
                best_match = woolworths_product
                best_index = i
        
        if best_match:
            used_woolworths.add(best_index)
            
            # Determine which has better price
            coles_price = float(coles_product['price'].replace('$', ''))
            woolworths_price = float(best_match['price'].replace('$', ''))
            
            best_deal = "COLES" if coles_price < woolworths_price else "WOOLWORTHS"
            if coles_price == woolworths_price:
                best_deal = "TIED"
            
            matches.append({
                'coles': coles_product,
                'woolworths': best_match,
                'similarity': best_similarity,
                'best_deal': best_deal,
                'price_difference': abs(coles_price - woolworths_price)
            })
    
    # Sort by similarity (highest first)
    matches.sort(key=lambda x: x['similarity'], reverse=True)
    return matches

def format_price_display(price: str, original_price: str, savings: str) -> tuple:
    """Format price display for HTML"""
    current = price
    original = original_price if original_price != price else None
    save = savings if savings and savings != "$0.00" else None
    return current, original, save

def generate_comparison_card(match: Dict, card_id: int) -> str:
    """Generate HTML for a single comparison card"""
    coles = match['coles']
    woolworths = match['woolworths']
    similarity = match['similarity']
    best_deal = match['best_deal']
    
    # Clean product names
    product_name = coles['productName'][:50] + "..." if len(coles['productName']) > 50 else coles['productName']
    
    # Format prices
    coles_current, coles_original, coles_save = format_price_display(
        coles['price'], coles['originalPrice'], coles['savings']
    )
    
    woolworths_current, woolworths_original, woolworths_save = format_price_display(
        woolworths['price'], woolworths['originalPrice'], woolworths['savings']
    )
    
    # Determine special badges
    coles_special = coles.get('specialType', 'REGULAR')
    woolworths_special = woolworths.get('specialType', 'REGULAR')
    
    # Generate best deal badges
    coles_best_deal = ""
    woolworths_best_deal = ""
    
    if best_deal == "COLES":
        coles_best_deal = '<div class="best-deal">BEST DEAL</div>'
    elif best_deal == "WOOLWORTHS":
        woolworths_best_deal = '<div class="best-deal">BEST DEAL</div>'
    elif best_deal == "TIED":
        coles_best_deal = '<div class="best-deal">TIED DEAL</div>'
        woolworths_best_deal = '<div class="best-deal">TIED DEAL</div>'
    
    similarity_percent = int(similarity * 100)
    
    card_html = f'''
                <!-- Product Comparison {card_id} -->
                <div class="comparison-card">
                    <div class="card-header">
                        <div class="product-name">{product_name}</div>
                        <div class="product-details">{coles['category']} ({similarity_percent}% similarity)</div>
                    </div>
                    <div class="price-comparison">
                        <div class="store-price">
                            <div class="store-info">
                                <div class="store-badge coles"></div>
                                <div class="store-name">Coles - {coles['productName'][:60]}</div>
                            </div>
                            <div class="price-info">
                                <div class="current-price">{coles_current}</div>'''
    
    if coles_original or coles_save:
        card_html += f'''
                                <div>'''
        if coles_original:
            card_html += f'<span class="original-price">{coles_original}</span>'
        if coles_save:
            card_html += f'<span class="savings">Save {coles_save}</span>'
        card_html += '</div>'
    
    if coles_special != 'REGULAR':
        card_html += f'''
                                <div class="special-badge">{coles_special}</div>'''
    
    card_html += f'''
                                {coles_best_deal}
                            </div>
                        </div>
                        <div class="store-price">
                            <div class="store-info">
                                <div class="store-badge woolworths"></div>
                                <div class="store-name">Woolworths - {woolworths['productName'][:60]}</div>
                            </div>
                            <div class="price-info">
                                <div class="current-price">{woolworths_current}</div>'''
    
    if woolworths_original or woolworths_save:
        card_html += f'''
                                <div>'''
        if woolworths_original:
            card_html += f'<span class="original-price">{woolworths_original}</span>'
        if woolworths_save:
            card_html += f'<span class="savings">Save {woolworths_save}</span>'
        card_html += '</div>'
    
    if woolworths_special != 'REGULAR':
        card_html += f'''
                                <div class="special-badge">{woolworths_special}</div>'''
    
    card_html += f'''
                                {woolworths_best_deal}
                            </div>
                        </div>
                    </div>
                </div>'''
    
    return card_html

def calculate_stats(matches: List[Dict]) -> Dict:
    """Calculate comparison statistics"""
    total_products = len(matches)
    
    coles_wins = sum(1 for match in matches if match['best_deal'] == 'COLES')
    woolworths_wins = sum(1 for match in matches if match['best_deal'] == 'WOOLWORTHS')
    tied_deals = sum(1 for match in matches if match['best_deal'] == 'TIED')
    
    total_savings = sum(
        float(match['coles']['savings'].replace('$', '')) + 
        float(match['woolworths']['savings'].replace('$', ''))
        for match in matches
    )
    
    coles_percentage = int((coles_wins / total_products) * 100) if total_products > 0 else 0
    woolworths_percentage = int((woolworths_wins / total_products) * 100) if total_products > 0 else 0
    
    return {
        'total_products': total_products,
        'total_savings': f"${total_savings:.0f}",
        'coles_percentage': coles_percentage,
        'woolworths_percentage': woolworths_percentage
    }

def generate_html_comparison(coles_file: str, woolworths_file: str, template_file: str, output_file: str):
    """Generate HTML comparison file"""
    print("Loading data files...")
    coles_data = load_json_data(coles_file)
    woolworths_data = load_json_data(woolworths_file)
    
    print(f"Loaded {len(coles_data)} Coles products and {len(woolworths_data)} Woolworths products")
    
    print("Finding matching products...")
    matches = find_matching_products(coles_data, woolworths_data)
    
    print(f"Found {len(matches)} matching products")
    
    # Load HTML template
    with open(template_file, 'r', encoding='utf-8') as file:
        template_content = file.read()
    
    # Generate comparison cards HTML
    comparison_cards_html = ""
    for i, match in enumerate(matches[:20], 1):  # Limit to top 20 matches
        comparison_cards_html += generate_comparison_card(match, i)
    
    # Calculate statistics
    stats = calculate_stats(matches)
    
    # Get current date for the report
    current_date = datetime.now().strftime("%d %b %Y")
    
    # Replace content in template
    html_content = template_content
    
    # Update title and dates
    html_content = html_content.replace("27 Aug 2025- 2 Sept 2025", "10-16 Sept 2025")
    html_content = html_content.replace("3-9 Sept", "10-16 Sept")
    html_content = html_content.replace("Week 3-9 Sept 2025", f"Week 10-16 Sept 2025")
    
    # Find and replace the comparison cards section
    # Look for the price-grid div content
    grid_start = html_content.find('<div class="price-grid">')
    grid_end = html_content.find('</div>\n        </div>\n    </section>', grid_start)
    
    if grid_start != -1 and grid_end != -1:
        # Replace the content between price-grid div
        new_grid_content = f'<div class="price-grid">{comparison_cards_html}\n            </div>'
        html_content = html_content[:grid_start] + new_grid_content + html_content[grid_end:]
    
    # Update statistics
    html_content = html_content.replace(
        '<div class="stat-number">35</div>',
        f'<div class="stat-number">{stats["total_products"]}</div>'
    )
    html_content = html_content.replace(
        '<div class="stat-number">$120+</div>',
        f'<div class="stat-number">{stats["total_savings"]}+</div>'
    )
    html_content = html_content.replace(
        '<div class="stat-number">70%</div>',
        f'<div class="stat-number">{stats["coles_percentage"]}%</div>'
    )
    html_content = html_content.replace(
        '<div class="stat-number">30%</div>',
        f'<div class="stat-number">{stats["woolworths_percentage"]}%</div>'
    )
    
    # Write output file
    with open(output_file, 'w', encoding='utf-8') as file:
        file.write(html_content)
    
    print(f"Generated comparison HTML: {output_file}")
    print(f"Statistics: {stats}")

if __name__ == "__main__":
    coles_file = r"C:\Users\advgen10\source\repos\AdvGenPriceComparer\data\cloes_10092025.json"
    woolworths_file = r"C:\Users\advgen10\source\repos\AdvGenPriceComparer\data\woolworths_10092025.json"
    template_file = r"C:\Users\advgen10\source\repos\AdvGenPriceComparer\data\html\price_super_market_02092025.html"
    output_file = r"C:\Users\advgen10\source\repos\AdvGenPriceComparer\data\html\price_super_market_10092025.html"
    
    generate_html_comparison(coles_file, woolworths_file, template_file, output_file)