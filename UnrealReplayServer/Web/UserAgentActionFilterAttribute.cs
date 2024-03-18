/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UnrealReplayServer.Web
{
    public class UserAgentActionFilterAttribute : ActionFilterAttribute
    {
        public UserAgentActionFilterAttribute()
        { }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_applicationDefaults.UserAgentDetails.bUseUserAgentFilter) 
            {
                base.OnActionExecuting(context);
                return;
            }

            try
            {
                string RequestedUserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

                // You can change StartsWith if you want.
                if (_applicationDefaults.UserAgentDetails.AllowedUserAgents.Any(RequestedUserAgent.StartsWith))
                {
                    base.OnActionExecuting(context);
                    return;
                }

                context.Result = new StatusCodeResult(403);
            }
            catch
            {
                context.Result = new StatusCodeResult(403);
                return;
            }
        }
    }
}
