using System.Collections.Generic;
using System.Linq;
using System;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public class AdminLevelMenuRepository : RepositoryBase<AdminLevelMenu>, IAdminLevelMenuRepository
    {
        public AdminLevelMenuRepository(TodoContext repositoryContext)
            : base(repositoryContext)
        {
        }

        public async Task<AdminLevelMenu> GetAdminLevelMenuByAdminLevelIDAdminMenuID(int AdminLevelID, int AdminMenuID)
        {
            var res = await FindByConditionAsync(AdminLevelMenu => AdminLevelMenu.AdminLevelId.Equals(AdminLevelID) && AdminLevelMenu.AdminMenuID.Equals(AdminMenuID));
            var resobj = res.FirstOrDefault();
            if(resobj == null)
                throw new Exception("Not Found Admin Level and Menu");
            else
                return resobj;
        }
        
    }
}
