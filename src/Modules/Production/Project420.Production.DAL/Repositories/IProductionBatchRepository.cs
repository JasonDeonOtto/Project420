using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository interface for ProductionBatch entity operations.
/// SAHPRA GMP Compliance: Production batch tracking from harvest to retail.
/// </summary>
public interface IProductionBatchRepository : IRepository<ProductionBatch>
{
    /// <summary>
    /// Gets all active production batches (IsActive = true).
    /// </summary>
    Task<IEnumerable<ProductionBatch>> GetByIsActiveAsync();

    /// <summary>
    /// Gets a production batch by its unique batch number.
    /// </summary>
    Task<ProductionBatch?> GetByProductionBatchNumberAsync(string batchNumber);

    /// <summary>
    /// Gets production batches linked to a specific harvest batch.
    /// </summary>
    Task<IEnumerable<ProductionBatch>> GetByHarvestBatchNumberAsync(string harvestBatchNumber);

    /// <summary>
    /// Gets production batches filtered by status.
    /// </summary>
    Task<IEnumerable<ProductionBatch>> GetByStatusAsync(int batchStatus);

    /// <summary>
    /// Gets batches that have completed lab testing.
    /// </summary>
    Task<IEnumerable<ProductionBatch>> GetByIdWithResultsDataAsync();

    /// <summary>
    /// Gets batches that passed all quality checks and lab testing (ready for inventory).
    /// </summary>
    Task<IEnumerable<ProductionBatch>> GetByReadyForInventoryAsync();

    /// <summary>
    /// Gets batches that failed quality control or lab testing.
    /// </summary>
    Task<IEnumerable<ProductionBatch>> GetByFailedBatchesAsync();

    /// <summary>
    /// Gets a production batch with all related processing steps, QC checks, and lab tests.
    /// </summary>
    Task<ProductionBatch?> GetByIdWithPackagingDataAsync(int id);

    Task<ProductionBatch?> GetByIdWithProcessingStepDataAsync(int id);

    /// <summary>
    /// Gets batches filtered by strain name.
    /// </summary>
    Task<IEnumerable<ProductionBatch>> GetByStrainAsync(string strainName);

    /// <summary>
    /// Checks if a batch number is unique.
    /// </summary>
    Task<bool> IsBatchNumberUniqueAsync(string batchNumber, int? excludeId = null);


}
