import re

file_path = "AdvGenPriceComparer.Server/Controllers/PricesController.cs"
with open(file_path, "r") as f:
    content = f.read()

content = content.replace('ErrorMessage = $"Internal error: {ex.Message}"', 'ErrorMessage = "An internal server error occurred processing the upload."')

with open(file_path, "w") as f:
    f.write(content)

print("Patched PricesController.cs")
