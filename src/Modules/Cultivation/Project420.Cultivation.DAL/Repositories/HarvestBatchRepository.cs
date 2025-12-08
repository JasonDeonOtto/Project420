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
}
