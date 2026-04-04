import re

file_path = "AdvGenPriceComparer.Server/Services/PriceDataService.cs"
with open(file_path, "r") as f:
    content = f.read()

# Add ILogger field and update constructor
content = content.replace(
    '    private readonly INotificationService _notificationService;',
    '    private readonly INotificationService _notificationService;\n    private readonly ILogger<PriceDataService> _logger;'
)

content = content.replace(
    '    public PriceDataService(PriceDataContext context, INotificationService notificationService)\n    {\n        _context = context;\n        _notificationService = notificationService;\n    }',
    '    public PriceDataService(PriceDataContext context, INotificationService notificationService, ILogger<PriceDataService> logger)\n    {\n        _context = context;\n        _notificationService = notificationService;\n        _logger = logger;\n    }'
)

# Update the catch block
content = content.replace(
    '        catch (Exception ex)\n        {\n            result.Success = false;\n            result.ErrorMessage = ex.Message;\n            session.IsSuccess = false;\n            session.ErrorMessage = ex.Message;\n            await _context.UploadSessions.AddAsync(session);\n            await _context.SaveChangesAsync();\n        }',
    '        catch (Exception ex)\n        {\n            _logger.LogError(ex, "Error processing upload data");\n            result.Success = false;\n            result.ErrorMessage = "An internal server error occurred while processing the upload data.";\n            session.IsSuccess = false;\n            session.ErrorMessage = "An internal server error occurred.";\n            await _context.UploadSessions.AddAsync(session);\n            await _context.SaveChangesAsync();\n        }'
)

with open(file_path, "w") as f:
    f.write(content)

print("Patched PriceDataService.cs")
