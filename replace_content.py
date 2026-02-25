#!/usr/bin/env python3
"""
Content Replacement Tool - Updated Version
Replaces liveinbne_deal.html with supermarket.md content and updates aldi.md section
"""

import os
import re
import shutil
from pathlib import Path
from datetime import datetime

class ContentReplacer:
    def __init__(self, base_path=None):
        """Initialize the content replacer with base path"""
        if base_path:
            self.base_path = Path(base_path)
        else:
            self.base_path = Path(__file__).parent
        
        self.html_file = self.base_path / "liveinbne_deal.html"
        self.aldi_file = self.base_path / "aldi.md"
        self.supermarket_file = self.base_path / "supermarket.md"
    
    def replace_html_with_supermarket_md(self):
        """Replace entire HTML content with supermarket.md formatted as HTML table"""
        try:
            # Read supermarket.md content
            with open(self.supermarket_file, 'r', encoding='utf-8') as f:
                markdown_content = f.read()
            
            # Convert markdown tables to HTML and create a proper HTML document
            html_content = self.convert_markdown_to_html(markdown_content)
            
            # Write to HTML file
            with open(self.html_file, 'w', encoding='utf-8') as f:
                f.write(html_content)
            
            print(f"[SUCCESS] Replaced {self.html_file} with content from {self.supermarket_file}")
            return True
            
        except Exception as e:
            print(f"[ERROR] Error replacing HTML with supermarket content: {e}")
            return False
    
    def convert_markdown_to_html(self, markdown_content):
        """Convert markdown content to proper HTML with tables"""
        # Parse markdown and convert to HTML tables
        lines = markdown_content.split('\n')
        html_parts = []
        in_table = False
        table_rows = []
        current_category = ""
        
        for line in lines:
            line = line.strip()
            
            if line.startswith('# '):
                # Main heading
                title = line[2:]
                html_parts.append(f'<h1 class="main-title">{title}</h1>')
            elif line.startswith('### '):
                # Category heading
                if in_table and table_rows:
                    # Close previous table
                    html_parts.append(self.build_html_table(table_rows, current_category))
                    table_rows = []
                    in_table = False
                
                current_category = line[4:]
                html_parts.append(f'<h3 class="category-title">{current_category}</h3>')
            elif line.startswith('| :---'):
                # Table separator - start table
                in_table = True
            elif line.startswith('|') and '|' in line[1:]:
                # Table row
                if in_table:
                    cells = [cell.strip() for cell in line.split('|')[1:-1]]
                    table_rows.append(cells)
            elif line and not line.startswith('*Note:') and not line.startswith('---'):
                # Regular text
                if in_table and table_rows:
                    # Close current table
                    html_parts.append(self.build_html_table(table_rows, current_category))
                    table_rows = []
                    in_table = False
                if line:
                    html_parts.append(f'<p>{line}</p>')
        
        # Close final table if needed
        if in_table and table_rows:
            html_parts.append(self.build_html_table(table_rows, current_category))
        
        # Create complete HTML document
        return self.build_complete_html('\n'.join(html_parts))
    
    def build_html_table(self, rows, category):
        """Build HTML table from rows"""
        if not rows:
            return ""
        
        # Get emoji from category title
        emoji_map = {
            'Instant Coffee': '☕',
            'Premium Chocolate': '🍫',
            'Premium Biscuits': '🍪',
            'Premium Ice Cream': '🍦',
            'Potato Chips & Snacks': '🥨',
            'Breakfast Cereals': '🥣',
            'Canned Tuna 95g': '🐟',
            'Rice': '🍚',
            'Laundry Products': '🧺',
            'Soft Drink': '🥤',
            'Butter': '🧈'
        }
        
        emoji = emoji_map.get(category.split(' ')[0] + ' ' + category.split(' ')[1] if len(category.split()) > 1 else category, '📦')
        
        html = f'<div class="category-section">\n'
        html += f'  <div class="category-header"><span class="category-icon">{emoji}</span> {category}</div>\n'
        html += f'  <div class="category-content">\n'
        html += f'    <table class="price-table">\n'
        
        # Header row
        if rows:
            html += f'      <thead>\n        <tr>\n'
            for header in rows[0]:
                html += f'          <th>{header}</th>\n'
            html += f'        </tr>\n      </thead>\n'
            
            # Data rows
            html += f'      <tbody>\n'
            for row in rows[1:]:
                html += f'        <tr>\n'
                for i, cell in enumerate(row):
                    # Style retailer column
                    if i == 1:  # Retailer column
                        if cell.startswith('**') and cell.endswith('**'):
                            cell = f'<strong class="best-deal">{cell[2:-2]}</strong>'
                        elif cell.startswith('*') and cell.endswith('*'):
                            cell = f'<em class="drakes-price">{cell[1:-1]}</em>'
                    # Style price column
                    elif i == 2:  # Price column
                        if cell.startswith('**') and cell.endswith('**'):
                            cell = f'<strong class="best-price">{cell[2:-2]}</strong>'
                        elif cell.startswith('*') and cell.endswith('*'):
                            cell = f'<em class="drakes-price">{cell[1:-1]}</em>'
                    
                    html += f'          <td>{cell}</td>\n'
                html += f'        </tr>\n'
            html += f'      </tbody>\n'
        
        html += f'    </table>\n'
        html += f'  </div>\n'
        html += f'</div>\n'
        
        return html
    
    def build_complete_html(self, content):
        """Build complete HTML document"""
        return f'''<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Supermarket Price Comparison - Brisbane Deals</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            line-height: 1.6;
            color: #333;
            background: #f5f5f5;
            min-height: 100vh;
        }}

        .container {{
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }}

        .main-title {{
            color: #009900;
            text-align: center;
            border-bottom: 3px solid #009900;
            padding-bottom: 1rem;
            margin-bottom: 2rem;
            font-size: 2.5rem;
        }}

        .category-section {{
            background: white;
            border-radius: 10px;
            margin-bottom: 2rem;
            overflow: hidden;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }}

        .category-header {{
            background: #009900;
            color: white;
            padding: 1rem 1.5rem;
            font-size: 1.3rem;
            font-weight: 600;
            display: flex;
            align-items: center;
        }}

        .category-icon {{
            margin-right: 0.8rem;
            font-size: 1.5rem;
        }}

        .category-content {{
            padding: 0;
        }}

        .price-table {{
            width: 100%;
            border-collapse: collapse;
        }}

        .price-table th {{
            background: #007700;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 600;
            border-bottom: 2px solid #005500;
        }}

        .price-table td {{
            padding: 12px;
            border-bottom: 1px solid #eee;
            vertical-align: middle;
        }}

        .price-table tr:nth-child(even) {{
            background: #f8f8f8;
        }}

        .price-table tr:hover {{
            background: #e8f5e8;
        }}

        .best-deal {{
            color: #009900;
            font-weight: bold;
        }}

        .best-price {{
            color: #e31837;
            font-weight: bold;
            font-size: 1.1em;
        }}

        .drakes-price {{
            color: #666;
            font-style: italic;
        }}

        p {{
            margin: 1rem 0;
            padding: 0 1.5rem;
            color: #666;
        }}

        @media (max-width: 768px) {{
            .container {{ padding: 10px; }}
            .main-title {{ font-size: 2rem; }}
            .price-table {{ font-size: 0.9rem; }}
            .price-table th, .price-table td {{ padding: 8px; }}
        }}
    </style>
</head>
<body>
    <div class="container">
        {content}
    </div>
</body>
</html>'''
    
    def update_aldi_section_content(self):
        """Update aldi.md with new comprehensive content"""
        new_aldi_content = '''# ALDI Special Buys Catalogue - Updated

## 🏪 Current Week Highlights (31 August 2025)

### 🥘 Grocery Specials
- **Barilla Spaghetti** - $4.99 (Special Buy)
- **Nutella 950g** - $11.99 (Special Buy)  
- **Moccona Coffee 400g** - $25.99 (Special Buy)
- **Smith's Large Packs** - $21.99 (Bulk Value)
- **Oreo Large Pack** - $25.99 (Special Buy)
- **Harvest Snaps** - $21.99 (Special Buy)
- **Kit Kat/Milkybar Share Packs** - $18.99 (Special Buy)
- **Vittoria Coffee Beans/Ground 1kg** - $38.99 (Special Buy)

### 🥩 Fresh Meat & Seafood
- **Smoked Salmon 200g** - $19.99 (Special Buy)
- **Pork Porterhouse Steaks** - $8.49/kg (Special Buy)
- **Greek Style Chicken Tortilla Wrap Meal Kit** - $14.99 (Special Buy)
- **The Whistling Butcher Habanero Chilli Beef Sausages 500g** - $7.99 (Special Buy)

### 🍺 Liquor Specials
- **Erdinger Beer 500ml** - $4.99 (Special Buy)
- **Bombay Sapphire Gin 700ml** - $49.99 (Special Buy)
- **Maker's Mark Bourbon 700ml** - $44.99 (Special Buy)

### 🔧 Tools & Appliances
- **Ferrex 20V Xfinity 6 Piece Set** - $199.00 (Special Buy)
- **Smart Lock Portable Key Safe** - $89.99 (Special Buy)
- **Cordless Screwdriver** - $24.99 (Special Buy)
- **Table Top Saw** - $149.00 (Special Buy)
- **Ambiano Multi Snack Maker** - $39.99 (Special Buy)
- **Stirling 60cm Glass Induction Cooktop** - $179.00 (Special Buy)
- **Stirling 80L Designer Built-In Oven** - $249.00 (Special Buy)
- **Kenwood Blender & Mill** - $49.99 (Special Buy)

### 🍽️ Kitchen Essentials
- **Stirling 30L Microwave Oven** - $169.00 (Special Buy)
- **Dreamfarm Fluicer** - $12.99 (Special Buy)
- **Crofton Ceramic Frying Pan 28cm** - $19.99 (Special Buy)
- **Multi-Function Pan Set 5L** - $49.99 (Special Buy)

### 🐕 Pet Products
- **PETPLAY Heavy Duty Dog Toy** - $5.99 (Special Buy)
- **PETPLAY Elevated Dog Bed** - $19.99 (Special Buy)
- **PETPLAY Retractable Lead** - $14.99 (Special Buy)
- **PETPLAY Interactive Dog Ball** - $11.99 (Special Buy)

---

## 📅 Coming Soon

### Available from Saturday 6th September
- **ECOVACS Deebot Neo 2.0 Robot Vacuum** - $279.00
- **TINECO GO H2O Sense Floor Washer** - $249.00
- **Various Cleaning Products & Home Storage Solutions**

### Available from Wednesday 10th September  
- **Travel Essentials Collection** - Suitcases, Power Banks, Travel Accessories
- **Fashion Range** - Men's, Women's & Children's Clothing
- **Health & Wellness Products**

---

*Prices valid while stocks last. Special Buys are available for a limited time only.*
*Store: Multiple locations across Australia*
*Last Updated: 31 August 2025*
'''
        
        try:
            with open(self.aldi_file, 'w', encoding='utf-8') as f:
                f.write(new_aldi_content)
            
            print(f"[SUCCESS] Updated ALDI section in {self.aldi_file}")
            return True
            
        except Exception as e:
            print(f"[ERROR] Error updating ALDI section: {e}")
            return False
        
    def update_html_date_badge(self, new_date_range):
        """Update the date badge in the HTML file"""
        try:
            with open(self.html_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Find and replace the date badge
            pattern = r'<div class="date-badge">[^<]+</div>'
            replacement = f'<div class="date-badge">{new_date_range}</div>'
            
            new_content = re.sub(pattern, replacement, content)
            
            with open(self.html_file, 'w', encoding='utf-8') as f:
                f.write(new_content)
                
            print(f"[SUCCESS] Updated HTML date badge to: {new_date_range}")
            return True
            
        except Exception as e:
            print(f"[ERROR] Error updating HTML date badge: {e}")
            return False
    
    def replace_html_category_section(self, category_id, new_section_html):
        """Replace an entire category section in the HTML file"""
        try:
            with open(self.html_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Pattern to match the entire category section
            pattern = rf'<!-- {category_id} -->\s*<div class="category-section">.*?</div>\s*</div>'
            
            # If no comment marker, try to find by category header onclick
            if not re.search(pattern, content, re.DOTALL):
                pattern = rf'<div class="category-section">\s*<div class="category-header" onclick="toggleCategory\(\'{category_id}\'\)">.*?</div>\s*</div>\s*</div>'
            
            new_content = re.sub(pattern, new_section_html, content, flags=re.DOTALL)
            
            if new_content != content:
                with open(self.html_file, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f"[SUCCESS] Replaced HTML category section: {category_id}")
                return True
            else:
                print(f"[WARNING] No changes made to category: {category_id}")
                return False
                
        except Exception as e:
            print(f"[ERROR] Error replacing HTML category section {category_id}: {e}")
            return False
    
    def replace_aldi_section(self, section_date, new_section_content):
        """Replace a specific ALDI section by date"""
        try:
            with open(self.aldi_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Pattern to match section by date
            pattern = rf'## Available from {re.escape(section_date)}.*?(?=## Available from|\Z)'
            
            new_content = re.sub(pattern, new_section_content, content, flags=re.DOTALL)
            
            if new_content != content:
                with open(self.aldi_file, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f"[SUCCESS] Replaced ALDI section: {section_date}")
                return True
            else:
                print(f"[WARNING] No changes made to ALDI section: {section_date}")
                return False
                
        except Exception as e:
            print(f"[ERROR] Error replacing ALDI section {section_date}: {e}")
            return False
    
    def add_aldi_section(self, new_section_content):
        """Add a new ALDI section at the end"""
        try:
            with open(self.aldi_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Add new section at the end
            new_content = content.rstrip() + '\n\n---\n\n' + new_section_content
            
            with open(self.aldi_file, 'w', encoding='utf-8') as f:
                f.write(new_content)
                
            print(f"[SUCCESS] Added new ALDI section")
            return True
            
        except Exception as e:
            print(f"[ERROR] Error adding ALDI section: {e}")
            return False
    
    def replace_html_aldi_deals_section(self, new_deals_html):
        """Replace the entire ALDI Special Buys section in HTML"""
        try:
            with open(self.html_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Pattern to match ALDI section
            pattern = r'<!-- ALDI Weekly Deals -->\s*<div class="category-section">.*?</div>\s*</div>\s*</div>'
            
            new_content = re.sub(pattern, new_deals_html, content, flags=re.DOTALL)
            
            if new_content != content:
                with open(self.html_file, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f"[SUCCESS] Replaced HTML ALDI Special Buys section")
                return True
            else:
                print(f"[WARNING] No changes made to ALDI section in HTML")
                return False
                
        except Exception as e:
            print(f"[ERROR] Error replacing HTML ALDI section: {e}")
            return False
    
    def update_aldi_section_from_md(self):
        """Update HTML ALDI section with content from aldi.md"""
        try:
            # Read ALDI markdown content
            with open(self.aldi_file, 'r', encoding='utf-8') as f:
                aldi_content = f.read()
            
            # Convert markdown to HTML deal cards
            html_deals = self.convert_aldi_md_to_html(aldi_content)
            
            # Replace the ALDI section in HTML
            return self.replace_html_aldi_deals_section(html_deals)
            
        except Exception as e:
            print(f"[ERROR] Error updating ALDI section from MD: {e}")
            return False
    
    def convert_aldi_md_to_html(self, aldi_content):
        """Convert ALDI markdown content to HTML deal cards"""
        html_cards = []
        
        # Parse sections by date
        sections = re.split(r'## Available from (.+?)\s*\((\d+) items\)', aldi_content)[1:]
        
        for i in range(0, len(sections), 3):
            if i + 2 < len(sections):
                date = sections[i]
                item_count = sections[i+1]
                content = sections[i+2]
                
                # Extract items from content
                items = re.findall(r'- \*\*(.+?)\*\* - \$(\d+\.?\d*)', content)
                
                for item_name, price in items:
                    # Create HTML card
                    card_html = f'''                    <div class="deal-card">
                        <div class="store-badge aldi">{date.upper()}</div>
                        <div class="product-name">{item_name}</div>
                        <div class="price-info">
                            <span class="current-price">${price}</span>
                        </div>
                        <div class="deal-badge best-deal">SPECIAL BUY</div>
                    </div>'''
                    html_cards.append(card_html)
        
        # Create the full ALDI section HTML
        full_html = f'''        <!-- ALDI Weekly Deals -->
        <div class="category-section">
            <div class="category-header" onclick="toggleCategory('aldi')">
                <div class="category-title">
                    <span class="category-icon">🏪</span>
                    ALDI Special Buys Summary
                </div>
                <span class="expand-icon" id="aldi-icon">+</span>
            </div>
            <div class="category-content" id="aldi-content">
                <div class="deals-grid">
{chr(10).join(html_cards)}
                </div>
            </div>
        </div>'''
        
        return full_html
    
    def backup_files(self):
        """Create backup copies of both files"""
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        
        try:
            # Backup HTML
            if self.html_file.exists():
                backup_html = self.base_path / f"liveinbne_deal_backup_{timestamp}.html"
                backup_html.write_text(self.html_file.read_text(encoding='utf-8'), encoding='utf-8')
                print(f"[SUCCESS] HTML backup created: {backup_html}")
            
            # Backup ALDI
            if self.aldi_file.exists():
                backup_aldi = self.base_path / f"aldi_backup_{timestamp}.md"
                backup_aldi.write_text(self.aldi_file.read_text(encoding='utf-8'), encoding='utf-8')
                print(f"[SUCCESS] ALDI backup created: {backup_aldi}")
                
            return True
            
        except Exception as e:
            print(f"[ERROR] Error creating backups: {e}")
            return False

def main():
    """Main function for content replacement operations"""
    replacer = ContentReplacer()
    
    print("Content Replacement Tool - Updated Version")
    print("=" * 60)
    
    # Create backups first
    print("\n[INFO] Creating backups...")
    replacer.backup_files()
    
    print("\n[INFO] Available operations:")
    print("1. Replace HTML with supermarket.md content (NEW)")
    print("2. Update ALDI section with new content (NEW)")
    print("3. Do both operations above (RECOMMENDED)")
    print("4. Update HTML date badge")
    print("5. Replace HTML category section") 
    print("6. Replace ALDI section by date")
    print("7. Add new ALDI section")
    print("8. Update ALDI deals section from aldi.md")
    print("9. Exit")
    
    while True:
        try:
            choice = input("\nSelect operation (1-9): ").strip()
            
            if choice == '1':
                replacer.replace_html_with_supermarket_md()
                
            elif choice == '2':
                replacer.update_aldi_section_content()
                
            elif choice == '3':
                print("[INFO] Performing both replacement operations...")
                success1 = replacer.replace_html_with_supermarket_md()
                success2 = replacer.update_aldi_section_content()
                
                if success1 and success2:
                    print("[SUCCESS] Both operations completed successfully!")
                else:
                    print("[WARNING] Some operations may have failed. Check output above.")
                
            elif choice == '4':
                new_date = input("Enter new date range (e.g., '30 Aug - 6 Sep 2025'): ")
                replacer.update_html_date_badge(new_date)
                
            elif choice == '5':
                category_id = input("Enter category ID to replace: ")
                print("Enter the new HTML section content (end with '---' on a new line):")
                lines = []
                while True:
                    line = input()
                    if line.strip() == '---':
                        break
                    lines.append(line)
                new_html = '\n'.join(lines)
                replacer.replace_html_category_section(category_id, new_html)
                
            elif choice == '6':
                section_date = input("Enter section date (e.g., 'Saturday 30th August'): ")
                print("Enter the new section content (end with '---' on a new line):")
                lines = []
                while True:
                    line = input()
                    if line.strip() == '---':
                        break
                    lines.append(line)
                new_content = '\n'.join(lines)
                replacer.replace_aldi_section(section_date, new_content)
                
            elif choice == '7':
                print("Enter the new ALDI section content (end with '---' on a new line):")
                lines = []
                while True:
                    line = input()
                    if line.strip() == '---':
                        break
                    lines.append(line)
                new_content = '\n'.join(lines)
                replacer.add_aldi_section(new_content)
                
            elif choice == '8':
                replacer.update_aldi_section_from_md()
                
            elif choice == '9':
                print("[INFO] Goodbye!")
                break
                
            else:
                print("[ERROR] Invalid choice. Please select 1-9.")
                
        except KeyboardInterrupt:
            print("\n\n[INFO] Goodbye!")
            break
        except Exception as e:
            print(f"[ERROR] Error: {e}")

# Legacy function for backward compatibility
def replace_html_content():
    """Legacy function - Replace entire HTML content with ALDI markdown"""
    print("[WARNING] This function completely replaces HTML with markdown content.")
    print("[WARNING] Use the new ContentReplacer class for targeted updates.")
    
    # Read content from aldi.md
    with open('aldi.md', 'r', encoding='utf-8') as f:
        aldi_content = f.read()
    
    # Write content to liveinbne_deal.html
    with open('liveinbne_deal.html', 'w', encoding='utf-8') as f:
        f.write(aldi_content)
    
    print("Successfully replaced liveinbne_deal.html content with aldi.md content")

if __name__ == "__main__":
    main()