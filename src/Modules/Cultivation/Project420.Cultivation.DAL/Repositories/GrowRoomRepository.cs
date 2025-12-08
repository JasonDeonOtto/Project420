using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository implementation for GrowRoom entity operations.
/// GMP Compliance: Facility and environment tracking.
/// </summary>
public class GrowRoomRepository : Repository<GrowRoom>, IGrowRoomRepository
{
    public GrowRoomRepository(CultivationDbContext context) : base(context)
    {
    }
}
