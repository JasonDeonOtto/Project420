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
}
