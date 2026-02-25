import json
import re
from difflib import SequenceMatcher
from datetime import datetime

def normalize_product_name(name):
    """Normalize product name for better matching"""
    # Convert to lowercase
    name = name.lower()
    # Remove common variations and units
    name = re.sub(r'\b(varieties|variety|or|and|each|pack|selected)\b', '', name)
    # Remove size/weight specifications but keep them for reference
    name = re.sub(r'\d+\s*x\s*\d+\s*(ml|l|g|kg|oz|pack)', '', name)
    name = re.sub(r'\d+\s*-\s*\d+\s*(ml|l|g|kg|oz|pack)', '', name)
    name = re.sub(r'\d+\s*(ml|l|g|kg|oz|pack)', '', name)
    # Remove special characters and extra spaces
    name = re.sub(r'[^\w\s]', ' ', name)
    name = re.sub(r'\s+', ' ', name).strip()
    return name

def extract_price_value(price_str):
    """Extract numeric value from price string"""
    if isinstance(price_str, str):
        # Remove $ and any non-numeric characters except decimal point
        price_clean = re.sub(r'[^\d.]', '', price_str)
        try:
            return float(price_clean)
        except ValueError:
            return 0.0
    return 0.0

def calculate_similarity(name1, name2):
    """Calculate similarity between two product names"""
    norm1 = normalize_product_name(name1)
    norm2 = normalize_product_name(name2)
    return SequenceMatcher(None, norm1, norm2).ratio()

def find_product_matches(woolworths_products, coles_products, similarity_threshold=0.6):
    """Find matching products between Woolworths and Coles"""
    matches = []

    for w_product in woolworths_products:
        best_match = None
        best_similarity = 0

        for c_product in coles_products:
            similarity = calculate_similarity(w_product['productName'], c_product['productName'])

            if similarity > similarity_threshold and similarity > best_similarity:
                best_similarity = similarity
                best_match = c_product

        if best_match:
            w_price = extract_price_value(w_product['price'])
            c_price = extract_price_value(best_match['price'])

            match_data = {
                'woolworths_product': {
                    'id': w_product['productID'],
                    'name': w_product['productName'],
                    'brand': w_product['brand'],
                    'price': w_product['price'],
                    'price_numeric': w_price,
                    'original_price': w_product.get('originalPrice', w_product['price']),
                    'savings': w_product.get('savings', '$0.00'),
                    'special_type': w_product.get('specialType', 'REGULAR')
                },
                'coles_product': {
                    'id': best_match['productID'],
                    'name': best_match['productName'],
                    'brand': best_match['brand'],
                    'price': best_match['price'],
                    'price_numeric': c_price,
                    'original_price': best_match.get('originalPrice', best_match['price']),
                    'savings': best_match.get('savings', '$0.00'),
                    'special_type': best_match.get('specialType', 'REGULAR')
                },
                'similarity_score': round(best_similarity, 3),
                'price_difference': round(w_price - c_price, 2),
                'cheaper_store': 'Coles' if c_price < w_price else 'Woolworths' if w_price < c_price else 'Same Price'
            }
            matches.append(match_data)

    return matches

def generate_comparison_report(matches):
    """Generate comprehensive comparison report"""
    total_matches = len(matches)

    coles_cheaper_count = sum(1 for m in matches if m['cheaper_store'] == 'Coles')
    woolworths_cheaper_count = sum(1 for m in matches if m['cheaper_store'] == 'Woolworths')
    same_price_count = sum(1 for m in matches if m['cheaper_store'] == 'Same Price')

    coles_total_savings = sum(abs(m['price_difference']) for m in matches if m['cheaper_store'] == 'Coles')
    woolworths_total_savings = sum(abs(m['price_difference']) for m in matches if m['cheaper_store'] == 'Woolworths')

    avg_price_difference = sum(abs(m['price_difference']) for m in matches) / total_matches if total_matches > 0 else 0

    report = {
        'comparison_summary': {
            'total_products_compared': total_matches,
            'coles_cheaper_count': coles_cheaper_count,
            'woolworths_cheaper_count': woolworths_cheaper_count,
            'same_price_count': same_price_count,
            'coles_cheaper_percentage': round((coles_cheaper_count / total_matches) * 100, 2) if total_matches > 0 else 0,
            'woolworths_cheaper_percentage': round((woolworths_cheaper_count / total_matches) * 100, 2) if total_matches > 0 else 0,
            'coles_total_potential_savings': round(coles_total_savings, 2),
            'woolworths_total_potential_savings': round(woolworths_total_savings, 2),
            'average_price_difference': round(avg_price_difference, 2),
            'comparison_date': datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        },
        'matched_products': matches,
        'top_coles_savings': sorted([m for m in matches if m['cheaper_store'] == 'Coles'],
                                   key=lambda x: abs(x['price_difference']), reverse=True)[:10],
        'top_woolworths_savings': sorted([m for m in matches if m['cheaper_store'] == 'Woolworths'],
                                        key=lambda x: abs(x['price_difference']), reverse=True)[:10]
    }

    return report

def main():
    """Main function to run the price comparison"""
    try:
        # Load data files
        print("Loading Woolworths data...")
        with open('data/woolworths_25022026.json', 'r', encoding='utf-8') as f:
            woolworths_data = json.load(f)

        print("Loading Coles data...")
        with open('data/coles_25022026.json', 'r', encoding='utf-8') as f:
            coles_data = json.load(f)

        print(f"Loaded {len(woolworths_data)} Woolworths products and {len(coles_data)} Coles products")

        # Find matches
        print("Finding product matches...")
        matches = find_product_matches(woolworths_data, coles_data, similarity_threshold=0.6)

        print(f"Found {len(matches)} matching products")

        # Generate report
        print("Generating comparison report...")
        report = generate_comparison_report(matches)

        # Save results
        output_filename = 'price_comparison_results.json'
        with open(output_filename, 'w', encoding='utf-8') as f:
            json.dump(report, f, indent=2, ensure_ascii=False)

        print(f"Results saved to {output_filename}")

        # Print summary
        summary = report['comparison_summary']
        print("\n" + "="*60)
        print("PRICE COMPARISON SUMMARY")
        print("="*60)
        print(f"Total products compared: {summary['total_products_compared']}")
        print(f"Coles cheaper: {summary['coles_cheaper_count']} ({summary['coles_cheaper_percentage']}%)")
        print(f"Woolworths cheaper: {summary['woolworths_cheaper_count']} ({summary['woolworths_cheaper_percentage']}%)")
        print(f"Same price: {summary['same_price_count']}")
        print(f"Average price difference: ${summary['average_price_difference']}")
        print(f"Total potential savings at Coles: ${summary['coles_total_potential_savings']}")
        print(f"Total potential savings at Woolworths: ${summary['woolworths_total_potential_savings']}")

        if summary['coles_cheaper_count'] > summary['woolworths_cheaper_count']:
            print(f"\nCOLES is generally cheaper with {summary['coles_cheaper_count']} products vs {summary['woolworths_cheaper_count']}")
        elif summary['woolworths_cheaper_count'] > summary['coles_cheaper_count']:
            print(f"\nWOOLWORTHS is generally cheaper with {summary['woolworths_cheaper_count']} products vs {summary['coles_cheaper_count']}")
        else:
            print(f"\nBoth stores are equally competitive")

        return report

    except FileNotFoundError as e:
        print(f"Error: Could not find data file - {e}")
    except json.JSONDecodeError as e:
        print(f"Error: Invalid JSON format - {e}")
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()