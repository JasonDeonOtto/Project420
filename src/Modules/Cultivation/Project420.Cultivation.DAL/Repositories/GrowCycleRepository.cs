using Microsoft.EntityFrameworkCore;
using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository implementation for GrowCycle entity operations.
/// SAHPRA/DALRRD Compliance: Cultivation cycle tracking for reporting.
/// </summary>
public class GrowCycleRepository : Repository<GrowCycle>, IGrowCycleRepository
{
    public GrowCycleRepository(CultivationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GrowCycle>> GetActiveCyclesAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<GrowCycle?> GetByCycleCodeAsync(string cycleCode)
    {
        if (string.IsNullOrWhiteSpace(cycleCode))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(c => c.CycleCode == cycleCode);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GrowCycle>> GetByStrainNameAsync(string strainName)
    {
        if (string.IsNullOrWhiteSpace(strainName))
            return Enumerable.Empty<GrowCycle>();

        return await _dbSet
            .Where(c => c.StrainName == strainName)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<GrowCycle?> GetByIdWithRelatedDataAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Plants)
            .Include(c => c.HarvestBatches)
            .Include(c => c.GrowRoom)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GrowCycle>> GetCyclesInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(c => c.StartDate >= startDate && c.StartDate <= endDate)
            .OrderBy(c => c.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GrowCycle>> GetCyclesForHarvestAsync(DateTime? plannedDate = null)
    {
        var query = _dbSet
            .Where(c => c.IsActive && c.ActualHarvestDate == null && c.PlannedHarvestDate.HasValue);

        if (plannedDate.HasValue)
        {
            // Get cycles where planned harvest date is on or before the specified date
            query = query.Where(c => c.PlannedHarvestDate!.Value <= plannedDate.Value);
        }

        return await query
            .OrderBy(c => c.PlannedHarvestDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> IsCycleCodeUniqueAsync(string cycleCode, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(cycleCode))
            return false;

        var query = _dbSet.Where(c => c.CycleCode == cycleCode);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
