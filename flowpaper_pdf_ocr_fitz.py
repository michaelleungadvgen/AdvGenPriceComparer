#!/usr/bin/env python3
"""
FlowPaper PDF OCR Reader using PyMuPDF (fitz)
Downloads and extracts text from FlowPaper PDF documents using OCR
Uses PyMuPDF instead of pdf2image (no poppler dependency required)
"""

import requests
import pytesseract
from PIL import Image
import fitz  # PyMuPDF
import io
import os
import sys
from pathlib import Path

class FlowPaperPDFReader:
    def __init__(self, tesseract_path=None):
        """Initialize the PDF OCR reader
        
        Args:
            tesseract_path: Path to tesseract executable if not in PATH
        """
        if tesseract_path:
            # Check if the path exists
            if not os.path.exists(tesseract_path):
                print(f"Error: Tesseract executable not found at: {tesseract_path}")
                print(f"Please install tesseract-ocr or update the path.")
                print(f"Windows: Download from https://github.com/UB-Mannheim/tesseract/wiki")
                sys.exit(1)
            pytesseract.pytesseract.tesseract_cmd = tesseract_path
        
        # Test if tesseract is available
        try:
            pytesseract.get_tesseract_version()
        except Exception as e:
            print(f"Error: Tesseract not found or not working properly.")
            print(f"Windows: Download from https://github.com/UB-Mannheim/tesseract/wiki")
            print(f"Error details: {e}")
            sys.exit(1)

    def download_pdf(self, url, output_path=None):
        """Download PDF from URL
        
        Args:
            url: PDF URL to download
            output_path: Where to save the PDF (optional)
            
        Returns:
            Path to downloaded PDF or bytes if output_path is None
        """
        try:
            print(f"Downloading PDF from: {url}")
            headers = {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
            }
            
            response = requests.get(url, headers=headers, timeout=30)
            response.raise_for_status()
            
            if output_path:
                with open(output_path, 'wb') as f:
                    f.write(response.content)
                print(f"PDF saved to: {output_path}")
                return output_path
            else:
                return response.content
                
        except requests.RequestException as e:
            print(f"Error downloading PDF: {e}")
            return None

    def pdf_to_images(self, pdf_path_or_bytes, dpi=300):
        """Convert PDF pages to images using PyMuPDF
        
        Args:
            pdf_path_or_bytes: Path to PDF file or PDF bytes
            dpi: Resolution for conversion
            
        Returns:
            List of PIL Images
        """
        try:
            print("Converting PDF to images...")
            
            # Open PDF with PyMuPDF
            if isinstance(pdf_path_or_bytes, (str, Path)):
                # File path
                doc = fitz.open(pdf_path_or_bytes)
            else:
                # Bytes
                doc = fitz.open(stream=pdf_path_or_bytes, filetype="pdf")
            
            images = []
            mat = fitz.Matrix(dpi/72, dpi/72)  # Create transformation matrix for DPI
            
            for page_num in range(len(doc)):
                page = doc.load_page(page_num)
                pix = page.get_pixmap(matrix=mat)
                img_data = pix.tobytes("png")
                img = Image.open(io.BytesIO(img_data))
                images.append(img)
            
            doc.close()
            print(f"Converted {len(images)} pages to images")
            return images
            
        except Exception as e:
            print(f"Error converting PDF to images: {e}")
            return []

    def extract_text_from_image(self, image, lang='eng'):
        """Extract text from image using OCR
        
        Args:
            image: PIL Image object
            lang: Language for OCR (default: English)
            
        Returns:
            Extracted text string
        """
        try:
            # OCR configuration for better accuracy
            custom_config = r'--oem 3 --psm 6'
            
            text = pytesseract.image_to_string(image, lang=lang, config=custom_config)
            return text.strip()
            
        except Exception as e:
            print(f"Error extracting text from image: {e}")
            return ""

    def extract_text_direct(self, pdf_path_or_bytes):
        """Extract text directly from PDF using PyMuPDF (faster, no OCR needed)
        
        Args:
            pdf_path_or_bytes: Path to PDF file or PDF bytes
            
        Returns:
            Extracted text as string
        """
        try:
            print("Extracting text directly from PDF...")
            
            # Open PDF with PyMuPDF
            if isinstance(pdf_path_or_bytes, (str, Path)):
                doc = fitz.open(pdf_path_or_bytes)
            else:
                doc = fitz.open(stream=pdf_path_or_bytes, filetype="pdf")
            
            all_text = []
            for page_num in range(len(doc)):
                page = doc.load_page(page_num)
                text = page.get_text()
                if text.strip():
                    all_text.append(f"=== PAGE {page_num + 1} ===\n{text}\n")
            
            doc.close()
            return "\n".join(all_text)
            
        except Exception as e:
            print(f"Error extracting text directly: {e}")
            return ""

    def process_pdf(self, url, output_file=None, temp_pdf_path=None, dpi=300, lang='eng', use_ocr=False):
        """Complete pipeline to process PDF from URL
        
        Args:
            url: PDF URL to process
            output_file: Where to save extracted text (optional)
            temp_pdf_path: Where to save downloaded PDF (optional)
            dpi: Image resolution for OCR (if used)
            lang: Language for OCR (if used)
            use_ocr: Force OCR instead of direct text extraction
            
        Returns:
            Extracted text as string
        """
        # Download PDF
        if temp_pdf_path:
            pdf_data = self.download_pdf(url, temp_pdf_path)
            if not pdf_data:
                return ""
        else:
            pdf_data = self.download_pdf(url)
            if not pdf_data:
                return ""

        full_text = ""
        
        # Try direct text extraction first (faster)
        if not use_ocr:
            full_text = self.extract_text_direct(pdf_data)
        
        # If no text found or OCR requested, use OCR
        if not full_text.strip() or use_ocr:
            print("Using OCR for text extraction...")
            
            # Convert to images
            images = self.pdf_to_images(pdf_data, dpi=dpi)
            if not images:
                return ""

            # Extract text from each page
            all_text = []
            for i, image in enumerate(images, 1):
                print(f"Processing page {i}/{len(images)}...")
                text = self.extract_text_from_image(image, lang=lang)
                if text:
                    all_text.append(f"=== PAGE {i} ===\n{text}\n")

            # Combine all text
            full_text = "\n".join(all_text)
        
        # Save to file if requested
        if output_file:
            with open(output_file, 'w', encoding='utf-8') as f:
                f.write(full_text)
            print(f"Text saved to: {output_file}")

        return full_text


