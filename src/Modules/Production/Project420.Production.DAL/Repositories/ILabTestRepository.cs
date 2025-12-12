using Project420.Production.DAL.Repositories.Common;
using Project420.Production.Models.Entities;

namespace Project420.Production.DAL.Repositories;

/// <summary>
/// Repository interface for LabTest entity operations.
/// SAHPRA Compliance: Laboratory testing and COA management.
/// </summary>
public interface ILabTestRepository : IRepository<LabTest>
{

    Task<IEnumerable<LabTest>> GetByLabNameAsync(string labName);
    Task<IEnumerable<LabTest>> GetByLabCertificateNumberAsync(string labCertificateNumber);
    Task<IEnumerable<LabTest>> GetByCOANumberAsync(string coaNumber);
    Task<IEnumerable<LabTest>> GetByResultsDateFromAsync(DateOnly dateFrom);
    Task<IEnumerable<LabTest>> GetBySampleDateFromAsync(DateOnly dateFrom);
    Task<IEnumerable<LabTest>> GetByOverallPassAsync(bool overallPassed);
    Task<LabTest?> GetByIdWithContaminantResultsAsync(int batchId);
    Task<LabTest?> GetByIdWithPotencyResultsAsync(int batchId);

}
