using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebApplication1
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // auth route
            config.Routes.MapHttpRoute(
                name: "Auth",
                routeTemplate: "api/auth",
                defaults: new { controller = "auth"}
            );

            // user route
            config.Routes.MapHttpRoute(
                name: "User",
                routeTemplate: "api/user",
                defaults: new { controller = "user" }
            );

        }
    }
}
