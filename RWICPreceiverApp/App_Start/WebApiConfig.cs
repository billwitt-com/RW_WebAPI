using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using System.Web.Routing;

namespace RWICPreceiverApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.EnableSystemDiagnosticsTracing();
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

           // InboundICPFinalsController
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //config.Routes.MapHttpRoute(
            //    "PostBlobUpload",
            //    "api/uploadImage",
            //    new { controller = "StationImage", action = "UploadStationImage" },
            //    new { httpMethod = new HttpMethodConstraint("POST") }
            //);

            //config.Routes.MapHttpRoute(
            //    "GetBlobDownload",
            //    "api/downloadImage",
            //    new { controller = "StationImage", action = "DownloadStationImage" },
            //    new { httpMethod = new HttpMethodConstraint("GET") }
            //);
        }
    }
}
