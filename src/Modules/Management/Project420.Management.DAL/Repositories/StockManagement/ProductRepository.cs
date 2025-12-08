using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.DAL.Repositories.Sales.Retail;
using Project420.Management.Models.Entities.ProductManagement;
using Project420.Management.Models.Entities.Sales.Retail;

namespace Project420.Management.DAL.Repositories.StockManagement
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ManagementDbContext context) : base(context)
        {
        }

    }
}
