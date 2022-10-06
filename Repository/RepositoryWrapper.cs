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

        private ICustomerRepository? oCustomer;
        public ICustomerRepository Customer
        {
            get
            {
                if (oCustomer == null)
                {
                    oCustomer = new CustomerRepository(_repoContext);
                }

                return oCustomer;
            }
        }
        private ICustomerTypeRepository? oCustomerType;
        public ICustomerTypeRepository CustomerType
        {
            get
            {
                if (oCustomerType == null)
                {
                    oCustomerType = new CustomerTypeRepository(_repoContext);
                }

                return oCustomerType;
            }
        }
        private ISupplierRepository? oSupplier;
        public ISupplierRepository Supplier
        {
            get
            {
                if (oSupplier == null)
                {
                    oSupplier = new SupplierRepository(_repoContext);
                }

                return oSupplier;
            }
        }
        private ISupplierTypeRepository? oSupplierType;
        public ISupplierTypeRepository SupplierType
        {
            get
            {
                if (oSupplierType == null)
                {
                    oSupplierType = new SupplierTypeRepository(_repoContext);
                }

                return oSupplierType;
            }
        }
         private IAdminRepository? oAdmin;
        public IAdminRepository Admin
        {
            get
            {
                if (oAdmin == null)
                {
                    oAdmin = new AdminRepository(_repoContext);
                }

                return oAdmin;
            }
        }
        private IAdminLevelRepository? oAdminLevel;
        public IAdminLevelRepository AdminLevel
        {
            get
            {
                if (oAdminLevel == null)
                {
                    oAdminLevel = new AdminLevelRepository(_repoContext);
                }

                return oAdminLevel;
            }
        }
    }
}