using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Retail;

namespace Project420.Management.DAL.Repositories.Sales.Retail
{
    public interface IRetailPricelistRepository : IRepository<RetailPricelist>
    {
        Task<RetailPricelist?> GetByNameAsync(string Name);

        Task<RetailPricelist?> GetByCodeAsync(string Code);

        Task<RetailPricelist?> GetByIsActiveAsync(bool IsActive);

    }
}
