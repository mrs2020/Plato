﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plato.Internal.Abstractions.Settings;
using Plato.Internal.Shell.Models;
using Plato.Internal.Abstractions;
using Plato.Internal.Stores.Abstractions;

namespace Plato.Internal.Hosting.Web.Routing
{
    public class PlatoRouterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly Dictionary<string, RequestDelegate> _pipelines = new Dictionary<string, RequestDelegate>();

        public PlatoRouterMiddleware(
            RequestDelegate next,
            ILogger<PlatoRouterMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task Invoke(HttpContext httpContext)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Begin Routing Request");
            
            var shellSettings = (ShellSettings)httpContext.Features[typeof(ShellSettings)];
            
            // Define a PathBase for the current request that is the RequestUrlPrefix.
            // This will allow any view to reference ~/ as the tenant's base url.
            // Because IIS or another middleware might have already set it, we just append the tenant prefix value.
            if (!string.IsNullOrEmpty(shellSettings.RequestedUrlPrefix))
            {
                httpContext.Request.PathBase += ("/" + shellSettings.RequestedUrlPrefix);
                httpContext.Request.Path = httpContext.Request.Path.ToString().Substring(httpContext.Request.PathBase.Value.Length);
            }
            
            // Do we need to rebuild the pipeline ?
            var rebuildPipeline = httpContext.Items["BuildPipeline"] != null;
            if (rebuildPipeline && _pipelines.ContainsKey(shellSettings.Name))
            {
                _pipelines.Remove(shellSettings.Name);
            }

            if (!_pipelines.TryGetValue(shellSettings.Name, out var pipeline))
            {
                // Building a pipeline can't be done by two requests
                lock (_pipelines)
                {
                    if (!_pipelines.TryGetValue(shellSettings.Name, out pipeline))
                    {
                        pipeline = BuildTenantPipeline(shellSettings, httpContext.RequestServices);

                        //if (shellSettings.State == Shell.Models.TenantState.Running)
                        //{
                       
                            _pipelines.Add(shellSettings.Name, pipeline);
                        //}
                    }
                }
            }

            await pipeline.Invoke(httpContext);
        }

        // Build the middleware pipeline for the current tenant
        public RequestDelegate BuildTenantPipeline(
            ShellSettings shellSettings, 
            IServiceProvider serviceProvider)
        {
            var startups = serviceProvider.GetServices<IStartup>();
            var inlineConstraintResolver = serviceProvider.GetService<IInlineConstraintResolver>();
            var appBuilder = new ApplicationBuilder(serviceProvider);

            var routePrefix = "";
            if (!string.IsNullOrWhiteSpace(shellSettings.RequestedUrlPrefix))
                routePrefix = shellSettings.RequestedUrlPrefix + "/";
            
            var routeBuilder = new RouteBuilder(appBuilder)
            {
                DefaultHandler = serviceProvider.GetRequiredService<MvcRouteHandler>()
            };

            var prefixedRouteBuilder = new PrefixedRouteBuilder(
                routePrefix, 
                routeBuilder,
                inlineConstraintResolver);

            foreach (var startup in startups)
            {
                startup.Configure(appBuilder, prefixedRouteBuilder, serviceProvider);
            }
                 
            //// The default route is added to each tenant as a template route, with a prefix
            prefixedRouteBuilder.Routes.Add(new Route(
                prefixedRouteBuilder.DefaultHandler,
                "areaRoute",
                "{area:exists}/{controller}/{action}/{id?}",
                null,
                null,
                null,
                inlineConstraintResolver)
            );


            // Attempt to get homepage route for tennet from site settings store
            // If the tennet has not been created yet siteService will return null
            // if siteService returns null users will be presented with the SetUp module
            var siteService = routeBuilder.ServiceProvider.GetService<ISiteSettingsStore>();
            if (siteService != null)
            {
                // Add home page route
                routeBuilder.Routes.Add(new HomePageRoute(
                    shellSettings.RequestedUrlPrefix,
                    siteService,
                    routeBuilder,
                    inlineConstraintResolver));
            }
            
            // Build router
            var router = prefixedRouteBuilder.Build();

            // Use router
            appBuilder.UseRouter(router);

            // Return new pipline
            var pipeline = appBuilder.Build();
            return pipeline;
        }
    }
}