namespace TodoApi.Repositories
{
    public interface IRepositoryWrapper
    {
        IEmployeeRepository Employee { get; }
        IHeroRepository Hero { get; }
        
    }
}
