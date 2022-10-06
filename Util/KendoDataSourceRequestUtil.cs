
using Kendo.Mvc;
using Kendo.Mvc.Infrastructure;
using Kendo.Mvc.UI;
using Serilog;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace TodoApi.Util
{
    public static class KendoDataSourceRequestUtil
    {
        public static DataSourceRequest Parse(dynamic param)
        {
            dynamic objParamInfo = param.gridState;
            string page = objParamInfo.page;
            string pageSize = objParamInfo.pageSize;
            string sort = objParamInfo.sort;
            string group = objParamInfo.group;
            string filter = objParamInfo.filter;
            string aggregates = objParamInfo.aggregate;

            DataSourceRequest request = new();

            try
            {

                if (!string.IsNullOrEmpty(page))
                {
                    _ = Int32.TryParse(page, out int p);
                    request.Page = p;
                }

                if (!string.IsNullOrEmpty(pageSize))
                {
                    _ = Int32.TryParse(pageSize, out int psize);
                    request.PageSize = psize;
                }

                if (!string.IsNullOrEmpty(sort))
                {
                    request.Sorts = DataSourceDescriptorSerializer.Deserialize<SortDescriptor>(sort);
                }

                if (!string.IsNullOrEmpty(filter))
                {
                    request.Filters = FilterDescriptorFactory.Create(filter);
                }

                if (!string.IsNullOrEmpty(group))
                {
                    request.Groups = DataSourceDescriptorSerializer.Deserialize<GroupDescriptor>(group);
                }

                if (!string.IsNullOrEmpty(aggregates))
                {
                    request.Aggregates = DataSourceDescriptorSerializer.Deserialize<AggregateDescriptor>(group);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return request;
        }


        private static string DescriptorToSqlServerQuery (FilterDescriptor fd, ExpandoObject? queryFilter)
        {
            if(queryFilter == null)
            {
                queryFilter = new ExpandoObject();
            }

            string parameterName = "@PARAMETER" + queryFilter.Count().ToString(); // command.Parameters.Count;
            string result;
        
            Object filterValue = fd.Value;
        
            switch (fd.Operator)
            {
                case FilterOperator.IsLessThan:             result = "`" + fd.Member + "`" + " < " + parameterName; break;
                case FilterOperator.IsLessThanOrEqualTo:    result = "`" + fd.Member + "`" + " <= " + parameterName; break;
                case FilterOperator.IsEqualTo:              result = "`" + fd.Member + "`" + " = " + parameterName; break;
                case FilterOperator.IsNotEqualTo:           result = "`" + fd.Member + "`" + " <> " + parameterName; break;
                case FilterOperator.IsGreaterThanOrEqualTo: result = "`" + fd.Member + "`" + " >= " + parameterName; break;
                case FilterOperator.IsGreaterThan:          result = "`" + fd.Member + "`" + " > " + parameterName; break;
                case FilterOperator.StartsWith:
                                                            filterValue = fd.Value.ToString()!.ToSqlSafeLikeData() + "%";
                                                            result = "`" + fd.Member + "`" + " LIKE " + parameterName; break;
                case FilterOperator.EndsWith:
                                                            filterValue = "%" + fd.Value.ToString()!.ToSqlSafeLikeData();
                                                            result = "`" + fd.Member + "`" + " LIKE " + parameterName; break;                   
                case FilterOperator.Contains:
                                                            filterValue = "%" + fd.Value.ToString()!.ToSqlSafeLikeData() + "%";
                                                            result= "`" + fd.Member + "`" + " LIKE " + parameterName; break;
                case FilterOperator.IsContainedIn:
                    throw new Exception("There is no translator for `" + fd.Member + "`" + " " + fd.Operator + " " + fd.Value);
                case FilterOperator.DoesNotContain:
                                                            filterValue = "%" + fd.Value.ToString()!.ToSqlSafeLikeData() + "%";
                                                            result = "`" + fd.Member + "`" + " NOT LIKE " + parameterName; break;
                case FilterOperator.IsNull:     result = "`" + fd.Member + "`" + " IS NULL"; break;
                case FilterOperator.IsNotNull:  result = "`" + fd.Member + "`" + " IS NOT NULL"; break;
                case FilterOperator.IsEmpty:    result = "`" + fd.Member + "`" + " = ''"; break;
                case FilterOperator.IsNotEmpty: result = "`" + fd.Member + "`" + " <> ''"; break;
                default:
                    throw new Exception("There is no translator for [" + fd.Member + "]" + " " + fd.Operator + " " + fd.Value);
            }
            queryFilter.TryAdd("PARAMETER" + queryFilter.Count().ToString(), filterValue);  //without @ sign in parameter
            return result;
        }
        public static string FiltersToParameterizedQuery(IList<IFilterDescriptor> filters, FilterCompositionLogicalOperator compositionOperator = FilterCompositionLogicalOperator.And, ExpandoObject? queryFilter = null) //queryFilter pass by reference. 
        {
       
            if (filters == null) return "";
        
            string result = "(";
            string combineWith = "";
        
            foreach (var filter in filters)
            {
                if (filter is FilterDescriptor fd)
                {
                    result += combineWith + "(" + DescriptorToSqlServerQuery(fd, queryFilter) + ")";
                }
                else if (filter is CompositeFilterDescriptor cfd)
                {
                    result += combineWith + "(" + FiltersToParameterizedQuery(cfd.FilterDescriptors, cfd.LogicalOperator, queryFilter) + ")";
                }
        
                combineWith = (compositionOperator == FilterCompositionLogicalOperator.And) ? " and " : " or ";
            }
            
            result += ")";
            return result;
        }
        public static string ToSqlSafeLikeData(this string val)
        {
            return Regex.Replace(val, @"([%_\[])", @"[$1]").Replace("'", "''");
        }
    }
}
