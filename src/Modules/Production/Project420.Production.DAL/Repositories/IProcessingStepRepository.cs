using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository interface for ProcessingStep entity operations.
/// GMP Compliance: Processing workflow documentation.
/// </summary>
public interface IProcessingStepRepository : IRepository<ProcessingStep>
{
    Task<IEnumerable<ProcessingStep>> GetByStatusAsync(string status);
    Task<ProcessingStep> GetByProductionBatchAsync(string productionBatch);
    Task<IEnumerable<ProcessingStep>> GetByStepTypeAsync(string stepNumber);
    Task<IEnumerable<ProcessingStep>> GetByStartTimeFromAsync(TimeOnly timeFrom);

    Task<IEnumerable<ProcessingStep>> GetByDurationHoursAsync(int durationHours);
    Task<ProcessingStep?> GetByIdWithEnvironmentalConditionDataAsync(int id);
    Task<ProcessingStep?> GetByIdWithStatusDataAsync(int id);


}
