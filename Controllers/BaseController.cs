using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Security.Claims;


namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : Controller
    {        
        private ActionExecutingContext? _actionExecutionContext;
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _actionExecutionContext = context;       
        }
        
        protected string GetTokenData(string claimname)
        {
            try
            {
                if(_actionExecutionContext == null)
                    return "";

                ClaimsIdentity objclaim = _actionExecutionContext.HttpContext.User.Identities.Last();
                if(objclaim != null)
                {
                    if(objclaim.FindFirst(claimname) != null)
                        return objclaim.FindFirst(claimname)!.Value;
                    else
                    {
                        Log.Warning("Get Token Data Not Found" + claimname);
                        return "";
                    }
                        
                }
                else
                    return "";
            }
            catch(Exception ex)
            {
                Log.Error("Get Token Data Exception :" + ex.Message);
                return "";
            }
            
        }
    }
}