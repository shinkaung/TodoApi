using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface IHeroRepository : IRepositoryBase<Hero>
    {
        Task<IEnumerable<Hero>> SearchHero(string searchName);
        bool IsExists(long id);
    }
}