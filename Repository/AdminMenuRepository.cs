using System.Collections.Generic;
using System.Linq;
using System;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Repositories
{
    public class AdminMenuRepository : RepositoryBase<AdminMenu>, IAdminMenuRepository
    {
        public AdminMenuRepository(TodoContext repositoryContext)
            : base(repositoryContext)
        {
        }

        public async Task<IEnumerable<dynamic>> GetAdminMenu(int adminLevelId)
        {

            var res1 = await (from main in RepositoryContext.AdminMenu
                    where main.SrNo <= 1000
                    orderby main.ParentID, main.SrNo
                    select new
                    {
                        main.AdminMenuId,
                        main.ParentID,
                        main.SrNo,
                        main.AdminMenuName,
                        main.Icon,
                        main.ControllerName
                    }
                    ).ToListAsync();

            var res2 = await (from detail in RepositoryContext.AdminMenuDetails
                            select new
                            {
                                detail.AdminMenuId,
                                ParentID = 0,
                                SrNo = 10000,
                                AdminMenuName = "",
                                Icon = "",
                                detail.ControllerName
                            }
                    ).ToListAsync();
            
            return res1.Union(res2)
                    .Select(q => new
                    {
                        AdminLevelId = adminLevelId,
                        MenuID = q.AdminMenuId,
                        q.ParentID,
                        q.SrNo,
                        MenuName = q.AdminMenuName,
                        q.Icon,
                        q.ControllerName,
                        Permission = string.Join(",", (from US in RepositoryContext.AdminMenu
                                                       where US.ParentID == q.AdminMenuId && US.SrNo > 1000
                                                       select US.AdminMenuName).ToList())
                    });
        }

        public async Task<IEnumerable<dynamic>> GetAdminMenuByAdminLevel(int adminLevelID)
        {
            var res1 = await (from ULM in RepositoryContext.AdminLevelMenu
                    join M in RepositoryContext.AdminMenu on ULM.AdminMenuID equals M.AdminMenuId
                    where ULM.AdminLevelId == adminLevelID && M.SrNo <= 1000
                    orderby M.ParentID, M.SrNo
                    select new
                    {
                        M.AdminMenuId,
                        M.ParentID,
                        M.SrNo,
                        M.AdminMenuName,
                        M.Icon,
                        M.ControllerName
                    }
                    ).ToListAsync();

            var res2 = await (from ULM in RepositoryContext.AdminLevelMenu
                            join detail in RepositoryContext.AdminMenuDetails on ULM.AdminMenuID equals detail.AdminMenuId
                            where ULM.AdminLevelId == adminLevelID
                            select new
                            {
                                detail.AdminMenuId,
                                ParentID = 0,
                                SrNo = 10000,
                                AdminMenuName = "",
                                Icon = "",
                                detail.ControllerName
                            }
                    ).ToListAsync();

            return res1.Union(res2)
                    .Select(q => new
                    {
                        AdminLevelID = adminLevelID,
                        MenuID = q.AdminMenuId,
                        q.ParentID,
                        q.SrNo,
                        MenuName = q.AdminMenuName,
                        q.Icon,
                        q.ControllerName,
                        Permission = string.Join(",", (from UU in RepositoryContext.AdminLevelMenu
                                                       join US in RepositoryContext.AdminMenu on UU.AdminMenuID equals US.AdminMenuId
                                                       where US.ParentID == q.AdminMenuId && UU.AdminLevelId == adminLevelID && US.SrNo > 1000 
                                                       select US.AdminMenuName).ToList())
                    });
        }

        public dynamic GetMenuName(int adminlevelID)
        {
            var query = (from ULM in RepositoryContext.AdminLevelMenu
                         join M in RepositoryContext.AdminMenu on ULM.AdminMenuID equals M.AdminMenuId
                         where ULM.AdminLevelId == adminlevelID && M.SrNo <= 1000
                         orderby M.ParentID, M.SrNo
                         select new
                         {
                             M.AdminMenuId,
                             M.ParentID,
                             M.SrNo,
                             M.AdminMenuName,
                             M.Icon,
                             M.ControllerName
                         }).ToList();
            return query;
        }
    }
}
