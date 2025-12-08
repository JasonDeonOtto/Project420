using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;

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
}
