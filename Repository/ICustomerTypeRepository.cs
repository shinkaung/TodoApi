using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface ICustomerTypeRepository : IRepositoryBase<CustomerType>
    {
        Task<IEnumerable<CustomerType>> SearchCustomerType(string searchName);
        bool IsExists(long id);
    }
    }
