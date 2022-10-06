using System.Data;
using System.Linq;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Repositories
{
    public class HeroRepository : RepositoryBase<Heroes>, IHeroRepository
    {
        public HeroRepository(TodoContext repositoryContext) : base(repositoryContext) { }

        public async Task<IEnumerable<Heroes>> SearchHero(string searchTerm)
        {
            return await RepositoryContext.Heroess
                        .Where(s => s.Name.Contains(searchTerm))
                        .OrderBy(s => s.Id).ToListAsync();
        }

        public bool IsExists(long id)
        {
            return RepositoryContext.Heroess.Any(e => e.Id == id);
        }
    }

}