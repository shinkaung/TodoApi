using TodoApi.Models;
using Kendo.Mvc.UI;

namespace TodoApi.Repositories
{
    public interface IAdminRepository : IRepositoryBase<Admin>
    {
        Task<Admin?> GetAdminByLoginName(string loginName);
        Task<dynamic> GetAdminListByLevel(int level);
        Task<int> FindAdminLevelID(int AdminLevelID);
        Task<dynamic> GetAdmins(DataSourceRequest request);
        Task<bool> CheckDuplicateAdminName(int adminID, string adminName);
        Task<bool> CheckDuplicateAdminLoginName(int adminID, string loginName);
        Task<bool> IsExists(long id);
        Task<IEnumerable<dynamic>> GetAdminLoginValidation(string username);


    }

}
