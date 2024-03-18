/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace UnrealReplayServer.Web
{
    public class AuthorizationHeaderActionFilterAttribute : ActionFilterAttribute
    {
        private readonly DatabaseContext _context;
        private readonly bool bUseAuthorizationHeader;

        public AuthorizationHeaderActionFilterAttribute(DatabaseContext context)
        {
            _context = context;

            bUseAuthorizationHeader = _context.applicationSettings.FirstOrDefault().bUseAuthorizationHeader;
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            if (!bUseAuthorizationHeader)
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
