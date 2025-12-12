using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository interface for HarvestBatch entity operations.
/// SAHPRA Compliance: Batch tracking for seed-to-sale traceability.
/// </summary>
public interface IHarvestBatchRepository : IRepository<HarvestBatch>
{
    /// <summary>
    /// Gets all active harvest batches (IsActive = true).
    /// </summary>
    /// <returns>Collection of active harvest batches</returns>
    /// <remarks>
    /// SAHPRA: Active batches are in processing or inventory.
    /// </remarks>
    Task<IEnumerable<HarvestBatch>> GetActiveBatchesAsync();

    /// <summary>
    /// Gets a harvest batch by its unique batch number.
    /// </summary>
    /// <param name="batchNumber">Batch number identifier (e.g., "HB-2024-001")</param>
    /// <returns>HarvestBatch if found, null otherwise</returns>
    /// <remarks>
    /// SAHPRA Compliance: Each batch MUST have unique identifier for traceability.
    /// </remarks>
    Task<HarvestBatch?> GetByBatchNumberAsync(string batchNumber);

    /// <summary>
    /// Gets all harvest batches from a specific grow cycle.
    /// </summary>
    /// <param name="growCycleId">GrowCycle primary key</param>
    /// <returns>Collection of harvest batches from the specified cycle</returns>
    /// <remarks>
    /// Links batches back to cultivation cycle for traceability.
    /// </remarks>
    Task<IEnumerable<HarvestBatch>> GetByGrowCycleIdAsync(int growCycleId);

    /// <summary>
    /// Gets harvest batches filtered by strain name.
    /// </summary>
    /// <param name="strainName">Cannabis strain name</param>
    /// <returns>Collection of batches of the specified strain</returns>
    /// <remarks>
    /// Used for strain-specific inventory and quality tracking.
    /// </remarks>
    Task<IEnumerable<HarvestBatch>> GetByStrainNameAsync(string strainName);

    /// <summary>
    /// Gets a harvest batch by ID with related data eagerly loaded.
    /// </summary>
    /// <param name="id">HarvestBatch primary key</param>
    /// <returns>HarvestBatch with GrowCycle and Plants navigation properties loaded</returns>
    /// <remarks>
    /// Includes: GrowCycle, Plants
    /// Use for complete batch traceability view.
    /// </remarks>
    Task<HarvestBatch?> GetByIdWithRelatedDataAsync(int id);

    /// <summary>
    /// Gets harvest batches ready for lab testing (dried but not tested).
    /// </summary>
    /// <returns>Collection of batches ready for lab testing</returns>
    /// <remarks>
    /// SAHPRA Compliance: All batches MUST pass lab testing before sale.
    /// Returns batches with DryDate set but LabTestDate null.
    /// </remarks>
    Task<IEnumerable<HarvestBatch>> GetBatchesReadyForLabTestingAsync();

    /// <summary>
    /// Gets harvest batches filtered by processing status.
    /// </summary>
    /// <param name="status">Processing status (e.g., "Harvested", "Drying", "Curing", "Testing", "Completed")</param>
    /// <returns>Collection of batches with the specified status</returns>
    /// <remarks>
    /// GMP Compliance: Used for workflow tracking and quality control.
    /// </remarks>
    Task<IEnumerable<HarvestBatch>> GetBatchesByProcessingStatusAsync(string status);

    /// <summary>
    /// Gets harvest batches within a specific date range (by harvest date).
    /// </summary>
    /// <param name="startDate">Range start date</param>
    /// <param name="endDate">Range end date</param>
    /// <returns>Collection of batches harvested within the date range</returns>
    /// <remarks>
    /// SAHPRA Reporting: Used for periodic harvest reports.
    /// </remarks>
    Task<IEnumerable<HarvestBatch>> GetBatchesInDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Checks if a batch number is unique (not used by another batch).
    /// </summary>
    /// <param name="batchNumber">Batch number to validate</param>
    /// <param name="excludeId">Optional: Exclude this batch ID from check (for updates)</param>
    /// <returns>True if unique, false if already exists</returns>
    /// <remarks>
    /// SAHPRA: Batch numbers MUST be unique for traceability.
    /// Used for validation before creating/updating batches.
    /// </remarks>
    Task<bool> IsBatchNumberUniqueAsync(string batchNumber, int? excludeId = null);
}
