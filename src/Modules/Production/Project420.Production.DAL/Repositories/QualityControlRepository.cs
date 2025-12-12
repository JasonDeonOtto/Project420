using System.Reflection.PortableExecutable;
using Microsoft.EntityFrameworkCore;
using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Infrastructure.DTOs;

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



    public async Task<IEnumerable<QualityControl>> GetByProductionBatchAsync(int productionBatch)
    {

        return await _dbSet
        .Where(q => q.ProductionBatchId == productionBatch)
        .OrderByDescending(q => q.CheckDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<QualityControl?>> GetByidAsync(int id)
    {
        return await _dbSet
        .Where(q => q.Id == id)
        .OrderByDescending(q => q.CheckDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<QualityControl?>> GetByCheckDateFromAsync(DateOnly dateFrom)
    {
        DateTime cleanDate = dateFrom.ToDateTime(new TimeOnly(00, 00, 0)); //we can reuse this when a user only needs to enter a date but we tracking datetime (easier  for user, more accurate for us?)

        return await _dbSet
    .Where(q => q.CheckDate >= cleanDate)
    .OrderByDescending(q => q.CheckDate)
    .ToListAsync();
    }

    public async Task<IEnumerable<QualityControl?>> GetByInspectorAsync(string inspectorName)
    {
        //Get inspector id from name?
        //Need to set up Inspector tables


        return await _dbSet
        .Where(q => q.Inspector == inspectorName)
        .OrderByDescending(q => q.CheckDate)
        .ToListAsync();
    }

    public async Task<QualityControl?> GetByIdWithResultsDataAsync(int batchId)
    {
        return await _context.QualityControls
        .Include(q => q.Results)
        .Include(q => q.Passed)
        .Include(q => q.Inspector)
        .Include(q => q.CheckDate)
        .FirstOrDefaultAsync(q => q.ProductionBatchId == batchId);
    }

    public async Task<QualityControl?> GetByIdWithDefectsFoundDataAsync(int batchId)
    {
        return await _context.QualityControls
    .Include(q => q.DefectsFound)
    .Include(q => q.CorrectiveActions)
    .Include(q => q.Inspector)
    .Include(q => q.CheckDate)
    .FirstOrDefaultAsync(q => q.ProductionBatchId == batchId);
    }

}

