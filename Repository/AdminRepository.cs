using System.Collections.Generic;
using System.Linq;
using System;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TodoApi.Repositories
{
    public class AdminRepository : RepositoryBase<Admin>, IAdminRepository
    {
        public AdminRepository(TodoContext repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<Admin?> GetAdminByLoginName(string loginName)
        {
            var result = await FindByConditionAsync(x => x.LoginName == loginName);
            return result.FirstOrDefault();
        }

        public async Task<dynamic> GetAdminListByLevel(int level)
        {
            var query = (from ad in RepositoryContext.Admin
                         where  ad.AdminLevelId == level
                         select new
                         {
                            ad.AdminName,
                            ad.Id
                         });
            return await query.ToListAsync();
        }
        public async Task<bool> CheckDuplicateAdminName(int adminID, string adminName)
        {
            return await RepositoryContext.Admin.AnyAsync(e => e.Id != adminID && e.AdminName == adminName);
            //var res = await FindByConditionAsync(x => x.AdminID != adminID && x.AdminName == adminName);
            //return res.Count();
        }
        public async Task<bool> CheckDuplicateAdminLoginName(int adminID, string loginName)
        {
            //var res = await FindByConditionAsync(x => x.AdminID != adminID && x.LoginName == loginName);
            //return res.Count();
            return await RepositoryContext.Admin.AnyAsync(e => e.Id != adminID && e.LoginName == loginName);
        }

        public async Task<dynamic> GetAdmins(DataSourceRequest request)
        {
            //string salt = Util.SaltedHash.GenerateSalt();

            //DataSourceRequest request = Util.KendoDataSourceRequestUtil.Parse(param);
            var mainQuery = (from main in RepositoryContext.Admin
                             join level in RepositoryContext.AdminLevel on main.AdminLevelId equals level.Id
                             select new
                             {
                                 main.Id,
                                 main.AdminLevelId,
                                 main.AdminName,
                                 main.LoginName,
                                 level.AdminLevelName,
                                 main.Email,
                                 main.Inactive,
                                 main.IsBlock
                             });
            return await mainQuery.ToDataSourceResultAsync(request);
        }
        
        public async Task<int> FindAdminLevelID(int AdminLevelID)
        {
            int count = await (from a in RepositoryContext.Admin where a.AdminLevelId==AdminLevelID select a).CountAsync();
            return count;
        }

        public async Task<IEnumerable<dynamic>> GetAdminLoginValidation(string username)
        {
            return await (from usr in RepositoryContext.Admin
                    join ul in RepositoryContext.AdminLevel on usr.AdminLevelId equals ul.Id into tmp
                    from c in tmp
                    where usr.LoginName == (string)username
                    select new
                    {
                        usr.Password,
                        usr.Salt,
                        usr.Id,
                        usr.AdminName,
                        usr.AdminLevelId,
                        usr.LoginFailCount,
                        usr.Inactive,
                        usr.IsBlock,
                        usr.Email,
                        usr.LoginName,
                        c.AdminLevelName,
                        c.IsAdministrator
                    }).ToListAsync();
        }

        public async Task<bool> IsExists(long id)
        {
            return await RepositoryContext.Admin.AnyAsync(e => e.Id == id);
        }
    }
}
