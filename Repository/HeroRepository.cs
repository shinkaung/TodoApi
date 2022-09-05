using System.Data;
using System.Linq;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Repositories
{
    public class HeroRepository : RepositoryBase<Hero>, IHeroRepository
    {
        public HeroRepository(TodoContext repositoryContext) : base(repositoryContext) { }

        public async Task<IEnumerable<Hero>> SearchHero(string searchTerm)
        {
            return await RepositoryContext.Hero
                        .Where(s => s.Name.Contains(searchTerm))
                        .OrderBy(s => s.id).ToListAsync();
        }

        public bool IsExists(long id)
        {
            return RepositoryContext.Hero.Any(e => e.id == id);
        }
    }

}