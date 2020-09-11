using CustomAuth.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace CustomAuth
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies["AuthCookie"];
            if(authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                var serializedModel = JsonConvert.DeserializeObject<AuthSerializeModel>(authTicket.UserData);

                var principal = new AuthPrincipal(authTicket.Name)
                {
                    Id = serializedModel.Id,
                    FirstName = serializedModel.FirstName,
                    LastName = serializedModel.LastName,
                    Roles = serializedModel.Roles
                };

                HttpContext.Current.User = principal;
            }
        }
    }
}
