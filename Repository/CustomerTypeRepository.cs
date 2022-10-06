using System.Data;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TodoApi.Repositories
{
     public class CustomerTypeRepository : RepositoryBase<CustomerType>, ICustomerTypeRepository
     {
          public CustomerTypeRepository(TodoContext repositoryContext) : base(repositoryContext) { }

          public async Task<IEnumerable<CustomerType>> SearchCustomerType(string searchName)
          {
               return await RepositoryContext.CustomerType
                           .Where(s => s.CustomerTypeName.Contains(searchName))
                           .OrderBy(s => s.Id).ToListAsync();
          }

          public bool IsExists(long id)
          {
               return RepositoryContext.CustomerType.Any(e => e.Id == id);
          }

     }

}