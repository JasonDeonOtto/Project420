using Microsoft.EntityFrameworkCore;
using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository implementation for LabTest entity operations.
/// SAHPRA Compliance: Laboratory testing and COA management.
/// </summary>
public class LabTestRepository : Repository<LabTest>, ILabTestRepository
{
    private readonly ProductionDbContext _context;
    public LabTestRepository(ProductionDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

     public async Task<IEnumerable<LabTest>> GetByLabNameAsync(string labName)
    {
     return await _dbSet
    .Where(l => l.LabName == labName)
    .OrderByDescending(l => l.ResultsDate)
    .ToListAsync();
    }

    public async Task<IEnumerable<LabTest>> GetByLabCertificateNumberAsync(string labCertificateNumber)
    {
        return await _dbSet
       .Where(l => l.LabCertificateNumber == labCertificateNumber)
       .OrderByDescending(l => l.ResultsDate)
       .ToListAsync();

    }

    public async Task<IEnumerable<LabTest>> GetByCOANumberAsync(string coaNumber)
    {
        return await _dbSet
       .Where(l => l.COANumber == coaNumber)
       .OrderByDescending(l => l.ResultsDate)
       .ToListAsync();

    }

    public async Task<IEnumerable<LabTest>> GetByResultsDateFromAsync(DateOnly dateFrom)
    {
        DateTime timeaddedDate = dateFrom.ToDateTime(new TimeOnly(00, 00, 0)); //we can reuse this when a user only needs to enter a date but we tracking datetime (easier  for user, more accurate for us?)


        return await _dbSet
        .Where(l => l.ResultsDate >= timeaddedDate)
        .OrderByDescending(l => l.ResultsDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<LabTest>> GetBySampleDateFromAsync(DateOnly dateFrom)
    {
        DateTime timeaddedDate = dateFrom.ToDateTime(new TimeOnly(00, 00, 0)); //we can reuse this when a user only needs to enter a date but we tracking datetime (easier  for user, more accurate for us?)


        return await _dbSet
        .Where(l => l.SampleDate >= timeaddedDate)
        .OrderByDescending(l => l.SampleDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<LabTest>> GetByOverallPassAsync(bool overallPassed)
    {
        return await _dbSet
        .Where(l => l.OverallPass == overallPassed)
        .OrderByDescending(l => l.ResultsDate)
        .ToListAsync();
    }

    public async Task<LabTest?> GetByIdWithContaminantResultsAsync(int batchId)
    {
        return await _context.LabTests
        .Include(l => l.MicrobialPassed)
        .Include(l => l.HeavyMetalsPassed)
        .Include(l => l.PesticidesPassed)
        .Include(l => l.OverallPass)
        .Include(l => l.FailureDetails)
        .Include(l => l.ResultsDate)
        .Include(l => l.SampleDate)
        .FirstOrDefaultAsync(l => l.ProductionBatchId == batchId);
    }

    public async Task<LabTest?> GetByIdWithPotencyResultsAsync(int batchId)
    {
        return await _context.LabTests
        .Include(l => l.THCPercentage)
        .Include(l => l.CBDPercentage)
        .Include(l => l.TotalCannabinoidsPercentage)
        .Include(l => l.ResultsDate)
        .Include(l => l.SampleDate)
        .FirstOrDefaultAsync(l => l.ProductionBatchId == batchId);
    }
}
