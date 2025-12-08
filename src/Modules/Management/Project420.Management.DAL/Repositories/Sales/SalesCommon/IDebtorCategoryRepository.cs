using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Common;

namespace Project420.Management.DAL.Repositories.Sales.SalesCommon;

public interface IDebtorCategoryRepository : IRepository<DebtorCategory>
{
    Task<DebtorCategory?> GetByIsActiveAsync(bool IsActive);

    Task<DebtorCategory?> GetByNameAsync(string Name);

}


