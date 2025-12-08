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
    public class WholesalePricelistItemRepository : Repository<WholesalePricelistItem>, IWholesalePricelistItemRepository
    {
        public WholesalePricelistItemRepository(ManagementDbContext context) : base(context)
        {
        }

        public async Task<WholesalePricelistItem?> GetByProductIdAsync(int ProductId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.ProductId == ProductId);
        }

        public async Task<IEnumerable<WholesalePricelistItem>> GetByPriceFromAsync(decimal Price)
        {
            return await _dbSet
                .Where(c => c.Price >= Price)
                .OrderBy(c => c.Price)
                .ToListAsync();
        }

        public async Task<IEnumerable<WholesalePricelistItem>> GetByPriceTosync(decimal Price)
        {
            return await _dbSet
                .Where(c => c.Price <= Price)
                .OrderBy(c => c.Price)
                .ToListAsync();
        }


    }
}
