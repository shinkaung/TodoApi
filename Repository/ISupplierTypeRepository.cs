using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface ISupplierTypeRepository : IRepositoryBase<SupplierType>
    {
        Task<IEnumerable<SupplierType>> SearchSupplierType(string searchName);
        bool IsExists(long id);
    }
    }
