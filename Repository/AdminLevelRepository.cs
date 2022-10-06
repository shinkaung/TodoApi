using System.Collections.Generic;
using System.Linq;
using System;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Serilog;
using System.Text.RegularExpressions;

namespace TodoApi.Repositories
{
    public class AdminLevelRepository: RepositoryBase<AdminLevel>, IAdminLevelRepository
    {
        public AdminLevelRepository(TodoContext repositoryContext):base(repositoryContext)
        {
        }

        public async Task<IEnumerable<AdminLevelMenu>> GetAdminLevelMenuBylID(int AdminLevelID) {
           return await (from adl in RepositoryContext.AdminLevelMenu
                     where adl.AdminLevelId == AdminLevelID
                     select adl).ToListAsync();
        }

        public async Task<dynamic> GetAdminMenu(int chk) {
            var res = await (from main in RepositoryContext.AdminMenu
                    orderby main.SrNo
                    select main).ToListAsync();

            return res.Select(q => new
                {
                    ID = q.AdminMenuId,
                    Name = q.AdminMenuName,
                    ParentID = q.ParentID,
                    Checked = chk
                });
        }

        public async Task<bool> CheckDuplicateAdminLevel(int AdminLevelId, string AdminLevel) 
        {
            return await RepositoryContext.AdminLevel.AnyAsync(e => e.Id != AdminLevelId && e.AdminLevelName == AdminLevel);
        } 

        public async Task<bool> CheckAdminLevelAccessURL(int AdminLevelID, string ServiceUrl)
        {
            ServiceUrl = ServiceUrl.TrimEnd('/');  //if end with /, truncate it
            var checkResult = await (from levelmenu in RepositoryContext.AdminLevelMenu
                            join menuurl in RepositoryContext.AdminMenuUrl on levelmenu.AdminMenuID equals menuurl.AdminMenuId
                            where levelmenu.AdminLevelId == AdminLevelID && 
                                Regex.IsMatch(ServiceUrl, "^(?i)/api/" + menuurl.ServiceURL + "$")   //case insensitive matching and prefix must be /api/
                            select levelmenu.AdminMenuID).AnyAsync();
            return checkResult;
        }

        public async Task<bool> DeleteAdminLevelMenu(int AdminLevelID)
        {
            try {
                var AdminLevelMenu = await (from alm in RepositoryContext.AdminLevelMenu
                                                where alm.AdminLevelId == AdminLevelID
                                                select alm).ToListAsync();
                RepositoryContext.AdminLevelMenu.RemoveRange(AdminLevelMenu);
                Save();
                return true;
            }
            catch(Exception ex) {
                Log.Error(ex.Message);
                return false;
            }
            
        }

        public async Task<bool> AddAdminLevelMenu(List<AdminLevelMenu> newlist)
        {
            try {
                await RepositoryContext.AdminLevelMenu.AddRangeAsync(newlist);
                Save();
                return true;
            }
            catch(Exception ex) {
                Log.Error(ex.Message);
                return false;
            }
        }
      
    }
}
