using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Retail;
using Project420.Management.Models.Entities.Sales.Wholesale;

namespace Project420.Management.DAL.Repositories.Sales.Wholesale
{
    public class WholesalePricelistRepository : Repository<WholesalePricelist>, IWholesalePricelistRepository
    {
        public WholesalePricelistRepository(ManagementDbContext context) : base(context)
        {
        }

        public async Task<WholesalePricelist?> GetByNameAsync(string Name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name == Name);
        }

        public async Task<WholesalePricelist?> GetByCodeAsync(string Code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Code == Code);
        }

        public async Task<WholesalePricelist?> GetByIsActiveAsync(bool IsActive)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.IsActive == IsActive);
        }

    }
}
