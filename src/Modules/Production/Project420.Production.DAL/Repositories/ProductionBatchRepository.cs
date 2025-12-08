using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

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
}
