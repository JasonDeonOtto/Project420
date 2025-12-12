using Microsoft.EntityFrameworkCore;
using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository implementation for HarvestBatch entity operations.
/// SAHPRA Compliance: Batch tracking for seed-to-sale traceability.
/// </summary>
public class HarvestBatchRepository : Repository<HarvestBatch>, IHarvestBatchRepository
{
    public HarvestBatchRepository(CultivationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<HarvestBatch>> GetActiveBatchesAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.HarvestDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<HarvestBatch?> GetByBatchNumberAsync(string batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(b => b.BatchNumber == batchNumber);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<HarvestBatch>> GetByGrowCycleIdAsync(int growCycleId)
    {
        return await _dbSet
            .Where(b => b.GrowCycleId == growCycleId)
            .OrderByDescending(b => b.HarvestDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<HarvestBatch>> GetByStrainNameAsync(string strainName)
    {
        if (string.IsNullOrWhiteSpace(strainName))
            return Enumerable.Empty<HarvestBatch>();

        return await _dbSet
            .Where(b => b.StrainName == strainName)
            .OrderByDescending(b => b.HarvestDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<HarvestBatch?> GetByIdWithRelatedDataAsync(int id)
    {
        return await _dbSet
            .Include(b => b.GrowCycle)
            .Include(b => b.Plants)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<HarvestBatch>> GetBatchesReadyForLabTestingAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive
                && b.DryDate.HasValue
                && b.LabTestDate == null)
            .OrderBy(b => b.DryDate) // Oldest dried batches first
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<HarvestBatch>> GetBatchesByProcessingStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return Enumerable.Empty<HarvestBatch>();

        return await _dbSet
            .Where(b => b.ProcessingStatus == status && b.IsActive)
            .OrderByDescending(b => b.HarvestDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<HarvestBatch>> GetBatchesInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.HarvestDate >= startDate && b.HarvestDate <= endDate)
            .OrderBy(b => b.HarvestDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
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
