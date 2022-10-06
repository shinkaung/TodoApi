using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Dapper;
using MySqlConnector;
using Kendo.Mvc.UI;
using Newtonsoft.Json.Linq;

namespace TodoApi.Models
{
    public class TodoContext : DbContext
    {
        private readonly string _connectionString;
        public TodoContext(DbContextOptions<TodoContext> options, IConfiguration configuration)
            : base(options)
        {
            _connectionString = configuration!["ConnectionStrings:DefaultConnection"]!;
        }

        public async Task<dynamic> RunExecuteSelectQuery(string selectClause, string whereClause, dynamic objGridState, ExpandoObject queryParams)
        {
            int pageIndex = Convert.ToInt32(objGridState.gridState.page);
            int pageSize = Convert.ToInt32(objGridState.gridState.pageSize);
            int startRow = (pageIndex - 1) * pageSize;
            string sortClause = "";
            if(queryParams == null)
            {
                queryParams = new ExpandoObject();
            }
            queryParams.TryAdd("startRow", startRow);
            queryParams.TryAdd("pageSize", pageSize);

            if(objGridState.gridState.sort != null)
            {
                string sort = objGridState.gridState.sort;
                if(sort.Split("-").Length == 2) 
                {
                    string sortfield = sort.Split("-")[0];
                    string sortdirection = "ASC ";
                    if(sort.Split("-")[1] == "desc")
                        sortdirection = "DESC ";
                    if(selectClause.IndexOf(sortfield) > 0)  //to void sql injection, validate sort column name in select clause
                    {
                        sortClause = " ORDER BY " + sortfield + " " + sortdirection;
                    }
                }
            }
                
            string Query = selectClause + " " + whereClause + " " + sortClause + " limit @startRow, @pageSize;";

            using (var conn = new MySqlConnection(_connectionString))  
            {  
                //var result = conn.Query(Query, queryParams);
                var result = await conn.QueryAsync(Query, queryParams);

                string CountQuery = "SELECT count(*) as TotalRecord " + whereClause;
                var rowcount = await conn.QueryFirstAsync<int>(CountQuery, queryParams);
                return new DataSourceResult
                {
                    Total = rowcount,
                    Data = result
                };
            }  
        }
        public async Task<int> RunExecuteNonQuery(string query, JObject queryparams)
        {
            try
            {
                using (MySqlConnection conn = new(_connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new(query, conn);

                    if (queryparams.Count > 0)
                    {
                        foreach(var obj in queryparams)
                        {
                            string fieldname = obj.Key;
                            var fieldvalue = ((JValue)obj.Value!).Value;

                            if (fieldname.Trim() != "")
                                cmd.Parameters.AddWithValue(fieldname, fieldvalue);
                        }
                    }
                    var affectrows = await cmd.ExecuteNonQueryAsync();
                    return affectrows;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " RunExecuteNonQuery Exception :" + ex.Message);
            }
            return 0;
        }

        public async Task<dynamic> RunExecuteSelectQuery(string Query, ExpandoObject? queryFilter = null)
        {
            if(queryFilter == null)
            {
                queryFilter = new ExpandoObject();
            }

            using (var conn = new MySqlConnection(_connectionString))  
            {  
                var result = await conn.QueryAsync(Query, queryFilter);
                return result;
            } 
        }
    

        public DbSet<TodoItem> TodoItems { get; set; } = null!;
        public DbSet<Heroes> Heroess { get; set; } = null!;
        public DbSet<TodoApi.Models.Employee> Employee { get; set; } = null!;
        public DbSet<Customer> Customer { get; set; } = null!;
        public DbSet<CustomerType> CustomerType { get; set; } = null!;

        public DbSet<Supplier> Supplier { get; set; } = null!;

        public DbSet<SupplierType> SupplierTypes { get; set; } = null!;

         public DbSet<Admin> Admin { get; set; } = null!;

        public DbSet<AdminLevel> AdminLevel { get; set; } = null!;

        public DbSet<AdminLevelMenu> AdminLevelMenu { get; set; } = null!;

        public DbSet<AdminMenu> AdminMenu { get; set; } = null!;

        public DbSet<AdminMenuUrl> AdminMenuUrl { get; set; } = null!;
        public DbSet<AdminMenuDetails> AdminMenuDetails { get; set; } = null!;
    }
}