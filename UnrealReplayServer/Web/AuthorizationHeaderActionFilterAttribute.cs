using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using UnrealReplayServer.Connectors;
using UnrealReplayServer.Databases.Models;

namespace UnrealReplayServer.Web
{
    public class AuthorizationHeaderActionFilterAttribute : ActionFilterAttribute
    {
        private readonly ApplicationDefaults _applicationDefaults;
        private readonly DatabaseContext _context;

        public AuthorizationHeaderActionFilterAttribute(DatabaseContext context, IOptions<ApplicationDefaults> options)
        {
            _applicationDefaults = options.Value;
            _context = context;
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_applicationDefaults.bUseAuthorizationHeader)
            {
                base.OnActionExecuting(context);
                return;
            }

            string AuthorizationHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

            var result = await _context.authorizationHeaders
                .Where(x => x.AuthorizationHeaderValue == AuthorizationHeader &&
                            (!x.bUseRemainingUse || x.RemainingUse < 0))
                .ToListAsync();
        }
    }
}
