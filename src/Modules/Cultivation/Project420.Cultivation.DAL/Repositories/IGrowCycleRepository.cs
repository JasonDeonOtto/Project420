using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository interface for GrowCycle entity operations.
/// SAHPRA/DALRRD Compliance: Cultivation cycle tracking for reporting.
/// </summary>
public interface IGrowCycleRepository : IRepository<GrowCycle>
{
    /// <summary>
    /// Gets all active grow cycles (IsActive = true).
    /// </summary>
    /// <returns>Collection of active grow cycles</returns>
    /// <remarks>
    /// SAHPRA/DALRRD: Only active cycles should appear in production planning.
    /// </remarks>
    Task<IEnumerable<GrowCycle>> GetActiveCyclesAsync();

    /// <summary>
    /// Gets a grow cycle by its unique cycle code.
    /// </summary>
    /// <param name="cycleCode">Cycle code identifier (e.g., "GC-2024-001")</param>
    /// <returns>GrowCycle if found, null otherwise</returns>
    /// <remarks>
    /// Cycle codes are typically unique identifiers for compliance tracking.
    /// </remarks>
    Task<GrowCycle?> GetByCycleCodeAsync(string cycleCode);

    /// <summary>
    /// Gets grow cycles filtered by strain name.
    /// </summary>
    /// <param name="strainName">Cannabis strain name</param>
    /// <returns>Collection of cycles for the specified strain</returns>
    /// <remarks>
    /// Used for strain-specific yield analysis and genetics tracking.
    /// </remarks>
    Task<IEnumerable<GrowCycle>> GetByStrainNameAsync(string strainName);

    /// <summary>
    /// Gets a grow cycle by ID with related data eagerly loaded.
    /// </summary>
    /// <param name="id">GrowCycle primary key</param>
    /// <returns>GrowCycle with Plants, HarvestBatches, and GrowRoom navigation properties loaded</returns>
    /// <remarks>
    /// Includes: Plants, HarvestBatches, GrowRoom
    /// Use for detailed cycle view with complete traceability.
    /// </remarks>
    Task<GrowCycle?> GetByIdWithRelatedDataAsync(int id);

    /// <summary>
    /// Gets grow cycles within a specific date range (by start date).
    /// </summary>
    /// <param name="startDate">Range start date</param>
    /// <param name="endDate">Range end date</param>
    /// <returns>Collection of cycles that started within the date range</returns>
    /// <remarks>
    /// SAHPRA Reporting: Used for periodic cultivation reports.
    /// </remarks>
    Task<IEnumerable<GrowCycle>> GetCyclesInDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets grow cycles ready for harvest (flowering complete, not yet harvested).
    /// </summary>
    /// <param name="plannedDate">Optional: Filter by planned harvest date</param>
    /// <returns>Collection of cycles ready for harvest</returns>
    /// <remarks>
    /// SAHPRA/GMP: Used for harvest planning and scheduling.
    /// Returns cycles where PlannedHarvestDate is due and ActualHarvestDate is null.
    /// </remarks>
    Task<IEnumerable<GrowCycle>> GetCyclesForHarvestAsync(DateTime? plannedDate = null);

    /// <summary>
    /// Checks if a cycle code is unique (not used by another cycle).
    /// </summary>
    /// <param name="cycleCode">Cycle code to validate</param>
    /// <param name="excludeId">Optional: Exclude this cycle ID from check (for updates)</param>
    /// <returns>True if unique, false if already exists</returns>
    /// <remarks>
    /// Used for validation before creating/updating cycles.
    /// </remarks>
    Task<bool> IsCycleCodeUniqueAsync(string cycleCode, int? excludeId = null);
}