def main():
    """Main function to process the FlowPaper PDF"""
    
    # PDF URL
    pdf_url = "https://78c4076d.flowpaper.com/Week8SAlowres/docs/Week-8-SA-lowres.pdf?refresh=1770960809915"
    
    # Output files
    output_text_file = "flowpaper_extracted_text.txt"
    temp_pdf_file = "temp_flowpaper_document.pdf"
    
    # Initialize reader with proper tesseract path
    tesseract_path = r"C:\Program Files\Tesseract-OCR\tesseract.exe"
    reader = FlowPaperPDFReader(tesseract_path=tesseract_path)
    
    print("FlowPaper PDF OCR Reader (PyMuPDF Version)")
    print("=" * 50)
    
    try:
        # Process the PDF - try direct text extraction first, then OCR if needed
        extracted_text = reader.process_pdf(
            url=pdf_url,
            output_file=output_text_file,
            temp_pdf_path=temp_pdf_file,
            dpi=300,  # High resolution for better OCR (if needed)
            lang='eng',  # English language
            use_ocr=False  # Try direct extraction first
        )
        
        if extracted_text:
            print(f"\nExtraction completed successfully!")
            print(f"Total characters extracted: {len(extracted_text)}")
            print(f"Text saved to: {output_text_file}")
            
            # Show first 500 characters as preview
            print(f"\nPreview of extracted text:")
            print("-" * 50)
            print(extracted_text[:500] + "..." if len(extracted_text) > 500 else extracted_text)
            
        else:
            print("No text could be extracted from the PDF")
            
    except Exception as e:
        print(f"Error processing PDF: {e}")
    
    finally:
        # Clean up temp PDF file
        if os.path.exists(temp_pdf_file):
            try:
                os.remove(temp_pdf_file)
                print(f"Cleaned up temporary file: {temp_pdf_file}")
            except:
                pass


if __name__ == "__main__":
    main()