using Microsoft.EntityFrameworkCore;
using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;
using Project420.Production.Models.Enums;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository implementation for ProductionBatch entity operations.
/// SAHPRA GMP Compliance: Production batch tracking from harvest to retail.
/// </summary>
public class ProductionBatchRepository : Repository<ProductionBatch>, IProductionBatchRepository
{
    

    public ProductionBatchRepository(ProductionDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<ProductionBatch>> GetByIsActiveAsync()
    {
        return await _dbSet
        .Where(b => b.IsActive)
        .OrderByDescending(b => b.StartDate)
        .ToListAsync();
    }

    public async Task<ProductionBatch?> GetByProductionBatchNumberAsync(string batchNumber) //how do we determine between ProductionBatchNumber and harvest batch and production batch etc
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(b => b.BatchNumber == batchNumber);
    }

    public async Task<IEnumerable<ProductionBatch>> GetByHarvestBatchNumberAsync(string harvestBatchNumber)
    {
        return await _dbSet
        .Where(b => b.HarvestBatchNumber == harvestBatchNumber)
        .OrderByDescending(b => b.StartDate)
        .ToListAsync();
    }


    public async Task<IEnumerable<ProductionBatch?>> GetByStrainAsync(string strainName)
    {
        return await _dbSet
        .Where(b => b.StrainName == strainName)
        .OrderByDescending(b => b.StartDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<ProductionBatch>> GetByQcPassedAsync(bool qcPassed)
    {
        return await _dbSet
        .Where(b => b.QualityControlPassed == qcPassed)
        .OrderByDescending(b => b.StartDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<ProductionBatch>> GetByLabTestPassedAsync(bool labTestPassed)
    {
        return await _dbSet
        .Where(b => b.LabTestPassed == labTestPassed)
        .OrderByDescending(b => b.StartDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<ProductionBatch>> GetByInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.StartDate >= startDate && b.StartDate <= endDate)
            .OrderBy(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionBatch>> GetByIdWithResultsDataAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive
                && b.QualityControlPassed == true
                && b.LabTestPassed == true)
            .OrderBy(b => b.StartDate) // Oldest dried batches first
            .ToListAsync();
    }

    public async Task<ProductionBatch?> GetByIdWithPackagingDataAsync(int id)
    {
        // Note: Packaging data (UnitsPackaged, PackageSize, PackagingDate) are scalar properties
        // and are always loaded automatically. Include is only for navigation properties.
        // Return entity with related ProcessingSteps for packaging workflow context.
        return await _dbSet
            .Include(b => b.ProcessingSteps.Where(s => s.StepType == ProcessingStepType.Packaging))
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    /// <summary>
    /// Get production batch with all related data (ProcessingSteps, QC, LabTests).
    /// </summary>
    public async Task<ProductionBatch?> GetByIdWithAllRelatedDataAsync(int id)
    {
        return await _dbSet
            .Include(b => b.ProcessingSteps)
            .Include(b => b.QualityControls)
            .Include(b => b.LabTests)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    /// <summary>
    /// Get production batches by date range.
    /// </summary>
    public async Task<IEnumerable<ProductionBatch>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.StartDate >= startDate && b.StartDate <= endDate)
            .OrderBy(b => b.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get active production batches (currently in process).
    /// </summary>
    public async Task<IEnumerable<ProductionBatch>> GetActiveBatchesAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive &&
                       b.ProductionBatchStatus != ProductionBatchStatus.Completed &&
                       b.ProductionBatchStatus != ProductionBatchStatus.Cancelled)
            .OrderBy(b => b.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get batches needing QC (not yet QC tested).
    /// </summary>
    public async Task<IEnumerable<ProductionBatch>> GetBatchesNeedingQCAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive && b.QualityControlPassed == null)
            .OrderBy(b => b.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get batches needing lab testing (QC passed but no lab test).
    /// </summary>
    public async Task<IEnumerable<ProductionBatch>> GetBatchesNeedingLabTestAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive &&
                       b.QualityControlPassed == true &&
                       b.LabTestPassed == null)
            .OrderBy(b => b.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get completed batches in date range (for reporting).
    /// </summary>
    public async Task<IEnumerable<ProductionBatch>> GetCompletedBatchesAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.ProductionBatchStatus == ProductionBatchStatus.Completed &&
                       b.CompletionDate.HasValue &&
                       b.CompletionDate.Value >= startDate &&
                       b.CompletionDate.Value <= endDate)
            .Include(b => b.ProcessingSteps)
            .OrderBy(b => b.CompletionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get batches by product type (pre-roll, flower, etc.).
    /// </summary>
    public async Task<IEnumerable<ProductionBatch>> GetByProductTypeAsync(string productType)
    {
        // ProductType can be inferred from PackageSize or Name
        // e.g., "Pre-Roll", "3.5g", "7g", "28g"
        return await _dbSet
            .Where(b => (b.PackageSize != null && b.PackageSize.Contains(productType)) ||
                       (b.Name != null && b.Name.Contains(productType)))
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionBatch>> GetByStatusAsync(int batchStatus)
    {
        // Cast int to ProductionBatchStatus enum for comparison
        var status = (ProductionBatchStatus)batchStatus;

        return await _dbSet
            .Where(b => b.ProductionBatchStatus == status)
            .OrderBy(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<ProductionBatch?> GetByIdWithProcessingStepDataAsync(int id)
    {
        return await _dbSet
            .Include(b => b.ProcessingSteps)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<ProductionBatch>> GetByFailedBatchesAsync()
    {
        return await _dbSet
        .Where(b => b.LabTestPassed == false || b.QualityControlPassed == false)  
        .OrderByDescending(b => b.StartDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<ProductionBatch>> GetByReadyForInventoryAsync()
    {
        //ProductionBatchStatus batchStatus = ProductionBatchStatus.Completed; //must be a better way than this? initialise higher up? update in 

        return await _dbSet
        .Where(b => b.ProductionBatchStatus == ProductionBatchStatus.Completed) //Link to Project420.Production.Models.Enums.ProductionBatchStatus 4 completed
        .OrderByDescending(b => b.StartDate)
        .ToListAsync();
    }

    public async Task<bool> IsBatchNumberUniqueAsync(string batchNumber, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return false;

        var query = _dbSet.Where(b => b.BatchNumber == batchNumber);

        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }


}
