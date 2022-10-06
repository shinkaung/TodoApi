using TodoApi.Models;

using Kendo.Mvc.UI;
namespace TodoApi.Repositories
{
    public interface ISupplierRepository : IRepositoryBase<Supplier>
    {
        Task<dynamic> GetSupplierInfoGrid(DataSourceRequest request);
        Task<IEnumerable<Supplier>> SearchSupplier(string searchName);
        bool IsExists(long id);
    }
}