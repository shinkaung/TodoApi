using System.Data;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TodoApi.Repositories
{
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(TodoContext repositoryContext) : base(repositoryContext) { }

        public async Task<IEnumerable<Employee>> SearchEmployee(string searchTerm)
        {
            return await RepositoryContext.Employee
                        .Where(s => s.empName.Contains(searchTerm))
                        .OrderBy(s => s.Id).ToListAsync();
        }

        public bool IsExists(long id)
        {
            return RepositoryContext.Employee.Any(e => e.Id == id);
        }
    }

}