#!/usr/bin/env python3
"""
HTML to Markdown Converter for Good Deals
Converts gooddeals.html deal cards to gooddeals.md format
"""

import re
from bs4 import BeautifulSoup

def extract_deals_from_html(html_content):
    """Extract deal information from HTML content"""
    soup = BeautifulSoup(html_content, 'html.parser')
    deals = []
    
    # Find all deal cards
    deal_cards = soup.find_all('div', class_='deal-card')
    
    for card in deal_cards:
        deal = {}
        
        # Extract rank
        rank_elem = card.find('div', class_='deal-rank')
        deal['rank'] = rank_elem.text.strip() if rank_elem else ''
        
        # Extract badge (if exists)
        badge_elem = card.find('div', class_='epic-badge')
        deal['badge'] = badge_elem.text.strip() if badge_elem else ''
        
        # Extract title
        title_elem = card.find('div', class_='deal-title')
        deal['title'] = title_elem.text.strip() if title_elem else ''
        
        # Extract store
        store_elem = card.find('div', class_='deal-store')
        deal['store'] = store_elem.text.strip() if store_elem else ''
        
        # Extract savings
        savings_elem = card.find('div', class_='deal-savings')
        deal['savings'] = savings_elem.text.strip() if savings_elem else ''
        
        # Extract price information
        price_elem = card.find('div', class_='deal-price')
        if price_elem:
            # Get the main price (before any strikethrough)
            price_text = price_elem.get_text()
            # Extract current price (first price mentioned)
            current_price = price_text.split()[0]
            deal['current_price'] = current_price
            
            # Extract original price from strikethrough span
            strikethrough = price_elem.find('span', style=lambda x: x and 'line-through' in x)
            if strikethrough:
                deal['original_price'] = strikethrough.text.strip()
            else:
                deal['original_price'] = ''
        
        # Extract link
        link_elem = card.find('a', class_='deal-link')
        if link_elem:
            deal['link'] = link_elem.get('href', '')
            deal['link_text'] = link_elem.text.strip()
        else:
            deal['link'] = ''
            deal['link_text'] = ''
        
        deals.append(deal)
    
    return deals

def convert_deals_to_markdown(deals):
    """Convert deals data to Markdown format"""
    markdown_content = "# 🛒 Best Deals - AI-Powered Deal Analytics\n\n"
    markdown_content += "*Discover the best deals from Australia's top retailers with AI-powered price tracking*\n\n"
    markdown_content += "🤖 **AI Data from JB Hi-Fi, The Good Guys & Harvey Norman**\n\n"
    markdown_content += "---\n\n"
    
    for deal in deals:
        # Deal header with rank and badge
        markdown_content += f"## #{deal['rank']}"
        if deal['badge']:
            markdown_content += f" 🏆 **{deal['badge']}**"
        markdown_content += "\n\n"
        
        # Deal title
        markdown_content += f"### {deal['title']}\n\n"
        
        # Store information
        markdown_content += f"**Store:** {deal['store']}\n\n"
        
        # Price information
        price_line = f"**Price:** {deal['current_price']}"
        if deal['original_price']:
            price_line += f" ~~{deal['original_price']}~~"
        markdown_content += price_line + "\n\n"
        
        # Savings information
        if deal['savings']:
            markdown_content += f"**Savings:** {deal['savings']}\n\n"
        
        # Deal link
        if deal['link']:
            markdown_content += f"[🔗 {deal['link_text']}]({deal['link']})\n\n"
        
        markdown_content += "---\n\n"
    
    # Footer
    markdown_content += "## About Good Deals\n\n"
    markdown_content += "AI-powered deal tracking across Australia's leading retailers. We monitor prices weekly to bring you the best offers from JB Hi-Fi, The Good Guys, and Harvey Norman.\n\n"
    markdown_content += "*© 2025 MichaelLeung.info. All rights reserved. | Powered by AI*\n"
    
    return markdown_content

def main():
    """Main function to convert HTML to Markdown"""
    try:
        # Read the HTML file
        with open('gooddeals.html', 'r', encoding='utf-8') as file:
            html_content = file.read()
        
        print("✓ Reading gooddeals.html...")
        
        # Extract deals from HTML
        deals = extract_deals_from_html(html_content)
        print(f"✓ Extracted {len(deals)} deals from HTML")
        
        # Convert to Markdown
        markdown_content = convert_deals_to_markdown(deals)
        print("✓ Converted deals to Markdown format")
        
        # Write to Markdown file
        with open('gooddeals.md', 'w', encoding='utf-8') as file:
            file.write(markdown_content)
        
        print("✓ Successfully created gooddeals.md")
        print(f"✓ Processed {len(deals)} deal cards")
        
    except FileNotFoundError:
        print("❌ Error: gooddeals.html file not found")
    except Exception as e:
        print(f"❌ Error: {str(e)}")

if __name__ == "__main__":
    main()