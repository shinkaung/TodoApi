using System.Collections.Generic;
using System.Linq;
using System;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public class AdminMenuUrlRepository : RepositoryBase<AdminMenuUrl>, IAdminMenuUrlRepository
    {
        public AdminMenuUrlRepository(TodoContext repositoryContext)
            : base(repositoryContext)
        {
        }
       
        public async Task<AdminMenuUrl> GetAdminMenuUrlByServiceUrl(string ServiceUrl)
        {
            var res = await FindByConditionAsync(AdminMenuUrl => AdminMenuUrl.ServiceURL.Equals(ServiceUrl));
            var resobj = res.FirstOrDefault();
            if(resobj == null)
                throw new Exception("Not found menu service url");
            else
                return resobj;
        }
    }
}
