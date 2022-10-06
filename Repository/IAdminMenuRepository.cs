//using Entities.ExtendedModels;
using System;
using System.Collections.Generic;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface IAdminMenuRepository  : IRepositoryBase<AdminMenu>
    {
        Task<IEnumerable<dynamic>> GetAdminMenu(int adminLevelID);
        Task<IEnumerable<dynamic>> GetAdminMenuByAdminLevel(int adminLevelID);
        dynamic GetMenuName(int adminlevelID);
    }
}
