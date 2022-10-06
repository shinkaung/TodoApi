//using Entities.ExtendedModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface IAdminLevelRepository :IRepositoryBase<AdminLevel>
    {
        Task<IEnumerable<AdminLevelMenu>> GetAdminLevelMenuBylID(int AdminLevelID);
        Task<bool> CheckDuplicateAdminLevel(int AdminLevelID, string AdminLevel);
        Task<bool> CheckAdminLevelAccessURL(int AdminLevelID, string ServiceUrl);
        Task<dynamic> GetAdminMenu(int chk);
        Task<bool> DeleteAdminLevelMenu(int AdminLevelID);
        Task<bool> AddAdminLevelMenu(List<AdminLevelMenu> newlist);

    }

}
