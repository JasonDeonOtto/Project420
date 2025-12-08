using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Retail;

namespace Project420.Management.DAL.Repositories.Sales.Retail
{
    public interface IRetailPricelistItemRepository : IRepository<RetailPricelistItem>
    {
        Task<RetailPricelistItem?> GetByProductIdAsync(int ProductId);
        Task<IEnumerable<RetailPricelistItem>> GetByPriceFromAsync(decimal Price);
        Task<IEnumerable<RetailPricelistItem>> GetByPriceTosync(decimal Price);
        //these should link to the pricelist we are currently looking at though (not all pricelists)
    }
}
