using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CustomAuth.Auth
{
    public class AuthAttrribute : AuthorizeAttribute
    {
        protected virtual AuthPrincipal CurrentUser => HttpContext.Current.User as AuthPrincipal;
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return CurrentUser != null && CurrentUser.IsInRole(Roles);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            RedirectToRouteResult routeData = CurrentUser is null
                ? new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }))
                : new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));

            filterContext.Result = routeData;
        }
    }
}