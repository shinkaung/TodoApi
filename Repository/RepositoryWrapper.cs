using TodoApi.Models;
namespace TodoApi.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly TodoContext _repoContext;

        public RepositoryWrapper(TodoContext repositoryContext)
        {
            _repoContext = repositoryContext;
        }

        private IEmployeeRepository? oEmployee;
        public IEmployeeRepository Employee
        {
            get
            {
                if (oEmployee == null)
                {
                    oEmployee = new EmployeeRepository(_repoContext);
                }

                return oEmployee;
            }
        }

        private IHeroRepository? oHero;
        public IHeroRepository Hero
        {
            get
            {
                if (oHero == null)
                {
                    oHero = new HeroRepository(_repoContext);
                }

                return oHero;
            }
        }
    }
}