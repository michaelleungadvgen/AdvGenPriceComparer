import json
from datetime import datetime

def load_comparison_data():
    """Load the price comparison results"""
    with open('price_comparison_results.json', 'r', encoding='utf-8') as f:
        return json.load(f)

def get_product_category(product_name):
    """Categorize products based on their names"""
    name_lower = product_name.lower()

    if any(word in name_lower for word in ['coca', 'sprite', 'fanta', 'soft drink', 'energy drink', 'red bull']):
        return 'Drinks'
    elif any(word in name_lower for word in ['chips', 'doritos', 'pringles', 'corn chips']):
        return 'Snacks'
    elif any(word in name_lower for word in ['sauce', 'tomato', 'barbecue']):
        return 'Condiments'
    elif any(word in name_lower for word in ['ice cream', 'frozen', 'dessert']):
        return 'Frozen'
    elif any(word in name_lower for word in ['chocolate', 'biscuit', 'cookie']):
        return 'Confectionery'
    elif any(word in name_lower for word in ['meat', 'beef', 'chicken', 'fish']):
        return 'Meat & Seafood'
    elif any(word in name_lower for word in ['milk', 'cheese', 'butter', 'yogurt']):
        return 'Dairy'
    elif any(word in name_lower for word in ['bread', 'bakery']):
        return 'Bakery'
    else:
        return 'General'

def create_product_card_html(product_match):
    """Create HTML for a single product comparison card"""
    woolworths = product_match['woolworths_product']
    coles = product_match['coles_product']

    # Determine which store is cheaper
    cheaper_store = product_match['cheaper_store']
    price_diff = abs(product_match['price_difference'])

    # Create savings badge for the cheaper option
    woolworths_savings_badge = ""
    coles_savings_badge = ""

    if cheaper_store == "Woolworths":
        woolworths_savings_badge = f'<div class="best-deal">BEST DEAL - Save ${price_diff:.2f}</div>'
    elif cheaper_store == "Coles":
        coles_savings_badge = f'<div class="best-deal">BEST DEAL - Save ${price_diff:.2f}</div>'

    # Handle special pricing
    woolworths_special = ""
    coles_special = ""

    if woolworths['special_type'] not in ['REGULAR', 'Every Day']:
        woolworths_special = f'<div class="special-badge">{woolworths["special_type"]}</div>'

    if coles['special_type'] not in ['REGULAR', 'Every Day']:
        coles_special = f'<div class="special-badge">{coles["special_type"]}</div>'

    # Create original price display
    woolworths_original = ""
    coles_original = ""

    if woolworths['original_price'] != woolworths['price'] and woolworths['original_price'] != 'N/A':
        woolworths_original = f'<span class="original-price">{woolworths["original_price"]}</span>'

    if coles['original_price'] != coles['price'] and coles['original_price'] != 'N/A':
        coles_original = f'<span class="original-price">{coles["original_price"]}</span>'

    # Get category for the product
    category = get_product_category(woolworths['name'])

    card_html = f"""
                <div class="comparison-card">
                    <div class="card-header">
                        <div class="product-name">{woolworths['name'][:50]}{'...' if len(woolworths['name']) > 50 else ''}</div>
                        <div class="product-details">{category} • Similarity: {product_match['similarity_score']:.1%}</div>
                    </div>
                    <div class="price-comparison">
                        <div class="store-price">
                            <div class="store-info">
                                <div class="store-badge woolworths"></div>
                                <div>
                                    <div class="store-name">Woolworths</div>
                                    {woolworths_special}
                                </div>
                            </div>
                            <div class="price-info">
                                <div class="current-price">{woolworths_original}{woolworths['price']}</div>
                                {woolworths_savings_badge}
                            </div>
                        </div>
                        <div class="store-price">
                            <div class="store-info">
                                <div class="store-badge coles"></div>
                                <div>
                                    <div class="store-name">Coles</div>
                                    {coles_special}
                                </div>
                            </div>
                            <div class="price-info">
                                <div class="current-price">{coles_original}{coles['price']}</div>
                                {coles_savings_badge}
                            </div>
                        </div>
                    </div>
                </div>"""

    return card_html

