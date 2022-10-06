//using Entities.ExtendedModels;
using System;
using System.Collections.Generic;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface IAdminLevelMenuRepository
    {
        Task<AdminLevelMenu> GetAdminLevelMenuByAdminLevelIDAdminMenuID(int AdminLevelID, int AdminMenuID);
    }
}
