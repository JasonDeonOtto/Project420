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
    public class RetailPricelistItemRepository : Repository<RetailPricelistItem>, IRetailPricelistItemRepository
    {
        public RetailPricelistItemRepository(ManagementDbContext context) : base(context)
        {
        }

        public async Task<RetailPricelistItem?> GetByProductIdAsync(int ProductId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.ProductId == ProductId);
        }

        public async Task<IEnumerable<RetailPricelistItem>> GetByPriceFromAsync(decimal Price)
        {
            return await _dbSet
                .Where(c => c.Price >= Price)
                .OrderBy(c => c.Price)
                .ToListAsync();
        }

        public async Task<IEnumerable<RetailPricelistItem>> GetByPriceTosync(decimal Price)
        {
            return await _dbSet
                .Where(c => c.Price <= Price)
                .OrderBy(c => c.Price)
                .ToListAsync();
        }


    }
}
