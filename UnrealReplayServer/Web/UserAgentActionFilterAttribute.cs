/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using UnrealReplayServer.Connectors;

namespace UnrealReplayServer.Web
{
    public class UserAgentActionFilterAttribute : ActionFilterAttribute
    {
        private readonly ApplicationDefaults _applicationDefaults;

        public UserAgentActionFilterAttribute(IOptions<ApplicationDefaults> options)
        {
            _applicationDefaults = options.Value;
        }

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
