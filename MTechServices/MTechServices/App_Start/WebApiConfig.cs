using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace MTechServices
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "RpcApi",
                //routeTemplate: "api/{controller}/{action}/{id}",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                //routeTemplate: "api/{controller}/{id}",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "RpcCustomApi",
                routeTemplate: "{controller}/{action}/{service}",
                defaults: null,
                constraints: new { service = @"^[a-z]+$" }
            );
        }
    }
}
