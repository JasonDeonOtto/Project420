using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Common;

namespace Project420.Management.DAL.Repositories.Sales.SalesCommon
{
    public class DebtorCategoryRepository : Repository<DebtorCategory>, IDebtorCategoryRepository
    {
        public DebtorCategoryRepository(ManagementDbContext context) : base(context)
        {
        }

        public async Task<DebtorCategory?> GetByNameAsync(string Name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name == Name);
        }

        public async Task<DebtorCategory?> GetByIsActiveAsync(bool IsActive)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.IsActive == IsActive);
        }
    }
}
