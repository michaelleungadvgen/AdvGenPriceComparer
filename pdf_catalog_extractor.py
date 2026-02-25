#!/usr/bin/env python3
"""
Supermarket Catalog PDF Text Extractor

This program extracts text from supermarket catalog PDFs and converts them to plain text.
It can handle both local PDF files and PDFs from URLs.
"""

import requests
import PyPDF2
import pdfplumber
import io
import sys
import argparse
from pathlib import Path
import tempfile
import os
import fitz  # PyMuPDF
from PIL import Image
import pytesseract
import re
import pytesseract
pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'

class PDFCatalogExtractor:
    def __init__(self):
        self.text_content = ""
        self.ocr_available = self._check_ocr_availability()
    
    def _check_ocr_availability(self):
        """Check if OCR is available and properly configured"""
        try:
            import pytesseract
            # Try a simple OCR test
            from PIL import Image
            import io
            
            # Create a small test image
            test_image = Image.new('RGB', (100, 30), color='white')
            pytesseract.image_to_string(test_image, config='--psm 6')
            return True
        except ImportError:
            print("OCR Setup: pytesseract not installed. Install with: pip install pytesseract")
            return False
        except Exception as e:
            if "tesseract is not installed" in str(e).lower() or "not in your path" in str(e).lower():
                print("OCR Setup: Tesseract engine not installed or not in PATH")
                print("Download and install from: https://github.com/UB-Mannheim/tesseract/wiki")
                print("Windows: Download installer from the GitHub releases")
                print("Make sure tesseract.exe is in your system PATH")
            else:
                print(f"OCR Setup: Configuration issue - {e}")
            return False
    
    def download_pdf(self, url):
        """Download PDF from URL and return file-like object"""
        try:
            response = requests.get(url, headers={'User-Agent': 'Mozilla/5.0'})
            response.raise_for_status()
            return io.BytesIO(response.content)
        except requests.RequestException as e:
            print(f"Error downloading PDF: {e}")
            return None
    
    def extract_with_pypdf2(self, pdf_file):
        """Extract text using PyPDF2"""
        text = ""
        try:
            pdf_reader = PyPDF2.PdfReader(pdf_file)
            for page_num in range(len(pdf_reader.pages)):
                page = pdf_reader.pages[page_num]
                text += f"\n--- Page {page_num + 1} ---\n"
                text += page.extract_text()
        except Exception as e:
            print(f"Error with PyPDF2 extraction: {e}")
        return text
    
    def extract_with_pdfplumber(self, pdf_file):
        """Extract text using pdfplumber (better for tables and layout)"""
        text = ""
        try:
            with pdfplumber.open(pdf_file) as pdf:
                for page_num, page in enumerate(pdf.pages):
                    text += f"\n--- Page {page_num + 1} ---\n"
                    page_text = page.extract_text()
                    if page_text:
                        text += page_text
                    
                    # Extract tables if present
                    tables = page.extract_tables()
                    if tables:
                        text += "\n[TABLES FOUND]\n"
                        for table in tables:
                            for row in table:
                                text += " | ".join(str(cell) if cell else "" for cell in row) + "\n"
                        text += "[END TABLES]\n"
        except Exception as e:
            print(f"Error with pdfplumber extraction: {e}")
        return text
    
    def extract_from_url(self, url):
        """Extract text from PDF URL"""
        print(f"Downloading PDF from: {url}")
        pdf_file = self.download_pdf(url)
        if not pdf_file:
            return ""
        
        return self.extract_from_file_object(pdf_file)
    
    def extract_from_file(self, file_path):
        """Extract text from local PDF file"""
        if not Path(file_path).exists():
            print(f"File not found: {file_path}")
            return ""
        
        with open(file_path, 'rb') as file:
            return self.extract_from_file_object(file)
    
    def extract_with_pymupdf(self, pdf_file):
        """Extract text using PyMuPDF with enhanced methods"""
        text = ""
        try:
            # Reset file pointer
            pdf_file.seek(0)
            pdf_bytes = pdf_file.read()
            
            # Open PDF with PyMuPDF
            pdf_document = fitz.open(stream=pdf_bytes, filetype="pdf")
            
            for page_num in range(len(pdf_document)):
                page = pdf_document.load_page(page_num)
                text += f"\n--- Page {page_num + 1} ---\n"
                
                # Try multiple extraction methods
                methods_tried = []
                
                # Method 1: Direct text extraction
                page_text = page.get_text()
                if page_text.strip():
                    print(f"Page {page_num + 1}: Using direct text extraction ({len(page_text)} chars)")
                    text += page_text
                    methods_tried.append("direct")
                
                # Method 2: Extract text with layout info
                if not page_text.strip() or len(page_text.strip()) < 50:
                    layout_text = page.get_text("dict")
                    extracted_text = self._extract_from_text_dict(layout_text)
                    if extracted_text.strip():
                        print(f"Page {page_num + 1}: Using layout-based extraction ({len(extracted_text)} chars)")
                        text += extracted_text
                        methods_tried.append("layout")
                
                # Method 3: Extract from blocks
                if not any(methods_tried):
                    blocks = page.get_text("blocks")
                    block_text = ""
                    for block in blocks:
                        if len(block) > 4:  # Text block
                            block_text += block[4] + "\n"
                    if block_text.strip():
                        print(f"Page {page_num + 1}: Using block-based extraction ({len(block_text)} chars)")
                        text += block_text
                        methods_tried.append("blocks")
                
                # Method 4: Try OCR if available and needed (fallback or if insufficient text)
                extracted_text_length = 0
                if 'extracted_text' in locals() and extracted_text:
                    extracted_text_length += len(extracted_text.strip())
                if 'block_text' in locals() and block_text:
                    extracted_text_length += len(block_text.strip())
                extracted_text_length += len(page_text.strip())
                
                if not any(methods_tried) or extracted_text_length < 100:
                    if self.ocr_available:
                        try:
                            print(f"Page {page_num + 1}: Attempting OCR (insufficient or no text found)")
                            
                            # Convert page to image with high resolution for better OCR
                            mat = fitz.Matrix(3.0, 3.0)  # 3x zoom for better OCR quality
                            pix = page.get_pixmap(matrix=mat)
                            img_data = pix.tobytes("png")
                            
                            # Convert to PIL Image
                            image = Image.open(io.BytesIO(img_data))
                            
                            # Enhance image for better OCR
                            # Convert to grayscale
                            image = image.convert('L')
                            
                            # Apply image enhancement techniques
                            from PIL import ImageEnhance, ImageFilter
                            
                            # Increase contrast
                            enhancer = ImageEnhance.Contrast(image)
                            image = enhancer.enhance(1.5)
                            
                            # Increase sharpness
                            enhancer = ImageEnhance.Sharpness(image)
                            image = enhancer.enhance(1.2)
                            
                            # Apply slight blur to reduce noise
                            image = image.filter(ImageFilter.MedianFilter(size=3))
                            
                            # OCR configuration optimized for catalog layouts
                            ocr_configs = [
                                '--psm 6',   # Uniform block of text
                                '--psm 11',  # Sparse text, find as much text as possible
                                '--psm 12',  # Sparse text with OSD
                                '--psm 8',   # Single word
                                '--psm 13'   # Raw line, treat image as single text line
                            ]
                            
                            best_ocr_text = ""
                            best_config = ""
                            
                            for config in ocr_configs:
                                try:
                                    ocr_text = pytesseract.image_to_string(image, config=config)
                                    if len(ocr_text.strip()) > len(best_ocr_text.strip()):
                                        best_ocr_text = ocr_text
                                        best_config = config
                                except Exception as config_error:
                                    continue  # Try next config
                            
                            if best_ocr_text.strip():
                                print(f"Page {page_num + 1}: OCR successful with config '{best_config}' ({len(best_ocr_text)} chars)")
                                text += f"\n[OCR EXTRACTED TEXT]\n{best_ocr_text}\n[END OCR]\n"
                                methods_tried.append("ocr")
                            else:
                                print(f"Page {page_num + 1}: OCR produced no readable text")
                                text += "[OCR attempted but no readable text found]\n"
                                
                        except Exception as ocr_error:
                            print(f"Page {page_num + 1}: OCR failed - {ocr_error}")
                            text += f"[OCR extraction failed: {str(ocr_error)}]\n"
                    else:
                        print(f"Page {page_num + 1}: OCR not available - skipping")
                        text += "[OCR not available - see setup instructions above]\n"
                
                if not methods_tried:
                    print(f"Page {page_num + 1}: No text extracted (possible image-only page)")
            
            pdf_document.close()
            
        except Exception as e:
            print(f"Error with PyMuPDF extraction: {e}")
            
        return text
    
    def _extract_from_text_dict(self, text_dict):
        """Extract text from PyMuPDF text dictionary format"""
        text = ""
        try:
            for block in text_dict.get("blocks", []):
                if "lines" in block:
                    for line in block["lines"]:
                        for span in line.get("spans", []):
                            text += span.get("text", "") + " "
                        text += "\n"
        except Exception as e:
            print(f"Error extracting from text dict: {e}")
        return text
    
    def extract_from_file_object(self, pdf_file):
        """Extract text from file object using multiple methods"""
        # Reset file pointer
        pdf_file.seek(0)
        
        print("Extracting text using pdfplumber...")
        text_plumber = self.extract_with_pdfplumber(pdf_file)
        
        # Reset file pointer for second extraction
        pdf_file.seek(0)
        
        print("Extracting text using PyPDF2...")
        text_pypdf2 = self.extract_with_pypdf2(pdf_file)
        
        # Reset file pointer for PyMuPDF extraction
        pdf_file.seek(0)
        
        print("Extracting text using PyMuPDF...")
        text_pymupdf = self.extract_with_pymupdf(pdf_file)
        
        # Choose the best result based on content length and quality
        results = [
            ("pdfplumber", text_plumber),
            ("PyPDF2", text_pypdf2),
            ("PyMuPDF", text_pymupdf)
        ]
        
        # Filter out results with very little content
        valid_results = [(name, text) for name, text in results if len(text.strip()) > 100]
        
        if valid_results:
            # Choose the result with the most content
            best_method, best_text = max(valid_results, key=lambda x: len(x[1]))
            print(f"Using {best_method} results (best extraction: {len(best_text)} chars)")
            self.text_content = best_text
        else:
            # If all methods failed, use PyMuPDF result anyway
            print("All methods produced minimal content, using PyMuPDF results")
            self.text_content = text_pymupdf
        
        return self.text_content
    
    def extract_product_info(self):
        """Extract structured product information from the text"""
        products = []
        
        # Common price patterns for Australian supermarkets
        price_patterns = [
            r'\$\d+\.?\d*',  # $1.50, $5, $12.99
            r'\d+\.?\d*\s*¢',  # 99¢, 1.50¢
            r'\d+\s*for\s*\$\d+\.?\d*',  # 2 for $5
            r'\$\d+\.?\d*\s*ea',  # $2.50 ea
            r'\$\d+\.?\d*\s*each',  # $2.50 each
        ]
        
        # Split text into lines and process
        lines = self.text_content.split('\n')
        
        for i, line in enumerate(lines):
            line = line.strip()
            if not line or line.startswith('---'):
                continue
                
            # Look for price patterns
            for pattern in price_patterns:
                matches = re.finditer(pattern, line, re.IGNORECASE)
                for match in matches:
                    price_text = match.group()
                    
                    # Extract product name (text before the price)
                    price_start = match.start()
                    product_name = line[:price_start].strip()
                    
                    # Clean up product name
                    if product_name and len(product_name) > 3:
                        # Remove common catalog noise
                        product_name = re.sub(r'^(save|was|now|special|\d+)', '', product_name, flags=re.IGNORECASE).strip()
                        
                        if product_name:
                            products.append({
                                'name': product_name,
                                'price': price_text,
                                'line': line,
                                'page_context': self._get_page_context(i, lines)
                            })
        
        return products
    
    def _get_page_context(self, line_index, lines):
        """Get the page number context for a line"""
        # Look backwards to find the most recent page marker
        for i in range(line_index, -1, -1):
            if lines[i].startswith('--- Page'):
                return lines[i].strip()
        return "Unknown page"
    
    def save_to_file(self, output_path):
        """Save extracted text to file"""
        try:
            with open(output_path, 'w', encoding='utf-8') as f:
                f.write(self.text_content)
            print(f"Text saved to: {output_path}")
        except Exception as e:
            print(f"Error saving file: {e}")
    
    def save_products_to_file(self, output_path):
        """Save structured product information to file"""
        try:
            products = self.extract_product_info()
            
            with open(output_path, 'w', encoding='utf-8') as f:
                f.write("EXTRACTED PRODUCTS\n")
                f.write("==================\n\n")
                
                for i, product in enumerate(products, 1):
                    f.write(f"{i}. {product['name']}\n")
                    f.write(f"   Price: {product['price']}\n")
                    f.write(f"   Context: {product['page_context']}\n")
                    f.write(f"   Full line: {product['line']}\n")
                    f.write("-" * 50 + "\n")
                
                f.write(f"\nTotal products found: {len(products)}\n")
            
            print(f"Structured products saved to: {output_path}")
            return products
            
        except Exception as e:
            print(f"Error saving products file: {e}")
            return []
    
    def print_summary(self):
        """Print extraction summary"""
        lines = self.text_content.split('\n')
        words = self.text_content.split()
        chars = len(self.text_content)
        
        print(f"\nExtraction Summary:")
        print(f"- Total characters: {chars}")
        print(f"- Total words: {len(words)}")
        print(f"- Total lines: {len(lines)}")