def generate_html_report(comparison_data):
    """Generate the complete HTML report"""

    summary = comparison_data['comparison_summary']
    matched_products = comparison_data['matched_products']

    # Generate product cards
    product_cards_html = ""
    for product_match in matched_products:
        product_cards_html += create_product_card_html(product_match)

    # Determine overall winner
    if summary['coles_cheaper_count'] > summary['woolworths_cheaper_count']:
        winner_text = f"Coles wins with {summary['coles_cheaper_count']} cheaper products"
        winner_color = "#e31e24"
    elif summary['woolworths_cheaper_count'] > summary['coles_cheaper_count']:
        winner_text = f"Woolworths wins with {summary['woolworths_cheaper_count']} cheaper products"
        winner_color = "#00a651"
    else:
        winner_text = "It's a tie!"
        winner_color = "#85c5d4"

    html_content = f"""<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Price Comparison Report | Coles vs Woolworths | {summary['comparison_date'][:10]}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            background: #f8f9fa;
        }}

        .container {{
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 20px;
        }}

        /* Header */
        .header {{
            background: #85c5d4;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            padding: 15px 0;
            position: sticky;
            top: 0;
            z-index: 100;
        }}

        .header-content {{
            display: flex;
            align-items: center;
            justify-content: space-between;
        }}

        .logo {{
            height: 50px;
            width: auto;
        }}

        .nav-title {{
            font-size: 24px;
            font-weight: 600;
            color: white;
        }}

        .nav-links {{
            display: flex;
            gap: 20px;
        }}

        .nav-links a {{
            color: white;
            text-decoration: none;
            padding: 8px 16px;
            border-radius: 20px;
            transition: background 0.3s ease;
            font-size: 14px;
        }}

        .nav-links a:hover {{
            background: rgba(255,255,255,0.2);
        }}

        .nav-links a.active {{
            background: rgba(255,255,255,0.3);
            font-weight: 600;
        }}

        /* Hero Section */
        .hero {{
            background: linear-gradient(135deg, #85c5d4 0%, #6bb0c0 100%);
            color: white;
            padding: 60px 0;
            text-align: center;
        }}

        .hero h1 {{
            font-size: 3rem;
            font-weight: 700;
            margin-bottom: 20px;
            text-shadow: 0 2px 4px rgba(0,0,0,0.3);
        }}

        .hero p {{
            font-size: 1.2rem;
            margin-bottom: 30px;
            opacity: 0.9;
        }}

        .week-selector {{
            display: flex;
            justify-content: center;
            gap: 15px;
            margin-top: 30px;
        }}

        .week-btn {{
            background: rgba(255,255,255,0.2);
            color: white;
            padding: 12px 25px;
            border-radius: 25px;
            text-decoration: none;
            font-weight: 600;
            transition: all 0.3s ease;
            border: 2px solid rgba(255,255,255,0.3);
        }}

        .week-btn:hover {{
            background: rgba(255,255,255,0.3);
            transform: translateY(-2px);
        }}

        .week-btn.current {{
            background: rgba(255,255,255,0.9);
            color: #85c5d4;
            border-color: white;
        }}

        .winner-announcement {{
            background: {winner_color};
            color: white;
            padding: 15px 30px;
            border-radius: 25px;
            font-size: 1.3rem;
            font-weight: 600;
            display: inline-block;
            margin-top: 20px;
        }}

        /* Retailer Logos Section */
        .retailer-section {{
            background: white;
            padding: 40px 0;
            border-bottom: 3px solid #f1f3f4;
            text-align: center;
        }}

        .retailer-logos {{
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 40px;
            margin-bottom: 20px;
        }}

        .retailer-logo {{
            padding: 20px 40px;
            border-radius: 15px;
            font-size: 2rem;
            font-weight: 700;
            color: white;
            text-align: center;
            min-width: 200px;
        }}

        .coles-logo {{
            background: #e31e24;
        }}

        .woolworths-logo {{
            background: #00a651;
        }}

        .vs-separator {{
            font-size: 2.5rem;
            font-weight: 900;
            color: #333;
            padding: 0 20px;
        }}

        /* Stats Section */
        .stats-section {{
            background: #f8f9fa;
            padding: 50px 0;
        }}

        .stats-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 30px;
        }}

        .stat-card {{
            background: white;
            padding: 30px;
            border-radius: 15px;
            text-align: center;
            box-shadow: 0 5px 15px rgba(0,0,0,0.1);
        }}

        .stat-number {{
            font-size: 2.5rem;
            font-weight: 700;
            color: #85c5d4;
            margin-bottom: 10px;
        }}

        .stat-label {{
            font-size: 1rem;
            color: #666;
        }}

        /* Product Comparison Section */
        .comparison-section {{
            padding: 50px 0;
        }}

        .section-title {{
            text-align: center;
            font-size: 2.5rem;
            margin-bottom: 50px;
            color: #2c3e50;
        }}

        .comparison-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
            gap: 30px;
        }}

        .comparison-card {{
            background: white;
            border-radius: 15px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.1);
            overflow: hidden;
            transition: transform 0.3s ease, box-shadow 0.3s ease;
        }}

        .comparison-card:hover {{
            transform: translateY(-5px);
            box-shadow: 0 10px 30px rgba(0,0,0,0.15);
        }}

        .card-header {{
            background: #85c5d4;
            color: white;
            padding: 20px;
            text-align: center;
        }}

        .product-name {{
            font-size: 1.2rem;
            font-weight: 600;
            margin-bottom: 10px;
        }}

        .product-details {{
            font-size: 0.9rem;
            opacity: 0.9;
        }}

        .price-comparison {{
            padding: 25px;
        }}

        .store-price {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 15px 0;
            border-bottom: 1px solid #f1f3f4;
        }}

        .store-price:last-child {{
            border-bottom: none;
        }}

        .store-info {{
            display: flex;
            align-items: center;
            gap: 15px;
            flex: 1;
        }}

        .store-badge {{
            width: 12px;
            height: 12px;
            border-radius: 50%;
        }}

        .store-badge.coles {{
            background: #e31e24;
        }}

        .store-badge.woolworths {{
            background: #00a651;
        }}

        .store-name {{
            font-weight: 600;
            color: #333;
            font-size: 0.9rem;
        }}

        .price-info {{
            text-align: right;
        }}

        .current-price {{
            font-size: 1.3rem;
            font-weight: 700;
            color: #333;
        }}

        .original-price {{
            font-size: 0.9rem;
            color: #999;
            text-decoration: line-through;
            margin-right: 8px;
        }}

        .best-deal {{
            background: #ffd700;
            color: #333;
            padding: 4px 12px;
            border-radius: 15px;
            font-size: 0.8rem;
            font-weight: 700;
            margin-top: 5px;
        }}

        .special-badge {{
            background: #ff6b6b;
            color: white;
            padding: 2px 8px;
            border-radius: 10px;
            font-size: 0.7rem;
            font-weight: 600;
            margin-top: 3px;
        }}

        /* Footer */
        .footer {{
            background: #2c3e50;
            color: white;
            padding: 40px 0;
        }}

        .footer-content {{
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 30px;
            flex-wrap: wrap;
        }}

        .footer-logo {{
            height: 50px;
            width: auto;
        }}

        .footer-text {{
            flex: 1;
            text-align: center;
        }}

        .footer-text p {{
            margin: 5px 0;
            opacity: 0.9;
            font-size: 0.9rem;
        }}

        .footer-links {{
            display: flex;
            gap: 20px;
        }}

        .footer-links a {{
            color: white;
            text-decoration: none;
            padding: 8px 16px;
            border-radius: 20px;
            transition: background 0.3s ease;
            font-size: 0.9rem;
        }}

        .footer-links a:hover {{
            background: rgba(255,255,255,0.2);
        }}

        /* Responsive Design */
        @media (max-width: 768px) {{
            .hero h1 {{
                font-size: 2rem;
            }}

            .comparison-grid {{
                grid-template-columns: 1fr;
            }}

            .stats-grid {{
                grid-template-columns: repeat(2, 1fr);
            }}

            .footer-content {{
                flex-direction: column;
                text-align: center;
            }}

            .footer-links {{
                justify-content: center;
            }}
        }}
    </style>
</head>
<body>
    <!-- Header -->
    <header class="header">
        <div class="container">
            <div class="header-content">
                <img src="https://www.michaelleung.info/images/logo_ml_3.png" alt="Michael Leung" class="logo">
                <div class="nav-title">Weekly Price Comparison</div>
                <nav class="nav-links">
                    <a href="price_comparison_report_20250924.html">Last Week</a>
                    <a href="#" class="active">This Week</a>
                    <a href="#about">About</a>
                </nav>
            </div>
        </div>
    </header>

    <!-- Hero Section -->
    <section class="hero">
        <div class="container">
            <h1>🛒 Smart Price Comparison</h1>
            <p>AI-powered analysis comparing Coles vs Woolworths weekly deals</p>

            <div class="week-selector">
                <a href="price_comparison_report_20250924.html" class="week-btn">24-30 Sept 2025</a>
                <a href="#" class="week-btn current">01-07 Oct 2025</a>
            </div>
        </div>
    </section>

    <!-- Retailer Logos Section -->
    <section class="retailer-section">
        <div class="container">
            <h2 class="section-title">🏆 Head-to-Head Price Battle</h2>

            <div class="retailer-logos">
                <div class="retailer-logo coles-logo">COLES</div>
                <div class="vs-separator">VS</div>
                <div class="retailer-logo woolworths-logo">WOOLWORTHS</div>
            </div>
            <div class="winner-announcement">{winner_text}</div>
        </div>
    </section>

    <!-- Stats Section -->
    <section class="stats-section" id="stats">
        <div class="container">
            <h2 class="section-title">📊 Weekly Analysis</h2>
            <div class="stats-grid">
                <div class="stat-card">
                    <div class="stat-number">{summary['total_products_compared']}</div>
                    <div class="stat-label">Products Compared</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">${summary['coles_total_potential_savings'] + summary['woolworths_total_potential_savings']:.0f}+</div>
                    <div class="stat-label">Total Savings Identified</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">{summary['coles_cheaper_percentage']:.0f}%</div>
                    <div class="stat-label">Coles Best Deals</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">{summary['woolworths_cheaper_percentage']:.0f}%</div>
                    <div class="stat-label">Woolworths Best Deals</div>
                </div>
            </div>
        </div>
    </section>

    <!-- Product Comparison Section -->
    <section class="comparison-section" id="products">
        <div class="container">
            <h2 class="section-title">Product Price Comparisons</h2>
            <div class="comparison-grid">
                {product_cards_html}
            </div>
        </div>
    </section>

    <footer class="footer">
        <div class="container">
            <div class="footer-content">
                <img src="https://www.michaelleung.info/images/logo_ml_3.png" alt="Michael Leung" class="footer-logo">
                <div class="footer-text">
                    <p>AI-Powered Price Comparison | Generated by AdvGen Price Comparer</p>
                    <p>Data sourced from official Coles and Woolworths catalogues - Week 1-7 Oct 2025</p>
                </div>
                <div class="footer-links">
                    <a href="https://www.michaelleung.info/prices">Home</a>
                    <a href="https://github.com/michaelleungadvgen/AdvGenPriceComparer">GitHub</a>
                    <a href="https://buymeacoffee.com/advgen">Buy me a Coffee</a>
                </div>
            </div>
        </div>
    </footer>
</body>
</html>"""

    return html_content

def main():
    """Generate the HTML price comparison report"""
    try:
        print("Loading price comparison data...")
        comparison_data = load_comparison_data()

        print("Generating HTML report...")
        html_content = generate_html_report(comparison_data)

        # Save the HTML file
        filename = f"price_comparison_report_{datetime.now().strftime('%Y%m%d')}.html"
        with open(filename, 'w', encoding='utf-8') as f:
            f.write(html_content)

        print(f"HTML report generated successfully: {filename}")

        # Also save to the data/html directory
        html_filename = f"data/html/price_comparison_report_{datetime.now().strftime('%Y%m%d')}.html"
        with open(html_filename, 'w', encoding='utf-8') as f:
            f.write(html_content)

        print(f"HTML report also saved to: {html_filename}")

        return filename

    except Exception as e:
        print(f"Error generating HTML report: {e}")
        return None

if __name__ == "__main__":
    main()