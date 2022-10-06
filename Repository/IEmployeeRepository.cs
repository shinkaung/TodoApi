using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface IEmployeeRepository : IRepositoryBase<Employee>
    {
        Task<IEnumerable<Employee>> SearchEmployee(string searchName);
        bool IsExists(long id);
    }
}