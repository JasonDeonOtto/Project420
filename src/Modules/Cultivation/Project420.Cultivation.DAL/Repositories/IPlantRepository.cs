using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository interface for Plant entity operations.
/// SAHPRA Compliance: Individual plant tracking from seed to sale.
/// </summary>
public interface IPlantRepository : IRepository<Plant>
{
}
