using Project420.Production.BLL.DTOs;
using Project420.Production.Models.Enums;

namespace Project420.Production.BLL.Services;

/// <summary>
/// Service for retail production workflows (pre-rolls, packaged flower).
/// </summary>
/// <remarks>
/// Phase 10: Production DAL Expansion - Retail Production Service
///
/// Key Features:
/// - Pre-roll production workflow (milling → filling → capping → packaging)
/// - Packaged flower production workflow
/// - Integration with MovementService (ProductionInput/ProductionOutput)
/// - Batch number generation (via BatchNumberGeneratorService)
/// - Serial number generation on packaging (via SerialNumberGeneratorService)
/// - Yield tracking and waste documentation
///
/// SAHPRA GMP Compliance:
/// - All processing steps documented
/// - Weight tracking at each step
/// - Environmental conditions recorded
/// - Full traceability from source batch to packaged units
/// </remarks>
public interface IRetailProductionService
{
    // ============================================================
    // PRE-ROLL PRODUCTION WORKFLOW
    // ============================================================

    /// <summary>
    /// Start a pre-roll production batch.
    /// Creates production batch, generates batch number, creates initial ProcessingInput movement.
    /// </summary>
    /// <param name="dto">Pre-roll production parameters.</param>
    /// <returns>Result with batch ID and batch number.</returns>
    Task<ProductionBatchResultDto> StartPreRollProductionAsync(StartPreRollProductionDto dto);

    /// <summary>
    /// Record completion of the milling step (grinding flower for pre-rolls).
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="dto">Step completion details.</param>
    /// <returns>Step result with yield information.</returns>
    Task<ProcessingStepResultDto> CompleteMillingStepAsync(int batchId, CompleteStepDto dto);

    /// <summary>
    /// Record completion of the filling step (loading cones).
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="dto">Step completion details (quantity filled).</param>
    /// <returns>Step result with yield information.</returns>
    Task<ProcessingStepResultDto> CompleteFillingStepAsync(int batchId, CompleteStepDto dto);

    /// <summary>
    /// Record completion of the capping step (closing pre-rolls).
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="dto">Step completion details.</param>
    /// <returns>Step result with yield information.</returns>
    Task<ProcessingStepResultDto> CompleteCappingStepAsync(int batchId, CompleteStepDto dto);

    // ============================================================
    // PACKAGED FLOWER PRODUCTION WORKFLOW
    // ============================================================

    /// <summary>
    /// Start a packaged flower production batch.
    /// Creates production batch for flower packaging (3.5g, 7g, etc.).
    /// </summary>
    /// <param name="dto">Production parameters.</param>
    /// <returns>Result with batch ID and batch number.</returns>
    Task<ProductionBatchResultDto> StartPackagedFlowerProductionAsync(StartPreRollProductionDto dto);

    /// <summary>
    /// Record completion of the selection step (selecting flower for packaging).
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="dto">Step completion details.</param>
    /// <returns>Step result with yield information.</returns>
    Task<ProcessingStepResultDto> CompleteSelectionStepAsync(int batchId, CompleteStepDto dto);

    /// <summary>
    /// Record completion of the weighing step (portioning flower).
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="dto">Step completion details.</param>
    /// <returns>Step result with yield information.</returns>
    Task<ProcessingStepResultDto> CompleteWeighingStepAsync(int batchId, CompleteStepDto dto);

    // ============================================================
    // COMMON PRODUCTION OPERATIONS
    // ============================================================

    /// <summary>
    /// Package production output and generate serial numbers.
    /// Creates ProductionOutput movements and generates serial numbers for each unit.
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="dto">Packaging parameters.</param>
    /// <returns>Packaging result with serial numbers.</returns>
    Task<PackagingResultDto> PackageAndGenerateSerialNumbersAsync(int batchId, PackageProductionDto dto);

    /// <summary>
    /// Record a generic processing step completion.
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="stepType">Type of processing step.</param>
    /// <param name="dto">Step completion details.</param>
    /// <returns>Step result with yield information.</returns>
    Task<ProcessingStepResultDto> CompleteProcessingStepAsync(int batchId, ProcessingStepType stepType, CompleteStepDto dto);

    /// <summary>
    /// Get production batch summary with all steps and current status.
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <returns>Batch summary or null if not found.</returns>
    Task<ProductionBatchSummaryDto?> GetBatchSummaryAsync(int batchId);

    /// <summary>
    /// Get all active production batches (in progress).
    /// </summary>
    /// <returns>List of active batch summaries.</returns>
    Task<IEnumerable<ProductionBatchSummaryDto>> GetActiveBatchesAsync();

    /// <summary>
    /// Get completed batches in date range (for reporting).
    /// </summary>
    /// <param name="startDate">Start date.</param>
    /// <param name="endDate">End date.</param>
    /// <returns>List of completed batch summaries.</returns>
    Task<IEnumerable<ProductionBatchSummaryDto>> GetCompletedBatchesAsync(DateTime startDate, DateTime endDate);

    // ============================================================
    // QUALITY CONTROL & LAB TESTING
    // ============================================================

    /// <summary>
    /// Record quality control check result.
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="passed">Whether QC passed.</param>
    /// <param name="notes">QC notes.</param>
    /// <param name="operatorId">Operator performing QC.</param>
    /// <returns>True if update successful.</returns>
    Task<bool> RecordQualityControlAsync(int batchId, bool passed, string notes, string operatorId);

    /// <summary>
    /// Record lab test result.
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="passed">Whether lab test passed.</param>
    /// <param name="thcPercentage">THC percentage result.</param>
    /// <param name="cbdPercentage">CBD percentage result.</param>
    /// <param name="notes">Lab test notes.</param>
    /// <returns>True if update successful.</returns>
    Task<bool> RecordLabTestAsync(int batchId, bool passed, string thcPercentage, string cbdPercentage, string notes);

    // ============================================================
    // BATCH STATUS MANAGEMENT
    // ============================================================

    /// <summary>
    /// Complete a production batch (all steps done, ready for inventory).
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="operatorId">Operator completing the batch.</param>
    /// <returns>True if completion successful.</returns>
    Task<bool> CompleteBatchAsync(int batchId, string operatorId);

    /// <summary>
    /// Cancel a production batch.
    /// </summary>
    /// <param name="batchId">Production batch ID.</param>
    /// <param name="reason">Cancellation reason.</param>
    /// <param name="operatorId">Operator cancelling the batch.</param>
    /// <returns>True if cancellation successful.</returns>
    Task<bool> CancelBatchAsync(int batchId, string reason, string operatorId);
}
