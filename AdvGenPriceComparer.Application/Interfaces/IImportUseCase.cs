using AdvGenPriceComparer.Application.DTOs;

namespace AdvGenPriceComparer.Application.Interfaces
{
    /// <summary>
    /// Use case interface for importing grocery data from various sources.
    /// This is part of the Clean Architecture Application layer.
    /// </summary>
    public interface IImportUseCase
    {
        /// <summary>
        /// Import data from a JSON file (Coles/Woolworths format)
        /// </summary>
        /// <param name="filePath">Path to the JSON file</param>
        /// <param name="storeId">Optional store ID to associate with imported items</param>
        /// <param name="options">Import options</param>
        /// <param name="progress">Progress reporter</param>
        /// <returns>Import result with statistics and any errors</returns>
        Task<ImportResultDto> ImportFromJsonAsync(
            string filePath, 
            string storeId, 
            ImportOptionsDto options,
            IProgress<ImportProgressDto>? progress = null);

        /// <summary>
        /// Import data from a markdown file (Drakes format)
        /// </summary>
        /// <param name="filePath">Path to the markdown file</param>
        /// <param name="storeId">Optional store ID to associate with imported items</param>
        /// <param name="options">Import options</param>
        /// <param name="progress">Progress reporter</param>
        /// <returns>Import result with statistics and any errors</returns>
        Task<ImportResultDto> ImportFromMarkdownAsync(
            string filePath, 
            string storeId, 
            ImportOptionsDto options,
            IProgress<ImportProgressDto>? progress = null);

        /// <summary>
        /// Preview import without saving to database
        /// </summary>
        /// <param name="filePath">Path to the file to preview</param>
        /// <returns>Preview of items that would be imported</returns>
        Task<IReadOnlyList<ImportPreviewItemDto>> PreviewImportAsync(string filePath);

        /// <summary>
        /// Bulk import multiple files
        /// </summary>
        /// <param name="filePaths">Array of file paths to import</param>
        /// <param name="storeId">Optional store ID to associate with imported items</param>
        /// <param name="options">Import options</param>
        /// <param name="progress">Progress reporter</param>
        /// <returns>Combined import result</returns>
        Task<ImportResultDto> BulkImportAsync(
            string[] filePaths, 
            string storeId, 
            ImportOptionsDto options,
            IProgress<ImportProgressDto>? progress = null);
    }
}
