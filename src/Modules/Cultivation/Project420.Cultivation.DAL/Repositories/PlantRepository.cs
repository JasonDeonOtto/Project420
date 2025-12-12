using Microsoft.EntityFrameworkCore;
using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;
using Project420.Cultivation.Models.Enums;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository implementation for Plant entity operations.
/// SAHPRA Compliance: Individual plant tracking from seed to sale.
/// </summary>
public class PlantRepository : Repository<Plant>, IPlantRepository
{
    public PlantRepository(CultivationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Plant>> GetActivePlantsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.PlantTag)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Plant?> GetByPlantTagAsync(string plantTag)
    {
        if (string.IsNullOrWhiteSpace(plantTag))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(p => p.PlantTag == plantTag);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Plant>> GetByGrowCycleIdAsync(int growCycleId)
    {
        return await _dbSet
            .Where(p => p.GrowCycleId == growCycleId)
            .OrderBy(p => p.PlantTag)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Plant>> GetByGrowRoomIdAsync(int growRoomId)
    {
        return await _dbSet
            .Where(p => p.CurrentGrowRoomId == growRoomId && p.IsActive)
            .OrderBy(p => p.CurrentStage)
            .ThenBy(p => p.PlantTag)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Plant>> GetByStageAsync(PlantStage stage)
    {
        return await _dbSet
            .Where(p => p.CurrentStage == stage && p.IsActive)
            .OrderBy(p => p.StageStartDate)
            .ThenBy(p => p.PlantTag)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Plant?> GetByIdWithRelatedDataAsync(int id)
    {
        return await _dbSet
            .Include(p => p.GrowCycle)
            .Include(p => p.CurrentGrowRoom)
            .Include(p => p.HarvestBatch)
            .Include(p => p.MotherPlant)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Plant>> GetPlantsReadyForHarvestAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive
                && p.CurrentStage == PlantStage.Flowering
                && p.HarvestDate == null)
            .OrderBy(p => p.StageStartDate) // Oldest flowering plants first
            .ThenBy(p => p.PlantTag)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Plant>> GetPlantsForDestructionAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive
                && p.DestroyedDate == null
                && (p.PlantSex == "Male" || p.PlantSex == "Hermaphrodite"))
            .OrderBy(p => p.PlantTag)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> IsPlantTagUniqueAsync(string plantTag, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(plantTag))
            return false;

        var query = _dbSet.Where(p => p.PlantTag == plantTag);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Plant>> GetPlantsByStrainAsync(string strainName)
    {
        if (string.IsNullOrWhiteSpace(strainName))
            return Enumerable.Empty<Plant>();

        return await _dbSet
            .Where(p => p.StrainName == strainName)
            .OrderBy(p => p.PlantedDate)
            .ThenBy(p => p.PlantTag)
            .ToListAsync();
    }
}
