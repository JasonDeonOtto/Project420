using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository implementation for QualityControl entity operations.
/// GMP Compliance: Quality control checks and validations.
/// </summary>
public class QualityControlRepository : Repository<QualityControl>, IQualityControlRepository
{
    public QualityControlRepository(ProductionDbContext context) : base(context)
    {
    }
}
