@echo off
echo Installing Tesseract OCR for Windows...
echo.

echo Step 1: Install pytesseract Python package
pip install pytesseract

echo.
echo Step 2: Download and install Tesseract engine
echo Please download and install Tesseract from:
echo https://github.com/UB-Mannheim/tesseract/wiki
echo.
echo For Windows, use the installer from:
echo https://github.com/tesseract-ocr/tesseract/releases
echo.
echo After installation, make sure tesseract.exe is in your PATH
echo or set the pytesseract path in your script:
echo pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'

pause