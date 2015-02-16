using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using log4net.Config;

namespace MTechServices
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            /*
            RouteTable.Routes.MapHttpRoute("SignOn", "AuthenticationService/SignOn/{username}/{password}/{appName}",
            new { username = UrlParameter.Optional, password = UrlParameter.Optional, appName = UrlParameter.Optional, Action = "Get" });
            */
            //RouteTable.Routes.MapHttpRoute("SignOff", "AuthenticationService/SignOff", null, null);
           
            /*
            RouteTable.Routes.MapHttpRoute(null, "api/{controller}/{action}/{paramater1}/{paramater2}/{paramater3}",
            new { paramater1 = UrlParameter.Optional, paramater2 = UrlParameter.Optional, paramater3 = UrlParameter.Optional, Action = "Get" });
            */

            XmlConfigurator.Configure();
        }
    }
}