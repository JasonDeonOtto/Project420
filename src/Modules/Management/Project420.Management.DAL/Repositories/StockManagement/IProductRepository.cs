using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.ProductManagement;
using Project420.Management.Models.Entities.Sales.Retail;

namespace Project420.Management.DAL.Repositories.StockManagement
{
    public interface IProductRepository : IRepository<Product>
    {
    }
}
