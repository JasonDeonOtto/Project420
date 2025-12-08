using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Wholesale;

namespace Project420.Management.DAL.Repositories.Sales.Wholesale
{
    public interface IWholesalePricelistRepository : IRepository<WholesalePricelist>
    {
        Task<WholesalePricelist?> GetByNameAsync(string Name);

        Task<WholesalePricelist?> GetByCodeAsync(string Code);

        Task<WholesalePricelist?> GetByIsActiveAsync(bool IsActive);

    }
}
