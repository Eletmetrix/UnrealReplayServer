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
        private readonly DatabaseContext _context;
        private readonly bool bUseUserAgentFilter;
        private readonly string[] AllowedUserAgents;

        public UserAgentActionFilterAttribute(DatabaseContext context)
        {
            _context = context;
            bUseUserAgentFilter = _context.applicationSettings.Where(x => x.Id == 1).FirstOrDefault().bUseUserAgentFilter;
            AllowedUserAgents = _context.applicationSettings.Where(x => x.Id == 1).FirstOrDefault().AllowedUserAgents;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!bUseUserAgentFilter) 
            {
                base.OnActionExecuting(context);
                return;
            }

            try
            {
                string RequestedUserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

                // You can change StartsWith if you want.
                if (AllowedUserAgents.Any(RequestedUserAgent.StartsWith))
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
