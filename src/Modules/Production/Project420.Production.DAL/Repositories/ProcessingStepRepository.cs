using Microsoft.EntityFrameworkCore;
using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository implementation for ProcessingStep entity operations.
/// GMP Compliance: Processing workflow documentation.
/// </summary>
public class ProcessingStepRepository : Repository<ProcessingStep>, IProcessingStepRepository
{
    private readonly ProductionDbContext _context;
    public ProcessingStepRepository(ProductionDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<ProcessingStep>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .Include(s => s.ProductionBatch)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<ProcessingStep?> GetByProductionBatchAsync(string productionBatchNumber)
    {
        return await _dbSet
            .Include(s => s.ProductionBatch)
            .FirstOrDefaultAsync(s => s.ProductionBatch.BatchNumber == productionBatchNumber);
    }

    public async Task<IEnumerable<ProcessingStep>> GetByStepTypeAsync(string stepNumber)
    {
        // Try to parse stepNumber as int for proper comparison
        if (!int.TryParse(stepNumber, out int stepNum))
        {
            return Enumerable.Empty<ProcessingStep>();
        }

        return await _dbSet
            .Include(s => s.ProductionBatch)
            .Where(s => s.StepNumber == stepNum)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProcessingStep>> GetByStartTimeFromAsync(TimeOnly timeFrom)
    {
        // Convert TimeOnly to a DateTime for comparison (using today's date as base)
        var today = DateTime.UtcNow.Date;
        var dateTimeFrom = today.Add(timeFrom.ToTimeSpan());

        return await _dbSet
            .Where(s => s.StartTime >= dateTimeFrom)
            .Include(s => s.ProductionBatch)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProcessingStep>> GetByDurationHoursAsync(int durationHours)
    {
        return await _dbSet
            .Where(s => s.DurationHours >= durationHours)
            .Include(s => s.ProductionBatch)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<ProcessingStep?> GetByIdWithWeightDataAsync(int id)
    {
        return await _dbSet
            .Include(s => s.ProductionBatch)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ProcessingStep?> GetByIdWithEnvironmentalConditionDataAsync(int id)
    {
        return await _dbSet
            .Include(s => s.ProductionBatch)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ProcessingStep?> GetByIdWithStatusDataAsync(int id)
    {
        return await _dbSet
            .Include(s => s.ProductionBatch)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
