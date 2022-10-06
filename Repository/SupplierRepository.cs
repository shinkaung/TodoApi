using System.Data;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;


namespace TodoApi.Repositories
{
    public class SupplierRepository : RepositoryBase<Supplier>, ISupplierRepository
    {
        public SupplierRepository(TodoContext repositoryContext) : base(repositoryContext) { }

        public async Task<dynamic> GetSupplierInfoGrid(DataSourceRequest request)
        {
            var mainQuery = (from main in RepositoryContext.Supplier
            join ct in RepositoryContext.SupplierTypes on main.SupplierTypeId equals ct.Id
            select new{
                main.Id,
                main.SupplierName,
                main.RegisterDate,
                main.SupplierAddress,
                ct.SupplierTypeName,
                main.SupplierTypeId,
                main.SupplierPhoto
            });
            return await mainQuery.ToDataSourceResultAsync(request);
        }

        public async Task<IEnumerable<Supplier>> SearchSupplier(string searchTerm)
        {
            return await RepositoryContext.Supplier
                        .Where(s => s.SupplierName.Contains(searchTerm))
                        .OrderBy(s => s.Id).ToListAsync();
        }

        public bool IsExists(long id)
        {
            return RepositoryContext.Supplier.Any(e => e.Id == id);
        }
    }

}