using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Retail;
using Project420.Management.Models.Entities.Sales.Wholesale;

namespace Project420.Management.DAL.Repositories.Sales.Wholesale
{
    public interface IWholesalePricelistItemRepository : IRepository<WholesalePricelistItem>
    {
        Task<WholesalePricelistItem?> GetByProductIdAsync(int ProductId);
        Task<IEnumerable<WholesalePricelistItem>> GetByPriceFromAsync(decimal Price);
        Task<IEnumerable<WholesalePricelistItem>> GetByPriceTosync(decimal Price);
        //these should link to the pricelist we are currently looking at though (not all pricelists)
    }
}
