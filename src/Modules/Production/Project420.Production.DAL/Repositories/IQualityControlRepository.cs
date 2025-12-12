using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository interface for QualityControl entity operations.
/// GMP Compliance: Quality control checks and validations.
/// </summary>
public interface IQualityControlRepository : IRepository<QualityControl>
{
    Task<IEnumerable<QualityControl>> GetByProductionBatchAsync(int productionBatch);

    Task<IEnumerable<QualityControl>> GetByidAsync(int id);

    Task<IEnumerable<QualityControl>> GetByCheckDateFromAsync(DateOnly dateFrom);

    Task<IEnumerable<QualityControl>> GetByInspectorAsync(string inspectorName);

    Task<QualityControl?> GetByIdWithResultsDataAsync(int batchId);

    Task<QualityControl?> GetByIdWithDefectsFoundDataAsync(int batchId);
}
