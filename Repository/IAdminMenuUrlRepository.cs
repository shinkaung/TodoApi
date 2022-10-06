//using Entities.ExtendedModels;
using System;
using System.Collections.Generic;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface IAdminMenuUrlRepository
    {
        Task<AdminMenuUrl> GetAdminMenuUrlByServiceUrl(string ServiceUrl);
    }
}
