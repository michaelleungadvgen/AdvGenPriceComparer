"""
Update product categories in liveinbne_deal.html based on
data from woolworths_18032026.json and coles_18032026.json.

Strategy:
- Only fill in EMPTY product-category divs (never overwrite existing values)
- Decode HTML entities before name matching
- For Woolworths: if matched => "General" (all WOL products are "General")
- For Coles: if matched => use the JSON category
- If no match => leave empty
"""
import json
import re
import html

def load_json(path):
    with open(path, encoding='utf-8') as f:
        return json.load(f)

def normalize(name):
    """
    Normalize product name for fuzzy matching:
    - Decode HTML entities
    - Lowercase
    - Remove size/weight patterns (100g, 1.25L, 4x300mL, etc.)
    - Remove punctuation
    - Collapse whitespace
    """
    name = html.unescape(name)
    name = name.lower()
    # Remove size patterns like 160g-190g, 1.25L, 4x300mL, 10x375mL, 22 Pack-28 Pack etc.
    name = re.sub(r'\b\d[\d./x]*\s*(?:g|ml|l|kg|pk|pack|litre|liters?)\b', '', name, flags=re.IGNORECASE)
    name = re.sub(r'\bpack\b|\bpk\b', '', name, flags=re.IGNORECASE)
    # Remove hyphens and punctuation
    name = re.sub(r"[',&\-–/]", ' ', name)
    name = re.sub(r'\s+', ' ', name).strip()
    return name

def word_set(name):
    words = normalize(name).split()
    # Remove very short/common words that don't help disambiguation
    stopwords = {'or', 'and', 'the', 'from', 'in', 'with', 'a', 'an', 'of', 'to', 'for', ''}
    return {w for w in words if w not in stopwords and len(w) > 1}

def best_match(html_product_name, json_products):
    """
    Find best matching JSON product. Returns (category, score).
    Score = Jaccard similarity of word sets.
    """
    query_words = word_set(html_product_name)
    if not query_words:
        return '', 0.0

    best_score = 0.0
    best_category = ''

    for product in json_products:
        candidate_words = word_set(product['productName'])
        if not candidate_words:
            continue

        intersection = query_words & candidate_words
        union = query_words | candidate_words
        score = len(intersection) / len(union) if union else 0.0

        if score > best_score:
            best_score = score
            best_category = product['category']

    return best_category, best_score

def main():
    coles_products = load_json('data/coles_18032026.json')
    woolworths_products = load_json('data/woolworths_18032026.json')

    with open('liveinbne_deal.html', encoding='utf-8') as f:
        html_content = f.read()

    lines = html_content.split('\n')
    updated_lines = []

    current_store = None  # 'coles' or 'woolworths'
    changes = 0
    skipped = 0

    # Minimum score threshold for accepting a match
    THRESHOLD = 0.50

    for line in lines:
        # Track current store from store-badge
        badge_match = re.search(r'class="store-badge (coles|woolworths)"', line)
        if badge_match:
            current_store = badge_match.group(1)
        elif re.search(r'class="store-badge (drakes|aldi)"', line):
            current_store = None

        # Only process lines with product-name + product-category pattern
        combined_match = re.search(
            r'(<div class="product-name">)(.*?)(</div>)'
            r'(<div class="product-category">)(.*?)(</div>)',
            line
        )

        if combined_match and current_store in ('coles', 'woolworths'):
            product_name = combined_match.group(2)
            current_category = combined_match.group(5)

            # ONLY update empty categories
            if current_category.strip() != '':
                skipped += 1
                updated_lines.append(line)
                continue

            # Choose JSON source
            json_source = coles_products if current_store == 'coles' else woolworths_products

            new_category, score = best_match(product_name, json_source)

            if score >= THRESHOLD:
                print(f"  [{current_store.upper()}] '{html.unescape(product_name)}'")
                print(f"    '' -> '{new_category}' (score={score:.2f})")
                new_line = (
                    line[:combined_match.start(5)]
                    + new_category
                    + line[combined_match.end(5):]
                )
                updated_lines.append(new_line)
                changes += 1
            else:
                if score > 0:
                    print(f"  [NO MATCH] [{current_store.upper()}] '{html.unescape(product_name)}' "
                          f"(best score={score:.2f}, would be '{new_category}')")
                else:
                    print(f"  [NO MATCH] [{current_store.upper()}] '{html.unescape(product_name)}' (no candidates)")
                updated_lines.append(line)
        else:
            updated_lines.append(line)

    print(f"\nUpdated {changes} categories, skipped {skipped} already-set categories.")

    if changes > 0:
        with open('liveinbne_deal.html', 'w', encoding='utf-8') as f:
            f.write('\n'.join(updated_lines))
        print("Saved liveinbne_deal.html")
    else:
        print("No changes made.")

if __name__ == '__main__':
    main()
