using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Retail;

namespace Project420.Management.DAL.Repositories.Sales.Retail
{
    public class RetailPricelistRepository : Repository<RetailPricelist>, IRetailPricelistRepository
    {
        public RetailPricelistRepository(ManagementDbContext context) : base(context)
        {
        }

        public async Task<RetailPricelist?> GetByNameAsync(string Name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name == Name);
        }

        public async Task<RetailPricelist?> GetByCodeAsync(string Code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Code == Code);
        }

        public async Task<RetailPricelist?> GetByIsActiveAsync(bool IsActive)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.IsActive == IsActive);
        }

    }
}
