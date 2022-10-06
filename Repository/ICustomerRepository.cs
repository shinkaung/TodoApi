using TodoApi.Models;

using Kendo.Mvc.UI;
namespace TodoApi.Repositories
{
    public interface ICustomerRepository : IRepositoryBase<Customer>
    {
        Task<dynamic> GetCustomerInfoGrid(DataSourceRequest request);
        Task<IEnumerable<Customer>> SearchCustomer(string searchName);
        bool IsExists(long id);

        Task<dynamic> GetCustomerReport(dynamic param);
    }
}