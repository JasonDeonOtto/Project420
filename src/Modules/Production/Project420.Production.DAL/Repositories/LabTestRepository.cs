using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository implementation for LabTest entity operations.
/// SAHPRA Compliance: Laboratory testing and COA management.
/// </summary>
public class LabTestRepository : Repository<LabTest>, ILabTestRepository
{
    public LabTestRepository(ProductionDbContext context) : base(context)
    {
    }
}
