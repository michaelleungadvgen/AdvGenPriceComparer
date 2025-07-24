# OCR Setup Guide for PDF Catalog Extractor

## Overview

The PDF Catalog Extractor has enhanced OCR functionality to handle image-based PDFs (like the Woolworths catalog) where text is not directly extractable. This guide helps you set up OCR functionality on Windows.

## Current Status

✅ **Enhanced OCR integration** - Automatically detects when OCR is needed
✅ **Multiple extraction methods** - Falls back to OCR when direct text extraction fails  
✅ **Image preprocessing** - Optimizes images for better OCR accuracy
✅ **Multiple OCR configs** - Tries different Tesseract settings for best results

❌ **Tesseract not installed** - OCR engine needs to be installed for full functionality

## Installation Steps

### 1. Install Python Dependencies
```bash
pip install pytesseract pillow
```

### 2. Install Tesseract OCR Engine

#### Windows Installation:
1. Download Tesseract installer from: https://github.com/UB-Mannheim/tesseract/wiki
2. Or directly from: https://github.com/tesseract-ocr/tesseract/releases
3. Run the installer (recommended: install to default location)
4. Add Tesseract to your system PATH, or configure the path in your script

#### Manual Path Configuration (if needed):
If Tesseract is not in your PATH, add this to the script:
```python
import pytesseract
pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'
```

### 3. Test Installation
Run the script to test OCR functionality:
```bash
python pdf_catalog_extractor.py
```

## OCR Features

### Automatic Fallback
- **Direct text extraction** tried first (fastest)
- **Layout-based extraction** for complex layouts
- **Block-based extraction** for structured documents
- **OCR extraction** when other methods fail or produce insufficient text

### Image Enhancement
- **High resolution** (3x zoom) for better accuracy
- **Grayscale conversion** for optimal OCR
- **Contrast enhancement** (1.5x)
- **Sharpness enhancement** (1.2x)
- **Noise reduction** with median filter

### Multiple OCR Modes
- **PSM 6**: Uniform block of text (catalogs)
- **PSM 11**: Sparse text, find as much as possible
- **PSM 12**: Sparse text with orientation detection
- **PSM 8**: Single word recognition
- **PSM 13**: Raw line treatment

## Current Test Results

### Woolworths Catalog (https://d3vvi2v9oj75wh.cloudfront.net/uploads/pdf/WW_QLD_230725_W9L66PJK3.pdf)
- **Status**: Image-based PDF detected ✅
- **Pages**: 52 pages, all requiring OCR
- **Image format**: DeviceRGB, 960x1509 pixels
- **OCR ready**: Waiting for Tesseract installation

## Usage Examples

### Basic Usage
```bash
# Process any PDF (auto-detects if OCR is needed)
python pdf_catalog_extractor.py "https://example.com/catalog.pdf"

# Process local file
python pdf_catalog_extractor.py "catalog.pdf"

# Extract structured product information
python pdf_catalog_extractor.py "catalog.pdf" --structured

# Show preview of extracted text
python pdf_catalog_extractor.py "catalog.pdf" --preview
```

### Advanced Usage
```bash
# Custom output file
python pdf_catalog_extractor.py "catalog.pdf" -o "my_output.txt"

# Extract products and show preview
python pdf_catalog_extractor.py "catalog.pdf" --structured --preview
```

## Troubleshooting

### OCR Not Working
1. **Check Tesseract installation**: Run `tesseract --version` in command prompt
2. **Check PATH**: Ensure tesseract.exe is in system PATH
3. **Manual path setup**: Add pytesseract path configuration to script
4. **Permissions**: Ensure user has permissions to run tesseract.exe

### Poor OCR Quality
1. **Image quality**: Higher resolution images work better
2. **Font types**: Simple, clear fonts work best
3. **Image preprocessing**: The script automatically enhances images
4. **OCR mode**: Script tries multiple PSM modes automatically

## Next Steps

Once Tesseract is installed, the script will:
1. ✅ Automatically detect image-based PDFs
2. ✅ Apply image enhancement for better OCR
3. ✅ Try multiple OCR configurations
4. ✅ Extract structured product information
5. ✅ Generate JSON data for price comparison

## Technical Notes

- **Memory usage**: OCR processing uses more RAM than direct text extraction
- **Processing time**: OCR is slower but necessary for image-based PDFs
- **Accuracy**: Results depend on image quality and font clarity
- **Language**: Currently configured for English text recognition