using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository implementation for ProcessingStep entity operations.
/// GMP Compliance: Processing workflow documentation.
/// </summary>
public class ProcessingStepRepository : Repository<ProcessingStep>, IProcessingStepRepository
{
    public ProcessingStepRepository(ProductionDbContext context) : base(context)
    {
    }
}
