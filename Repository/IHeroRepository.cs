using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface IHeroRepository : IRepositoryBase<Heroes>
    {
        Task<IEnumerable<Heroes>> SearchHero(string searchName);
        bool IsExists(long id);
    }
}