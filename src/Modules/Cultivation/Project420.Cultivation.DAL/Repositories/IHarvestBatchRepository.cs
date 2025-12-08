using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository interface for HarvestBatch entity operations.
/// SAHPRA Compliance: Batch tracking for seed-to-sale traceability.
/// </summary>
public interface IHarvestBatchRepository : IRepository<HarvestBatch>
{
}
