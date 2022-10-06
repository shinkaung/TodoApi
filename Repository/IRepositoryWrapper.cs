namespace TodoApi.Repositories
{
    public interface IRepositoryWrapper
    {
        IEmployeeRepository Employee { get; }
        IHeroRepository Hero { get; }
        ICustomerRepository Customer { get; }    
       ICustomerTypeRepository CustomerType { get; }   
       ISupplierRepository Supplier { get; }   

       ISupplierTypeRepository SupplierType { get; }

       IAdminRepository Admin { get; }   

       IAdminLevelRepository AdminLevel { get; }
    }
}
