using AdvGenPriceComparer.Application.DTOs;

namespace AdvGenPriceComparer.Application.Interfaces
{
    /// <summary>
    /// Use case interface for exporting grocery data to various formats.
    /// This is part of the Clean Architecture Application layer.
    /// </summary>
    public interface IExportUseCase
    {
        /// <summary>
        /// Export all data to JSON format
        /// </summary>
        /// <param name="options">Export options (what to include, date filters)</param>
        /// <param name="outputPath">Output file path</param>
        /// <param name="progress">Progress reporter</param>
        /// <returns>Export result with file path and statistics</returns>
        Task<ExportResultDto> ExportToJsonAsync(
            ExportOptionsDto options, 
            string outputPath,
            IProgress<ExportProgressDto>? progress = null);

        /// <summary>
        /// Export data to compressed JSON format (.json.gz)
        /// </summary>
        /// <param name="options">Export options (what to include, date filters)</param>
        /// <param name="outputPath">Output file path</param>
        /// <param name="progress">Progress reporter</param>
        /// <returns>Export result with file path and statistics</returns>
        Task<ExportResultDto> ExportToJsonGzAsync(
            ExportOptionsDto options, 
            string outputPath,
            IProgress<ExportProgressDto>? progress = null);

        /// <summary>
        /// Incremental export - only items changed since last export
        /// </summary>
        /// <param name="lastExportDate">Date of last export</param>
        /// <param name="outputPath">Output file path</param>
        /// <param name="progress">Progress reporter</param>
        /// <returns>Export result with file path and statistics</returns>
        Task<ExportResultDto> IncrementalExportAsync(
            DateTime lastExportDate, 
            string outputPath,
            IProgress<ExportProgressDto>? progress = null);

        /// <summary>
        /// Export stores to Shop.json format
        /// </summary>
        /// <param name="filePath">Output file path</param>
        Task ExportShopsAsync(string filePath);

        /// <summary>
        /// Export items to Goods.json format
        /// </summary>
        /// <param name="filePath">Output file path</param>
        Task ExportGoodsAsync(string filePath);

        /// <summary>
        /// Export price records to price-{timestamp}.json format
        /// </summary>
        /// <param name="filePath">Output file path</param>
        /// <param name="fromDate">Optional start date filter</param>
        /// <param name="toDate">Optional end date filter</param>
        Task ExportPricesAsync(string filePath, DateTime? fromDate, DateTime? toDate);
    }
}
