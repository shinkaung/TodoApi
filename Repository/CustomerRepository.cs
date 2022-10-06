using System.Data;
using System.Linq;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Dynamic;
using Kendo.Mvc;
using TodoApi.Util;

namespace TodoApi.Repositories
{
    public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
    {
        public CustomerRepository(TodoContext repositoryContext) : base(repositoryContext) { }

        public async Task<dynamic> GetCustomerInfoGrid(DataSourceRequest request){
            var mainQuery = (from main in RepositoryContext.Customer
                                join ct in RepositoryContext.CustomerType on main.CustomerTypeId equals ct.Id
                            select new{
                                main.Id,
                                main.CustomerName,
                                main.CustomerAddress,
                                ct.CustomerTypeName,
                                main.CustomerTypeId
                            });
            return await mainQuery.ToDataSourceResultAsync(request);
        }

        public async Task<IEnumerable<Customer>> SearchCustomer(string searchTerm)
        {
            return await RepositoryContext.Customer
                        .Where(s => s.CustomerName.Contains(searchTerm))
                        .OrderBy(s => s.Id).ToListAsync();
        }

        public bool IsExists(long id)
        {
            return RepositoryContext.Customer.Any(e => e.Id == id);
        }

        public async Task<dynamic> GetCustomerReport(dynamic param){
            string WhereQuery = " WHERE 1=1 ";
            ExpandoObject queryFilter = new();
            DataSourceRequest request = KendoDataSourceRequestUtil.Parse(param);
            dynamic paramData = param.data; //External data for additional filter

            string GridQuery = KendoDataSourceRequestUtil.FiltersToParameterizedQuery(request.Filters, FilterCompositionLogicalOperator.And, queryFilter);
            if(GridQuery != ""){
                WhereQuery += " AND " + GridQuery;
            }

            // append external filter into where query
            if(paramData.CustomerName.ToString() != ""){
                string cusName = paramData.CustomerName.Value;
                queryFilter.TryAdd("@CustomerName", "%" + cusName + "%");
                WhereQuery += " AND CustomerName LIKE @CustomerName";
            }

            if(paramData.FromDate.ToString() != ""){
                DateTime fromDate = paramData.FromDate + "00:00:00";
                queryFilter.TryAdd("@FromDate", fromDate);
                WhereQuery += " AND RegisterDate >= @FromDate";
            }

            if(paramData.ToDate.ToString() != ""){
                DateTime toDate = paramData.ToDate + "23:59:59";
                queryFilter.TryAdd("@ToDate", toDate);
                WhereQuery += " AND RegisterDate >= @ToDate";
            }

            if(paramData.CustomerTypeId.ToString() != ""){
                long custypeId = paramData.CustomerTypeId.Value;
                queryFilter.TryAdd("@CustomerTypeId", custypeId);
                WhereQuery += " AND c.CustomerTypeId = @CustomerTypeId";
            }

            var SelectQuery = "SELECT c.Id , CustomerName, CustomerAddress, ct.CustomerTypeName";
            var FilterQuery = " FROM customer c " +
                                " INNER JOIN customertype ct ON c.CustomerTypeId = ct.Id " + WhereQuery;
            
            return await RepositoryContext.RunExecuteSelectQuery(SelectQuery, FilterQuery, param, queryFilter);
        }
    }

}