def main():
    parser = argparse.ArgumentParser(description='Extract text from supermarket catalog PDFs')
    parser.add_argument('input', help='PDF file path or URL')
    parser.add_argument('-o', '--output', help='Output text file path')
    parser.add_argument('-p', '--preview', action='store_true', help='Show first 500 characters of extracted text')
    parser.add_argument('-s', '--structured', action='store_true', help='Extract structured product information')
    
    args = parser.parse_args()
    
    extractor = PDFCatalogExtractor()
    
    # Determine if input is URL or file path
    if args.input.startswith(('http://', 'https://')):
        text = extractor.extract_from_url(args.input)
    else:
        text = extractor.extract_from_file(args.input)
    
    if not text.strip():
        print("No text extracted from PDF")
        return
    
    # Show preview if requested
    if args.preview:
        print("\nPreview (first 500 characters):")
        print("-" * 50)
        print(text[:500])
        if len(text) > 500:
            print("...")
        print("-" * 50)
    
    # Determine base filename
    if args.input.startswith(('http://', 'https://')):
        base_name = "catalog"
    else:
        base_name = Path(args.input).stem
    
    # Save raw text
    if args.output:
        extractor.save_to_file(args.output)
    else:
        extractor.save_to_file(f"{base_name}_extracted.txt")
    
    # Extract and save structured product information
    if args.structured:
        products = extractor.save_products_to_file(f"{base_name}_products.txt")
        print(f"\nFound {len(products)} products")
        
        # Show a few examples
        if products:
            print("\nSample products found:")
            for i, product in enumerate(products[:5]):
                print(f"  {i+1}. {product['name']} - {product['price']}")
            if len(products) > 5:
                print(f"  ... and {len(products) - 5} more")
    
    extractor.print_summary()

if __name__ == "__main__":
    # Check if running with arguments
    if len(sys.argv) > 1:
        main()
    else:
        # Demo mode with the provided URL
        print("Demo mode: Extracting from Woolworths catalog URL")
        url = "https://d3vvi2v9oj75wh.cloudfront.net/uploads/pdf/WW_QLD_230725_W9L66PJK3.pdf"
        
        extractor = PDFCatalogExtractor()
        text = extractor.extract_from_url(url)
        
        if text.strip():
            print("\nPreview (first 1000 characters):")
            print("-" * 50)
            print(text[:1000])
            if len(text) > 1000:
                print("...")
            print("-" * 50)
            
            # Save to default file
            extractor.save_to_file("woolworths_catalog_extracted.txt")
            extractor.print_summary()
        else:
            print("Failed to extract text from the PDF")