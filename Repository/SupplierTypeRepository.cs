using System.Data;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TodoApi.Repositories
{
     public class SupplierTypeRepository : RepositoryBase<SupplierType>, ISupplierTypeRepository
     {
          public SupplierTypeRepository(TodoContext repositoryContext) : base(repositoryContext) { }

          public async Task<IEnumerable<SupplierType>> SearchSupplierType(string searchName)
          {
               return await RepositoryContext.SupplierTypes
                           .Where(s => s.SupplierTypeName.Contains(searchName))
                           .OrderBy(s => s.Id).ToListAsync();
          }

          public bool IsExists(long id)
          {
               return RepositoryContext.SupplierTypes.Any(e => e.Id == id);
          }

     }

}